using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private const int Speed = -400;
    private Vector2 _velocity;
    
    [Puppet] private Vector2 _puppetPosition = new Vector2();

    public override void _PhysicsProcess(float delta)
    {
        _velocity = new Vector2(1,0);
        if (IsNetworkMaster())
        {
            KinematicCollision2D collision = MoveAndCollide(_velocity.Normalized().Rotated(Rotation + Mathf.Pi / 2) * Speed * delta);
            //_velocity = MoveAndSlide(_velocity.Normalized().Rotated(Rotation + Mathf.Pi / 2) * Speed);
            if (collision != null)
            {
                Node2D collider = (Node2D) collision.Collider;
                if (collider.IsInGroup("Level"))
                {
                    Rpc(nameof(DeleteBullet), GetInstanceId());
                    QueueFree();
                } else if (collider.IsInGroup("Player"))
                {
                    RpcId(collider.Name.ToInt(), nameof(Player.BulletHit));
                    QueueFree();
                }
            }
            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
        }
        else
        {
            GlobalPosition = _puppetPosition;
        }
    }

    [Sync]
    private void DeleteBullet(int id)
    {
        
    }
}