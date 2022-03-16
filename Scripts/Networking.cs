using Godot;

// ReSharper disable once CheckNamespace
public class Networking : Node
{
    private const int DefaultPort = 5672;
    private const int MaxPlayers = 10;
    
    private readonly Godot.Collections.Dictionary<int, string> _players =
        new Godot.Collections.Dictionary<int, string>();

    private string _playerName = "";

    public override void _Ready()
    {
        // multiplayer signals
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(ConnectedOk));
        GetTree().Connect("connection_failed", this, nameof(ConnectedFail));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    private void CreateServer(string newPlayerName)
    {
        GD.Print("Creating Server");
        _playerName = newPlayerName;
        OS.SetWindowTitle("Server");
        
        NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
        peer.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = peer;
    }

    private void JoinServer(string playerName, string serverAddress)
    {
        NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
        peer.CreateClient(serverAddress, DefaultPort);
        GetTree().NetworkPeer = peer;
    }

    private void PlayerConnected(int id)
    {
        RpcId(id, nameof(RegisterPlayer), _playerName);
    }

    private void PlayerDisconnected(int id)
    {
        
    }

    private void ConnectedOk()
    {
        
    }

    private void ConnectedFail()
    {
        
    }

    private void ServerDisconnected()
    {
        
    }


    [Remote] private void RegisterPlayer(string playerName)
    {
        int id = GetTree().GetRpcSenderId();
        _players.Add(id, playerName);
    }
}