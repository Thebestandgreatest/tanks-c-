using Godot;

public class player : KinematicBody2D
{
    // Declare member variables here. Examples:

    // private int a = 2;
    // private string b = "text";
    
    private const int TankSpeed = 100;
    private const float AimSpeed = 0.03F;
    private const float TankRotationSpeed = 0.05F;

    private Vector2 _velocity;
    private Sprite _tankTurret;
    private Sprite _tankBody;

    private float _steeringAngle;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _tankTurret = (Sprite) GetNode("tankTurret");
        _tankBody = (Sprite) GetNode("tankBody");
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        GetInput();
        _velocity = MoveAndSlide(_velocity);
    }

    private void GetInput()
    {
        // bug right side of tank breaks the turret movement

        _steeringAngle = _tankTurret.Position.AngleToPoint(GetLocalMousePosition()) - Mathf.Pi / 2;
        if (_steeringAngle - _tankTurret.Rotation < 0.1)
        {
            _tankTurret.Rotation -= AimSpeed;
        }
        else if (_steeringAngle - _tankTurret.Rotation > 0.1)
        {
            _tankTurret.Rotation += AimSpeed;
        }

        // todo make tank base rotation follow movement having the tank snap to whatever axis is closest

        if (Input.IsActionPressed("fire"))
        {
            // todo add shooting
        }
        
        _velocity = new Vector2();
        if (Input.IsActionPressed("ui_right"))
        {
            _velocity.x += 1;
        }
        else if (Input.IsActionPressed("ui_left"))
        {
            _velocity.x -= 1;
        }

        if (Input.IsActionPressed("ui_up"))
        {
            _velocity.y -= 1;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            _velocity.y += 1;
        }

        _velocity = _velocity.Normalized() * TankSpeed;
    }
}