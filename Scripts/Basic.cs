using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private const int Speed = 200;

    private float _playerRotation;
    
    private Vector2 _velocity;
    private Vector2 _initialPosition;
    private int _owner = 0;

    [Puppet] private Vector2 _puppetPosition = new Vector2();
    [Puppet] private Vector2 _puppetVelocity = new Vector2();
    [Puppet] private float _puppetRotation = 0;

    public override void _Ready()
    {
        _initialPosition = GlobalPosition;

        if (IsNetworkMaster())
        {
            _velocity = _velocity.Rotated(_playerRotation);
            Rotation = _playerRotation;
            Rset(nameof(_puppetVelocity), _velocity);
            Rset(nameof(_puppetRotation), Rotation);
            Rset(nameof(_puppetPosition), GlobalPosition);
        }
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            _velocity = MoveAndSlide(_velocity);
        }
        else
        {
            Rotation = _puppetRotation;
            GlobalPosition = _puppetPosition;
        }
        _velocity = new Vector2(Speed, 0).Rotated(Rotation - Mathf.Pi / 2);
        _velocity = MoveAndSlide(_velocity);
    }

    [Sync]
    public void Destroy()
    {
        QueueFree();
    }
}