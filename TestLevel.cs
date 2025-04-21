using Godot;
using System;

public partial class TestLevel : Node3D
{
    Camera3D Camera;
    DirectionalLight3D Sun;

    public override void _Ready()
    {
        Camera = GetNode<Camera3D>("Camera");
        Camera.LookAt(Vector3.Zero);

        Sun = GetNode<DirectionalLight3D>("Sun");
        Sun.LookAt(Vector3.Zero);
    }

}
