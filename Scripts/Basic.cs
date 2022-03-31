using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private Vector2 _velocity = new Vector2(1, 0);

    private float playerRotation;

    private const int speed = 200;
    private const int damage = 10;

    [Puppet] private Vector2 _puppetPosition;
    [Puppet] private Vector2 _puppetVelocity;
    [Puppet] private float _puppetRotation;

    private int playerOwner = 0;

    public override void _Ready()
    {
        if (GetTree().HasNetworkPeer())
        {
            if (IsNetworkMaster())
            {
                _velocity = _velocity.Rotated(playerRotation);
                Rotation = playerRotation;
                Rset(nameof(_puppetVelocity), _velocity);
                Rset(nameof(_puppetRotation), Rotation);
                Rset(nameof(_puppetPosition), GlobalPosition);
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (GetTree().HasNetworkPeer())
        {
            if (IsNetworkMaster())
            {
                MoveAndSlide(_velocity.Rotated(Rotation - Mathf.Pi/2) * speed);
            }
            else
            {
                Rotation = _puppetRotation;
                MoveAndSlide(_puppetVelocity.Rotated(Rotation - Mathf.Pi/2) * speed);
            }
        }
    }
}