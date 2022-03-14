using System;
using Godot;


// ReSharper disable once CheckNamespace
// ReSharper disable once ClassNeverInstantiated.Global
public class Player : KinematicBody2D
{
    // load values from file or server for custom games
    private const int TankSpeed = 100;
    private const double TurretRotateSpeed = 1;
    private const double BodyRotateSpeed = 2;
    private const int TankReloadMultiplier = 1;

    private readonly PackedScene _bulletScene = GD.Load<PackedScene>("res://Scenes/Bullets/Basic.tscn");
    private double _bodyAngle;
    private bool _canFire = true;
    private CollisionPolygon2D _tankBodyCollision;

    private Sprite _tankTurret;
    private Timer _timer;
    private double _turretAngle;

    private Vector2 _turretOffset;
    private Vector2 _velocity;

    [Puppet] private Vector2 _puppetPosition;
    [Puppet] private float _puppetBodyRotation;
    [Puppet] private float _puppetTurretRotation;
    [Puppet] private Vector2 _puppetVelocity;
    public override void _Ready()
    {
        _tankTurret = GetNode<Sprite>("CollisionShape2D/tankBody/tankTurret");
        _tankBodyCollision = GetNode<CollisionPolygon2D>("CollisionShape2D");
        _turretOffset = new Vector2(0, -80);
        _timer = GetNode<Timer>("Timer");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            GetInput();
        }
        else
        {
            Position = _puppetPosition;
            _velocity = _puppetVelocity;
            _tankTurret.RotationDegrees = _puppetTurretRotation;
            _tankBodyCollision.RotationDegrees = _puppetBodyRotation;
        }
        
        _velocity = MoveAndSlide(_velocity);

        if (!IsNetworkMaster())
        {
            _puppetPosition = Position;
            _puppetVelocity = _velocity;
            _puppetTurretRotation = _tankTurret.RotationDegrees;
            _puppetBodyRotation = _tankBodyCollision.RotationDegrees;
        }
    }

    private void GetInput()
    {
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;

        Rset(nameof(_puppetPosition), Position);
        Rset(nameof(_puppetVelocity), _velocity);

        // if (Input.IsActionPressed("fire") && _canFire)
        // {
        //     string bulletName = Name + BombIndex;
        //     Vector2 bulletPosition = Position;
        //     Rpc(nameof(FireBullet),_bulletScene, bulletName, bulletPosition, GetTree().GetNetworkUniqueId());
        // }


    }

    private void Animate()
    {
        //body angle code
        double velocityAngle = Mathf.Round(Mathf.Rad2Deg(_velocity.Angle()) + 90);
        _bodyAngle = Mathf.Round(_tankBodyCollision.GlobalRotationDegrees);
        double bodyAngleDifference = AngleDifference(_bodyAngle, velocityAngle);

        if (_velocity == Vector2.Zero) return;
        if (bodyAngleDifference > 1)
            _bodyAngle += BodyRotateSpeed;
        else if (bodyAngleDifference < -1) _bodyAngle -= BodyRotateSpeed;
        _tankBodyCollision.GlobalRotationDegrees = (float) _bodyAngle;

        //turret angle code
        double mouseAngle =
            Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetGlobalMousePosition()))) - 90;
        _turretAngle = (float) Math.Round(_tankTurret.GlobalRotationDegrees);
        double turretAngleDifference = AngleDifference(mouseAngle, _turretAngle);
        if (turretAngleDifference > 1)
            _turretAngle -= TurretRotateSpeed;
        else if (turretAngleDifference < 1) _turretAngle += TurretRotateSpeed;

        //_rotationLabel.Text = "Target Angle: " + mouseAngle + ", Current Angle: " + turretAngle;
        _tankTurret.GlobalRotationDegrees = (float) Math.Round(_turretAngle);
    }

    private static double AngleDifference(double testAngle, double currentAngle)
    {
        // Takes two angles in degrees and compares the distance between the angles, works for all angles including those outside of [-360,360]
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    [RemoteSync]
    private async void FireBullet(PackedScene bullet, string bulletName, Vector2 position, float rotation, float bodyRotation)
    {
        KinematicBody2D bulletInstance = bullet.Instance<KinematicBody2D>();
        bulletInstance.Name = bulletName;
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