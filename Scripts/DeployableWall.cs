using Godot;

public class DeployableWall : RayCast
{
    private PackedScene wallShadow;
    private Player player;
    private bool canPlace;
    private int wallAmount;

    public override void _Ready()
    {
        wallShadow = GD.Load<PackedScene>("res://Scenes/DeployableWallShadow.tscn");
        player = (Player) GetParent().Owner;
    }

    public override void _Input(InputEvent @event)
    {
        if (player.wallAmount > 0)
        {
            if (Input.IsActionJustPressed("wall"))
            {
                if (GetChildCount() == 0)
                {
                    var ws = wallShadow.Instance();
                    AddChild(ws);
                }
                else GetChild(0).QueueFree();
            }
        }
    }
}
