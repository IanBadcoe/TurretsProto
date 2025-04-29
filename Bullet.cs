using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

using Godot;

[Meta(typeof(IAutoConnect))]
public partial class Bullet : StaticBody3D
{
    // --------------------------------------------------------------
    // IAutoNode boilerplate
    public override void _Notification(int what) => this.Notify(what);
    // --------------------------------------------------------------

    const float MaxLifeSeconds = 5;
    const float Speed = 10;

    public Vector3 Direction
    {
        get => GlobalTransform.Basis.Y;
    }

    float Age
    {
        get;
        set;
    } = 0;

    public Bullet()
    {
    }

    public void OnResolved()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        // delta is in ms
        Age += (float)delta;

        if (Age > MaxLifeSeconds)
        {
            QueueFree();

            return;
        }

        var collide = MoveAndCollide(Direction * Speed * (float)delta);

        if (collide != null && collide.GetCollisionCount() > 0)
        {
            if (collide.GetCollider() is Actor collide_with)
            {
                collide_with.TakeDamage(1);
            }

            QueueFree();
        }
    }
}
