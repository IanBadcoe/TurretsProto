using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using System;

[Meta(typeof(IProvider))]
public partial class TestLevel : Node3D, IProvide<FactionManager>
{
    // --------------------------------------------------------------
    // IAutoNode boilerplate
    public override void _Notification(int what) => this.Notify(what);
    // --------------------------------------------------------------

    Camera3D Camera;
    DirectionalLight3D Sun;

    FactionManager FM { get; set; } = new();

#region providers
    FactionManager IProvide<FactionManager>.Value() => FM;
#endregion

    public override void _Ready()
    {
        Camera = GetNode<Camera3D>("Camera");
        Camera.LookAt(Vector3.Zero);

        Sun = GetNode<DirectionalLight3D>("Sun");
        Sun.LookAt(Vector3.Zero);

        this.Provide();
    }
}
