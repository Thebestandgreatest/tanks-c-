using Godot;
using System;

public class Basic : KinematicBody2D
{
    private Vector2 _velocity = new Vector2(200,0);

    public override void _PhysicsProcess(float delta)
    {
        Position += _velocity * delta;
    }
}
