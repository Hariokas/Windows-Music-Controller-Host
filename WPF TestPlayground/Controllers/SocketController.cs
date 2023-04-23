﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WPF_TestPlayground.EventClasses;

namespace WPF_TestPlayground.Controllers;

public class SocketController
{
    private readonly List<WebSocket> _clients = new();
    private readonly MediaManager _mediaManager;

    private int _lastMessageHash;
    private int _lastImageHash;

    public event EventHandler<MediaSessionEventArgs> MediaSessionCommandReceived;
    public event EventHandler<MasterVolumeEventArgs> MasterVolumeCommandReceived;
    public event EventHandler<VolumeMixerEventArgs> VolumeMixerCommandReceived;

    public SocketController(string serverIp, string port, MediaManager mediaManager)
    {
        _mediaManager = mediaManager;
        _ = RunAsync(serverIp, port);
    }

    private async Task RunAsync(string serverIp, string serverPort)
    {
        Trace.WriteLine($"WebSocket Server running on ws://{serverIp}:{serverPort}");
        var server = StartServer(serverIp, serverPort);
        _ = AcceptClientsAsync(server);
    }

    private static HttpListener StartServer(string serverIp, string serverPort)
    {
        var server = new HttpListener();
        server.Prefixes.Add($"http://{serverIp}:{serverPort}/");
        server.Start();
        return server;
    }

    private async Task AcceptClientsAsync(HttpListener server)
    {
        while (true)
        {
            var context = await server.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                _ = HandleClientAsync(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
            }
        }
    }

    private async Task HandleClientAsync(HttpListenerContext context)
    {
        WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
        var client = wsContext.WebSocket;
        //client.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        _clients.Add(client);

        Trace.WriteLine("Client connected");

        try
        {
            var buffer = new byte[1024];

            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleClientMessage(client, receivedMessage);
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _clients.Remove(client);
            Trace.WriteLine("Client disconnected");
        }
    }

    private void HandleClientMessage(WebSocket client, string message)
    {
        Trace.WriteLine($"Received message: {message}");
        try
        {
            var baseEvent = JsonConvert.DeserializeObject<BaseEvent>(message);

            switch (baseEvent.EventType)
            {
                case BaseEventType.MasterVolumeEvent:
                    var masterVolumeEvent = JsonConvert.DeserializeObject<MasterVolumeEvent>(message) 
                                            ?? throw new ArgumentNullException($"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    MasterVolumeCommandReceived?.Invoke(this, new MasterVolumeEventArgs(masterVolumeEvent));
                    break;

                case BaseEventType.MediaSessionEvent:
                    var mediaSessionEvent = JsonConvert.DeserializeObject<MediaSessionEvent>(message) 
                                            ?? throw new ArgumentNullException($"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    MediaSessionCommandReceived?.Invoke(this, new MediaSessionEventArgs(mediaSessionEvent));
                    break;

                case BaseEventType.VolumeMixerEvent:
                    var volumeMixerEvent = JsonConvert.DeserializeObject<VolumeMixerEvent>(message) 
                                           ?? throw new ArgumentNullException($"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    VolumeMixerCommandReceived?.Invoke(this, new VolumeMixerEventArgs(volumeMixerEvent));
                    break;

                default:
                    Trace.WriteLine($"Unknown event type: {baseEvent.EventType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task BroadcastImage(byte[] imageData)
    {
        Trace.WriteLine($"Sending image of size: {imageData.Length}");
        var segment = new ArraySegment<byte>(imageData);
        for (var i = _clients.Count - 1; i >= 0; i--)
        {
            var client = _clients[i];

            if (client.State == WebSocketState.Open)
                await client.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
            else
                _clients.RemoveAt(i);
        }
    }

    public async Task BroadcastMessage(string message)
    {
        Trace.WriteLine($"Sending message: {message}");
        var data = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(data);

        for (var i = _clients.Count - 1; i >= 0; i--)
        {
            var client = _clients[i];

            if (client.State == WebSocketState.Open)
                _ = client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            else
                _clients.RemoveAt(i);
        }
    }

    public async Task DistributeJsonAsync(object jsonObject)
    {
        var jsonString = JsonConvert.SerializeObject(jsonObject);

        var jsonStringHash = jsonString.GetHashCode();
        //if (_lastMessageHash == jsonStringHash) return;

        await BroadcastMessage(jsonString);

        _lastMessageHash = jsonStringHash;
    }

    public async Task DistributeImageAsync(byte[] imageData)
    {
        var imageDataHash = imageData.GetHashCode();
        //if (_lastImageHash == imageDataHash) return;

        await BroadcastImage(imageData);

        _lastImageHash = imageDataHash;
    }

}