using Godot;

namespace HakimiAdventure;

/// <summary>
/// 玩家控制器 — 以《我的世界》为基准的物理/动作引擎。
///
/// 物理模型（Minecraft 式）：
///   - 地面：高摩擦 × 高加速 → 按了就冲到顶速，松手马上停
///   - 空中：极低操控（～5%），方向定了就飘出去
///   - 重力：强引力 ≈ 25 m/s²，跳跃干脆利落
///   - 跳跃高度 ≈ 1.25 格
/// </summary>
public partial class PlayerController : CharacterBody3D
{
	// ── 地面移动 ────────────────────────────────
	[ExportGroup("地面移动")]
	[Export] public float WalkSpeed   { get; set; } = 4.317f;
	[Export] public float SprintSpeed { get; set; } = 5.612f;
	[Export] public float CrouchSpeed { get; set; } = 1.295f;
	[Export] public float GroundAccel { get; set; } = 50f;
	[Export] public float GroundFriction { get; set; } = 50f;

	// ── 空中 ────────────────────────────────────
	[ExportGroup("空中")]
	[Export] public float AirAccel     { get; set; } = 3f;
	[Export] public float AirFriction  { get; set; } = 0f;
	[Export] public float AirSpeedCap  { get; set; } = 0.3f;

	// ── 跳跃 ────────────────────────────────────
	[ExportGroup("跳跃")]
	[Export] public float JumpVelocity    { get; set; } = 7.9f;
	[Export] public int   JumpBufferFrames { get; set; } = 3;

	// ── 重力 ────────────────────────────────────
	[ExportGroup("重力")]
	[Export] public float GravityOverride { get; set; } = 25f; // 0=项目设置

	// ── 下蹲 ────────────────────────────────────
	[ExportGroup("下蹲")]
	[Export] public float StandHeight { get; set; } = 1.8f;
	[Export] public float SneakHeight { get; set; } = 1.4f;

	// ── 鼠标 ────────────────────────────────────
	[ExportGroup("鼠标")]
	[Export] public float Sensitivity { get; set; } = 0.003f;
	[Export] public bool  InvertY     { get; set; } = false;
	// ── 内部状态 ────────────────────────────────
	private Camera3D? _cam;
	private CollisionShape3D? _col;
	private CapsuleShape3D? _cap;

	private float _pitch, _yaw;
	private float _curCapHeight;
	private bool  _isSneaking;
	private int   _jumpBuffer;

	// ── 初始化 ──────────────────────────────────
	public override void _Ready()
	{
		_cam = GetNodeOrNull<Camera3D>("Camera3D");
		if (_cam == null)
		{
			_cam = new Camera3D { Position = new Vector3(0, StandHeight * 0.875f, 0) };
			AddChild(_cam);
		}

		_col = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
		_cap = _col?.Shape as CapsuleShape3D;

		_curCapHeight = StandHeight;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// ── 输入 ────────────────────────────────────
	public override void _Input(InputEvent e)
	{
		// 鼠标视角
		if (e is InputEventMouseMotion m && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			_yaw   -= m.Relative.X * Sensitivity;
			_pitch -= m.Relative.Y * Sensitivity * (InvertY ? -1 : 1);
			_pitch  = Mathf.Clamp(_pitch, Mathf.DegToRad(-85f), Mathf.DegToRad(85f));

			Rotation = new Vector3(0, _yaw, 0);
			if (_cam != null)
				_cam.Rotation = new Vector3(_pitch, 0, 0);
		}

		// ESC 释放 / 左键捕获
		if (e is InputEventKey { Keycode: Key.Escape, Pressed: true })
			Input.MouseMode = Input.MouseModeEnum.Visible;
		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } &&
			Input.MouseMode != Input.MouseModeEnum.Captured)
			Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// ── 物理帧 ──────────────────────────────────
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// ──── 下蹲（直接检测 Ctrl，不依赖 InputMap） ────
		SneakTick(dt);

		// ──── 输入向量 ────
		// GetVector: X←[-1]→[+1], Y: 前[-1] 后[+1]
		Vector2 input = Input.GetVector("move_left", "move_right",
										"move_forward", "move_backward");
		bool hasInput = input.LengthSquared() > 0.001f;
		bool onGround = IsOnFloor();

		// ──── 水平速度 ────
		// 注意：input.Y 负 = 前 → 映射到 -Z（Godot 前方）
		Vector3 wish = (Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();

		// 冲刺（直接检测 Shift）
		bool sprint = Input.IsKeyPressed(Key.Shift) && !_isSneaking && input.Y <= 0 && hasInput;
		float topSpeed = _isSneaking ? CrouchSpeed
					   : sprint       ? SprintSpeed
					   :               WalkSpeed;

		Vector3 hz = new Vector3(Velocity.X, 0, Velocity.Z);
		float curSpd = hz.Length();

		if (onGround)
		{
			float accel = hasInput ? GroundAccel : GroundFriction;
			float speed = Mathf.MoveToward(curSpd, hasInput ? topSpeed : 0f, accel * dt);
			hz = wish * speed;
		}
		else
		{
			// 空中保留旧动量 + 微调
			hz = hz * 0.85f + wish * (topSpeed * AirSpeedCap) * AirAccel * dt;
			hz = hz.LimitLength(topSpeed);
		}

		Velocity = new Vector3(hz.X, Velocity.Y, hz.Z);

		// ──── 重力 ────
		float g = GravityOverride > 0
			? GravityOverride
			: ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
		if (!onGround)
			Velocity += new Vector3(0, -g, 0) * dt;

		// ──── 跳跃（直接检测 Space） ────
		if (Input.IsKeyPressed(Key.Space) && !_isSneaking)
			_jumpBuffer = JumpBufferFrames;

		if (_jumpBuffer > 0 && onGround)
		{
			Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
			_jumpBuffer = 0;
		}
		if (_jumpBuffer > 0) _jumpBuffer--;

		// ──── 步进 ────
		MoveAndSlide();

		// ──── 落地清残速 ────
		if (onGround && Velocity.Y < 0)
		{
			var v = Velocity;
			v.Y = 0;
			Velocity = v;
		}

		// 交互 (F)
		if (Input.IsActionJustPressed("interact"))
			GD.Print("Interact — TODO");
	}

	// ── 下蹲 ────────────────────────────────────
	private void SneakTick(float dt)
	{
		bool pressed = Input.IsKeyPressed(Key.Ctrl);

		if (pressed && IsOnFloor())
			_isSneaking = true;
		else if (!pressed)
		{
			if (RoomToStand())
				_isSneaking = false;
		}

		float target = _isSneaking ? SneakHeight : StandHeight;
		_curCapHeight = Mathf.Lerp(_curCapHeight, target, 12f * dt);
		if (_cap != null) _cap.Height = _curCapHeight;

		float camTarget = _isSneaking ? SneakHeight * 0.85f : StandHeight * 0.875f;
		if (_cam != null)
		{
			var p = _cam.Position;
			p.Y = Mathf.Lerp(p.Y, camTarget, 12f * dt);
			_cam.Position = p;
		}
	}

	private bool RoomToStand()
	{
		if (_col == null) return true;
		var space = GetWorld3D().DirectSpaceState;
		var q = PhysicsRayQueryParameters3D.Create(
			GlobalPosition + Vector3.Up * (SneakHeight + 0.05f),
			GlobalPosition + Vector3.Up * StandHeight);
		return space.IntersectRay(q).Count == 0;
	}
}
