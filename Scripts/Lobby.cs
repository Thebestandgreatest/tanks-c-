using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Lobby : Panel
{
    private const int DefaultPort = 5672;
    private const int MaxPlayers = 10;

    private readonly Godot.Collections.Dictionary<int, string> _players =
        new Godot.Collections.Dictionary<int, string>();

    private LineEdit _address;
    private Button _hostButton;
    private Button _joinButton;
    private LineEdit _name;
    private Node2D _level;

    private Panel _playerPanel;
    private Button _startButton;
    private Label _status;
    private ItemList _teamAList;
    private ItemList _teamBList;

    public override void _Ready()
    {
        _level = GetNode<Node2D>("../Level");
        _address = GetNode<LineEdit>("Address");
        _hostButton = GetNode<Button>("Host");
        _joinButton = GetNode<Button>("Join");
        _status = GetNode<Label>("Status");
        _name = GetNode<LineEdit>("Name");

        _playerPanel = GetNode<Panel>("../Players");
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

    public void OnHostPressed()
    {
        GD.Print("hosting server");
        NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
        peer.CreateServer(DefaultPort, MaxPlayers);
        GetTree().NetworkPeer = peer;

        OS.SetWindowTitle("Host");
        //TODO: validate name
        Hide();
        _playerPanel.Show();
        RefreshLobby();
    }

    public void OnJoinPressed()
    {
        GD.Print("joining server");
        string address = _address.Text;
        OS.SetWindowTitle("Client");
        
        //TODO: validate ip
        //TODO: validate name

        NetworkedMultiplayerENet clientPeer = new NetworkedMultiplayerENet();
        Error result = clientPeer.CreateClient(address, DefaultPort);
        GetTree().NetworkPeer = clientPeer;
        Hide();
        //_playerPanel.Show();
        //todo: temporary code
        _playerPanel.Hide();
        _level.Show();
    }
    
    public void StartGame()
    {
        PrepareGame();
        RegisterPlayer(_name.Text);
        GD.Print("starting game");
        _playerPanel.Hide();
        _level.Show();
        SpawnPlayers();
    }

    private void LeaveGame()
    {
        GD.Print("leaving server");
        foreach (var player in _players)
        {
            GetNode(player.Key.ToString()).QueueFree();
        }
        _players.Clear();
        GetNode(GetTree().GetNetworkUniqueId().ToString()).QueueFree();
        Rpc(nameof(RemovePlayer), GetTree().GetNetworkUniqueId());
        ((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
        GetTree().NetworkPeer = null;
        Show();
    }

    private void PlayerConnected(int id)
    {
        GD.Print("player connected");
        RpcId(id, nameof(RegisterPlayer), _name.Text);
        RefreshLobby();
    }

    private void PlayerDisconnected(int id)
    {
        GD.Print("player disconnected");
        RemovePlayer(id);
        RefreshLobby();
    }

    private void ConnectedOk()
    {
        GD.Print("Connected okay");
        //StartGame();
    }

    private void ConnectedFail()
    {
        GD.Print("connected fail");
        GetTree().NetworkPeer = null;
    }

    private void ServerDisconnected()
    {
        GD.Print("server disconnected");
        LeaveGame();
    }

    [RemoteSync]
    private void PrepareGame()
    {
        Hide();
        _playerPanel.Hide();
        _level.Show();
    }
    
    [Remote]
    private void RegisterPlayer(string playerName)
    {
        GD.Print("registering player " + playerName);
        int id = GetTree().GetRpcSenderId();
        _players.Add(id, playerName);
        RefreshLobby();
    }

    [RemoteSync]
    private void SpawnPlayers()
    {
        PackedScene playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
        int i = 1;
        GD.Print(_players);
        foreach (var player in _players)
        {
            Player playerNode = playerScene.Instance<Player>();
            playerNode.Name = player.Key.ToString();
            GD.Print();
            GD.Print("spawning " + player.Value + " and taking over id:" + player.Key);
            playerNode.SetNetworkMaster(player.Key);
            playerNode.Position = new Vector2(500, 250 * i);
            _level.AddChild(playerNode);
            i++;
        }
    }

    [Remote]
    private void RemovePlayer(int id)
    {
        GD.Print("removing player");
        if (!_players.ContainsKey(id)) return;
        _players.Remove(id);
        GetNode(id.ToString()).QueueFree();
        RefreshLobby();
    }
    
    private void RefreshLobby()
    {
        _teamAList.Clear();
        _teamBList.Clear();
        _teamAList.AddItem(_name.Text + " (You)");
        foreach (var p in _players)
        {
            _teamAList.AddItem(p.Value);
        }

        _startButton.Disabled = !GetTree().IsNetworkServer();
    }
}