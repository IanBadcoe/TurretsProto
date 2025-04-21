using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using Godot;
using SubD;

public partial class Turret : Node3D
{
    public static List<Turret> Turrets
    {
        get;
        private set;
    } = [];

    public Node3D Base
    {
        get;
        private set;
    }
    public Node3D Body
    {
        get;
        private set;
    }
    public Node3D Weapon
    {
        get;
        private set;
    }

    RandomNumberGenerator RNG = new();

    WeakReference<Turret> Target
    {
        get;
        set;
    }

    enum State
    {
        ChooseTarget,
        Tracking,
        ReadyToFire,
        Firing,
        Cooldown
    }

    State CurrentState
    {
        get;
        set;
    }

    public override void _Ready()
    {
        Base = GetNode<Node3D>("Base");
        Body = Base.GetNode<Node3D>("Body");
        Weapon = Body.GetNode<Node3D>("Weapon");

        Turrets.Add(this);

        ChainTween();
    }

    public override void _ExitTree()
    {
        Turrets.Remove(this);
    }

    public override void _Process(double delta)
    {
        switch(CurrentState)
        {
            case State.ChooseTarget:
                SetRandomTarget();
                break;

            case State.Tracking:
                break;
            case State.ReadyToFire:
                if (Target == null)
                {
                    SetState(State.ChooseTarget);
                }
                else
                {
                    Fire();
                }

                break;
            case State.Firing:
                break;
            case State.Cooldown:
                break;
        }


    }

    private void Fire()
    {
        throw new NotImplementedException();
    }


    private void SetState(State chooseTarget)
    {
        throw new NotImplementedException();
    }


    private void SetRandomTarget()
    {
        Target = RNG.RandChoice(Turrets.Where(x => x != this).ToList()).AsWeak();
    }

    private void ChainTween()
    {

        Tween tween = CreateTween();
        tween.TweenProperty(Body, "rotation_degrees", new Vector3(0, RNG.Randf() * 360, 0), 2.0f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Weapon, "rotation_degrees", new Vector3(0, 0, RNG.Randf() * -70 + 10), 2.0f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenInterval(RNG.Randf() * 3);
        tween.TweenCallback(new Callable(this, "ChainTween"));
    }

}
