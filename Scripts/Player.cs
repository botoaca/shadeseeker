using Godot;

public class Player : KinematicBody
{
    [Export]
    private float gravity = -28;
    [Export]
    private float speed = 20;
    [Export]
    private float acceleration = 8;
    [Export]
    private float deacceleration = 16;
    [Export]
    private float maxSlopeAngle = 40;
    [Export]
    private float mouseSensitivity = 0.3f;

    private float zoomSmooth = 5;
    private float zoomFov = 40;
    private float normalFov = 90;
    private bool isZoomed = false;

    private Vector3 _vel = new Vector3();
    private Vector3 _dir = new Vector3();

    private Spatial _holder;
    private Camera _camera;
    private RayCast _raycast;

    private ProgressBar healthBar;
    private int health = 100;

    public bool canShoot = true;
    private ProgressBar ammoBar;

    private AnimationPlayer anim;
    private Timer ammoTimer;
    private AudioStreamPlayer gunAudio;
    private AnimationPlayer pistolAnim;
    
    public int wallAmount = 1;
    private Label wallAmountIndicator;
    private int killCount;
    private Label killCountLabel;
    private int coinCount;
    private Label coinCountIndicator;

    private AudioStreamPlayer powerupAudio;

    public Timer afterWallTimer;
    
    public override void _Ready()
    {
        GD.Randomize();
        
        _holder = GetNode<Spatial>("Holder");
        _camera = GetNode<Camera>("Holder/Camera");
        _raycast = GetNode<RayCast>("Holder/ShootRaycast");

        healthBar = GetNode<ProgressBar>("HUD/HealthBar");
        ammoBar = GetNode<ProgressBar>("HUD/AmmoBar");

        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        ammoTimer = GetNode<Timer>("AmmoTimer");

        pistolAnim = GetNode<AnimationPlayer>("Holder/Camera/Pistol/AnimationPlayer");

        wallAmountIndicator = GetNode<Label>("HUD/WallIndicator/Label");
        killCountLabel = GetNode<Label>("HUD/KillCount");
        coinCountIndicator = GetNode<Label>("HUD/CoinIndicator/Label");

        gunAudio = GetNode<AudioStreamPlayer>("Holder/Camera/Pistol/AudioStreamPlayer");
        powerupAudio = GetNode<AudioStreamPlayer>("Powerup");

        afterWallTimer = GetNode<Timer>("AfterWallTimer");

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _Process(float delta)
    {
        ProcessInput(delta);
        ProcessMovement(delta);

        if (health <= 0) GetTree().ReloadCurrentScene();
        if (health >= 100)
        {
            health = 100;
            healthBar.Value = health;
        }
        wallAmountIndicator.Text = wallAmount.ToString();
        if (wallAmount <= 0) wallAmountIndicator.Text = "0";
    }

    private async void ProcessInput(float delta)
    {
        _dir = new Vector3();
        Transform camXform = _camera.GlobalTransform;

        Vector2 inputMovementVector = new Vector2();

        if (Input.IsActionPressed("movement_forwards")) { inputMovementVector.y += 1; Walk(); }
        if (Input.IsActionPressed("movement_backwards")) { inputMovementVector.y -= 1; Walk(); }
        if (Input.IsActionPressed("movement_left")) { inputMovementVector.x -= 1; Walk(); }
        if (Input.IsActionPressed("movement_right")) { inputMovementVector.x += 1; Walk(); }

        if (canShoot)
        {
            if (Input.IsActionJustPressed("fire"))
            {
                _raycast.ForceRaycastUpdate();
                canShoot = false;
                pistolAnim.Play("player.shoot");
                gunAudio.PitchScale = (float)GD.RandRange(0.7, 1.3);
                gunAudio.Play();
                ammoBar.Call("Unload");
                if (_raycast.IsColliding())
                {
                    var body = _raycast.GetCollider();
                    if (body.HasMethod("Damage")) body.Call("Damage");

                }
                ammoTimer.Start(0.2f);
                await ToSignal(ammoTimer, "timeout");
                ammoBar.Call("Reload");
                ammoTimer.Start(0.5f);
                await ToSignal(ammoTimer, "timeout");
                canShoot = true;
            }
        }


        if (Input.IsActionJustPressed("zoom"))
        {
            isZoomed = !isZoomed;
        }

        if (isZoomed) _camera.Fov = Mathf.Lerp(_camera.Fov, zoomFov, delta * zoomSmooth);
        else _camera.Fov = Mathf.Lerp(_camera.Fov, normalFov, delta * zoomSmooth);


        inputMovementVector = inputMovementVector.Normalized();

        _dir += -camXform.basis.z * inputMovementVector.y;
        _dir += camXform.basis.x * inputMovementVector.x;

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (Input.GetMouseMode() == Input.MouseMode.Visible) Input.SetMouseMode(Input.MouseMode.Captured);
            else Input.SetMouseMode(Input.MouseMode.Visible);
        }
    }

    private void ProcessMovement(float delta)
    {
        _dir.y = 0;
        _dir = _dir.Normalized();

        _vel.y += delta * gravity;

        Vector3 hvel = _vel;
        hvel.y = 0;

        Vector3 target = _dir;

        target *= speed;

        float accel;
        if (_dir.Dot(hvel) > 0) accel = acceleration;
        else accel = deacceleration;

        hvel = hvel.LinearInterpolate(target, accel * delta);
        _vel.x = hvel.x;
        _vel.z = hvel.z;
        _vel = MoveAndSlide(_vel, new Vector3(0, 1, 0), false, 4, Mathf.Deg2Rad(maxSlopeAngle));
    }

    private void Walk()
    {
        if (IsOnFloor()) anim.Play("player.walk");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
            _holder.RotateX(Mathf.Deg2Rad(-mouseEvent.Relative.y * mouseSensitivity));
            RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * mouseSensitivity));

            Vector3 cameraRot = _holder.RotationDegrees;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -80, 80);
            _holder.RotationDegrees = cameraRot;
        }
    }

    public void Damage(int amount)
    {
        health -= amount;
        healthBar.Value = health;
    }
    
    public void PickupWall(int amount)
    {
        wallAmount += amount;
    }

    public void PickupHeal(int amount)
    {
        powerupAudio.PitchScale = (float)GD.RandRange(0.7, 1.1);
        powerupAudio.Play();
        health += amount;
        healthBar.Value = health;
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
        coinCountIndicator.Text = coinCount.ToString();
    }
    
    public void KilledEnemies(int amount)
    {
        killCount += amount;
        if (killCount >= 100) killCountLabel.AddColorOverride("font_color", new Color(255, 185, 0));  
        killCountLabel.Text = $"kills : {killCount}";
    }

    private void OnAfterWallTimeout()
    {
        canShoot = true;
    }
}