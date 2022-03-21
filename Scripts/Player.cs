using System;
using System.Net.NetworkInformation;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Player : KinematicBody2D
{
    [Signal]
    private delegate void Shoot(PackedScene bullet, float direction, Vector2 location);
    
    // load values from file or server for custom games
    private const int TankSpeed = 100;
    private const double TurretRotateSpeed = 1;
    private const double BodyRotateSpeed = 2;
    
    private readonly PackedScene _bulletScene = GD.Load<PackedScene>("res://Scenes/Bullets/Basic.tscn");
    private CollisionPolygon2D _tankBody;
    private Sprite _tankTurret;
    private Timer _timer;
    private Tween _tween;
    
    
    private double _turretAngle;
    private Vector2 _turretOffset;
    private Vector2 _velocity;
    private double _bodyAngle;
    private bool _canFire = true;

    [Puppet] private Vector2 _puppetPosition = new Vector2();
    [Puppet] private Vector2 _puppetVelocity = new Vector2();
    [Puppet] private float _puppetBodyRotation = 0;
    [Puppet] private float _puppetTurretRotation = 0;
  
    public override void _Ready()
    {
        _tankTurret = GetNode<Sprite>("CollisionShape2D/tankBody/tankTurret");
        _tankBody = GetNode<CollisionPolygon2D>("CollisionShape2D");
        _turretOffset = new Vector2(0, -80);
        _timer = GetNode<Timer>("ShootTimer");
        _tween = GetNode<Tween>("Tween");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            GetInput();
            Animate();
            
            _velocity = MoveAndSlide(_velocity);

            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
            RsetUnreliable(nameof(_puppetBodyRotation), _tankBody.GlobalRotationDegrees);
            RsetUnreliable(nameof(_puppetTurretRotation), _tankTurret.GlobalRotationDegrees);
            RsetUnreliable(nameof(_puppetVelocity), _velocity);
        }
        else
        {
            _tween.InterpolateProperty(this, "global_position", GlobalPosition, _puppetPosition, (float) 0.1);
            RotationDegrees = Mathf.Lerp(_tankBody.GlobalRotationDegrees, _puppetBodyRotation, delta * 8);
            _tankTurret.RotationDegrees = Mathf.Lerp((float) _tankTurret.GlobalRotationDegrees, _puppetTurretRotation, delta * 8);

            if (!_tween.IsActive())
            {
                MoveAndSlide(_puppetVelocity);
            }
        }
    }

    private void GetInput()
    {
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;
        

         if (Input.IsActionPressed("fire") && _canFire)
         {
             EmitSignal(nameof(Shoot), _bulletScene, _tankTurret.RotationDegrees, _tankBody.RotationDegrees,
                 _tankTurret.GlobalPosition);
         }
    }

    private void Animate()
    {
        //body angle code
        double velocityAngle = Mathf.Round(Mathf.Rad2Deg(_velocity.Angle()) + 90);
        _bodyAngle = Mathf.Round(_tankBody.GlobalRotationDegrees);
        double bodyAngleDifference = AngleDifference(_bodyAngle, velocityAngle);

        if (_velocity == Vector2.Zero) return;
        if (bodyAngleDifference > 1)
            _bodyAngle += BodyRotateSpeed;
        else if (bodyAngleDifference < -1) _bodyAngle -= BodyRotateSpeed;
        Mathf.LerpAngle(_tankTurret.GlobalRotationDegrees, (float) _bodyAngle, (float) 0.1);
        //_tankBody.GlobalRotationDegrees = (float) _bodyAngle;

        //turret angle code
        double mouseAngle =
            Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetGlobalMousePosition()))) - 90;
        _turretAngle = (float) Math.Round(_tankTurret.GlobalRotationDegrees);
        double turretAngleDifference = AngleDifference(mouseAngle, _turretAngle);
        if (turretAngleDifference > 1)
            _turretAngle -= TurretRotateSpeed;
        else if (turretAngleDifference < 1) _turretAngle += TurretRotateSpeed;

        Mathf.LerpAngle(_tankTurret.GlobalRotationDegrees, (float) Math.Round(_turretAngle), (float) 0.1);
        //_tankTurret.GlobalRotationDegrees = (float) Math.Round(_turretAngle);
    }

    private static double AngleDifference(double testAngle, double currentAngle)
    {
        // Takes two angles in degrees and compares the distance between the angles, works for all angles including those outside of [-360,360]
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    private async void PlayerShoot(PackedScene bullet, float rotation, float bodyRotation, Vector2 location)
    {
        KinematicBody2D bulletInstance = bullet.Instance<KinematicBody2D>();
        rotation += bodyRotation;
        bulletInstance.RotationDegrees = rotation;
        bulletInstance.Position = location + _turretOffset.Rotated(Mathf.Deg2Rad(rotation));
        GetParent().AddChild(bulletInstance);

        _canFire = false;
        _timer.Start((float) 0.5);
        await ToSignal(_timer, "timout");
        _canFire = true;
    }
}