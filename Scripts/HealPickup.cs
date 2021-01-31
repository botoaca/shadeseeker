using Godot;

public class HealPickup : Area
{
    private void OnCollision(Area area)
    {
        if (area.GetParent() is Player player)
        {
            player.Call("PickupHeal", 50);
            QueueFree();
        }
    }
}