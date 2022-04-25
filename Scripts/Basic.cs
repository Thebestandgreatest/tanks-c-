using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private const int Speed = 400;
    
    
    [Puppet] private Vector2 _puppetPosition = new Vector2();

    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            Position += Transform.y * Speed * delta;

            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
        }
        else
        {
            GlobalPosition = _puppetPosition;
        }
    }

}