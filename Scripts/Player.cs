using System;
using System.Globalization;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Player : KinematicBody2D
{
    [Signal]
    // ReSharper disable once ArrangeTypeMemberModifiers
    delegate void Shoot(PackedScene bullet, float direction, Vector2 location);
    
    // load values from file or server for custom games
    private const int TankSpeed = 100;
    private const double TurretRotateSpeed = 1;
    private const double BodyRotateSpeed = 2;
    private const int TankReloadMultiplier = 1;

    private Vector2 _turretOffset;
    private Vector2 _velocity;
    private bool _canFire = true;
    
    private Sprite _tankTurret;
    private CollisionPolygon2D _tankBodyCollision;
    private Label _rotationLabel;
    private readonly PackedScene _bulletScene = (PackedScene) GD.Load("res://Scenes/Bullets/Basic.tscn");
    private Timer _timer;
    public override void _Ready()
    {
        _tankTurret = (Sprite) GetNode("CollisionShape2D/tankBody/tankTurret");
        _tankBodyCollision = (CollisionPolygon2D) GetNode("CollisionShape2D");
        _rotationLabel = (Label) GetNode("Label");
        _turretOffset = new Vector2(0, -80);
        _timer = (Timer) GetNode("Timer");
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
        if (Input.IsActionPressed("fire") && _canFire)
        {
            EmitSignal(nameof(Shoot), _bulletScene, _tankTurret.RotationDegrees, _tankBodyCollision.RotationDegrees, _tankTurret.GlobalPosition);
        }
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;
    }

    private void Animate()
    {
        double velocityAngle = Mathf.Round(Mathf.Rad2Deg(_velocity.Angle()) + 90);
        double bodyAngle = Mathf.Round(_tankBodyCollision.GlobalRotationDegrees);
        double angleDifference = AngleDifference(bodyAngle, velocityAngle);

        if (_velocity == Vector2.Zero) return;
        if (angleDifference > 1)
        {
            bodyAngle += BodyRotateSpeed;
        }
        else if (angleDifference < -1)
        {
            bodyAngle -= BodyRotateSpeed;
        }
        else
        {
            return;
        }

        //_rotationLabel.Text = "Current Angle:" + bodyAngle + " Velocity Angle:" + velocityAngle +
        //                      " Difference:" + angleDifference.ToString(CultureInfo.CurrentCulture);
        _tankBodyCollision.GlobalRotationDegrees = (float) bodyAngle;
    }

    private static double AngleDifference(double testAngle, double currentAngle)
    {
        // thank you random person on stack overflow!
        // Takes two angles in degrees and compares the distance between the angles, works for all angles including those outside of [-360,360]
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    // ReSharper disable once UnusedMember.Local
    private async void _on_player_Shoot(PackedScene bullet, float rotation, float bodyRotation, Vector2 position)
    {
        KinematicBody2D bulletInstance = (KinematicBody2D) bullet.Instance();
        rotation += bodyRotation;
        bulletInstance.RotationDegrees = rotation;
        bulletInstance.Position = position + _turretOffset.Rotated(Mathf.Deg2Rad(rotation));
        GetParent().AddChild(bulletInstance);
        
        _canFire = false;
        _timer.Start((float) 0.5 * TankReloadMultiplier);
        await ToSignal(_timer, "timeout");
        _canFire = true;
    }
}