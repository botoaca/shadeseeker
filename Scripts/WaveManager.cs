using Godot;

public class WaveManager : Spatial
{
    private PackedScene ghost;
    private Navigation nav;
    private int waveGhostAmount = 3;
    private int waveIncrement = 3;
    private int waveCount;
    private Label waveCountLabel;
    private bool startWave = false;
    
    public override void _Ready()
    {
        GD.Randomize();
        nav = GetNode<Navigation>("../Navigation");
        ghost = GD.Load<PackedScene>("res://Scenes/Ghost.tscn");
        waveCountLabel = GetNode<Label>("../Player/HUD/WaveCount");
    }

    public override void _Process(float delta)
    {
        switch (waveCount)
        {
            case 20:
                waveIncrement = 5;
                break;
            case 50:
                waveIncrement = 7;
                break;
            case 100:
                waveIncrement = 10;
                break;
        }
        
        if (GetTree().GetNodesInGroup("ghosts").Count == 0) startWave = true;

        if (startWave)
        {
            for (int i = 0; i < waveGhostAmount; i++)
            {
                KinematicBody instanceGhost = (KinematicBody) ghost.Instance();
                instanceGhost.Translation = new Vector3((float)GD.RandRange(-45, 45), Translation.y, (float)GD.RandRange(45, -45));
                nav.CallDeferred("add_child", instanceGhost);
            }

            startWave = false;
            waveGhostAmount += waveIncrement;
            waveCount++;
            waveCountLabel.Text = $"wave: {waveCount}";
        }
    }
}