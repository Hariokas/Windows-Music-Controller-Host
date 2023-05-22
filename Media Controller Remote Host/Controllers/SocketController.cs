using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fleck;
using Media_Controller_Remote_Host.EventClasses;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Host.Controllers;

public class SocketController : IDisposable
{
    private readonly List<IWebSocketConnection> _clients = new();

    private bool _disposed;

    private WebSocketServer _server;

    public SocketController(string serverIp, string port)
    {
        IpAddress = serverIp;
        Port = port;
        Run();
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

    private void Run()
    {
        Trace.WriteLine($"WebSocket Server running on ws://{IpAddress}:{Port}");
        _server = StartServer(IpAddress, Port);
    }

    private WebSocketServer StartServer(string serverIp, string serverPort)
    {
        var server = new WebSocketServer($"ws://{serverIp}:{serverPort}");

        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Trace.WriteLine($"Client connected! [{socket.ConnectionInfo.ClientIpAddress}]");
                _clients.Add(socket);
            };

            socket.OnClose = () =>
            {
                Trace.WriteLine($"Client disconnected! [{socket.ConnectionInfo.ClientIpAddress}]");
                _clients.Remove(socket);
            };

            socket.OnMessage = message => { HandleClientMessage(socket, message); };
        });

        return server;
    }

    private void HandleClientMessage(IWebSocketConnection client, string message)
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

    public void BroadcastImage(byte[] imageData)
    {
        Trace.WriteLine($"Sending image of size: {imageData.Length}");

        foreach (var client in _clients)
            if (client.IsAvailable)
                client.Send(imageData);
    }

    public void BroadcastMessage(string message)
    {
        Trace.WriteLine($"Sending message: {message}");

        foreach (var client in _clients)
            if (client.IsAvailable)
                client.Send(message);
    }

    public void DistributeJsonAsync(object jsonObject)
    {
        var jsonString = JsonConvert.SerializeObject(jsonObject);
        BroadcastMessage(jsonString);
    }

    public void DistributeImageAsync(byte[] imageData)
    {
        BroadcastImage(imageData);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _server?.Dispose();
            _server = null;

            foreach (var client in _clients.Where(client => client.IsAvailable))
                client.Close();

            _clients.Clear();
        }

        _disposed = true;
    }

    ~SocketController()
    {
        Dispose(false);
    }
}