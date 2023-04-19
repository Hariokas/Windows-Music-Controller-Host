using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WPF_TestPlayground;

public class WebSocketCommunicator
{
    private readonly SocketController _socketController;

    public WebSocketCommunicator(string serverIp, string serverPort)
    {
        _socketController = new SocketController(serverIp, serverPort, this);
    }

    public event EventHandler<MediaSessionEventArgs> CommandReceived;

    public async Task DistributeJsonAsync(object jsonObject)
    {
        var jsonString = JsonConvert.SerializeObject(jsonObject);
        await _socketController.BroadcastMessage(jsonString);
    }

    public async Task DistributeImageAsync(byte[] imageData)
    {
        await _socketController.BroadcastImage(imageData);
    }

    public void RaiseCommandReceived(object sender, MediaSessionEventArgs args)
    {
        CommandReceived?.Invoke(this, args);
    }
}