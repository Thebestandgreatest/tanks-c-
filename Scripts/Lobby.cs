using System;
using Godot;

public class Lobby : Panel
{
    private const int DefaultPort = 8982;
    private const int MaxPlayers = 10;

    private LineEdit _address;
    private Button _hostButton;
    private Button _joinButton;
    private Label _status;
    private LineEdit _name;
    private NetworkedMultiplayerENet _peer;

    public override void _Ready()
    {
        _address = GetNode<LineEdit>("Address");
        _hostButton = GetNode<Button>("Host");
        _joinButton = GetNode<Button>("Join");
        _status = GetNode<Label>("Status");
        _name = GetNode<LineEdit>("Name");
    
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
        peer.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = peer;
    }

    private void JoinGame()
    {
        string ip = _address.Text;
        if (!ip.IsValidIPAddress())
        {
            _status.Text = "Invalid ip address";
            return;
        }
        
        var peer = new NetworkedMultiplayerENet();
        peer.CreateClient(_address.Text, DefaultPort);
        GetTree().NetworkPeer = peer;
    }
    
    private void PlayerConnected(int id)
    {
        var playerNode = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Level1.tscn").Instance();

        GetTree().Root.AddChild(playerNode);
    }

    private void PlayerDisconnected(int id)
    {
        
    }

    private void ConnectedOk()
    {
        
    }

    private void ConnectedFail()
    {
        GetTree().NetworkPeer = null;
        GD.PushWarning("Failed to connect to server");
    }

    private void ServerDisconnected()
    {
        GD.PushWarning("Disconnected from the server");
    }
}