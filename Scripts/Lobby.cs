using Godot;
using Godot.Collections;

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
    private LineEdit _name;

    private Dictionary<int, string> _players = new Dictionary<int, string>();
    private const string PlayerName = "one";

    public override void _Ready()
    {
        _address = GetNode<LineEdit>("Address");
        _hostButton = GetNode<Button>("Host");
        _joinButton = GetNode<Button>("Join");
        _status = GetNode<Label>("Status");
        _name = GetNode<LineEdit>("Name");
        
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

    private void OnHostPressed()
    {
        
    }

    private void OnJoinPressed()
    {
        
    }

    private void PlayerConnected()
    {
        
    }

    private void PlayerDisconnected()
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
}
