using Godot;

public class AmmoBar : ProgressBar
{
    private AnimationPlayer anim;
    public override void _Ready() { anim = GetNode<AnimationPlayer>("AnimationPlayer"); }
    public void Reload() { anim.Play("ammo.reload"); }
    public void Unload() { anim.Play("ammo.unload"); }
}
