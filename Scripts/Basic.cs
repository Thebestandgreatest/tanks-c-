using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : Area2D
{
    private const int Speed = -500;

    [Puppet] private Vector2 _puppetPosition = new Vector2();

    public override void _Ready()
    {
        Connect("body_entered", this, nameof(DeleteBullet));
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            Position += Transform.y * Speed * delta;

            Rset(nameof(_puppetPosition), GlobalPosition);
        }
        else
        {
            GlobalPosition = _puppetPosition;
        }
    }

    [Sync]
    internal void DeleteBullet(string name)
    {
        QueueFree();
    }
}