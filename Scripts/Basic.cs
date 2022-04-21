using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private Vector2 _velocity = new Vector2(1, 0);

    private const int Speed = 400;

    [Puppet] private Vector2 _puppetPosition = new Vector2();
    [Puppet] private Vector2 _puppetVelocity = new Vector2();
    
    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            _velocity = MoveAndSlide(_velocity.Normalized().Rotated(Rotation) * Speed);

            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
            RsetUnreliable(nameof(_puppetVelocity), _velocity);
        }
        else
        {
            GlobalPosition = _puppetPosition;
            _velocity = _puppetVelocity;
        }
    }

}