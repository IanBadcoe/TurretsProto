using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IProvider))]
public partial class TestLevel : Node3D, IProvide<FactionManager>, IProvide<RSG.PromiseTimer>, IProvide<GameControl>
{
    // --------------------------------------------------------------
    // IAutoNode boilerplate
    public override void _Notification(int what) => this.Notify(what);
    // --------------------------------------------------------------

    Camera3D Camera;
    DirectionalLight3D Sun;

    FactionManager FM { get; set; } = new();

    RSG.PromiseTimer PT { get; set; } = new();

    [Export]
    public Vector3 CameraLookAt = Vector3.Zero;

    GameControl GC = new();

#region providers
    FactionManager IProvide<FactionManager>.Value() => FM;

    RSG.PromiseTimer IProvide<RSG.PromiseTimer>.Value() => PT;

    public GameControl Value() => GC;
#endregion

    public override void _Ready()
    {
        Camera = GetNode<Camera3D>("Camera");
        Camera.LookAt(CameraLookAt);

        Sun = GetNode<DirectionalLight3D>("Sun");
        Sun.LookAt(Vector3.Zero);

        this.Provide();
    }

    public override void _Process(double delta)
    {
        PT.Update((float)delta);

        GC.Process();
    }
}
