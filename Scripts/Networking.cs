using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Networking : Node
{
    [Signal]
    internal delegate void PlayerDiedSignal(int id);
    
    internal const int DefaultPort = 5672;
    private const int MaxPlayers = 10;

    private NetworkedMultiplayerENet _server;
    private NetworkedMultiplayerENet _client;

    internal int BulletIndex = 1;

    public override void _Ready()
    {
        GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    internal void CreateServer()
    {
        _server = new NetworkedMultiplayerENet();
        _server.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = _server;
    }

    internal void JoinServer(string ipAddress, int port)
    {
        _client = new NetworkedMultiplayerENet();
        _client.CreateClient(ipAddress, port);
        GetTree().NetworkPeer = _client;
    }

    internal void ConnectedToServer()
    {
        GD.Print("Connected to server");
    }

    internal void ServerDisconnected()
    {
        GD.Print("Disconnected from server");
    }

    internal void ResetNetwork()
    {
        GetTree().NetworkPeer = null;
        _client = null;
        _server = null;
    }
}