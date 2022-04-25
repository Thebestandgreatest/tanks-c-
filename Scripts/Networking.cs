using Godot;
using Godot.Collections;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Networking : Node
{
    public const int DefaultPort = 5672;
    private const int MaxPlayers = 10;

    private NetworkedMultiplayerENet _server;
    private NetworkedMultiplayerENet _client;

    public int BulletIndex = 1;

    public readonly Dictionary<int, string> Players = new Dictionary<int,string>();

    public override void _Ready()
    {
        GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    public void CreateServer()
    {
        _server = new NetworkedMultiplayerENet();
        _server.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = _server;
    }

    public void JoinServer(string ipAddress, int port)
    {
        _client = new NetworkedMultiplayerENet();
        _client.CreateClient(ipAddress, port);
        GetTree().NetworkPeer = _client;
    }

    public void ConnectedToServer()
    {
        GD.Print("Connected to server");
    }

    public void ServerDisconnected()
    {
        GD.Print("Disconnected from server");
        GetTree().NetworkPeer = null;
    }

    public void AddPlayer(int id, string name)
    {
        Players.Add(id, name);
    }
}