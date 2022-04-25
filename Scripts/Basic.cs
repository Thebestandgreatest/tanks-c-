using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private const int Speed = 400;
    
    private Vector2 _velocity = new Vector2(Speed, 0);
    
    [Puppet] private Vector2 _puppetPosition = new Vector2();
    [Puppet] private Vector2 _puppetVelocity = new Vector2();
    
    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            _velocity = MoveAndSlide(_velocity.Normalized() * Speed);

            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
        }
        else
        {
            GlobalPosition = _puppetPosition;
        }
    }

}