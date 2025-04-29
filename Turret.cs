using System;
using System.Linq;
using System.Threading.Tasks;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;

using Godot;

using Godot_Util;

[Meta(typeof(IAutoConnect), typeof(IDependent))]
public partial class Turret : Node3D, Actor
{
    // --------------------------------------------------------------
    // Constants

    const float TrackSpeedRadsPerSecond = 2;

    // --------------------------------------------------------------
    // IAutoNode boilerplate
    public override void _Notification(int what) => this.Notify(what);
    // --------------------------------------------------------------

    // --------------------------------------------------------------
    // Dependencies
    [Dependency]
    FactionManager FM => this.DependOn<FactionManager>();

    [Dependency]
    RSG.PromiseTimer PT => this.DependOn<RSG.PromiseTimer>();

    [Dependency]
    GameControl GC => this.DependOn<GameControl>();
    // --------------------------------------------------------------

    // --------------------------------------------------------------
    // Child nodes
    [Node]
    public Node3D Base
    {
        get;
        private set;
    }

    [Node]
    public Node3D Body
    {
        get;
        private set;
    }

    [Node]
    public Node3D Weapon
    {
        get;
        private set;
    }

    public PackedScene MunitionScene
    {
        get;
        private set;
    }

    // --------------------------------------------------------------

    public FactionManager.FactionId FactionId { get; private set; } = FactionManager.FactionId.Enemy;

    FactionManager.FactionData MyFaction => FM.Faction(FactionId);

    // temp
    FactionManager.FactionData EnemyFaction => FM.Faction(FactionManager.FactionId.Player);

    RandomNumberGenerator RNG = new();

    WeakReference<Turret> Target
    {
        get;
        set;
    }

    readonly TurretLB SM = new();

    readonly TurretLB.IBinding SM_Binding;

    bool Started { get; set; } = false;

    // Construction
    public Turret()
    {
        SM_Binding = SM.Bind();

        SM_Binding.Handle((in TurretLB.Output.Cooldown _) => Cooldown());
        SM_Binding.Handle((in TurretLB.Output.FindTarget _) => FindTarget());
        SM_Binding.Handle((in TurretLB.Output.FireOn output) => Fire(output.Actor));
        SM_Binding.Handle((in TurretLB.Output.TrackTo output) => TrackTo(output.Actor));

        MunitionScene = GD.Load<PackedScene>("res://Bullet.tscn");
    }

    public void OnResolved()
    {
        MyFaction.Add(this);
        SM.Start();

        EnemyFaction.MemberAdded += EnemyAdded;
    }

    private void EnemyAdded(Actor _)
    {
        SM.Input(new TurretLB.Input.EnemyAdded());
    }

    public override void _ExitTree()
    {
        // may want these earier, on some event when we know they are going to be removed
        EnemyFaction.MemberRemoved -= EnemyAdded;
        MyFaction.Remove(this);
    }

    public override void _Process(double delta)
    {
        if (!Started && GC.Start)
        {
            Started = true;
            SM.Input(new TurretLB.Input.Start());
        }
    }

    // StateMachine operations

    private void Cooldown()
    {
        CoolDownInner()
            .Then(() => SM.Input(new TurretLB.Input.CooldownComplete()));
    }

    private RSG.IPromise CoolDownInner()
    {
        return PT.WaitFor(0.5f);;
    }

    private void Fire(Actor target)
    {
        if (!IsInstanceValid(target.AsNode3D()))
        {
            SM.Input(new TurretLB.Input.TargetLost());
        }

        // direct fire does not need the target, but parabolic fire
        FireN(3)
            .Then((i) => {
                SM.Input(new TurretLB.Input.FireComplete());
            });
    }

    private RSG.IPromise<int> FireN(int n)
    {
        RSG.IPromise<int> promise = null;

        for(int i = 0; i < n; i++)
        {
            if (promise == null)
            {
                promise = FireOne(1);
            }
            else
            {
                promise = promise.Then((i) => FireOne(i * 2));
            }
        }

        return promise;
    }

    private RSG.IPromise<int> FireOne(int i)
    {
        RSG.Promise<int> promise = new();

        PT.WaitFor(0.1f)
            .Then(() => {
                CallDeferred("FireInner");

                promise.Resolve(i + 1);
            });

        return promise;
    }

    private void FireInner()
    {
        Transform3D trans = Weapon.GlobalTransform;

        Bullet bullet = MunitionScene.Instantiate<Bullet>();
        GetTree().Root.AddChild(bullet);

        // the barrel is vertical when unrotated, so its length
        // is along Z
        bullet.GlobalTransform = trans.Translated(trans.Basis.Y * 1.3f);
    }

    private void FindTarget()
    {
        // there must me at least us, an one other...
        if (EnemyFaction.Count == 0)
        {
            SM.Input(new TurretLB.Input.NoTargetFound());
        }
        else
        {
            SM.Input(new TurretLB.Input.FoundTarget(RandomTarget()));
        }
    }

    private Actor RandomTarget()
    {
        return RNG.RandChoice(EnemyFaction.ToList());
    }

    private void TrackTo(Actor target)
    {
        if (!IsInstanceValid(target.AsNode3D()))
        {
            SM.Input(new TurretLB.Input.TargetLost());
        }

        Vector3 d = target.AsNode3D().Position - Position;

        float heading = Mathf.Atan2(-d.Z, d.X);

        Vector3 h_d = new Vector3(d.X, 0, d.Z);

        float elevation = Mathf.Atan2(h_d.Length(), d.Y);

        TrackTween(heading, elevation);
    }

    // internal

    private void TrackTween(float heading, float elevation)
    {
        Quaternion body_quat = new Quaternion(Vector3.Up, heading);
        Quaternion weapon_quat = new Quaternion(Vector3.Forward, elevation);

        float body_angle_change = body_quat.AngleTo(Body.Quaternion);
        float weapon_angle_change = weapon_quat.AngleTo(Weapon.Quaternion);

        float body_duration = body_angle_change / TrackSpeedRadsPerSecond;
        float weapon_duration = weapon_angle_change / TrackSpeedRadsPerSecond;

        Tween tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(Body, "quaternion", body_quat, body_duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Weapon, "quaternion", weapon_quat, weapon_duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);

        tween.Chain().TweenCallback(Callable.From(() =>
            {
                SM.Input(new TurretLB.Input.TrackComplete());
            }
        ));
    }

    public Node3D AsNode3D()
    {
        return this;
    }

    public void TakeDamage(int damage)
    {
        throw new NotImplementedException();
    }
}
