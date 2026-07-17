using Raylib_cs;
using System.Numerics;

namespace OMF2097;

public enum RobotType
{
    Jaguar,
    Shadow,
    Thorn,
    Flail,
    Pyros
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
    Forward,  // schnell, schwach
    Backward, // langsam, stark
    Neutral   // mittel
}

/// <summary>
/// Gemeinsame Basisklasse für alle Robotermodelle.
/// Enthält Physik, Zustandsverwaltung, Trefferlogik und allgemeine Zeichenhilfsmittel.
/// Jede konkrete Roboterklasse überschreibt nur die visuellen und typ-spezifischen Aspekte.
/// </summary>
public abstract class Robot
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; protected set; }
    public bool FacingRight { get; set; } = true;
    public float Health { get; private set; } = 100f;
    public RobotType Type { get; }
    public bool IsPlayer1 { get; }
    public RobotState State { get; protected set; } = RobotState.Idle;

    public bool IsAttacking => State == RobotState.Attack;
    public bool HitboxActive { get; set; } = false;
    public int AttackDamage { get; private set; } = 0;
    public AttackType CurrentAttackType { get; private set; } = AttackType.Punch;
    public AttackVariant CurrentAttackVariant { get; private set; } = AttackVariant.Neutral;

    public virtual float Width { get; } = 70f;
    public virtual float Height { get; } = 110f;

    protected const float Gravity = 1800f;
    protected const float WalkSpeed = 220f;
    protected const float JumpSpeed = -820f;
    protected const float CrouchJumpSpeed = -1450f;
    protected const float FloorY = 560f;

    protected float StateTimer = 0f;
    protected float AttackCooldown = 0f;
    protected float HitStun = 0f;

    protected Color Color;
    protected Color Accent;
    protected Color Dark;
    protected Color Light;

    protected Robot(RobotType type, bool isPlayer1, Vector2 startPosition)
    {
        Type = type;
        IsPlayer1 = isPlayer1;
        Position = startPosition;
        FacingRight = isPlayer1;
        InitializeColors();
    }

    protected abstract void InitializeColors();

    public Rectangle Hurtbox => new Rectangle(
        Position.X - Width / 2f,
        Position.Y - Height,
        Width,
        Height);

    public virtual Rectangle AttackHitbox
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
        StateTimer = 0f;
        AttackCooldown = 0f;
        HitStun = 0f;
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
        KeyboardKey punch = isPlayer1 ? KeyboardKey.KEY_F : KeyboardKey.KEY_ENTER;
        KeyboardKey kick = isPlayer1 ? KeyboardKey.KEY_SPACE : KeyboardKey.KEY_RIGHT_CONTROL;
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
            bool crouching = Raylib.IsKeyDown(block);
            float jumpSpeed = crouching ? CrouchJumpSpeed : JumpSpeed;
            Velocity = new Vector2(Velocity.X, jumpSpeed);
            State = RobotState.Jump;
        }

        bool punchPressed = Raylib.IsKeyPressed(punch);
        bool kickPressed = Raylib.IsKeyPressed(kick);
        if ((punchPressed || kickPressed) && AttackCooldown <= 0f)
        {
            StartAttack(kickPressed ? AttackType.Kick : AttackType.Punch, move);
            OnSpecialAttack(kickPressed ? AttackType.Kick : AttackType.Punch);
        }
    }

    protected virtual void OnSpecialAttack(AttackType attackType) { }

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

        float baseDuration = GetAttackDuration(attackType);
        float durationMultiplier = GetAttackDurationMultiplier(attackType);

        StateTimer = baseDuration * durationMultiplier;
        AttackCooldown = StateTimer + 0.15f;
        HitboxActive = true;

        AttackDamage = CalculateDamage(attackType);
    }

    protected virtual float GetAttackDuration(AttackType attackType) =>
        attackType == AttackType.Kick ? 0.45f : 0.35f;

    protected virtual float GetAttackDurationMultiplier(AttackType attackType) =>
        CurrentAttackVariant switch
        {
            AttackVariant.Forward => 0.75f,
            AttackVariant.Backward => 1.35f,
            _ => 1.0f
        };

    protected virtual int CalculateDamage(AttackType attackType)
    {
        int baseDamage = attackType == AttackType.Kick ? 14 : 10;
        int variantDamage = CurrentAttackVariant switch
        {
            AttackVariant.Forward => -2,
            AttackVariant.Backward => 4,
            _ => 0
        };

        return Math.Max(1, baseDamage + variantDamage);
    }

    public void Update(float dt)
    {
        AttackCooldown -= dt;

        if (HitStun > 0f)
        {
            HitStun -= dt;
            if (HitStun <= 0f)
                State = RobotState.Idle;
        }

        if (State == RobotState.Attack)
        {
            StateTimer -= dt;
            float activeRatio = GetHitboxActiveRatio();
            float activeEnd = StateTimer * (1f - activeRatio);
            if (StateTimer <= activeEnd && StateTimer > 0f)
                HitboxActive = false;
            if (StateTimer <= 0f)
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

    protected virtual float GetHitboxActiveRatio() =>
        CurrentAttackType == AttackType.Kick ? 0.6f : 0.55f;

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
        HitStun = 0.35f;
        Velocity = new Vector2(knockbackDirection * 250f, -180f);
        HitboxActive = false;
    }

    public void Draw()
    {
        float crouchOffset = State == RobotState.Block ? Height * 0.35f : 0f;
        Vector2 center = new Vector2(Position.X, Position.Y - Height / 2f + crouchOffset);
        float bob = State == RobotState.Walk ? MathF.Sin((float)Raylib.GetTime() * 15f) * 6f : 0f;
        float time = (float)Raylib.GetTime();

        DrawShadow(center, crouchOffset);
        DrawRobot(center, bob, time);

        if (Raylib.IsKeyDown(KeyboardKey.KEY_F1))
        {
            Raylib.DrawRectangleLinesEx(Hurtbox, 2f, Color.GREEN);
            if (HitboxActive)
                Raylib.DrawRectangleLinesEx(AttackHitbox, 2f, Color.RED);
        }
    }

    protected abstract void DrawRobot(Vector2 center, float bob, float time);

    protected virtual void DrawShadow(Vector2 center, float crouchOffset)
    {
        float shadowW = Width * 1.2f;
        float shadowH = 16f;
        float alpha = 1f - Math.Clamp((FloorY - Position.Y) / 200f, 0f, 0.6f);
        Color shadowColor = new Color(0, 0, 0, (int)(120 * alpha));
        Raylib.DrawEllipse((int)Position.X, (int)(Position.Y - 4f + crouchOffset), shadowW / 2f, shadowH / 2f, shadowColor);
    }

    // ========================
    // HILFSMETHODEN FÜR SHAPES
    // ========================

    protected void DrawRoundedRect(float x, float y, float w, float h, float radius, Color fill, Color border, float borderThickness)
    {
        Raylib.DrawRectangle((int)x, (int)(y + radius), (int)w, (int)(h - 2 * radius), fill);
        Raylib.DrawRectangle((int)(x + radius), (int)y, (int)(w - 2 * radius), (int)h, fill);

        float r = Math.Min(radius, Math.Min(w / 2f, h / 2f));
        Raylib.DrawCircle((int)(x + r), (int)(y + r), r, fill);
        Raylib.DrawCircle((int)(x + w - r), (int)(y + r), r, fill);
        Raylib.DrawCircle((int)(x + r), (int)(y + h - r), r, fill);
        Raylib.DrawCircle((int)(x + w - r), (int)(y + h - r), r, fill);

        if (borderThickness > 0)
        {
            Raylib.DrawRectangleLines((int)x, (int)(y + r), (int)w, (int)(h - 2 * r), border);
            Raylib.DrawRectangleLines((int)(x + r), (int)y, (int)(w - 2 * r), (int)h, border);
            Raylib.DrawCircleLines((int)(x + r), (int)(y + r), r, border);
            Raylib.DrawCircleLines((int)(x + w - r), (int)(y + r), r, border);
            Raylib.DrawCircleLines((int)(x + r), (int)(y + h - r), r, border);
            Raylib.DrawCircleLines((int)(x + w - r), (int)(y + h - r), r, border);
        }
    }

    protected void DrawCircle3D(float x, float y, float radius, Color main, Color shadow)
    {
        Raylib.DrawCircle((int)(x + 2f), (int)(y + 2f), radius, shadow);
        Raylib.DrawCircle((int)x, (int)y, radius, main);
        Raylib.DrawCircle((int)(x - radius * 0.3f), (int)(y - radius * 0.3f), radius * 0.25f, new Color(255, 255, 255, 120));
        Raylib.DrawCircleLines((int)x, (int)y, radius, Accent);
    }

    protected void DrawRoundedLimb(Vector2 start, Vector2 end, float width, float angle, Color fill, Color border)
    {
        Vector2 direction = end - start;
        float length = direction.Length();
        if (length < 1f) length = 1f;

        Vector2 normal = new Vector2(-direction.Y, direction.X) / length;

        Vector2 p1 = start + normal * width * 0.5f;
        Vector2 p2 = start - normal * width * 0.5f;
        Vector2 p3 = end - normal * width * 0.4f;
        Vector2 p4 = end + normal * width * 0.4f;

        Raylib.DrawTriangle(p1, p2, p3, fill);
        Raylib.DrawTriangle(p1, p3, p4, fill);

        Raylib.DrawCircle((int)start.X, (int)start.Y, width * 0.5f, fill);
        Raylib.DrawCircle((int)end.X, (int)end.Y, width * 0.4f, fill);

        Raylib.DrawLineEx(p1, p4, 2f, border);
        Raylib.DrawLineEx(p4, p3, 2f, border);
        Raylib.DrawLineEx(p3, p2, 2f, border);
        Raylib.DrawLineEx(p2, p1, 2f, border);
    }
}
