using System;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Lobby : Panel
{
	private LineEdit _address;
	private Button _hostButton;
	private Button _joinButton;
	private LineEdit _name;

	private Panel _playerPanel;
	private Button _startButton;
	private Label _status;
	private ItemList _teamAList;
	private ItemList _teamBList;

	private Networking _network;

	public override void _Ready()
	{
		// autoloads
		_network = GetNode<Networking>("/root/Network");
		
		// join panel
		_address = GetNode<LineEdit>("Address");
		_hostButton = GetNode<Button>("Host");
		_joinButton = GetNode<Button>("Join");
		_status = GetNode<Label>("Status");
		_name = GetNode<LineEdit>("Name");

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
		//GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
	}

	private void PlayerConnected(int id)
	{
		GD.Print("Player " + id + " connected");
	}

	private void PlayerDisconnected(int id)
	{
		GD.Print("Player " + id + " disconnected");
	}

	public void OnHostPressed()
	{
		Hide();
		_network.CreateServer();
	}

	public void OnJoinPressed()
	{
		if (_address.Text != "")
		{
			Hide();
			_network._ipAddress = _address.Text;
			_network.JoinServer();
		}
		// todo: return error
	}
	
	public void OnStartPressed()
	{
		
	}
}
