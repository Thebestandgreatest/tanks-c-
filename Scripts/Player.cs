using System;
using Godot;

public class Player : KinematicBody2D
{
    [Signal]
    delegate void Shoot(PackedScene bullet, float direction, Vector2 location);
    
    // load values from file or server for custom games
    private const int TankSpeed = 100;
    private const double TurretRotateSpeed = 1;
    private const double BodyRotateSpeed = 2;

    private Vector2 _turretOffset;
    private Vector2 _velocity;

    private Sprite _tankTurret;
    private Sprite _tankBody;
    private CollisionPolygon2D _tankBodyCollision;
    private Label _rotationLabel;
    private PackedScene _bulletScene = (PackedScene) GD.Load("res://Scenes/Bullets/Basic.tscn");
    public override void _Ready()
    {
        _tankTurret = (Sprite) GetNode("CollisionShape2D/tankBody/tankTurret");
        _tankBody = (Sprite) GetNode("CollisionShape2D/tankBody");
        _tankBodyCollision = (CollisionPolygon2D) GetNode("CollisionShape2D");
        _rotationLabel = (Label) GetNode("Label");
        _turretOffset = new Vector2(0, -80);
    }

    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        Animate();
        _velocity = MoveAndSlide(_velocity);

        //turret angle code
        double mouseAngle = Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetGlobalMousePosition()))) - 90;
        double turretAngle = (float) Math.Round(_tankTurret.GlobalRotationDegrees);
        double angleDifference = AngleDifference(mouseAngle, turretAngle);
        if (angleDifference > 1)
        {
            turretAngle -= TurretRotateSpeed;
        }
        else if (angleDifference < 1)
        {
            turretAngle += TurretRotateSpeed;
        }

        //_rotationLabel.Text = "Target Angle: " + mouseAngle + ", Current Angle: " + turretAngle;
        _tankTurret.GlobalRotationDegrees = (float) Math.Round(turretAngle);
    }

    private void GetInput()
    {
        if (Input.IsActionPressed("fire"))
        {
            EmitSignal(nameof(Shoot), _bulletScene, Rotation, Position);
        }
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;
    }

    private void Animate()
    {
        double angleDifference = AngleDifference(Mathf.Rad2Deg(_velocity.Angle()) - 90, _tankBody.RotationDegrees);
        double bodyAngle = _tankBody.RotationDegrees;

        if (_velocity != Vector2.Zero)
        {
            if (angleDifference > 0)
            {
                bodyAngle += BodyRotateSpeed;
            }
            else if (angleDifference < 0)
            {
                bodyAngle -= BodyRotateSpeed;
            }
            _tankBodyCollision.RotationDegrees = (float) bodyAngle;
        }
    }

    private static double AngleDifference(double testAngle, double currentAngle)
    {
        // thank you random person on stack overflow!
        // Takes two angles in degrees and compares the distance between the angles, works for all angles including those outside of [-360,360]
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    private void _on_player_Shoot(PackedScene bullet, float direction, Vector2 location)
    {
        KinematicBody2D bulletInstance = (KinematicBody2D) _bulletScene.Instance();
        bulletInstance.RotationDegrees = _tankTurret.RotationDegrees;
        bulletInstance.Position = Position + _turretOffset.Rotated(_tankTurret.Rotation);
        GetParent().AddChild(bulletInstance);
    }
}