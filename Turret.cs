using System;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoConnect), typeof(IDependent))]
public partial class Turret : Node3D
{
    // --------------------------------------------------------------
    // IAutoNode boilerplate
    public override void _Notification(int what) => this.Notify(what);
    // --------------------------------------------------------------

    // --------------------------------------------------------------
    // Dependencies
    [Dependency]
    FactionManager FM => this.DependOn<FactionManager>();
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
    // --------------------------------------------------------------

    public FactionManager.FactionId FactionId { get; private set; } = FactionManager.FactionId.Enemy;

    FactionManager.FactionData Faction => FM.Faction(FactionId);

    RandomNumberGenerator RNG = new();

    WeakReference<Turret> Target
    {
        get;
        set;
    }

    readonly TurretLB SM = new();

    readonly TurretLB.IBinding SM_Binding;

    // Construction
    public Turret()
    {
        SM_Binding = SM.Bind();

        SM_Binding.Handle((in TurretLB.Output.Cooldown _) => Cooldown());
        SM_Binding.Handle((in TurretLB.Output.FindTarget _) => FindTarget());
        SM_Binding.Handle((in TurretLB.Output.FireOn output) => FireOn(output.turret));
        SM_Binding.Handle((in TurretLB.Output.TrackTo output) => TrackTo(output.turret));
    }

    public void OnResolved()
    {
        Faction.Add(this);
        SM.Start();

        Faction.MemberAdded += OnMemberAdded;
    }

    private void OnMemberAdded(Turret _)
    {
        SM.Input(new TurretLB.Input.TurretAdded());
    }

    // Godot overrides


    public override void _ExitTree()
    {
        // may want these earier, on some event when we know they are going to be removed
        Faction.MemberRemoved -= OnMemberAdded;
        Faction.Remove(this);
    }

    // public override void _Process(double delta)
    // {
    // }

    // StateMachine operations

    private void Cooldown()
    {
        // throw new NotImplementedException();
    }

    private void FireOn(Turret target)
    {
        // throw new NotImplementedException();
    }

    private void FindTarget()
    {
        // there must me at least us, an one other...
        if (Faction.Count < 2)
        {
            SM.Input(new TurretLB.Input.NoTargetFound());
        }
        else
        {
            SM.Input(new TurretLB.Input.FoundTarget(RandomTarget()));
        }
    }

    private Turret RandomTarget()
    {
        return RNG.RandChoice(Faction.Where(x => x != this).ToList());
    }

    private void TrackTo(Turret target)
    {
        Vector3 d = target.Position - Position;

        float heading = Mathf.Atan2(-d.Z, d.X);

        Vector3 h_d = new Vector3(d.X, 0, d.Z);

        float elevation = -Mathf.Atan2(h_d.Length(), d.Y);

        TrackTween(heading, elevation);
    }

    // internal

    private void TrackTween(float heading, float elevation)
    {
        if (heading - Rotation.Y < -Mathf.Pi)
        {
            heading += 2 * Mathf.Pi;
        }

        if (heading - Rotation.Y > Mathf.Pi)
        {
            heading -= 2 * Mathf.Pi;
        }

        Tween tween = CreateTween();
        tween.TweenProperty(Body, "rotation", new Vector3(0, heading, 0), 2.0f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(Weapon, "rotation", new Vector3(0, 0, elevation), 2.0f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenInterval(RNG.Randf() * 3);
        tween.TweenCallback(Callable.From(() =>
            {
                SM.Input(new TurretLB.Input.TrackComplete());
            }
        ));
    }
}
