using Godot;

public class DeployableWallShadow : Spatial
{
    private RayCast raycast;
    private PackedScene wall;
    private Player player;

    public override void _Ready()
    {
        wall = GD.Load<PackedScene>("res://Scenes/DeployableWall.tscn");
        player = (Player)GetParent().Owner;
        raycast = (RayCast)GetParent();
        SetAsToplevel(true);
        Translation = raycast.GetCollisionPoint();
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("fire"))
        {
            player.canShoot = false;
            StaticBody w = (StaticBody)wall.Instance();
            w.GlobalTransform = GlobalTransform;
            GetTree().Root.AddChild(w);
            player.wallAmount--;
            player.afterWallTimer.Start(.1f);
            QueueFree();
        }
    }

    public override void _Process(float delta)
    {
        Translation = raycast.GetCollisionPoint();
        Rotation = player.Rotation;
    }
}
