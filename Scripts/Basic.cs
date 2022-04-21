using Godot;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedType.Global
public class Basic : KinematicBody2D
{
    private Vector2 _velocity = new Vector2(1, 0);

    private const int Speed = 400;

    public override void _PhysicsProcess(float delta)
    {
        MoveAndSlide(_velocity.Rotated(Rotation - Mathf.Pi / 2) * Speed);
    }

}