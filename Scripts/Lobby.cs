using System;
using System.Collections.Generic;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Lobby : Panel
{
	private static Dictionary<int, string> Players = new Dictionary<int, string>();
	private static Dictionary<int, bool> PlayersAlive = new Dictionary<int, bool>();

	private PackedScene _player;
	private Node2D _world;
	
	private LineEdit _address;
	private Button _hostButton;
	private Button _joinButton;
	private LineEdit _name;

	private Panel _hostPanel;

	private Panel _playerPanel;
	private Button _startButton;
	private Label _status;
	private ItemList _teamAList;

	private Networking _network;

	public override void _Ready()
	{
		_player = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
		
		// autoloads
		_network = GetNode<Networking>("/root/Network");
		GetNode<Global>("/root/Global");
		_world = GetNode<Node2D>("/root/Players");
		
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

		// button signals
		_hostButton.Connect("pressed", this, nameof(OnHostPressed));
		_joinButton.Connect("pressed", this, nameof(OnJoinPressed));
		_startButton.Connect("pressed", this, nameof(OnStartPressed));

		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
	}

	private void PlayerConnected(int id)
	{
		Console.WriteLine($"Player {id} connected");
		InstancePlayer(id, "");
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
		InstancePlayer(GetTree().GetNetworkUniqueId(), _name.Text);
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
		if (_name.Text.Trim() != "") return true;
		_status.Text = "Invalid Username";
		return false;

	}

	private void ConnectedToServer()
	{
		InstancePlayer(GetTree().GetNetworkUniqueId(), _name.Text);
		Rpc(nameof(RefreshLobby));
	}
	
	private void InstancePlayer(int id, string name)
	{
		GetTree().Paused = true;
		Players.Add(id, name);
		Node2D playerInstance =
			Global.InstanceNodeAtLocation(_player, _world, new Vector2((float) GD.RandRange(-1700, 1700), (float) GD.RandRange(-1400, 1600)));
		playerInstance.Name = id.ToString();
		playerInstance.SetNetworkMaster(id);
		GetTree().Paused = true;
	}

	private void PreStartGame()
	{
		_playerPanel.Show();
		RefreshLobby();
	}

	[RemoteSync]
	private void RefreshLobby()
	{
		Console.WriteLine(Players);
		foreach (KeyValuePair<int, string> player in Players)
		{
			_teamAList.AddItem(player.Value);
		}
	}

	[Sync]
	private void StartGame()
	{
		_playerPanel.Hide();
		GetTree().Paused = false;
	}
	
	internal void OnStartPressed()
	{
		Rpc(nameof(StartGame));
	}
}
