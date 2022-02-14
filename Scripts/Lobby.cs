using Godot;

public class Lobby : Panel
{
    private const int DefaultPort = 8982;
    private const int MaxClients = 1;

    private LineEdit _address;
    private Button _hostButton;
    private Button _joinButton;
    private Label _status;
    private NetworkedMultiplayerENet _peer;

    public override void _Ready()
    {
        _address = GetNode<LineEdit>("Address");
        _hostButton = GetNode<Button>("Host");
        _joinButton = GetNode<Button>("Join");
        _status = GetNode<Label>("Status");
    
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(ConnectedOk));
        GetTree().Connect("connection_failed", this, nameof(ConnectedFail));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));

        _hostButton.Connect("pressed", this, nameof(HostGame));
        _joinButton.Connect("pressed", this, nameof(JoinGame));
    }

    private void HostGame()
    {
        var peer = new NetworkedMultiplayerENet();
        peer.CreateServer(DefaultPort, MaxClients);
        GetTree().NetworkPeer = peer;

        GD.Print($"Hosting game at {_address.Text}:{DefaultPort}");
    }

    private void JoinGame()
    {
        var clientPeer = new NetworkedMultiplayerENet();
        var result = clientPeer.CreateClient(_address.Text, DefaultPort);
        GetTree().NetworkPeer = clientPeer;
        
        GD.Print($"Connecting to game at {_address.Text}:{DefaultPort}");
    }
    
    private void PlayerConnected(int id)
    {
        
    }

    private void PlayerDisconnected(int id)
    {
        
    }

    private void ConnectedOk()
    {
        GD.Print("Connected");
        InGame();
        StartGame();
    }

    private void ConnectedFail()
    {
        GetTree().NetworkPeer = null;
        GD.PushWarning("Failed to Connect!");
        InMenu();
    }

    private void ServerDisconnected()
    {
        GD.PushWarning("Disconnected from server");
        InMenu();
    }

    [Remote]
    private void StartGame()
    {
        SpawnPlayer(GetTree().GetNetworkUniqueId());
    }

    private void SpawnPlayer(int id)
    {
        var playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");

        var playerNode = playerScene.Instance<Player>();
        playerNode.Name = id.ToString();
        playerNode.SetNetworkMaster(id);

        AddChild(playerNode);
    }

    [Remote]
    private void RemovePlayer(int id)
    {
        GetNode(id.ToString()).QueueFree();
    }

    private void InGame()
    {
        _hostButton.Disabled = true;
        _joinButton.Disabled = true;
        _address.Editable = false;
    }

    private void InMenu()
    {
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
        _address.Editable = false;
    }
}