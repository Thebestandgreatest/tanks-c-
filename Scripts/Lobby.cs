using System.Collections.Generic;
using Godot;
using Godot.Collections;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Lobby : Panel
{
    private const int DefaultPort = 5672;
    private const int MaxPlayers = 10;

    private readonly Godot.Collections.Dictionary<int, string> _players = new Godot.Collections.Dictionary<int, string>();

    private LineEdit _address;
    private Button _hostButton;
    private Button _joinButton;
    private LineEdit _name;
    private NetworkedMultiplayerENet _peer;

    private Panel _playerPanel;
    private Button _startButton;
    private Label _status;
    private ItemList _teamAList;
    private ItemList _teamBList;

    public override void _Ready()
    {
        _address = GetNode<LineEdit>("Address");
        _hostButton = GetNode<Button>("Host");
        _joinButton = GetNode<Button>("Join");
        _status = GetNode<Label>("Status");
        _name = GetNode<LineEdit>("Name");

        _playerPanel = GetParent().GetNode<Panel>("Players");
        _teamAList = _playerPanel.GetNode<ItemList>("Team A List");
        _teamBList = _playerPanel.GetNode<ItemList>("Team B List");
        _startButton = _playerPanel.GetNode<Button>("Start");

        // button signals
        _hostButton.Connect("pressed", this, nameof(OnHostPressed));
        _joinButton.Connect("pressed", this, nameof(OnJoinPressed));
        _startButton.Connect("pressed", this, nameof(StartGame));

        // multiplayer signals
        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(ConnectedOk));
        GetTree().Connect("connection_failed", this, nameof(ConnectedFail));
        GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
    }

    private void OnHostPressed()
    {
        if (_name.Text == "")
        {
            _status.Text = "Invalid Name";
            return;
        }

        Hide();
        _playerPanel.Show();
        _status.Text = "";
        
        HostGame();
        RefreshLobby();
    }

    private void OnJoinPressed()
    {
        if (_name.Text == "")
        {
            _status.Text = "Invalid Name";
            return;
        }
        
        if (!_address.Text.IsValidIPAddress())
        {
            _status.Text = "Invalid IP address";
            return;
        }

        _status.Text = "";
        _hostButton.Disabled = true;
        _joinButton.Disabled = true;
        _name.Editable = false;
        _address.Editable = false;

        JoinGame();
    }
    
    private void StartGame()
    {
        Godot.Collections.Dictionary<int, string> spawnPoints = new Godot.Collections.Dictionary<int, string>();
        
        foreach (var p in _players)
        {
            RpcId(p.Key, nameof(PreloadGame), spawnPoints);
        }
    }

    private void PlayerConnected(int id)
    {
        RpcId(id, nameof(RegisterPlayer), _name.Text);
    }

    private void PlayerDisconnected(int id)
    {
        if (GetTree().IsNetworkServer())
            EndGame();
        else
            UnregisterPlayer(id);
    }

    private void ConnectedOk()
    {
        Hide();
        _playerPanel.Show();
    }

    private void ConnectedFail()
    {
        GetTree().NetworkPeer = null;
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
        _address.Editable = true;
        _name.Editable = true;
    }

    private void ServerDisconnected()
    {
        EndGame();
    }

    private void UnregisterPlayer(int id)
    {
        if (_players.ContainsKey(id)) _players.Remove(id);
        RefreshLobby();
    }

    private void HostGame()
    {
        _peer = new NetworkedMultiplayerENet();
        _peer.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = _peer;
    }

    private void JoinGame()
    {
        _peer = new NetworkedMultiplayerENet();
        _peer.CreateClient(_address.Text, DefaultPort);
        GetTree().NetworkPeer = _peer;
    }

    private void EndGame()
    {
        if (HasNode("/root/level")) GetNode("/root/level").QueueFree();

        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
        _address.Editable = true;
        _name.Editable = true;
        Show();
    }

    private void RefreshLobby()
    {
        _teamAList.Clear();
        _teamBList.Clear();
        _teamAList.AddItem(_name.Text + " (You)");
        foreach (var p in _players) _teamAList.AddItem(p.Value);

        _startButton.Disabled = !GetTree().IsNetworkServer();
    }

    [Remote]
    private void RegisterPlayer(string playerName)
    {
        int id = GetTree().GetRpcSenderId();
        _players[id] = playerName;
        RefreshLobby();
    }

    [Remote]
    private void PreloadGame(Dictionary spawnPoints)
    {
        Node level = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Level A.tscn").Instance();
        GetTree().Root.AddChild(level);
        _playerPanel.Hide();

        Node playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn").Instance();
        
    }
}