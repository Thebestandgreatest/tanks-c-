using System;
using Godot;
using Object = Godot.Object;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private const int Speed = -400;
    private Vector2 _velocity;
    
    [Puppet] private Vector2 _puppetPosition = new Vector2();

    public override void _PhysicsProcess(float delta)
    {
        _velocity = new Vector2(1,0);
        if (IsNetworkMaster())
        {
            KinematicCollision2D collision = MoveAndCollide(_velocity.Normalized().Rotated(Rotation + Mathf.Pi / 2) * Speed);
            if (collision != null)
            {
                var collider = collision.Collider;
                
            }
            
            RsetUnreliable(nameof(_puppetPosition), GlobalPosition);
            
            
        }
        else
        {
            GlobalPosition = _puppetPosition;
        }
    }

}