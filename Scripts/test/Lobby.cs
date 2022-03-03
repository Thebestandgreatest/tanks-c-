using Godot;
using Godot.Collections;

// change if using 
public class lobby : Control
{
    private const int DefaultPort = 5672;
    private const int MaxPlayers = 32;

    private NetworkedMultiplayerPeer _peer = null;
    private string _playerName = "Tank1";
    private Dictionary<int, string> _players = new Dictionary<int, string>();
    private Dictionary<int, string> _readyPlayers = new Dictionary<int, string>();

    private void PlayerConnected(int id)
    {
        RpcId(id, nameof(RegisterPlayer), _playerName);
    }

    private void PlayerDisconnected(int id)
    {
        if (HasNode("/root/Level"))
        {
            if (GetTree().IsNetworkServer())
            {
                //todo: emit_signal("game_error", "Player ", + players[id] + " disconnected")
                EndGame();
            }
        }
        else
        {
            UnregisterPlayer(id);
        }
    }

    private void ConnectedOk()
    {
        //todo: emit_signal("game_error", "connection_succeeded")
    }

    private void ConnectedFail()
    {
        GetTree().NetworkPeer = null;
        //todo: emit_signal("connection_failed")
    }

    private void ServerDisconnected()
    {
        //todo: emit_signal("game_error", "server disconnected")
        EndGame;
    }
    
    // remote functions
    [Remote] private void RegisterPlayer(string playerName)
    {
        int id = GetTree().GetRpcSenderId();
        _players[id] = playerName;
        //todo: emit_signal("player_list_changed")
    }

    [Remote] private void UnregisterPlayer(int id)
    {
        _players.Remove(id);
        //todo: emit_signal("player_list_changed")
    }

    [Remote] private void PreStartGame(Dictionary spawnPoints)
    {
        Node level = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Level A.tscn").Instance();
        GetTree().Root.AddChild(level);
        
        //todo: get_tree().get_root().get_node("Lobby").hide()

        PackedScene playerScene = ResourceLoader.Load<PackedScene>("res://Player.tscn");
        
        
    }
}