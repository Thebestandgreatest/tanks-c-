using System.Linq;
using System.Net.NetworkInformation;
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
        _level = GetTree().Root.GetNode<Node2D>("Level");
        _level.Visible = false;
        
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

    public void OnHostPressed()
    {
        _level.Visible = true;
        NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
        peer.CreateServer(DefaultPort, 32);
        GetTree().NetworkPeer = peer;
        GD.Print("Hosting Server");

        //TODO: validate name
        StartGame();
    }

    public void OnJoinPressed()
    {
        _level.Visible = true;
        string address = _address.Text;

        //TODO: validate ip
        //TODO: validate name

        NetworkedMultiplayerENet clientPeer = new NetworkedMultiplayerENet();
        Error result = clientPeer.CreateClient(address, DefaultPort);

        GetTree().NetworkPeer = clientPeer;
    }

    public void LeaveGame()
    {
        foreach (var player in _players)
        {
            GetNode(player.Key.ToString()).QueueFree();
        }
        _players.Clear();
        GetNode(GetTree().GetNetworkUniqueId().ToString()).QueueFree();
        Rpc(nameof(RemovePlayer), GetTree().GetNetworkUniqueId());
        ((NetworkedMultiplayerENet)GetTree().NetworkPeer).CloseConnection();
        GetTree().NetworkPeer = null;
    }

    private void PlayerConnected(int id)
    {
        RpcId(id, nameof(RegisterPlayer), _name.Text);
    }

    private void PlayerDisconnected(int id)
    {
        RemovePlayer(id);
    }

    private void ConnectedOk()
    {
        StartGame();
    }

    private void ConnectedFail()
    {
        GetTree().NetworkPeer = null;
    }

    private void ServerDisconnected()
    {
        LeaveGame();
    }

    [Remote]
    private void RegisterPlayer(string playerName)
    {
        int id = GetTree().GetRpcSenderId();
        _players.Add(id, playerName);

        SpawnPlayer(id, playerName);
    }

    [Remote]
    public void StartGame()
    {
        SpawnPlayer(GetTree().GetNetworkUniqueId(), _name.Text);
    }

    [Remote]
    private void SpawnPlayer(int id, string playerName)
    {
        Player playerNode = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn").Instance<Player>();
        playerNode.Name = id.ToString();
        playerNode.SetNetworkMaster(id);

        playerNode.Position = new Vector2(500, 250 * id);

        GetTree().Root.GetNode("Level").AddChild(playerNode);
    }

    [Remote]
    private void RemovePlayer(int id)
    {
        if (_players.ContainsKey(id))
        {
            _players.Remove(id);
            GetNode(id.ToString()).QueueFree();
        }
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