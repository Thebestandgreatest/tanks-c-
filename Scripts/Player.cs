using System;
using Godot;

public class Player : KinematicBody2D
{
    // load values from file or server for custom games
    private const int TankSpeed = 100;
    private const double AimSpeed = 1;

    private double _turretAngle;
    private double _mouseAngle;

    private Vector2 _velocity;
    
    private Sprite _tankTurret;
    private Sprite _tankBody;
    private Label _rotationLabel;
    private readonly PackedScene _bullet = GD.Load<PackedScene>("res://Scenes/Bullets/Basic.tscn");

    public override void _Ready()
    {
        _tankTurret = (Sprite) GetNode("tankTurret");
        _tankBody = (Sprite) GetNode("tankBody");
        _rotationLabel = (Label) GetNode("Label");
    }

    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        _velocity = MoveAndSlide(_velocity);
        
        //turret angle code
        _mouseAngle = Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetLocalMousePosition()))) - 90;
        _turretAngle = (float)Math.Round(_tankTurret.RotationDegrees);
        double targetAngle = AngleDifference(_mouseAngle, _turretAngle);
        if (targetAngle > 1)
        {
            _turretAngle -= AimSpeed;
        }
        else if (targetAngle < 1)
        {
            _turretAngle += AimSpeed;
        }

        _rotationLabel.Text = "Target Angle: " + _mouseAngle + " Current Angle: " + _tankTurret.RotationDegrees;
        _tankTurret.RotationDegrees = (float)Math.Round(_turretAngle);
    }

    private void GetInput()
    {
        // todo finalize tank movement from options
        
        // option 1
        //      tank moves in the direction indicated by the arrow keys, movement is not controlled by the player
        //      pros: easy to maneuver, simple to program
        //      cons: 
        // option 2
        //      tank rotates to the indicated direction and then moves, movement is partially controlled by the player
        //      pros: 
        //      cons: delayed movement in wanted direction
        
        //todo shooting
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;
    }

    private static double AngleDifference(double testAngle, double currentAngle)
    {
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }
}