using Godot;

public class MainMenu : Control
{
    private void Play() { GetTree().ChangeScene("res://Scenes/Sandbox.tscn"); }
    private void Quit() { GetTree().Quit(); }
}