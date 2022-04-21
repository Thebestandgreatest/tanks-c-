using System;
using System.Collections.Generic;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Lobby : Panel
{
	private PackedScene _player;
	private Node2D _players;
	
	private LineEdit _address;
	private Button _hostButton;
	private Button _joinButton;
	private LineEdit _name;

	private Panel _hostPanel;

	private Panel _playerPanel;
	private Button _startButton;
	private Label _status;
	private ItemList _teamAList;
	private ItemList _teamBList;

	private Networking _network;
	private Global _global;

	public override void _Ready()
	{
		_player = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
		
		// autoloads
		_network = GetNode<Networking>("/root/Network");
		_global = GetNode<Global>("/root/Global");
		_players = GetNode<Node2D>("/root/Players");
		
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
		_teamBList = _playerPanel.GetNode<ItemList>("Team B List");
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
		GD.Print("Player " + id + " connected");
		InstancePlayer(id);
	}

	private void PlayerDisconnected(int id)
	{
		GD.Print("Player " + id + " disconnected");
		if (_players.HasNode(id.ToString()))
		{
			_players.GetNode(id.ToString()).QueueFree();
		}
	}

	public void OnHostPressed()
	{
		Hide();

		GD.Print("Server");
		OS.SetWindowTitle("Server");
		Hide();
		_network.CreateServer();
		InstancePlayer(GetTree().GetNetworkUniqueId());
		PreStartGame();
	}

	public void OnJoinPressed()
	{
		GD.Print("Client");
		OS.SetWindowTitle("Client");
		string[] address = _address.Text.Split(":");

		string ip = address[0];
		int port;
		try
		{
			port = address[1].ToInt();
		}
		catch (Exception e)
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
		}

		_startButton.Disabled = true;
		PreStartGame();
	}

	private void ConnectedToServer()
	{
		InstancePlayer(GetTree().GetNetworkUniqueId());
	}
	
	private void InstancePlayer(int id)
	{
		//todo: add names
		_network.AddPlayer(id, "");
		Node2D playerInstance =
			Global.InstanceNodeAtLocation(_player, _players, new Vector2((float) GD.RandRange(0, 500), (float) GD.RandRange(0, 500)));
		playerInstance.Name = id.ToString();
		playerInstance.SetNetworkMaster(id);
		playerInstance.PauseMode = PauseModeEnum.Stop;
		GetTree().Paused = true;
		GD.Print(_network._players);
	}

	private void PreStartGame()
	{
		_playerPanel.Show();
		RefreshLobby();
	}

	[Sync]
	private void RefreshLobby()
	{
		foreach (KeyValuePair<int, string> player in _network._players)
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
	
	public void OnStartPressed()
	{
		Rpc(nameof(StartGame));
	}
}
