using Godot;

public interface Actor
{
    Node3D AsNode3D();
    void TakeDamage(int damage);
}