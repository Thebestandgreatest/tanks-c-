using Godot;
using System;

public class Basic : KinematicBody2D
{
    private const int Speed = 200;

    private Vector2 _velocity;
    
    public override void _PhysicsProcess(float delta)
    {
        _velocity = new Vector2(Speed, 0).Rotated(Rotation - Mathf.Pi/2);
        _velocity = MoveAndSlide(_velocity);
    }
}
