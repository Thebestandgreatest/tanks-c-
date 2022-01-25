using System;
using Godot;

public class Player : KinematicBody2D
{
    // load values from file or server for custom games
    private const int TankSpeed = 100;
    private const double TurretRotateSpeed = 1;
    private const double BodyRotateSpeed = 1;

    private Vector2 _velocity;

    private Sprite _tankTurret;
    private Sprite _tankBody;
    private Label _rotationLabel;
    private readonly PackedScene _bullet = GD.Load<PackedScene>("res://Scenes/Bullets/Basic.tscn");

    public override void _Ready()
    {
        _tankTurret = (Sprite) GetNode("tankTurret");
        _tankBody = (Sprite) GetNode("CollisionShape2D/tankBody");
        _rotationLabel = (Label) GetNode("Label");
    }

    public override void _PhysicsProcess(float delta)
    {
        // bug if tank moves turret rotation breaks
        GetInput();
        //Animate();
        _velocity = MoveAndSlide(_velocity);

        //turret angle code
        double mouseAngle = Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetLocalMousePosition()))) - 90;
        double turretAngle = (float) Math.Round(_tankTurret.RotationDegrees);
        double angleDifference = AngleDifference(mouseAngle, turretAngle);
        if (angleDifference > 1)
        {
            turretAngle -= TurretRotateSpeed;
        }
        else if (angleDifference < 1)
        {
            turretAngle += TurretRotateSpeed;
        }

        //_rotationLabel.Text = "X Velocity: " + _velocity.x + " Y Velocity:" + _velocity.y + " Tankbody Rotation: " + _tankBody.RotationDegrees + " Vector Rotation: " +Mathf.Rad2Deg(_velocity.Angle());
        _tankTurret.RotationDegrees = (float) Math.Round(turretAngle);
    }

    private void GetInput()
    {
        //todo shooting
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;
    }

    private void Animate()
    {
        double angleDifference = AngleDifference(Mathf.Rad2Deg(_velocity.Angle()) + 90, _tankBody.RotationDegrees);
        double bodyAngle = _tankBody.RotationDegrees;
        
        if (angleDifference > 0)
        {
            bodyAngle += BodyRotateSpeed;
        }
        else if (angleDifference < 0)
        {
            bodyAngle -= BodyRotateSpeed;
        }

        _tankBody.RotationDegrees = (float) bodyAngle;
    }

private static double AngleDifference(double testAngle, double currentAngle)
    {
        // thank you random person on stack overflow!
        // Takes two angles in degrees and compares the distance between the angles, works for all angles including those outside of [-360,360]
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }
}