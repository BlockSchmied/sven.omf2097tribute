using System.Numerics;
using Raylib_cs;

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
    public bool IsPyros => Type == RobotType.Pyros;

    private const float Gravity = 1800f;
    private const float WalkSpeed = 220f;
    private const float JumpSpeed = -650f;
    private const float CrouchJumpSpeed = -1150f;
    private const float FloorY = 560f;

    private float _stateTimer = 0f;
    private float _attackCooldown = 0f;
    private float _hitStun = 0f;
    private Color _color;
    private Color _accent;
    private Color _dark;
    private Color _light;

    public Robot(RobotType type, bool isPlayer1, Vector2 startPosition)
    {
        Type = type;
        IsPlayer1 = isPlayer1;
        Position = startPosition;
        FacingRight = isPlayer1;

        (_color, _accent, _dark, _light) = type switch
        {
            RobotType.Jaguar => (new Color(255, 100, 30, 255), new Color(80, 80, 80, 255), new Color(140, 50, 15, 255), new Color(255, 160, 60, 255)),
            RobotType.Shadow => (new Color(60, 60, 80, 255), new Color(180, 40, 220, 255), new Color(30, 30, 45, 255), new Color(100, 100, 130, 255)),
            RobotType.Thorn => (new Color(40, 140, 60, 255), new Color(160, 200, 40, 255), new Color(25, 90, 35, 255), new Color(80, 190, 90, 255)),
            RobotType.Flail => (new Color(180, 160, 40, 255), new Color(120, 60, 20, 255), new Color(110, 95, 20, 255), new Color(230, 210, 80, 255)),
            RobotType.Pyros => (new Color(255, 140, 20, 255), new Color(139, 0, 0, 255), new Color(80, 20, 10, 255), new Color(255, 220, 80, 255)),
            _ => (Color.GRAY, Color.WHITE, Color.DARKGRAY, Color.LIGHTGRAY)
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
            float reach;
            float height;
            float yOffset;
            float x;

            if (IsPyros && CurrentAttackType == AttackType.Kick)
            {
                reach = 130f;
                height = 60f;
                yOffset = Height * 0.35f;
                x = FacingRight ? Position.X + Width / 2f - 10f : Position.X - Width / 2f - reach + 10f;
            }
            else
            {
                reach = CurrentAttackType == AttackType.Kick ? 110f : 90f;
                height = CurrentAttackType == AttackType.Kick ? 40f : 50f;
                yOffset = CurrentAttackType == AttackType.Kick ? Height * 0.25f : Height * 0.65f;
                x = FacingRight ? Position.X + Width / 2f : Position.X - Width / 2f - reach;
            }

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
            bool crouching = Raylib.IsKeyDown(block);
            float jumpSpeed = crouching ? CrouchJumpSpeed : JumpSpeed;
            Velocity = new Vector2(Velocity.X, jumpSpeed);
            State = RobotState.Jump;
        }

        bool punchPressed = Raylib.IsKeyPressed(punch);
        bool kickPressed = Raylib.IsKeyPressed(kick);
        if ((punchPressed || kickPressed) && _attackCooldown <= 0f)
        {
            StartAttack(kickPressed ? AttackType.Kick : AttackType.Punch, move);

            if (IsPyros && kickPressed && !onGround)
            {
                // Düsenstoß in der Luft: zusätzlicher Schub nach oben und vom Gegner weg
                float pushDir = FacingRight ? 1f : -1f;
                Velocity = new Vector2(-pushDir * 180f, -420f);
            }
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

        float baseDuration = CurrentAttackType == AttackType.Kick ? (IsPyros ? 0.55f : 0.45f) : 0.35f;
        float durationMultiplier = CurrentAttackVariant switch
        {
            AttackVariant.Forward => IsPyros && CurrentAttackType == AttackType.Kick ? 0.85f : 0.75f,
            AttackVariant.Backward => IsPyros && CurrentAttackType == AttackType.Kick ? 1.2f : 1.35f,
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
            RobotType.Pyros => CurrentAttackType == AttackType.Kick ? 5 : 3,
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
            float activeRatio = CurrentAttackType == AttackType.Kick ? (IsPyros ? 0.75f : 0.6f) : 0.55f;
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
        float time = (float)Raylib.GetTime();

        // Schatten unter dem Roboter
        DrawShadow();

        // Zeichne von hinten nach vorne für korrekte Überlappung
        DrawTypeDetailsBack(center, bob, time);
        DrawWeaponBack(center, bob);
        if (IsPyros)
        {
            DrawPyrosBody(center, bob);
            DrawPyrosHead(center, bob);
            DrawPyrosLimbs(center, bob, time);
            DrawPyrosFlame(center, bob);
        }
        else
        {
            DrawBody(center, bob);
            DrawHead(center, bob);
            DrawLimbs(center, bob, time);
            DrawWeapon(center, bob);
        }
        DrawTypeDetailsFront(center, bob, time);

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

    private void DrawShadow()
    {
        float shadowW = IsPyros ? Width * 1.4f : Width * 1.2f;
        float shadowH = IsPyros ? 22f : 16f;
        float alpha = 1f - Math.Clamp((FloorY - Position.Y) / 200f, 0f, 0.6f);
        Color shadowColor = new Color(0, 0, 0, (int)(120 * alpha));
        Raylib.DrawEllipse((int)Position.X, (int)(Position.Y - 4f), shadowW / 2f, shadowH / 2f, shadowColor);
    }

    private void DrawBody(Vector2 center, float bob)
    {
        float torsoW = Width * 0.85f;
        float torsoH = Height * 0.58f;
        float torsoY = center.Y + bob;

        // Hüfte / Becken
        DrawRoundedRect(center.X - torsoW * 0.35f, torsoY + Height * 0.18f, torsoW * 0.7f, Height * 0.16f, 8f, _dark, _accent, 2f);

        // Unterer Torso / Abdomen (segmentiert)
        DrawRoundedRect(center.X - torsoW * 0.4f, torsoY + Height * 0.02f, torsoW * 0.8f, Height * 0.22f, 10f, _color, _accent, 2f);
        // Abdomen-Details
        for (int i = 0; i < 3; i++)
        {
            float segY = torsoY + Height * 0.06f + i * 10f;
            Raylib.DrawLine((int)(center.X - torsoW * 0.3f), (int)segY, (int)(center.X + torsoW * 0.3f), (int)segY, _dark);
        }

        // Oberer Torso / Brustpanzer
        DrawRoundedRect(center.X - torsoW * 0.45f, torsoY - Height * 0.32f, torsoW * 0.9f, Height * 0.38f, 14f, _color, _accent, 3f);

        // Brust-Highlight für Rundung
        DrawRoundedRect(center.X - torsoW * 0.35f, torsoY - Height * 0.28f, torsoW * 0.7f, Height * 0.18f, 10f, _light, Color.WHITE, 1f);

        // Reaktor-Kern mit mehreren Ringen
        DrawReactorCore(center, torsoY);

        // Schultergelenke
        float shoulderY = torsoY - Height * 0.28f;
        DrawCircle3D(center.X - torsoW * 0.48f, shoulderY, 14f, _accent, _dark);
        DrawCircle3D(center.X + torsoW * 0.48f, shoulderY, 14f, _accent, _dark);

        // Typ-spezifische Brust-Details
        DrawChestDetail(center, torsoY, bob);
    }

    private void DrawReactorCore(Vector2 center, float torsoY)
    {
        float coreY = torsoY - Height * 0.12f;
        // Äußerer Ring
        Raylib.DrawCircle((int)center.X, (int)coreY, 16f, _accent);
        // Mittlerer Ring
        Raylib.DrawCircle((int)center.X, (int)coreY, 12f, _dark);
        // Innerer leuchtender Kern
        Raylib.DrawCircle((int)center.X, (int)coreY, 8f, Color.WHITE);
        // Glühen
        Raylib.DrawCircle((int)center.X, (int)coreY, 5f, new Color(255, 255, 200, 220));
        // Reflexion
        Raylib.DrawCircle((int)(center.X - 2f), (int)(coreY - 2f), 2f, Color.WHITE);
    }

    private void DrawChestDetail(Vector2 center, float torsoY, float bob)
    {
        float coreY = torsoY - Height * 0.12f + bob * 0.5f;
        switch (Type)
        {
            case RobotType.Jaguar:
                // Dreieckiges Emblem mit innerem Detail
                Raylib.DrawTriangle(
                    new Vector2(center.X, coreY - 22f),
                    new Vector2(center.X - 16f, coreY + 10f),
                    new Vector2(center.X + 16f, coreY + 10f),
                    new Color(255, 200, 50, 255));
                Raylib.DrawTriangle(
                    new Vector2(center.X, coreY - 12f),
                    new Vector2(center.X - 8f, coreY + 4f),
                    new Vector2(center.X + 8f, coreY + 4f),
                    _color);
                // Seiten-Lüftungsschlitze
                for (int i = -1; i <= 1; i += 2)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        float sx = center.X + i * 28f;
                        float sy = coreY - 8f + j * 7f;
                        Raylib.DrawRectangle((int)sx, (int)sy, 10, 4, _dark);
                    }
                }
                break;

            case RobotType.Shadow:
                // Vertikaler Reaktor-Schlitz
                DrawRoundedRect(center.X - 8f, coreY - 18f, 16f, 36f, 4f, new Color(120, 40, 160, 255), _accent, 1f);
                // Horizontale Linien
                for (int i = -2; i <= 2; i++)
                {
                    Raylib.DrawLine((int)(center.X - 6f), (int)(coreY + i * 6f), (int)(center.X + 6f), (int)(coreY + i * 6f), Color.WHITE);
                }
                // Seitliche Rüstungsplatten
                for (int i = -1; i <= 1; i += 2)
                {
                    DrawRoundedRect(center.X + i * 26f - 8f, coreY - 14f, 16f, 28f, 3f, _dark, _accent, 1f);
                }
                break;

            case RobotType.Thorn:
                // Dornenpanzer mit mittlerem Kristall
                DrawRoundedRect(center.X - 12f, coreY - 18f, 24f, 36f, 6f, new Color(100, 80, 40, 255), _accent, 2f);
                Raylib.DrawCircle((int)center.X, (int)coreY, 8f, new Color(160, 200, 40, 255));
                Raylib.DrawCircle((int)center.X, (int)coreY, 4f, Color.WHITE);
                // Seitliche Dornen
                for (int i = -1; i <= 1; i += 2)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        float sx = center.X + i * 26f;
                        float sy = coreY - 6f + j * 16f;
                        Raylib.DrawTriangle(
                            new Vector2(sx + i * 14f, sy),
                            new Vector2(sx, sy - 5f),
                            new Vector2(sx, sy + 5f),
                            _accent);
                    }
                }
                break;

            case RobotType.Flail:
                // Schwere Panzerplatte mit Bolzen
                DrawRoundedRect(center.X - 16f, coreY - 14f, 32f, 28f, 4f, new Color(80, 60, 30, 255), new Color(220, 180, 60, 255), 2f);
                // Bolzen
                for (int i = -1; i <= 1; i += 2)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        float bx = center.X + i * 10f;
                        float by = coreY + j * 8f;
                        Raylib.DrawCircle((int)bx, (int)by, 3f, new Color(220, 180, 60, 255));
                    }
                }
                break;
        }
    }

    private void DrawHead(Vector2 center, float bob)
    {
        float headSize = Type == RobotType.Flail ? 40f : 34f;
        Vector2 headPos = new Vector2(center.X, center.Y - Height * 0.38f + bob);
        float neckW = 22f;
        float neckH = 18f;

        // Nacken
        DrawRoundedRect(headPos.X - neckW / 2f, headPos.Y + 8f, neckW, neckH, 6f, _dark, _accent, 2f);

        switch (Type)
        {
            case RobotType.Jaguar:
                DrawJaguarHead(headPos, headSize);
                break;
            case RobotType.Shadow:
                DrawShadowHead(headPos, headSize);
                break;
            case RobotType.Thorn:
                DrawThornHead(headPos, headSize);
                break;
            case RobotType.Flail:
                DrawFlailHead(headPos, headSize);
                break;
        }

        // Visor mit mehr Details
        DrawVisor(headPos);
    }

    private void DrawJaguarHead(Vector2 headPos, float headSize)
    {
        float dir = FacingRight ? 1f : -1f;

        // Hauptkopf
        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f, _accent);
        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f - 3f, _color);

        // Schnauze
        Raylib.DrawEllipse((int)(headPos.X + dir * 10f), (int)(headPos.Y + 6f), 14f, 10f, _light);
        Raylib.DrawCircle((int)(headPos.X + dir * 16f), (int)(headPos.Y + 6f), 4f, _dark);

        // Ohren
        Raylib.DrawTriangle(
            new Vector2(headPos.X + dir * 8f, headPos.Y - 18f),
            new Vector2(headPos.X + dir * 2f, headPos.Y - 8f),
            new Vector2(headPos.X + dir * 18f, headPos.Y - 6f),
            _accent);
        Raylib.DrawTriangle(
            new Vector2(headPos.X - dir * 8f, headPos.Y - 18f),
            new Vector2(headPos.X - dir * 2f, headPos.Y - 8f),
            new Vector2(headPos.X - dir * 18f, headPos.Y - 6f),
            _accent);

        // Wangen-Panzerung
        Raylib.DrawCircle((int)(headPos.X - dir * 10f), (int)(headPos.Y + 4f), 8f, _color);
    }

    private void DrawShadowHead(Vector2 headPos, float headSize)
        {
        float dir = FacingRight ? 1f : -1f;

        // Ninja-Kapuze (mehrere Schichten)
        DrawRoundedRect(headPos.X - headSize / 2f, headPos.Y - headSize / 2f, headSize, headSize, 8f, _accent, _dark, 2f);
        DrawRoundedRect(headPos.X - headSize / 2f + 4f, headPos.Y - headSize / 2f + 4f, headSize - 8f, headSize - 8f, 6f, _color, _accent, 1f);

        // Stirnband
        Raylib.DrawRectangle((int)(headPos.X - headSize / 2f), (int)(headPos.Y - 6f), (int)headSize, 8, _accent);

        // Schal/Helm-Detail hinten
        Raylib.DrawCircle((int)(headPos.X - dir * 18f), (int)(headPos.Y + 4f), 6f, _accent);
    }

    private void DrawThornHead(Vector2 headPos, float headSize)
    {
        float dir = FacingRight ? 1f : -1f;

        // Helm-Basis
        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f, _accent);
        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f - 4f, _color);

        // Mitteldorn
        Raylib.DrawTriangle(
            new Vector2(headPos.X, headPos.Y - 22f),
            new Vector2(headPos.X - 5f, headPos.Y - 10f),
            new Vector2(headPos.X + 5f, headPos.Y - 10f),
            _accent);

        // Seitliche Dornen
        for (int i = -1; i <= 1; i += 2)
        {
            Raylib.DrawTriangle(
                new Vector2(headPos.X + i * 16f, headPos.Y - 14f),
                new Vector2(headPos.X + i * 10f, headPos.Y - 4f),
                new Vector2(headPos.X + i * 22f, headPos.Y - 4f),
                _accent);
            Raylib.DrawTriangle(
                new Vector2(headPos.X + i * 14f, headPos.Y + 8f),
                new Vector2(headPos.X + i * 8f, headPos.Y + 16f),
                new Vector2(headPos.X + i * 20f, headPos.Y + 16f),
                _accent);
        }

        // Kiefer-Panzerung
        Raylib.DrawRectangle((int)(headPos.X - 10f), (int)(headPos.Y + 8f), 20, 10, _dark);
    }

    private void DrawFlailHead(Vector2 headPos, float headSize)
    {
        float dir = FacingRight ? 1f : -1f;

        // Schwere boxige Helm-Struktur
        DrawRoundedRect(headPos.X - headSize / 2f, headPos.Y - headSize / 2f, headSize, headSize, 6f, _accent, new Color(220, 180, 60, 255), 3f);
        DrawRoundedRect(headPos.X - headSize / 2f + 6f, headPos.Y - headSize / 2f + 6f, headSize - 12f, headSize - 12f, 4f, _color, _accent, 2f);

        // Stirnplatte
        DrawRoundedRect(headPos.X - headSize / 2f + 4f, headPos.Y - headSize / 2f + 4f, headSize - 8f, 16f, 3f, _dark, new Color(220, 180, 60, 255), 1f);

        // Seitliche Ohren/Sensoren
        Raylib.DrawRectangle((int)(headPos.X + dir * (headSize / 2f - 2f)), (int)(headPos.Y - 8f), 10, 16, _dark);
        Raylib.DrawCircle((int)(headPos.X + dir * (headSize / 2f + 4f)), (int)headPos.Y, 5f, new Color(255, 50, 50, 255));

        // Kinn-Panzerung
        DrawRoundedRect(headPos.X - 12f, headPos.Y + 8f, 24f, 12f, 4f, _accent, new Color(220, 180, 60, 255), 1f);
    }

    private void DrawVisor(Vector2 headPos)
    {
        float visorW = Type == RobotType.Flail ? 20f : 24f;
        float visorH = 10f;
        float visorX = FacingRight ? headPos.X + 2f : headPos.X - visorW - 2f;
        Color visorColor = Type == RobotType.Shadow ? new Color(200, 50, 255, 255) : Color.SKYBLUE;
        Color visorGlow = Type == RobotType.Shadow ? new Color(200, 50, 255, 120) : new Color(100, 200, 255, 120);

        // Visor-Rahmen
        Raylib.DrawRectangle((int)(visorX - 2f), (int)(headPos.Y - 6f), (int)(visorW + 4f), (int)(visorH + 4f), _dark);
        // Visor-Glas
        Raylib.DrawRectangle((int)visorX, (int)(headPos.Y - 4f), (int)visorW, (int)visorH, visorColor);
        // Visor-Glühen
        Raylib.DrawRectangle((int)(visorX + 2f), (int)(headPos.Y - 2f), (int)(visorW - 4f), (int)(visorH - 4f), visorGlow);
        // Reflexion
        Raylib.DrawRectangle((int)(visorX + 4f), (int)(headPos.Y - 3f), (int)(visorW * 0.3f), 2, Color.WHITE);
    }

    private void DrawLimbs(Vector2 center, float bob, float time)
    {
        float armW = Type == RobotType.Flail ? 20f : 14f;
        float armH = Type == RobotType.Flail ? 52f : 46f;
        float legW = Type == RobotType.Thorn ? 24f : 18f;
        float legH = Type == RobotType.Thorn ? 58f : 52f;

        float armExtension = CurrentAttackType == AttackType.Punch ? 45f : (CurrentAttackType == AttackType.Kick ? 22f : 28f);
        float armOffset = State == RobotState.Attack ? (FacingRight ? armExtension : -armExtension) : (FacingRight ? 20f : -20f);
        float armAngle = State == RobotState.Attack ? (FacingRight ? -0.9f : 0.9f) : 0f;

        Vector2 shoulderY = new Vector2(center.X, center.Y - Height * 0.26f + bob);

        // Arme mit Schulter, Oberarm, Ellenbogen, Unterarm und Hand
        Vector2 leftArmShoulder = new Vector2(shoulderY.X - armOffset, shoulderY.Y);
        Vector2 rightArmShoulder = new Vector2(shoulderY.X + armOffset, shoulderY.Y);

        DrawArm(leftArmShoulder, armW, armH, armAngle, true, time);
        DrawArm(rightArmShoulder, armW, armH, -armAngle, false, time);

        // Beine mit Hüfte, Oberschenkel, Knie, Unterschenkel und Fuß
        Vector2 hipY = new Vector2(center.X, center.Y + Height * 0.18f + bob);
        Vector2 leftLegHip = new Vector2(hipY.X - 20f, hipY.Y);
        Vector2 rightLegHip = new Vector2(hipY.X + 20f, hipY.Y);

        float legBob = State == RobotState.Walk ? MathF.Sin(time * 15f + MathF.PI) * 8f : 0f;
        float kickExtension = CurrentAttackType == AttackType.Kick && State == RobotState.Attack ? 28f : 0f;
        float kickDir = FacingRight ? 1f : -1f;

        DrawLeg(leftLegHip, legW, legH, legBob, false, time);
        DrawLeg(new Vector2(rightLegHip.X + kickDir * kickExtension, rightLegHip.Y - legBob), legW, legH + kickExtension * 0.3f, -legBob, kickExtension > 0, time);
    }

    private void DrawArm(Vector2 shoulder, float w, float h, float angle, bool isLeft, float time)
    {
        // Schultergelenk
        DrawCircle3D(shoulder.X, shoulder.Y, 12f, _accent, _dark);

        // Oberarm
        Vector2 elbow = new Vector2(shoulder.X, shoulder.Y + h * 0.45f);
        DrawRoundedLimb(shoulder, elbow, w, angle, _color, _accent);

        // Ellenbogen
        DrawCircle3D(elbow.X, elbow.Y, 10f, _accent, _dark);

        // Unterarm
        Vector2 hand = new Vector2(elbow.X + MathF.Sin(angle) * h * 0.4f, elbow.Y + MathF.Cos(angle) * h * 0.4f);
        DrawRoundedLimb(elbow, hand, w * 0.85f, angle, _color, _accent);

        // Hand
        DrawHand(hand, isLeft);
    }

    private void DrawLeg(Vector2 hip, float w, float h, float bob, bool isKicking, float time)
    {
        // Hüftgelenk
        DrawCircle3D(hip.X, hip.Y, 12f, _accent, _dark);

        // Oberschenkel
        Vector2 knee = new Vector2(hip.X + bob * 0.3f, hip.Y + h * 0.5f);
        DrawRoundedLimb(hip, knee, w, bob * 0.01f, _color, _accent);

        // Knie
        DrawCircle3D(knee.X, knee.Y, 11f, _accent, _dark);

        // Unterschenkel
        Vector2 foot = new Vector2(knee.X - bob * 0.2f, knee.Y + h * 0.5f);
        if (isKicking)
            foot = new Vector2(knee.X + (FacingRight ? 35f : -35f), knee.Y + 10f);
        DrawRoundedLimb(knee, foot, w * 0.85f, isKicking ? (FacingRight ? -0.4f : 0.4f) : 0f, _color, _accent);

        // Fuß
        DrawFoot(foot, isKicking);
    }

    private void DrawHand(Vector2 pos, bool isLeft)
    {
        float dir = FacingRight ? 1f : -1f;
        float handW = 18f;
        float handH = 16f;

        DrawRoundedRect(pos.X - handW / 2f, pos.Y - handH / 2f, handW, handH, 5f, _dark, _accent, 2f);

        // Finger/Claws
        for (int i = -1; i <= 1; i++)
        {
            float fx = pos.X + i * 5f;
            float fy = pos.Y + handH / 2f;
            Raylib.DrawRectangle((int)(fx - 2f), (int)fy, 4, 8, _accent);
            // Fingerspitze
            Raylib.DrawTriangle(
                new Vector2(fx, fy + 10f),
                new Vector2(fx - 3f, fy + 6f),
                new Vector2(fx + 3f, fy + 6f),
                _light);
        }

        // Handrücken-Detail
        Raylib.DrawCircle((int)pos.X, (int)(pos.Y - 2f), 4f, _light);
    }

    private void DrawFoot(Vector2 pos, bool isKicking)
    {
        float footW = Type == RobotType.Flail ? 34f : 28f;
        float footH = 14f;
        float dir = FacingRight ? 1f : -1f;

        if (isKicking)
        {
            // Fuß als Waffe
            DrawRoundedRect(pos.X - footW / 2f, pos.Y - footH / 2f, footW, footH, 6f, _accent, _light, 2f);
            // Stollen
            for (int i = 0; i < 3; i++)
            {
                float sx = pos.X - footW / 2f + 6f + i * 10f;
                Raylib.DrawRectangle((int)sx, (int)(pos.Y + footH / 2f), 6, 6, _dark);
            }
        }
        else
        {
            // Normaler Fuß
            DrawRoundedRect(pos.X - footW / 2f, pos.Y - footH / 2f, footW, footH, 5f, _dark, _accent, 2f);
            // Sohle
            Raylib.DrawRectangle((int)(pos.X - footW / 2f + 3f), (int)(pos.Y + 2f), (int)(footW - 6f), 5, _accent);
            // Zehen/Details
            for (int i = -1; i <= 1; i++)
            {
                Raylib.DrawCircle((int)(pos.X + i * 8f), (int)(pos.Y - 2f), 3f, _light);
            }
        }
    }

    private void DrawWeapon(Vector2 center, float bob)
    {
        float reach = CurrentAttackType == AttackType.Kick ? 0f : 60f;
        if (reach <= 0f) return;

        float dir = FacingRight ? 1f : -1f;
        float handY = center.Y - Height * 0.05f + bob;
        float handX = FacingRight ? center.X + Width / 2f - 5f : center.X - Width / 2f + 5f;

        Color weaponColor = Type switch
        {
            RobotType.Jaguar => Color.ORANGE,
            RobotType.Shadow => Color.PURPLE,
            RobotType.Thorn => Color.GREEN,
            RobotType.Flail => Color.GOLD,
            _ => Color.WHITE
        };

        // Waffengriff
        DrawRoundedRect(handX - 6f, handY - 6f, 12f, 24f, 3f, _dark, _accent, 1f);

        // Waffenkörper
        float weaponX = FacingRight ? handX : handX - reach;
        float weaponY = handY - 7f;
        DrawRoundedRect(weaponX, weaponY, reach, 14f, 4f, weaponColor, Color.WHITE, 2f);

        // Mittelstreifen
        Raylib.DrawRectangle((int)(weaponX + reach * 0.35f), (int)(weaponY + 2f), (int)(reach * 0.3f), 10, _light);

        // Typ-spezifische Waffenform
        switch (Type)
        {
            case RobotType.Jaguar:
                // Klauen-Klinge
                Raylib.DrawTriangle(
                    new Vector2(FacingRight ? weaponX + reach : weaponX, weaponY + 7f),
                    new Vector2(FacingRight ? weaponX + reach + 16f : weaponX - 16f, weaponY),
                    new Vector2(FacingRight ? weaponX + reach + 16f : weaponX - 16f, weaponY + 14f),
                    Color.ORANGE);
                Raylib.DrawTriangle(
                    new Vector2(FacingRight ? weaponX + reach + 8f : weaponX - 8f, weaponY + 7f),
                    new Vector2(FacingRight ? weaponX + reach + 20f : weaponX - 20f, weaponY + 2f),
                    new Vector2(FacingRight ? weaponX + reach + 20f : weaponX - 20f, weaponY + 12f),
                    _light);
                break;

            case RobotType.Shadow:
                // Dreifach-Klingen
                for (int i = 0; i < 3; i++)
                {
                    float bladeX = FacingRight ? weaponX + reach + i * 10f : weaponX - i * 10f;
                    float by = weaponY + 2f + i * 3f;
                    Raylib.DrawLine((int)bladeX, (int)by, (int)(FacingRight ? bladeX + 12f : bladeX - 12f), (int)(by + 10f), Color.WHITE);
                    Raylib.DrawLine((int)bladeX, (int)(by + 1f), (int)(FacingRight ? bladeX + 12f : bladeX - 12f), (int)(by + 11f), _light);
                }
                break;

            case RobotType.Thorn:
                // Dornenreihe
                for (int i = 0; i < 5; i++)
                {
                    float spikeX = FacingRight ? weaponX + 8f + i * 12f : weaponX + reach - 8f - i * 12f;
                    Raylib.DrawTriangle(
                        new Vector2(spikeX, weaponY - 6f),
                        new Vector2(spikeX - 4f, weaponY + 8f),
                        new Vector2(spikeX + 4f, weaponY + 8f),
                        new Color(160, 200, 40, 255));
                    Raylib.DrawTriangle(
                        new Vector2(spikeX, weaponY + 20f),
                        new Vector2(spikeX - 4f, weaponY + 6f),
                        new Vector2(spikeX + 4f, weaponY + 6f),
                        new Color(160, 200, 40, 255));
                }
                break;

            case RobotType.Flail:
                // Kette + Kugel
                float chainStartX = FacingRight ? weaponX + reach : weaponX;
                float ballX = FacingRight ? chainStartX + 22f : chainStartX - 22f;
                // Kettenglieder
                for (int i = 0; i < 4; i++)
                {
                    float cx = FacingRight ? chainStartX + i * 5f : chainStartX - i * 5f;
                    Raylib.DrawCircle((int)cx, (int)(weaponY + 7f), 3f, Color.GRAY);
                }
                // Kugel
                Raylib.DrawCircle((int)ballX, (int)(weaponY + 7f), 12f, Color.GOLD);
                Raylib.DrawCircle((int)ballX, (int)(weaponY + 7f), 12f, Color.WHITE);
                Raylib.DrawCircle((int)ballX, (int)(weaponY + 7f), 8f, _light);
                // Stacheln auf der Kugel
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * MathF.PI / 2f;
                    float sx = ballX + MathF.Cos(angle) * 14f;
                    float sy = weaponY + 7f + MathF.Sin(angle) * 14f;
                    Raylib.DrawCircle((int)sx, (int)sy, 3f, _accent);
                }
                break;
        }
    }

    private void DrawWeaponBack(Vector2 center, float bob)
    {
        // Rücken-Waffe/Detail für Shadow und Thorn
        if (Type != RobotType.Shadow && Type != RobotType.Thorn) return;

        float dir = FacingRight ? -1f : 1f;
        float x = center.X + dir * Width * 0.4f;
        float y = center.Y - Height * 0.1f + bob;

        if (Type == RobotType.Shadow)
        {
            // Zweite Klinge am Rücken
            Raylib.DrawRectangle((int)(x - 4f), (int)(y - 30f), 8, 50, _accent);
            Raylib.DrawTriangle(
                new Vector2(x, y + 28f),
                new Vector2(x - 6f, y + 18f),
                new Vector2(x + 6f, y + 18f),
                _light);
        }
        else if (Type == RobotType.Thorn)
        {
            // Rückendornen
            for (int i = 0; i < 3; i++)
            {
                float dy = y - 20f + i * 18f;
                Raylib.DrawTriangle(
                    new Vector2(x + dir * 12f, dy),
                    new Vector2(x, dy - 5f),
                    new Vector2(x, dy + 5f),
                    _accent);
            }
        }
    }

    private void DrawTypeDetailsBack(Vector2 center, float bob, float time)
    {
        float dir = FacingRight ? -1f : 1f;

        if (IsPyros)
        {
            // Pyros hat keine hinteren Details
            return;
        }

        switch (Type)
        {
            case RobotType.Jaguar:
                // Schwanz mit mehreren Segmenten
                float tailBaseX = center.X + dir * Width * 0.4f;
                float tailBaseY = center.Y + Height * 0.15f + bob;
                float sway = MathF.Sin(time * 8f) * 8f;

                Vector2[] tailPoints = new Vector2[5];
                tailPoints[0] = new Vector2(tailBaseX, tailBaseY);
                tailPoints[1] = new Vector2(tailBaseX + dir * 15f + sway * 0.3f, tailBaseY + 15f);
                tailPoints[2] = new Vector2(tailBaseX + dir * 28f + sway * 0.6f, tailBaseY + 30f);
                tailPoints[3] = new Vector2(tailBaseX + dir * 38f + sway * 0.9f, tailBaseY + 45f);
                tailPoints[4] = new Vector2(tailBaseX + dir * 42f + sway, tailBaseY + 58f);

                for (int i = 0; i < tailPoints.Length - 1; i++)
                {
                    Raylib.DrawLineEx(tailPoints[i], tailPoints[i + 1], 6f - i, _accent);
                }
                // Schwanzspitze
                Raylib.DrawTriangle(
                    tailPoints[4],
                    new Vector2(tailPoints[4].X - dir * 8f, tailPoints[4].Y - 6f),
                    new Vector2(tailPoints[4].X - dir * 8f, tailPoints[4].Y + 6f),
                    _light);
                break;

            case RobotType.Shadow:
                // Schal mit Wellenform
                float scarfBaseX = center.X + dir * Width * 0.42f;
                float scarfBaseY = center.Y - Height * 0.08f + bob;
                float scarfSway = MathF.Sin(time * 10f) * 10f;

                Vector2[] scarfPoints = new Vector2[6];
                scarfPoints[0] = new Vector2(scarfBaseX, scarfBaseY);
                for (int i = 1; i < scarfPoints.Length; i++)
                {
                    float t = i / (float)(scarfPoints.Length - 1);
                    scarfPoints[i] = new Vector2(
                        scarfBaseX + dir * (20f + i * 10f) + MathF.Sin(time * 10f + i * 0.8f) * 8f,
                        scarfBaseY + i * 8f + scarfSway * t);
                }
                for (int i = 0; i < scarfPoints.Length - 1; i++)
                {
                    Raylib.DrawLineEx(scarfPoints[i], scarfPoints[i + 1], 7f, new Color(180, 40, 220, 180));
                    Raylib.DrawLineEx(scarfPoints[i], scarfPoints[i + 1], 3f, new Color(220, 100, 255, 120));
                }
                break;
        }
    }

    private void DrawTypeDetailsFront(Vector2 center, float bob, float time)
    {
        float dir = FacingRight ? 1f : -1f;

        if (IsPyros)
        {
            // Pyros hat keine vorderen Details außer den Flammen, die separat gezeichnet werden
            return;
        }

        switch (Type)
        {
            case RobotType.Thorn:
                // Schulterdornen
                for (int i = -1; i <= 1; i += 2)
                {
                    float sx = center.X + i * Width * 0.48f;
                    float sy = center.Y - Height * 0.22f + bob;
                    Raylib.DrawTriangle(
                        new Vector2(sx + i * 14f, sy - 8f),
                        new Vector2(sx + i * 6f, sy + 4f),
                        new Vector2(sx - i * 4f, sy - 2f),
                        _accent);
                    Raylib.DrawTriangle(
                        new Vector2(sx + i * 12f, sy + 12f),
                        new Vector2(sx + i * 4f, sy + 2f),
                        new Vector2(sx - i * 6f, sy + 8f),
                        _accent);
                }
                // Arm-Dornen
                for (int i = -1; i <= 1; i += 2)
                {
                    float ax = center.X + i * 28f;
                    float ay = center.Y - Height * 0.05f + bob;
                    Raylib.DrawTriangle(
                        new Vector2(ax + i * 10f, ay),
                        new Vector2(ax, ay - 5f),
                        new Vector2(ax, ay + 5f),
                        _accent);
                }
                break;

            case RobotType.Flail:
                // Schwere Schulterpolster
                for (int i = -1; i <= 1; i += 2)
                {
                    float sx = center.X + i * Width * 0.5f;
                    float sy = center.Y - Height * 0.26f + bob;
                    DrawRoundedRect(sx - 12f, sy - 10f, 24f, 36f, 6f, _accent, new Color(220, 180, 60, 255), 2f);
                    // Bolzen
                    Raylib.DrawCircle((int)(sx - 6f), (int)(sy + 4f), 3f, new Color(220, 180, 60, 255));
                    Raylib.DrawCircle((int)(sx + 6f), (int)(sy + 4f), 3f, new Color(220, 180, 60, 255));
                }
                // Gürtel
                DrawRoundedRect(center.X - Width * 0.38f, center.Y + Height * 0.05f + bob, Width * 0.76f, 16f, 5f, _dark, new Color(220, 180, 60, 255), 2f);
                // Gürtelschnalle
                Raylib.DrawCircle((int)center.X, (int)(center.Y + Height * 0.13f + bob), 10f, new Color(220, 180, 60, 255));
                Raylib.DrawCircle((int)center.X, (int)(center.Y + Height * 0.13f + bob), 6f, _dark);
                break;

            case RobotType.Jaguar:
                // Seiten-Lüftungsöffnungen
                for (int i = -1; i <= 1; i += 2)
                {
                    float sx = center.X + i * Width * 0.38f;
                    float sy = center.Y - Height * 0.05f + bob;
                    Raylib.DrawRectangle((int)(sx - 6f), (int)(sy - 12f), 12, 24, _dark);
                    for (int j = 0; j < 4; j++)
                    {
                        Raylib.DrawLine((int)(sx - 5f), (int)(sy - 8f + j * 6f), (int)(sx + 5f), (int)(sy - 8f + j * 6f), _accent);
                    }
                }
                break;

            case RobotType.Shadow:
                // Gürtel mit Werkzeugtaschen
                DrawRoundedRect(center.X - Width * 0.35f, center.Y + Height * 0.08f + bob, Width * 0.7f, 14f, 4f, _dark, _accent, 1f);
                for (int i = -1; i <= 1; i += 2)
                {
                    float px = center.X + i * 22f;
                    float py = center.Y + Height * 0.15f + bob;
                    Raylib.DrawRectangle((int)(px - 8f), (int)(py - 8f), 16, 16, _accent);
                    Raylib.DrawRectangle((int)(px - 6f), (int)(py - 6f), 12, 12, _dark);
                }
                break;
        }
    }

    private void DrawShield()
    {
        float dir = FacingRight ? -1f : 1f;
        float x = Position.X + dir * (Width / 2f + 14f);
        float y = Position.Y - Height * 0.7f;
        float w = 16f;
        float h = Height * 0.65f;

        // Schild mit abgerundeten Ecken
        DrawRoundedRect(x, y, w, h, 6f, new Color(100, 200, 255, 160), new Color(150, 230, 255, 200), 2f);
        // Energie-Ringe
        Raylib.DrawRectangle((int)(x + 3f), (int)(y + 8f), (int)(w - 6f), 4, new Color(200, 240, 255, 180));
        Raylib.DrawRectangle((int)(x + 3f), (int)(y + h - 12f), (int)(w - 6f), 4, new Color(200, 240, 255, 180));
        // Glühen
        Raylib.DrawRectangle((int)(x - 4f), (int)y, (int)(w + 8f), (int)h, new Color(100, 200, 255, 40));
    }

    // ========================
    // PYROS-SPEZIFISCHE ZEICHNUNG
    // ========================

    private void DrawPyrosBody(Vector2 center, float bob)
    {
        float torsoW = Width * 0.95f;

        // Langes Gewand / Unterteil
        DrawRoundedRect(center.X - torsoW * 0.4f, center.Y + Height * 0.05f + bob, torsoW * 0.8f, Height * 0.55f, 14f, _dark, _accent, 2f);

        // Gewand-Falten
        for (int i = -2; i <= 2; i++)
        {
            float fx = center.X + i * 14f;
            Raylib.DrawLine((int)fx, (int)(center.Y + Height * 0.1f + bob), (int)fx, (int)(center.Y + Height * 0.55f + bob), new Color(_accent.r, _accent.g, _accent.b, (byte)120));
        }

        // Hüfte
        DrawRoundedRect(center.X - torsoW * 0.45f, center.Y - Height * 0.05f + bob, torsoW * 0.9f, Height * 0.22f, 12f, _color, _accent, 3f);

        // Feuerdüsen an der Hüfte
        float dir = FacingRight ? 1f : -1f;
        for (int i = -1; i <= 1; i += 2)
        {
            float dx = center.X + i * torsoW * 0.35f;
            float dy = center.Y + Height * 0.02f + bob;
            DrawRoundedRect(dx - 8f, dy - 6f, 16f, 18f, 4f, _accent, _light, 2f);
            Raylib.DrawCircle((int)dx, (int)(dy + 8f), 5f, Color.ORANGE);
        }

        // Massiver Brustkorb
        DrawRoundedRect(center.X - torsoW * 0.5f, center.Y - Height * 0.38f + bob, torsoW, Height * 0.42f, 16f, _color, _accent, 3f);

        // Brust-Highlight
        DrawRoundedRect(center.X - torsoW * 0.38f, center.Y - Height * 0.34f + bob, torsoW * 0.76f, Height * 0.18f, 10f, _light, Color.WHITE, 1f);

        // Reaktor-Kern (glühend heiß)
        float coreY = center.Y - Height * 0.18f + bob;
        Raylib.DrawCircle((int)center.X, (int)coreY, 18f, _accent);
        Raylib.DrawCircle((int)center.X, (int)coreY, 13f, Color.ORANGE);
        Raylib.DrawCircle((int)center.X, (int)coreY, 8f, Color.YELLOW);
        Raylib.DrawCircle((int)center.X, (int)coreY, 4f, Color.WHITE);

        // Schultergelenke (breit)
        float shoulderY = center.Y - Height * 0.32f + bob;
        DrawCircle3D(center.X - torsoW * 0.52f, shoulderY, 16f, _accent, _dark);
        DrawCircle3D(center.X + torsoW * 0.52f, shoulderY, 16f, _accent, _dark);
    }

    private void DrawPyrosHead(Vector2 center, float bob)
    {
        float headSize = 36f;
        Vector2 headPos = new Vector2(center.X, center.Y - Height * 0.45f + bob);
        float dir = FacingRight ? 1f : -1f;

        // Nacken
        DrawRoundedRect(headPos.X - 12f, headPos.Y + 12f, 24f, 18f, 6f, _dark, _accent, 2f);

        // Wespenkopf - Hauptform (oval)
        Raylib.DrawEllipse((int)headPos.X, (int)headPos.Y, headSize * 0.6f, headSize * 0.7f, _color);
        Raylib.DrawEllipseLines((int)headPos.X, (int)headPos.Y, headSize * 0.6f, headSize * 0.7f, _accent);

        // Facettenaugen
        Raylib.DrawCircle((int)(headPos.X + dir * 10f), (int)(headPos.Y - 4f), 8f, new Color(255, 200, 0, 255));
        Raylib.DrawCircle((int)(headPos.X + dir * 10f), (int)(headPos.Y - 4f), 5f, Color.ORANGE);
        Raylib.DrawCircle((int)(headPos.X + dir * 12f), (int)(headPos.Y - 6f), 2f, Color.WHITE);

        // Zweites Auge
        Raylib.DrawCircle((int)(headPos.X + dir * 4f), (int)(headPos.Y - 10f), 5f, new Color(255, 200, 0, 255));

        // Mandibeln / Schnauze
        Raylib.DrawTriangle(
            new Vector2(headPos.X + dir * 16f, headPos.Y + 6f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 2f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 10f),
            _accent);
        Raylib.DrawTriangle(
            new Vector2(headPos.X + dir * 16f, headPos.Y + 14f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 10f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 18f),
            _accent);

        // Stachel hinten
        Raylib.DrawTriangle(
            new Vector2(headPos.X - dir * 22f, headPos.Y),
            new Vector2(headPos.X - dir * 10f, headPos.Y - 6f),
            new Vector2(headPos.X - dir * 10f, headPos.Y + 6f),
            _light);

        // Fühler
        float antennaSway = MathF.Sin((float)Raylib.GetTime() * 12f) * 4f;
        Raylib.DrawLine((int)(headPos.X - dir * 4f), (int)(headPos.Y - 18f), (int)(headPos.X + dir * 8f + antennaSway), (int)(headPos.Y - 32f), _accent);
        Raylib.DrawCircle((int)(headPos.X + dir * 8f + antennaSway), (int)(headPos.Y - 32f), 3f, _light);
    }

    private void DrawPyrosLimbs(Vector2 center, float bob, float time)
    {
        float shoulderY = center.Y - Height * 0.32f + bob;
        float armExtension = State == RobotState.Attack && CurrentAttackType == AttackType.Punch ? 50f : 28f;
        float armOffset = State == RobotState.Attack ? (FacingRight ? armExtension : -armExtension) : (FacingRight ? 24f : -24f);
        float armAngle = State == RobotState.Attack ? (FacingRight ? -1.0f : 1.0f) : 0f;

        // Beim Laufen Arme entgegengesetzt zum Bein-Bob schwingen lassen
        float walkArmBob = State == RobotState.Walk ? MathF.Sin(time * 15f + MathF.PI) * 10f : 0f;

        Vector2 leftShoulder = new Vector2(center.X - armOffset, shoulderY + walkArmBob);
        Vector2 rightShoulder = new Vector2(center.X + armOffset, shoulderY - walkArmBob);

        DrawPyrosArm(leftShoulder, armAngle, true, time);
        DrawPyrosArm(rightShoulder, -armAngle, false, time);
    }

    private void DrawPyrosArm(Vector2 shoulder, float angle, bool isLeft, float time)
    {
        // Schultergelenk
        DrawCircle3D(shoulder.X, shoulder.Y, 14f, _accent, _dark);

        // Dünner Oberarm
        Vector2 elbow = new Vector2(shoulder.X + MathF.Sin(angle) * 18f, shoulder.Y + 28f);
        DrawRoundedLimb(shoulder, elbow, 10f, angle, _color, _accent);

        // Ellenbogen
        DrawCircle3D(elbow.X, elbow.Y, 9f, _accent, _dark);

        // Starker Unterarm mit Klaue
        Vector2 hand = new Vector2(elbow.X + MathF.Sin(angle) * 28f, elbow.Y + MathF.Cos(angle) * 22f);
        DrawRoundedLimb(elbow, hand, 16f, angle, _color, _accent);

        // Klaue-Hand
        DrawPyrosClaw(hand);
    }

    private void DrawPyrosClaw(Vector2 hand)
    {
        float dir = FacingRight ? 1f : -1f;

        // Handbasis
        DrawRoundedRect(hand.X - 10f, hand.Y - 10f, 20f, 20f, 5f, _dark, _accent, 2f);

        // Klauen
        for (int i = -1; i <= 1; i++)
        {
            float cx = hand.X + i * 6f + dir * 4f;
            float cy = hand.Y + 8f;
            Raylib.DrawTriangle(
                new Vector2(cx + dir * 10f, cy - 4f),
                new Vector2(cx - dir * 3f, cy + 2f),
                new Vector2(cx - dir * 3f, cy - 10f),
                _light);
        }
    }

    private void DrawPyrosFlame(Vector2 center, float bob)
    {
        if (CurrentAttackType != AttackType.Kick || State != RobotState.Attack)
            return;

        float dir = FacingRight ? 1f : -1f;
        float nozzleY = center.Y + Height * 0.02f + bob;
        float nozzleX = center.X + dir * Width * 0.35f;
        float progress = 1f - (_stateTimer / (0.55f * (CurrentAttackVariant == AttackVariant.Forward ? 0.85f : (CurrentAttackVariant == AttackVariant.Backward ? 1.2f : 1.0f))));
        progress = Math.Clamp(progress, 0f, 1f);

        for (int i = -1; i <= 1; i += 2)
        {
            float nx = center.X + i * dir * Width * 0.35f;
            float flameReach = 40f + progress * 90f;
            float flameHeight = 25f + progress * 35f;

            // Äußere Flamme (rot)
            Raylib.DrawEllipse((int)(nx + dir * flameReach * 0.5f), (int)nozzleY, flameReach * 0.5f, flameHeight * 0.5f, new Color(255, 60, 0, 200));
            // Mittlere Flamme (orange)
            Raylib.DrawEllipse((int)(nx + dir * flameReach * 0.55f), (int)nozzleY, flameReach * 0.35f, flameHeight * 0.35f, new Color(255, 140, 0, 220));
            // Innere Flamme (gelb/weiß)
            Raylib.DrawEllipse((int)(nx + dir * flameReach * 0.6f), (int)nozzleY, flameReach * 0.18f, flameHeight * 0.18f, new Color(255, 255, 100, 240));

            // Funken
            for (int j = 0; j < 5; j++)
            {
                float fx = nx + dir * (10f + j * 18f) + (MathF.Sin((float)Raylib.GetTime() * 20f + j * 1.5f) * 6f);
                float fy = nozzleY + MathF.Cos((float)Raylib.GetTime() * 18f + j * 2f) * 10f;
                Raylib.DrawCircle((int)fx, (int)fy, 2f + j * 0.5f, new Color(255, 200, 50, 220));
            }
        }
    }

    // ========================
    // HILFSMETHODEN FÜR SHAPES
    // ========================

    private void DrawRoundedRect(float x, float y, float w, float h, float radius, Color fill, Color border, float borderThickness)
    {
        // Hauptrechteck
        Raylib.DrawRectangle((int)x, (int)(y + radius), (int)w, (int)(h - 2 * radius), fill);
        Raylib.DrawRectangle((int)(x + radius), (int)y, (int)(w - 2 * radius), (int)h, fill);

        // Ecken als Kreise
        float r = Math.Min(radius, Math.Min(w / 2f, h / 2f));
        Raylib.DrawCircle((int)(x + r), (int)(y + r), r, fill);
        Raylib.DrawCircle((int)(x + w - r), (int)(y + r), r, fill);
        Raylib.DrawCircle((int)(x + r), (int)(y + h - r), r, fill);
        Raylib.DrawCircle((int)(x + w - r), (int)(y + h - r), r, fill);

        // Rand
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

    private void DrawCircle3D(float x, float y, float radius, Color main, Color shadow)
    {
        // Schatten für Tiefe
        Raylib.DrawCircle((int)(x + 2f), (int)(y + 2f), radius, shadow);
        // Hauptkreis
        Raylib.DrawCircle((int)x, (int)y, radius, main);
        // Highlight
        Raylib.DrawCircle((int)(x - radius * 0.3f), (int)(y - radius * 0.3f), radius * 0.25f, new Color(255, 255, 255, 120));
        // Rand
        Raylib.DrawCircleLines((int)x, (int)y, radius, _accent);
    }

    private void DrawRoundedLimb(Vector2 start, Vector2 end, float width, float angle, Color fill, Color border)
    {
        Vector2 direction = end - start;
        float length = direction.Length();
        if (length < 1f) length = 1f;

        // Richtung normalisieren
        Vector2 normal = new Vector2(-direction.Y, direction.X) / length;

        // Eckpunkte des gerundeten Glieds
        Vector2 p1 = start + normal * width * 0.5f;
        Vector2 p2 = start - normal * width * 0.5f;
        Vector2 p3 = end - normal * width * 0.4f;
        Vector2 p4 = end + normal * width * 0.4f;

        // Füllung als Polygon
        Raylib.DrawTriangle(p1, p2, p3, fill);
        Raylib.DrawTriangle(p1, p3, p4, fill);

        // Runde Enden
        Raylib.DrawCircle((int)start.X, (int)start.Y, width * 0.5f, fill);
        Raylib.DrawCircle((int)end.X, (int)end.Y, width * 0.4f, fill);

        // Rand
        Raylib.DrawLineEx(p1, p4, 2f, border);
        Raylib.DrawLineEx(p4, p3, 2f, border);
        Raylib.DrawLineEx(p3, p2, 2f, border);
        Raylib.DrawLineEx(p2, p1, 2f, border);
    }
}
