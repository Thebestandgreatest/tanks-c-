using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Networking : Node
{
    private const int DefaultPort = 5672;
    private const int MaxPlayers = 10;

    private NetworkedMultiplayerENet _server = null;
    private NetworkedMultiplayerENet _client = null;

    public string IpAddress = "";

    public override void _Ready()
    {
        foreach (object ip in IP.GetLocalAddresses())
        {
            string address = (string) ip;
            if (address.StartsWith("192.168") && !address.EndsWith(".1"))
            {
                IpAddress = (string) ip;
            }
        }

        GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    public void CreateServer()
    {
        _server = new NetworkedMultiplayerENet();
        _server.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = _server;
    }

    public void JoinServer()
    {
        _client = new NetworkedMultiplayerENet();
        _client.CreateClient(IpAddress, DefaultPort);
        GetTree().NetworkPeer = _client;
    }

    public void ConnectedToServer()
    {
        GD.Print("Connected to server");
    }

    public void ServerDisconnected()
    {
        GD.Print("Disconnected from server");
    }
}