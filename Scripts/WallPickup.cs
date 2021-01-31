using Godot;

public class WallPickup : Area
{
    private void OnCollision(Area area)
    {
        if (area.GetParent() is Player player)
        {
            player.Call("PickupWall", 1);
            QueueFree();
        }
    }
}