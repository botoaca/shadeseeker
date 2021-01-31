using Godot;

public class Ghost : KinematicBody
{
    private RayCast raycast;
    private Player player;
    private int health = 2;
    private readonly float speed = 40;

    private Timer damageTimer;
    private Timer stunTimer;
    private bool stunned = false;

    private Navigation navigation;
    private Vector3[] path = {};
    private int pathNode = 0;
    private Timer moveTimer;

    public override void _Ready()
    {
        navigation = (Navigation) GetParent();
        
        raycast = GetNode<RayCast>("RayCast");
        player = GetNode<Player>("../../Player");

        var area = GetNode<Area>("DamageArea");
        area.Connect("area_entered", this, nameof(OnCollision));
        area.Connect("area_exited", this, nameof(OnCollisionExit));
        
        damageTimer = GetNode<Timer>("DamageTimer");
        damageTimer.Connect("timeout", this, nameof(OnDamageTimerTimeout));

        stunTimer = GetNode<Timer>("StunTimer");

        moveTimer = GetNode<Timer>("MoveTimer");
        moveTimer.Connect("timeout", this, nameof(OnMoveTimerTimeout));
    }

    public override void _Process(float delta)
    {
        if (health <= 0)
        {
            player.Call("KilledEnemies", 1);
            player.Call("AddCoins", 2);
            QueueFree();
        }
    
        if (pathNode < path.Length)
        {
            var direction = path[pathNode] - GlobalTransform.origin;
            if (direction.Length() < 1) pathNode++;
            else if (!stunned) MoveAndCollide(direction.Normalized() * speed * delta);
        }
    }

    private void MoveTo(Vector3 targetPos)
    {
        path = navigation.GetSimplePath(GlobalTransform.origin, targetPos);
        pathNode = 0;
    }

    private void OnMoveTimerTimeout()
    {
        MoveTo(player.GlobalTransform.origin);
    }

    public async void Damage()
    {
        health--;
        stunned = true;
        stunTimer.Start(0.2f);
        await ToSignal(stunTimer, "timeout");
        stunned = false;
    }

    private void OnCollision(Area area)
    {
        if (area.GetParent() is Player)
        {
            damageTimer.Start();
        }
    }

    private void OnCollisionExit(Area area)
    {
        if (area.GetParent() is Player)
        {
            damageTimer.Stop();
        }
    }

    private void OnDamageTimerTimeout()
    {
        player.Damage(20);
    }
}
