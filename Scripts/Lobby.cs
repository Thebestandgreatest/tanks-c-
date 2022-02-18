using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Lobby : Panel
{
    private const int DefaultPort = 5672;
    private const int MaxPlayers = 10;

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
        
        // button signals
        _hostButton.Connect("pressed", this, nameof(OnHostPressed));
        _joinButton.Connect("pressed", this, nameof(OnJoinPressed));
        
        // multiplayer signals
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(ConnectedOk));
        GetTree().Connect("connection_failed", this, nameof(ConnectedFail));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    private void PlayerConnected(int id)
    {
        // start game
        var level = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Level1.tscn").Instance();
        GetTree().Root.AddChild(level);
        Hide();
        
        var playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
        var player = (Player) playerScene.Instance();
        player.SetNetworkMaster(id);
        GetTree().Root.AddChild(player);
    }

    private void PlayerDisconnected(int id)
    {
        // not used currently
    }

    private void ConnectedOk()
    {
        // not used currently
    }

    private void ConnectedFail()
    {
        _status.Text = "Couldn't Connect to server";

        GetTree().NetworkPeer = null;
    }

    private void ServerDisconnected()
    {
        GetTree().NetworkPeer = null;
    }

    private void OnHostPressed()
    {
        _peer = new NetworkedMultiplayerENet();

        Error err = _peer.CreateServer(DefaultPort, MaxPlayers);
        if (err != Error.Ok)
        {
            _status.Text = "Couldn't create a server";
            return;
        }

        GetTree().NetworkPeer = _peer;
        var level = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Level1.tscn").Instance();
        GetTree().Root.AddChild(level);
        Hide();
    }

    private void OnJoinPressed()
    {
        string ip = _address.Text;
        if (!ip.IsValidIPAddress())
        {
            _status.Text = "Invalid IP address";
            return;
        }

        _peer = new NetworkedMultiplayerENet();
        _peer.CreateClient(ip, DefaultPort);
        GetTree().NetworkPeer = _peer;
    }
}
