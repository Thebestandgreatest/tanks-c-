using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class Lobby : Panel
{
	private static readonly Dictionary<int, string> Players = new Dictionary<int, string>();
	private static readonly Dictionary<int, bool> PlayersAlive = new Dictionary<int, bool>();

	private PackedScene _playerScene;
	
	private Networking _network;
	private Node2D _world;

	private Camera2D _camera;
	
	private LineEdit _address;
	private Button _hostButton;
	private Button _joinButton;
	private LineEdit _name;

	private Panel _hostPanel;

	private Panel _playerPanel;
	private Button _startButton;
	private Label _status;
	private ItemList _teamAList;

	private Panel _endPanel;
	private Label _winningPlayer;
	private Button _menuButton;

	public override void _Ready()
	{
		_playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
		
		// autoloads
		_network = GetNode<Networking>("/root/Network");
		_world = GetNode<Node2D>("/root/Players");

		_camera = GetNode<Camera2D>("../Camera2D");
		
		// join panel
		_address = GetNode<LineEdit>("Address");
		_hostButton = GetNode<Button>("Host");
		_joinButton = GetNode<Button>("Join");
		_status = GetNode<Label>("Status");
		_name = GetNode<LineEdit>("Name");

		//hosting panel
		_hostPanel = GetNode<Panel>("../Host");
		
		// player panel
		_playerPanel = GetNode<Panel>("../Players");
		_teamAList = _playerPanel.GetNode<ItemList>("Team A List");
		_playerPanel.GetNode<ItemList>("Team B List");
		_startButton = _playerPanel.GetNode<Button>("Start");
		
		// end panel
		_endPanel = GetNode<Panel>("../End");
		_winningPlayer = _endPanel.GetNode<Label>("VBoxContainer/Winner");
		_menuButton = _endPanel.GetNode<Button>("VBoxContainer/HBoxContainer/Menu Button");

		// button signals
		_hostButton.Connect("pressed", this, nameof(OnHostPressed));
		_joinButton.Connect("pressed", this, nameof(OnJoinPressed));
		_startButton.Connect("pressed", this, nameof(OnStartPressed));
		_menuButton.Connect("pressed", this, nameof(OnMenuPressed));
		
		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));

		_network.Connect("PlayerDiedSignal", this, nameof(PlayerDied));
	}

	private void PlayerDisconnected(int id)
	{
		Console.WriteLine($"Player {id} disconnected");
		if (_world.HasNode($"Players/{id.ToString()}"))
		{
			_world.GetNode($"Players/{id.ToString()}").QueueFree();
		}
	}

	internal void OnHostPressed()
	{
		if (!CheckUsername()) return;
		
		Console.WriteLine("Server");
		OS.SetWindowTitle("Server");
		Hide();
		_network.CreateServer();
		InstancePlayer(GetTree().GetNetworkUniqueId());
		RpcId(1, nameof(AddPlayer), GetTree().GetNetworkUniqueId(), _name.Text);
		PreStartGame();
	}

	internal void OnJoinPressed()
	{
		if (!CheckUsername()) return;

		Console.WriteLine("Client");
		OS.SetWindowTitle("Client");
		string[] address = _address.Text.Split(":");
		
		string ip = address[0];
		ip = IP.ResolveHostname(ip);
		if (ip.Length == 0)
		{
			_status.Text = "failed to resolve domain name";
			return;
		}
		
		int port;
		try
		{
			port = address[1].ToInt();
		}
		catch
		{
			port = Networking.DefaultPort;
		}

		if (ip != "" && ip.IsValidIPAddress())
		{
			Hide();
			_network.JoinServer(ip, port);
		}
		else
		{
			_status.Text = "Invalid ip address";
			return;
		}

		_startButton.Disabled = true;
		PreStartGame();
	}

	private bool CheckUsername()
	{
		if (_name.Text.Trim() != "")
		{
			return true;
		}
		_status.Text = "Invalid Username";
		return false;

	}

	private void PlayerConnected(int id)
	{
		Console.WriteLine($"Player {id} connected");
		InstancePlayer(id);
	}
	
	private void ConnectedToServer()
	{
		InstancePlayer(GetTree().GetNetworkUniqueId());
		RpcId(1, nameof(AddPlayer), GetTree().GetNetworkUniqueId(), _name.Text);
		RpcId(1, nameof(SendPlayerList));
	}
	
	private void InstancePlayer(int id)
	{
		GetTree().Paused = true;
		
		Node2D playerInstance =
		Global.InstanceNodeAtLocation(_playerScene, _world, new Vector2((float) GD.RandRange(-17, 17) * 100, (float) GD.RandRange(-14, 16) * 100), 0F, 9);
		playerInstance.Name = id.ToString();
		playerInstance.SetNetworkMaster(id);
		GetTree().Paused = true;
	}

	private void PreStartGame()
	{
		_playerPanel.Show();
		UpdateLobby();
	}

	[Sync]
	private void AddPlayer(int id, string name)
	{
		Players.Add(id, name);
		PlayersAlive.Add(id, true);
	}

	[Puppet]
	private void UpdatePlayerList(int id, string name)
	{
		Players.Add(id, name);
		PlayersAlive.Add(id, true);
	}

	[Puppet]
	private void ClearPlayerList()
	{
		Players.Clear();
		PlayersAlive.Clear();
	}

	[Sync]
	private void SendPlayerList()
	{
		Rpc(nameof(ClearPlayerList));
		foreach (KeyValuePair<int, string> player in Players)
		{
			Rpc(nameof(UpdatePlayerList), player.Key, player.Value);
		}

		Rpc(nameof(UpdateLobby));
	}

	[Sync]
	private void UpdateLobby()
	{
		_teamAList.Clear();
		
		foreach (KeyValuePair<int, string> player in Players)
		{
			_teamAList.AddItem(player.Value);
		}
	}

	[Sync]
	private void StartGame()
	{
		PackedScene level = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Level A.tscn");
		Global.InstanceNodeAtLocation(level, _world);
		_playerPanel.Hide();
		GetTree().Paused = false;
	}
	
	internal void OnStartPressed()
	{
		Rpc(nameof(StartGame));
	}

	public void PlayerDied(int id)
	{
		Console.WriteLine($"player {id} died");
		
		PlayersAlive[id] = false;

		List<int> alive = PlayersAlive.Where(x => x.Value).Select(x => x.Key).ToList();

		if (alive.Count > 1) return;
		Console.WriteLine($"Player {alive[0]}");
		Rpc(nameof(EndGame), alive[0]);
	}

	[Sync]
	private void EndGame(int id)
	{
		GetTree().Paused = true;
		_camera.MakeCurrent();
		_endPanel.Show();
		_winningPlayer.Text = $"{Players[id]} won the game!";
	}

	internal void OnMenuPressed()
	{
		_endPanel.Hide();
		Show();
		_network.ResetNetwork();
		Players.Clear();
		PlayersAlive.Clear();
		foreach (Node child in _world.GetChildren())
		{
			child.QueueFree();
		}
	}
}
