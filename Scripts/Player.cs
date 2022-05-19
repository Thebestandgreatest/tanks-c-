using System;
using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class Player : KinematicBody2D
{
    private const int TankSpeed = 200;
    private const double TurretRotateSpeed = 2;
    private const double BodyRotateSpeed = 2;
    
    private readonly PackedScene _bulletScene = GD.Load<PackedScene>("res://Scenes/Bullet.tscn");
    
    private CollisionPolygon2D _tankBody;
    private Sprite _tankTurret;
    private Timer _timer;
    private Camera2D _camera;
    private AnimatedSprite _animatedSprite;
    private Area2D _bulletCollider;

    private double _turretAngle;
    private Vector2 _turretOffset = new Vector2(0,-80);
    private Vector2 _velocity;
    private double _bodyAngle;
    private bool _canFire = true;
    private bool _gameStarted;
    private int _playerHealth = 3;
    private bool _alive = true;
    
    private Networking _network;
    
    [Puppet] private Vector2 _puppetPosition = new Vector2();
    [Puppet] private Vector2 _puppetVelocity = new Vector2();
    [Puppet] private float _puppetBodyRotation = 0;
    [Puppet] private float _puppetTurretRotation = 0;

    public override void _Ready()
    {
        _network = GetNode<Networking>("/root/Network");

        _tankTurret = GetNode<Sprite>("CollisionShape2D/TankBody/TankTurret");
        _tankBody = GetNode<CollisionPolygon2D>("CollisionShape2D");
        _timer = GetNode<Timer>("Timer");
        _camera = GetNode<Camera2D>("Camera2D");
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _bulletCollider = GetNode<Area2D>("Area2D");

        _bulletCollider.Connect("area_entered", this, nameof(BulletCollision));
        
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!_alive) return;
        if (IsNetworkMaster() && _gameStarted == false)
        {
            _camera.Current = true;
            _gameStarted = true;
        }
        
        if (IsNetworkMaster())
        {
            GetInput();

            _velocity = MoveAndSlide(_velocity);

            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
            RsetUnreliable(nameof(_puppetBodyRotation), _tankBody.GlobalRotationDegrees);
            RsetUnreliable(nameof(_puppetTurretRotation), _tankTurret.GlobalRotationDegrees);
            RsetUnreliable(nameof(_puppetVelocity), _velocity);
        }
        else
        {
            //GlobalPosition.LinearInterpolate(_puppetPosition, delta);
            //_tankBody.GlobalRotationDegrees = Mathf.LerpAngle(_tankBody.GlobalRotationDegrees, _puppetBodyRotation, delta);
            //_tankTurret.GlobalRotationDegrees = Mathf.LerpAngle(_tankTurret.GlobalRotationDegrees, _puppetTurretRotation, delta);

            GlobalPosition = _puppetPosition;
            _tankBody.GlobalRotationDegrees = _puppetBodyRotation;
            _tankTurret.GlobalRotationDegrees = _puppetTurretRotation;
            _velocity = _puppetVelocity;
        }
        Animate();
    }

    private void GetInput()
    {
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;

        if (!Input.IsActionPressed("fire") || !_canFire) return;
        float bulletRotation = _tankTurret.RotationDegrees + _tankBody.RotationDegrees + 180;
        Vector2 bulletPosition = _tankTurret.GlobalPosition + _turretOffset.Rotated(Mathf.Deg2Rad(bulletRotation));
        Rpc(nameof(PlayerShoot), bulletPosition, bulletRotation, GetTree().GetNetworkUniqueId());
    }

    private void Animate()
    {
        //turret angle code
        double mouseAngle =
            Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetGlobalMousePosition()))) + 90;
        _turretAngle = (float) Math.Round(_tankTurret.GlobalRotationDegrees);
        double turretAngleDifference = AngleDifference(mouseAngle, _turretAngle);
        if (turretAngleDifference >  TurretRotateSpeed)
            _turretAngle -= TurretRotateSpeed;
        else if (turretAngleDifference < -TurretRotateSpeed)
        {
            _turretAngle += TurretRotateSpeed;
        }

        //Mathf.LerpAngle(_tankTurret.GlobalRotationDegrees, (float) Math.Round(_turretAngle), (float) 0.1);
        _tankTurret.GlobalRotationDegrees = (float) Math.Round(_turretAngle);
        
        //body angle code
        double velocityAngle = Mathf.Round(Mathf.Rad2Deg(_velocity.Angle()) + 90);
        _bodyAngle = Mathf.Round(_tankBody.GlobalRotationDegrees);
        double bodyAngleDifference = AngleDifference(_bodyAngle, velocityAngle);

        if (_velocity == Vector2.Zero) return;
        if (bodyAngleDifference > 1)
            _bodyAngle += BodyRotateSpeed;
        else if (bodyAngleDifference < -1) _bodyAngle -= BodyRotateSpeed;
        //Mathf.LerpAngle(_tankTurret.GlobalRotationDegrees, (float) _bodyAngle, (float) 0.1);
        _tankBody.GlobalRotationDegrees = (float) _bodyAngle;
    }

    private void BulletCollision(Node area)
    {
        if (!IsNetworkMaster()) return;
        if (!area.IsInGroup("Bullet") || area.GetParent().GetTree().GetNetworkUniqueId() != Name.ToInt() ||
            _tankBody.Disabled) return;
        area.GetParent().Rpc("DeleteBullet");
        Rpc(nameof(BulletHit), GetTree().GetNetworkUniqueId());
    }

    private static double AngleDifference(double testAngle, double currentAngle)
    {
        double diff = (currentAngle - testAngle + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }
    
    [Sync]
    private async void PlayerShoot(Vector2 bulletPosition, float bulletRotation, int id)
    {
        Node2D bulletInstance = Global.InstanceNodeAtLocation(_bulletScene, GetTree().Root.GetNode("Players"), bulletPosition, bulletRotation);
        bulletInstance.Name = "Bullet " + _network.BulletIndex;
        bulletInstance.SetNetworkMaster(id);
        _network.BulletIndex++;

        _canFire = false;
        _timer.Start((float) 0.5);
        await ToSignal(_timer, "timeout");
        _canFire = true;
    }

    [Sync]
    internal void BulletHit(int playerId)
    {
        if (_playerHealth == 0 || _playerHealth < 0)
        {
            _tankBody.SetDeferred("disabled", true);
            _alive = false;
            _tankBody.Hide();
            _animatedSprite.Play("explode");
            _network.EmitSignal("PlayerDiedSignal", playerId);
        }
        else
        {
            _playerHealth--;
        }
    }
}