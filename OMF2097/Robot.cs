using Raylib_cs;

using System.Numerics;

namespace OMF2097;

public enum RobotType
{
    Jaguar,
    Shadow,
    Thorn,
    Flail
}

public enum RobotState
{
    Idle,
    Walk,
    Jump,
    Attack,
    Hit,
    Block
}

public enum AttackType
{
    Punch,
    Kick
}

public enum AttackVariant
{
    Forward, // schnell, schwach
    Backward, // langsam, stark
    Neutral // mittel
}

public class Robot
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; private set; }
    public bool FacingRight { get; set; } = true;
    public float Health { get; private set; } = 100f;
    public RobotType Type { get; }
    public bool IsPlayer1 { get; }
    public RobotState State { get; private set; } = RobotState.Idle;

    public bool IsAttacking => State == RobotState.Attack;
    public bool HitboxActive { get; set; } = false;
    public int AttackDamage { get; private set; } = 0;
    public AttackType CurrentAttackType { get; private set; } = AttackType.Punch;
    public AttackVariant CurrentAttackVariant { get; private set; } = AttackVariant.Neutral;

    public float Width { get; } = 70f;
    public float Height { get; } = 110f;

    private const float Gravity = 1800f;
    private const float WalkSpeed = 220f;
    private const float JumpSpeed = -650f;
    private const float FloorY = 560f;

    private float _stateTimer = 0f;
    private float _attackCooldown = 0f;
    private float _hitStun = 0f;
    private Color _color;
    private Color _accent;

    public Robot(RobotType type, bool isPlayer1, Vector2 startPosition)
    {
        Type = type;
        IsPlayer1 = isPlayer1;
        Position = startPosition;
        FacingRight = isPlayer1;

        (_color, _accent) = type switch
        {
            RobotType.Jaguar => (new Color(255, 100, 30, 255), new Color(80, 80, 80, 255)),
            RobotType.Shadow => (new Color(60, 60, 80, 255), new Color(180, 40, 220, 255)),
            RobotType.Thorn => (new Color(40, 140, 60, 255), new Color(160, 200, 40, 255)),
            RobotType.Flail => (new Color(180, 160, 40, 255), new Color(120, 60, 20, 255)),
            _ => (Color.GRAY, Color.WHITE)
        };
    }

    public Rectangle Hurtbox => new Rectangle(
        Position.X - Width / 2f,
        Position.Y - Height,
        Width,
        Height
    );

    public Rectangle AttackHitbox
    {
        get
        {
            float reach = CurrentAttackType == AttackType.Kick ? 110f : 90f;
            float height = CurrentAttackType == AttackType.Kick ? 40f : 50f;
            float yOffset = CurrentAttackType == AttackType.Kick ? Height * 0.25f : Height * 0.65f;
            float x = FacingRight ? Position.X + Width / 2f : Position.X - Width / 2f - reach;
            return new Rectangle(x, Position.Y - yOffset, reach, height);
        }
    }

    public void Reset(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Health = 100f;
        State = RobotState.Idle;
        _stateTimer = 0f;
        _attackCooldown = 0f;
        _hitStun = 0f;
        HitboxActive = false;
        FacingRight = IsPlayer1;
    }

    public void HandleInput(bool isPlayer1)
    {
        if (State == RobotState.Hit)
            return;

        KeyboardKey left = isPlayer1 ? KeyboardKey.KEY_A : KeyboardKey.KEY_LEFT;
        KeyboardKey right = isPlayer1 ? KeyboardKey.KEY_D : KeyboardKey.KEY_RIGHT;
        KeyboardKey jump = isPlayer1 ? KeyboardKey.KEY_W : KeyboardKey.KEY_UP;
        KeyboardKey punch = isPlayer1 ? KeyboardKey.KEY_SPACE : KeyboardKey.KEY_ENTER;
        KeyboardKey kick = isPlayer1 ? KeyboardKey.KEY_F : KeyboardKey.KEY_RIGHT_SHIFT;
        KeyboardKey block = isPlayer1 ? KeyboardKey.KEY_S : KeyboardKey.KEY_DOWN;

        bool onGround = Position.Y >= FloorY - 1f;

        if (Raylib.IsKeyDown(block) && onGround && State != RobotState.Attack)
        {
            State = RobotState.Block;
            Velocity = new Vector2(0, Velocity.Y);
            return;
        }

        float move = 0f;
        if (Raylib.IsKeyDown(left)) move -= 1f;
        if (Raylib.IsKeyDown(right)) move += 1f;

        if (State != RobotState.Attack)
        {
            if (move != 0f)
            {
                FacingRight = move > 0;
                if (onGround)
                    State = RobotState.Walk;
            }
            else if (onGround)
            {
                State = RobotState.Idle;
            }

            Velocity = new Vector2(move * WalkSpeed, Velocity.Y);
        }

        if (Raylib.IsKeyPressed(jump) && onGround)
        {
            Velocity = new Vector2(Velocity.X, JumpSpeed);
            State = RobotState.Jump;
        }

        bool punchPressed = Raylib.IsKeyPressed(punch);
        bool kickPressed = Raylib.IsKeyPressed(kick);
        if ((punchPressed || kickPressed) && _attackCooldown <= 0f)
        {
            StartAttack(kickPressed ? AttackType.Kick : AttackType.Punch, move);
        }
    }

    private void StartAttack(AttackType attackType, float moveDirection)
    {
        State = RobotState.Attack;
        CurrentAttackType = attackType;

        bool movingForward = (FacingRight && moveDirection > 0) || (!FacingRight && moveDirection < 0);
        bool movingBackward = (FacingRight && moveDirection < 0) || (!FacingRight && moveDirection > 0);

        if (movingForward)
            CurrentAttackVariant = AttackVariant.Forward;
        else if (movingBackward)
            CurrentAttackVariant = AttackVariant.Backward;
        else
            CurrentAttackVariant = AttackVariant.Neutral;

        float baseDuration = CurrentAttackType == AttackType.Kick ? 0.45f : 0.35f;
        float durationMultiplier = CurrentAttackVariant switch
        {
            AttackVariant.Forward => 0.75f,
            AttackVariant.Backward => 1.35f,
            _ => 1.0f
        };

        _stateTimer = baseDuration * durationMultiplier;
        _attackCooldown = _stateTimer + 0.15f;
        HitboxActive = true;

        int baseDamage = CurrentAttackType == AttackType.Kick ? 14 : 10;
        int typeBonus = Type switch
        {
            RobotType.Jaguar => 2,
            RobotType.Shadow => 0,
            RobotType.Thorn => 4,
            RobotType.Flail => 6,
            _ => 0
        };

        int variantDamage = CurrentAttackVariant switch
        {
            AttackVariant.Forward => -2,
            AttackVariant.Backward => 4,
            _ => 0
        };

        AttackDamage = Math.Max(1, baseDamage + typeBonus + variantDamage);
    }

    public void Update(float dt)
    {
        _attackCooldown -= dt;

        if (_hitStun > 0f)
        {
            _hitStun -= dt;
            if (_hitStun <= 0f)
                State = RobotState.Idle;
        }

        if (State == RobotState.Attack)
        {
            _stateTimer -= dt;
            float activeRatio = CurrentAttackType == AttackType.Kick ? 0.6f : 0.55f;
            float activeEnd = _stateTimer * (1f - activeRatio);
            if (_stateTimer <= activeEnd && _stateTimer > 0f)
                HitboxActive = false;
            if (_stateTimer <= 0f)
                State = RobotState.Idle;
        }

        Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);
        Position = new Vector2(Position.X + Velocity.X * dt, Position.Y + Velocity.Y * dt);

        if (Position.Y >= FloorY)
        {
            Position = new Vector2(Position.X, FloorY);
            Velocity = new Vector2(Velocity.X, 0f);

            if (State == RobotState.Jump)
                State = RobotState.Idle;
        }

        if (State == RobotState.Block && !Raylib.IsKeyDown(IsPlayer1 ? KeyboardKey.KEY_S : KeyboardKey.KEY_DOWN))
        {
            State = RobotState.Idle;
        }
    }

    public void TakeDamage(int damage, int knockbackDirection, bool attackerFacingRight)
    {
        bool movingAwayFromAttacker = (knockbackDirection > 0 && !FacingRight) || (knockbackDirection < 0 && FacingRight);

        if (State == RobotState.Block || movingAwayFromAttacker)
        {
            Health -= damage / 4;
            Velocity = new Vector2(knockbackDirection * 80f, Velocity.Y);
            return;
        }

        Health -= damage;
        State = RobotState.Hit;
        _hitStun = 0.35f;
        Velocity = new Vector2(knockbackDirection * 250f, -180f);
        HitboxActive = false;
    }

    public void Draw()
    {
        Vector2 center = new Vector2(Position.X, Position.Y - Height / 2f);
        float bob = State == RobotState.Walk ? MathF.Sin((float)Raylib.GetTime() * 15f) * 6f : 0f;

        DrawBody(center, bob);
        DrawHead(center, bob);
        DrawLimbs(center, bob);
        DrawWeapon(center, bob);
        DrawTypeDetails(center, bob);

        if (State == RobotState.Block)
        {
            DrawShield();
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_F1))
        {
            Raylib.DrawRectangleLinesEx(Hurtbox, 2f, Color.GREEN);
            if (HitboxActive)
                Raylib.DrawRectangleLinesEx(AttackHitbox, 2f, Color.RED);
        }
    }

    private void DrawBody(Vector2 center, float bob)
    {
        float bodyW = Width;
        float bodyH = Height * 0.55f;
        Rectangle body = new Rectangle(center.X - bodyW / 2f, center.Y - bodyH / 2f + bob, bodyW, bodyH);
        Raylib.DrawRectangleRec(body, _color);
        Raylib.DrawRectangleLinesEx(body, 3f, _accent);

        // Chest reactor core
        Raylib.DrawCircle((int)center.X, (int)(center.Y + bob), 10f, _accent);
        Raylib.DrawCircle((int)center.X, (int)(center.Y + bob), 5f, Color.WHITE);

        // Type-specific chest detail
        switch (Type)
        {
            case RobotType.Jaguar:
                Raylib.DrawTriangle(
                    new Vector2(center.X, center.Y - 20f + bob),
                    new Vector2(center.X - 12f, center.Y + 8f + bob),
                    new Vector2(center.X + 12f, center.Y + 8f + bob),
                    new Color(255, 200, 50, 255));
                break;
            case RobotType.Shadow:
                Raylib.DrawRectangle((int)(center.X - 8f), (int)(center.Y - 12f + bob), 16, 24, new Color(120, 40, 160, 255));
                break;
            case RobotType.Thorn:
                Raylib.DrawRectangle((int)(center.X - 10f), (int)(center.Y - 14f + bob), 20, 28, new Color(100, 80, 40, 255));
                Raylib.DrawCircle((int)center.X, (int)(center.Y + bob), 6f, new Color(160, 200, 40, 255));
                break;
            case RobotType.Flail:
                Raylib.DrawRectangle((int)(center.X - 14f), (int)(center.Y - 10f + bob), 28, 20, new Color(80, 60, 30, 255));
                Raylib.DrawRectangleLines((int)(center.X - 14f), (int)(center.Y - 10f + bob), 28, 20, new Color(220, 180, 60, 255));
                break;
        }
    }

    private void DrawHead(Vector2 center, float bob)
    {
        float headSize = Type == RobotType.Flail ? 38f : 32f;
        Vector2 headPos = new Vector2(center.X, center.Y - Height * 0.35f + bob);

        switch (Type)
        {
            case RobotType.Jaguar:
                // Cat-like rounded head with ears
                Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f, _accent);
                Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f - 4f, _color);
                // Ears
                float earDir = FacingRight ? 1f : -1f;
                Raylib.DrawTriangle(
                    new Vector2(headPos.X + earDir * 10f, headPos.Y - 18f),
                    new Vector2(headPos.X + earDir * 2f, headPos.Y - 8f),
                    new Vector2(headPos.X + earDir * 18f, headPos.Y - 6f),
                    _accent);
                break;
            case RobotType.Shadow:
                // Ninja hood shape
                Raylib.DrawRectangle((int)(headPos.X - headSize / 2f), (int)(headPos.Y - headSize / 2f), (int)headSize, (int)headSize, _accent);
                Raylib.DrawRectangle((int)(headPos.X - headSize / 2f + 4f), (int)(headPos.Y - headSize / 2f + 4f), (int)headSize - 8, (int)headSize - 8, _color);
                break;
            case RobotType.Thorn:
                // Spiked helmet
                Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f, _accent);
                Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f - 4f, _color);
                for (int i = -1; i <= 1; i += 2)
                {
                    Raylib.DrawTriangle(
                        new Vector2(headPos.X + i * 14f, headPos.Y - 14f),
                        new Vector2(headPos.X + i * 10f, headPos.Y - 4f),
                        new Vector2(headPos.X + i * 20f, headPos.Y - 4f),
                        _accent);
                }
                break;
            case RobotType.Flail:
                // Heavy boxy helmet
                Raylib.DrawRectangle((int)(headPos.X - headSize / 2f), (int)(headPos.Y - headSize / 2f), (int)headSize, (int)headSize, _accent);
                Raylib.DrawRectangle((int)(headPos.X - headSize / 2f + 5f), (int)(headPos.Y - headSize / 2f + 5f), (int)headSize - 10, (int)headSize - 10, _color);
                break;
        }

        // Visor
        float visorW = Type == RobotType.Flail ? 18f : 22f;
        float visorH = 8f;
        float visorX = FacingRight ? headPos.X + 2f : headPos.X - visorW - 2f;
        Color visorColor = Type == RobotType.Shadow ? new Color(200, 50, 255, 255) : Color.SKYBLUE;
        Raylib.DrawRectangle((int)visorX, (int)(headPos.Y - 4f), (int)visorW, (int)visorH, visorColor);
    }

    private void DrawLimbs(Vector2 center, float bob)
    {
        float armW = Type == RobotType.Flail ? 22f : 16f;
        float armH = Type == RobotType.Flail ? 55f : 50f;
        float legW = Type == RobotType.Thorn ? 26f : 20f;
        float legH = Type == RobotType.Thorn ? 60f : 55f;

        float armExtension = CurrentAttackType == AttackType.Punch ? 40f : (CurrentAttackType == AttackType.Kick ? 20f : 25f);
        float armOffset = State == RobotState.Attack ? (FacingRight ? armExtension : -armExtension) : (FacingRight ? 18f : -18f);
        float armAngle = State == RobotState.Attack ? (FacingRight ? -0.8f : 0.8f) : 0f;

        Vector2 leftArm = new Vector2(center.X - armOffset, center.Y - Height * 0.1f + bob);
        Vector2 rightArm = new Vector2(center.X + armOffset, center.Y - Height * 0.1f + bob);

        DrawLimb(leftArm, armW, armH, armAngle);
        DrawLimb(rightArm, armW, armH, -armAngle);

        Vector2 leftLeg = new Vector2(center.X - 18f, center.Y + Height * 0.22f + bob);
        Vector2 rightLeg = new Vector2(center.X + 18f, center.Y + Height * 0.22f + bob);

        float legBob = State == RobotState.Walk ? MathF.Sin((float)Raylib.GetTime() * 15f + MathF.PI) * 8f : 0f;
        float kickExtension = CurrentAttackType == AttackType.Kick && State == RobotState.Attack ? 25f : 0f;
        float kickDir = FacingRight ? 1f : -1f;

        DrawLimb(new Vector2(leftLeg.X, leftLeg.Y + legBob), legW, legH, 0f);
        DrawLimb(new Vector2(rightLeg.X + kickDir * kickExtension, rightLeg.Y - legBob), legW, legH + kickExtension * 0.3f, kickDir * (kickExtension > 0 ? -0.3f : 0f));
    }

    private void DrawLimb(Vector2 pos, float w, float h, float angle)
    {
        Rectangle rect = new Rectangle(pos.X - w / 2f, pos.Y - h / 2f, w, h);
        Raylib.DrawRectanglePro(rect, new Vector2(w / 2f, h / 2f), angle * Raylib.RAD2DEG, _color);
        Raylib.DrawRectangleLinesEx(rect, 2f, _accent);
    }

    private void DrawWeapon(Vector2 center, float bob)
    {
        float reach = CurrentAttackType == AttackType.Kick ? 0f : 55f;
        if (reach <= 0f) return;

        float x = FacingRight ? center.X + Width / 2f - 5f : center.X - Width / 2f - reach + 5f;
        float y = center.Y - Height * 0.15f + bob;

        Color weaponColor = Type switch
        {
            RobotType.Jaguar => Color.ORANGE,
            RobotType.Shadow => Color.PURPLE,
            RobotType.Thorn => Color.GREEN,
            RobotType.Flail => Color.GOLD,
            _ => Color.WHITE
        };

        Rectangle weapon = new Rectangle(x, y, reach, 14f);
        Raylib.DrawRectangleRec(weapon, weaponColor);
        Raylib.DrawRectangleLinesEx(weapon, 2f, Color.WHITE);

        // Weapon detail
        float detailX = FacingRight ? x + 10f : x + reach - 20f;
        Raylib.DrawRectangle((int)detailX, (int)(y + 2f), 12, 10, Color.WHITE);

        // Type-specific weapon shape
        switch (Type)
        {
            case RobotType.Jaguar:
                Raylib.DrawTriangle(
                    new Vector2(FacingRight ? x + reach : x, y + 7f),
                    new Vector2(FacingRight ? x + reach + 12f : x - 12f, y + 2f),
                    new Vector2(FacingRight ? x + reach + 12f : x - 12f, y + 12f),
                    Color.ORANGE);
                break;
            case RobotType.Shadow:
                for (int i = 0; i < 3; i++)
                {
                    float bladeX = FacingRight ? x + reach + i * 8f : x - i * 8f;
                    Raylib.DrawLine((int)bladeX, (int)(y + 2f), (int)(FacingRight ? bladeX + 8f : bladeX - 8f), (int)(y + 12f), Color.WHITE);
                }
                break;
            case RobotType.Thorn:
                for (int i = 0; i < 4; i++)
                {
                    float spikeX = FacingRight ? x + 10f + i * 12f : x + reach - 10f - i * 12f;
                    Raylib.DrawTriangle(
                        new Vector2(spikeX, y - 4f),
                        new Vector2(spikeX - 4f, y + 8f),
                        new Vector2(spikeX + 4f, y + 8f),
                        new Color(160, 200, 40, 255));
                }
                break;
            case RobotType.Flail:
                float ballX = FacingRight ? x + reach + 14f : x - 14f;
                Raylib.DrawCircle((int)ballX, (int)(y + 7f), 10f, Color.GOLD);
                Raylib.DrawCircle((int)ballX, (int)(y + 7f), 10f, Color.WHITE);
                break;
        }
    }

    private void DrawTypeDetails(Vector2 center, float bob)
    {
        switch (Type)
        {
            case RobotType.Jaguar:
                // Tail
                float tailDir = FacingRight ? -1f : 1f;
                float tailSway = MathF.Sin((float)Raylib.GetTime() * 8f) * 6f;
                Raylib.DrawLine(
                    (int)(center.X + tailDir * Width / 2f),
                    (int)(center.Y + Height * 0.15f + bob),
                    (int)(center.X + tailDir * (Width / 2f + 30f) + tailSway),
                    (int)(center.Y + Height * 0.35f + bob),
                    _accent);
                break;
            case RobotType.Shadow:
                // Scarf / shadow trail
                float scarfDir = FacingRight ? -1f : 1f;
                float scarfSway = MathF.Sin((float)Raylib.GetTime() * 10f) * 8f;
                Raylib.DrawLine(
                    (int)(center.X + scarfDir * Width / 2f),
                    (int)(center.Y - Height * 0.1f + bob),
                    (int)(center.X + scarfDir * (Width / 2f + 40f) + scarfSway),
                    (int)(center.Y + Height * 0.1f + bob),
                    new Color(180, 40, 220, 180));
                break;
            case RobotType.Thorn:
                // Shoulder spikes
                for (int i = -1; i <= 1; i += 2)
                {
                    Raylib.DrawTriangle(
                        new Vector2(center.X + i * Width / 2f, center.Y - Height * 0.15f + bob),
                        new Vector2(center.X + i * (Width / 2f + 12f), center.Y - Height * 0.05f + bob),
                        new Vector2(center.X + i * (Width / 2f - 5f), center.Y + Height * 0.05f + bob),
                        _accent);
                }
                break;
            case RobotType.Flail:
                // Heavy shoulder pads
                for (int i = -1; i <= 1; i += 2)
                {
                    Raylib.DrawRectangle(
                        (int)(center.X + i * Width / 2f - 8f),
                        (int)(center.Y - Height * 0.25f + bob),
                        16, 30, _accent);
                }
                break;
        }
    }

    private void DrawShield()
    {
        float x = FacingRight ? Position.X - Width / 2f - 10f : Position.X + Width / 2f + 10f;
        float y = Position.Y - Height * 0.7f;
        float w = 12f;
        float h = Height * 0.6f;

        Rectangle shield = new Rectangle(x, y, w, h);
        Raylib.DrawRectangleRec(shield, new Color(100, 200, 255, 180));
        Raylib.DrawRectangleLinesEx(shield, 2f, Color.SKYBLUE);
    }
}
