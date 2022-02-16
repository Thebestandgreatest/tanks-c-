using System;
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
    private double _bodyAngle;
    private double _turretAngle;
    
    private Sprite _tankTurret;
    private CollisionPolygon2D _tankBodyCollision;
    private Label _rotationLabel;
    private readonly PackedScene _bulletScene = (PackedScene) GD.Load("res://Scenes/Bullets/Basic.tscn");
    private Timer _timer;
    private KinematicBody2D _playerTwo;

    [Puppet] public Vector2 PuppetPosition;
    [Puppet] public Vector2 PuppetVelocity;
    [Puppet] public float PuppetTurretRotation;
    [Puppet] public float PuppetBodyRotation;
    
    public override void _Ready()
    {
        _tankTurret = GetNode<Sprite>("CollisionShape2D/tankBody/tankTurret");
        _tankBodyCollision = GetNode<CollisionPolygon2D>("CollisionShape2D");
        _rotationLabel = GetNode<Label>("Label");
        _turretOffset = new Vector2(0, -80);
        _timer = GetNode<Timer>("Timer");
        _playerTwo = GetNode<KinematicBody2D>("");

        if (GetTree().IsNetworkServer())
        {
            _playerTwo.SetNetworkMaster(GetTree().GetNetworkConnectedPeers()[0]);
        }
        else
        {
            _playerTwo.SetNetworkMaster(GetTree().GetNetworkUniqueId());
        }

        GD.Print("Unique id: ", GetTree().GetNetworkUniqueId());
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsNetworkMaster())
        {
            GetInput();
            Animate();
        }
        else
        {
            Position = PuppetPosition;
            _velocity = PuppetVelocity;
            _turretAngle = PuppetTurretRotation;
            _bodyAngle = PuppetBodyRotation;
        }
        _velocity = MoveAndSlide(_velocity);

        if (!IsNetworkMaster())
        {
            PuppetPosition = Position;
            PuppetVelocity = _velocity;
            PuppetTurretRotation = (float)_turretAngle;
            PuppetBodyRotation = (float)_bodyAngle;
        }
    }

    private void GetInput()
    {
        if (Input.IsActionPressed("fire") && _canFire)
        {
            EmitSignal(nameof(Shoot), _bulletScene, _tankTurret.RotationDegrees, _tankBodyCollision.RotationDegrees, _tankTurret.GlobalPosition);
        }
        _velocity = new Vector2();
        _velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * TankSpeed;
        Rset(nameof(PuppetPosition), Position);
        Rset(nameof(PuppetVelocity), _velocity);
    }

    private void Animate()
    {
        //body angle code
        double velocityAngle = Mathf.Round(Mathf.Rad2Deg(_velocity.Angle()) + 90);
        _bodyAngle = Mathf.Round(_tankBodyCollision.GlobalRotationDegrees);
        double bodyAngleDifference = AngleDifference(_bodyAngle, velocityAngle);

        if (_velocity == Vector2.Zero) return;
        if (bodyAngleDifference > 1)
        {
            _bodyAngle += BodyRotateSpeed;
        }
        else if (bodyAngleDifference < -1)
        {
            _bodyAngle -= BodyRotateSpeed;
        }
        _tankBodyCollision.GlobalRotationDegrees = (float) _bodyAngle;
        
        //turret angle code
        double mouseAngle = Math.Round(Mathf.Rad2Deg(_tankTurret.GlobalPosition.AngleToPoint(GetGlobalMousePosition()))) - 90;
        _turretAngle = (float) Math.Round(_tankTurret.GlobalRotationDegrees);
        double turretAngleDifference = AngleDifference(mouseAngle, _turretAngle);
        if (turretAngleDifference > 1)
        {
            _turretAngle -= TurretRotateSpeed;
        }
        else if (turretAngleDifference < 1)
        {
            _turretAngle += TurretRotateSpeed;
        }

        //_rotationLabel.Text = "Target Angle: " + mouseAngle + ", Current Angle: " + turretAngle;
        _tankTurret.GlobalRotationDegrees = (float) Math.Round(_turretAngle);
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