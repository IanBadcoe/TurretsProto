using Godot;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

[Meta(typeof(IDependent))]
public partial class TestTarget : StaticBody3D, Actor
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

    public FactionManager.FactionId FactionId { get; private set; } = FactionManager.FactionId.Player;

    FactionManager.FactionData Faction => FM.Faction(FactionId);

    int Health { get; set; } = 3;

    public void OnResolved()
    {
        Faction.Add(this);
    }

    public Node3D AsNode3D()
    {
        return this;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Faction.Remove(this);

            QueueFree();
        }
    }
}
