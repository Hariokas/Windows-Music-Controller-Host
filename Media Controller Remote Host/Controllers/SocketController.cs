using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Media_Controller_Remote_Host.EventClasses;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Host.Controllers;

public class SocketController : IDisposable
{
    private readonly List<WebSocket> _clients = new();
    private bool _disposed;
    private int _lastImageHash;

    private int _lastMessageHash;

    private HttpListener _server;

    public SocketController(string serverIp, string port)
    {
        IpAddress = serverIp;
        Port = port;
        _ = RunAsync();
    }

    public string IpAddress { get; }
    public string Port { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event EventHandler<MediaSessionEventArgs> MediaSessionCommandReceived;
    public event EventHandler<MasterVolumeEventArgs> MasterVolumeCommandReceived;
    public event EventHandler<VolumeMixerEventArgs> VolumeMixerCommandReceived;

    private async Task RunAsync()
    {
        Trace.WriteLine($"WebSocket Server running on ws://{IpAddress}:{Port}");
        _server = StartServer(IpAddress, Port);
        _ = AcceptClientsAsync(_server);
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
                                            ?? throw new ArgumentNullException(
                                                $"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    MasterVolumeCommandReceived?.Invoke(this, new MasterVolumeEventArgs(masterVolumeEvent));
                    break;

                case BaseEventType.MediaSessionEvent:
                    var mediaSessionEvent = JsonConvert.DeserializeObject<MediaSessionEvent>(message)
                                            ?? throw new ArgumentNullException(
                                                $"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    MediaSessionCommandReceived?.Invoke(this, new MediaSessionEventArgs(mediaSessionEvent));
                    break;

                case BaseEventType.VolumeMixerEvent:
                    var volumeMixerEvent = JsonConvert.DeserializeObject<VolumeMixerEvent>(message)
                                           ?? throw new ArgumentNullException(
                                               $"Failed to deserialize {nameof(MasterVolumeEvent)}");
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

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (!disposing)
        {
            _server?.Stop();
            _server?.Close();
            _server = null;

            foreach (var client in _clients.Where(client => client.State == WebSocketState.Open))
                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait();
            _clients.Clear();
        }

        _disposed = true;
    }

    ~SocketController()
    {
        Dispose(false);
    }
}