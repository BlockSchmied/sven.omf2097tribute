using Raylib_cs;
using System.Numerics;

namespace OMF2097.Robots;

/// <summary>
/// PYROS – schwebende Wespe mit Feuerdüsen und Flammenangriff.
/// Zustände: Idle, Walk, Jump, Attack, Hit, Block.
/// Besonderheiten: Keine Beine, Düsenstoß in der Luft, Flammen-Kick.
/// </summary>
public class PyrosRobot : Robot
{
    public PyrosRobot(bool isPlayer1, Vector2 startPosition) : base(RobotType.Pyros, isPlayer1, startPosition) { }

    protected override void InitializeColors()
    {
        Color = new Color(255, 140, 20, 255);
        Accent = new Color(139, 0, 0, 255);
        Dark = new Color(80, 20, 10, 255);
        Light = new Color(255, 220, 80, 255);
    }

    public override float Width => 70f;
    public override float Height => 110f;

    protected override float GetAttackDuration(AttackType attackType) =>
        attackType == AttackType.Kick ? 0.55f : 0.35f;

    protected override float GetAttackDurationMultiplier(AttackType attackType) =>
        CurrentAttackVariant switch
        {
            AttackVariant.Forward => attackType == AttackType.Kick ? 0.85f : 0.75f,
            AttackVariant.Backward => attackType == AttackType.Kick ? 1.2f : 1.35f,
            _ => 1.0f
        };

    protected override float GetHitboxActiveRatio() =>
        CurrentAttackType == AttackType.Kick ? 0.75f : 0.55f;

    protected override int CalculateDamage(AttackType attackType)
    {
        int baseDamage = attackType == AttackType.Kick ? 14 : 10;
        int typeBonus = attackType == AttackType.Kick ? 5 : 3;
        int variantDamage = CurrentAttackVariant switch
        {
            AttackVariant.Forward => -2,
            AttackVariant.Backward => 4,
            _ => 0
        };
        return Math.Max(1, baseDamage + typeBonus + variantDamage);
    }

    public override Rectangle AttackHitbox
    {
        get
        {
            if (CurrentAttackType == AttackType.Kick)
            {
                float reach = 130f;
                float height = 60f;
                float yOffset = Height * 0.35f;
                float x = FacingRight ? Position.X + Width / 2f - 10f : Position.X - Width / 2f - reach + 10f;
                return new Rectangle(x, Position.Y - yOffset, reach, height);
            }
            return base.AttackHitbox;
        }
    }

    protected override void OnSpecialAttack(AttackType attackType)
    {
        if (attackType == AttackType.Kick && State == RobotState.Attack && Position.Y < FloorY - 1f)
        {
            float pushDir = FacingRight ? 1f : -1f;
            Velocity = new Vector2(-pushDir * 180f, -420f);
        }
    }

    protected override void DrawShadow(Vector2 center)
    {
        float shadowW = Width * 1.4f;
        float shadowH = 22f;
        float alpha = 1f - Math.Clamp((FloorY - Position.Y) / 200f, 0f, 0.6f);
        Color shadowColor = new Color(0, 0, 0, (int)(120 * alpha));
        Raylib.DrawEllipse((int)Position.X, (int)(Position.Y - 4f), shadowW / 2f, shadowH / 2f, shadowColor);
    }

    protected override void DrawRobot(Vector2 center, float bob, float time)
    {
        DrawBody(center, bob);
        DrawHead(center, bob);
        DrawLimbs(center, bob, time);
        DrawFlame(center, bob);
    }

    private void DrawBody(Vector2 center, float bob)
    {
        float torsoW = Width * 0.95f;
        float dir = FacingRight ? 1f : -1f;

        DrawRoundedRect(center.X - torsoW * 0.4f, center.Y + Height * 0.05f + bob, torsoW * 0.8f, Height * 0.55f, 14f, Dark, Accent, 2f);
        for (int i = -2; i <= 2; i++)
        {
            float fx = center.X + i * 14f;
            Raylib.DrawLine((int)fx, (int)(center.Y + Height * 0.1f + bob), (int)fx, (int)(center.Y + Height * 0.55f + bob), new Color(Accent.r, Accent.g, Accent.b, (byte)120));
        }

        DrawRoundedRect(center.X - torsoW * 0.45f, center.Y - Height * 0.05f + bob, torsoW * 0.9f, Height * 0.22f, 12f, Color, Accent, 3f);
        for (int i = -1; i <= 1; i += 2)
        {
            float dx = center.X + i * torsoW * 0.35f;
            float dy = center.Y + Height * 0.02f + bob;
            DrawRoundedRect(dx - 8f, dy - 6f, 16f, 18f, 4f, Accent, Light, 2f);
            Raylib.DrawCircle((int)dx, (int)(dy + 8f), 5f, Color.ORANGE);
        }

        DrawRoundedRect(center.X - torsoW * 0.5f, center.Y - Height * 0.38f + bob, torsoW, Height * 0.42f, 16f, Color, Accent, 3f);
        DrawRoundedRect(center.X - torsoW * 0.38f, center.Y - Height * 0.34f + bob, torsoW * 0.76f, Height * 0.18f, 10f, Light, Color.WHITE, 1f);

        float coreY = center.Y - Height * 0.18f + bob;
        Raylib.DrawCircle((int)center.X, (int)coreY, 18f, Accent);
        Raylib.DrawCircle((int)center.X, (int)coreY, 13f, Color.ORANGE);
        Raylib.DrawCircle((int)center.X, (int)coreY, 8f, Color.YELLOW);
        Raylib.DrawCircle((int)center.X, (int)coreY, 4f, Color.WHITE);

        float shoulderY = center.Y - Height * 0.32f + bob;
        DrawCircle3D(center.X - torsoW * 0.52f, shoulderY, 16f, Accent, Dark);
        DrawCircle3D(center.X + torsoW * 0.52f, shoulderY, 16f, Accent, Dark);
    }

    private void DrawHead(Vector2 center, float bob)
    {
        float headSize = 36f;
        Vector2 headPos = new Vector2(center.X, center.Y - Height * 0.45f + bob);
        float dir = FacingRight ? 1f : -1f;

        DrawRoundedRect(headPos.X - 12f, headPos.Y + 12f, 24f, 18f, 6f, Dark, Accent, 2f);

        Raylib.DrawEllipse((int)headPos.X, (int)headPos.Y, headSize * 0.6f, headSize * 0.7f, Color);
        Raylib.DrawEllipseLines((int)headPos.X, (int)headPos.Y, headSize * 0.6f, headSize * 0.7f, Accent);

        Raylib.DrawCircle((int)(headPos.X + dir * 10f), (int)(headPos.Y - 4f), 8f, new Color(255, 200, 0, 255));
        Raylib.DrawCircle((int)(headPos.X + dir * 10f), (int)(headPos.Y - 4f), 5f, Color.ORANGE);
        Raylib.DrawCircle((int)(headPos.X + dir * 12f), (int)(headPos.Y - 6f), 2f, Color.WHITE);
        Raylib.DrawCircle((int)(headPos.X + dir * 4f), (int)(headPos.Y - 10f), 5f, new Color(255, 200, 0, 255));

        Raylib.DrawTriangle(
            new Vector2(headPos.X + dir * 16f, headPos.Y + 6f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 2f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 10f),
            Accent);
        Raylib.DrawTriangle(
            new Vector2(headPos.X + dir * 16f, headPos.Y + 14f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 10f),
            new Vector2(headPos.X + dir * 8f, headPos.Y + 18f),
            Accent);

        Raylib.DrawTriangle(
            new Vector2(headPos.X - dir * 22f, headPos.Y),
            new Vector2(headPos.X - dir * 10f, headPos.Y - 6f),
            new Vector2(headPos.X - dir * 10f, headPos.Y + 6f),
            Light);

        float antennaSway = MathF.Sin((float)Raylib.GetTime() * 12f) * 4f;
        Raylib.DrawLine((int)(headPos.X - dir * 4f), (int)(headPos.Y - 18f), (int)(headPos.X + dir * 8f + antennaSway), (int)(headPos.Y - 32f), Accent);
        Raylib.DrawCircle((int)(headPos.X + dir * 8f + antennaSway), (int)(headPos.Y - 32f), 3f, Light);
    }

    private void DrawLimbs(Vector2 center, float bob, float time)
    {
        float shoulderY = center.Y - Height * 0.32f + bob;
        float armExtension = State == RobotState.Attack && CurrentAttackType == AttackType.Punch ? 50f : 28f;
        float armOffset = State == RobotState.Attack ? (FacingRight ? armExtension : -armExtension) : (FacingRight ? 24f : -24f);
        float armAngle = State == RobotState.Attack ? (FacingRight ? -1.0f : 1.0f) : 0f;

        float walkArmBob = State == RobotState.Walk ? MathF.Sin(time * 15f + MathF.PI) * 10f : 0f;

        Vector2 leftShoulder = new Vector2(center.X - armOffset, shoulderY + walkArmBob);
        Vector2 rightShoulder = new Vector2(center.X + armOffset, shoulderY - walkArmBob);

        DrawArm(leftShoulder, armAngle, true, time);
        DrawArm(rightShoulder, -armAngle, false, time);
    }

    private void DrawArm(Vector2 shoulder, float angle, bool isLeft, float time)
    {
        DrawCircle3D(shoulder.X, shoulder.Y, 14f, Accent, Dark);
        Vector2 elbow = new Vector2(shoulder.X + MathF.Sin(angle) * 18f, shoulder.Y + 28f);
        DrawRoundedLimb(shoulder, elbow, 10f, angle, Color, Accent);
        DrawCircle3D(elbow.X, elbow.Y, 9f, Accent, Dark);
        Vector2 hand = new Vector2(elbow.X + MathF.Sin(angle) * 28f, elbow.Y + MathF.Cos(angle) * 22f);
        DrawRoundedLimb(elbow, hand, 16f, angle, Color, Accent);
        DrawClaw(hand);
    }

    private void DrawClaw(Vector2 hand)
    {
        float dir = FacingRight ? 1f : -1f;
        DrawRoundedRect(hand.X - 10f, hand.Y - 10f, 20f, 20f, 5f, Dark, Accent, 2f);
        for (int i = -1; i <= 1; i++)
        {
            float cx = hand.X + i * 6f + dir * 4f;
            float cy = hand.Y + 8f;
            Raylib.DrawTriangle(
                new Vector2(cx + dir * 10f, cy - 4f),
                new Vector2(cx - dir * 3f, cy + 2f),
                new Vector2(cx - dir * 3f, cy - 10f),
                Light);
        }
    }

    private void DrawFlame(Vector2 center, float bob)
    {
        if (CurrentAttackType != AttackType.Kick || State != RobotState.Attack)
            return;

        float dir = FacingRight ? 1f : -1f;
        float nozzleY = center.Y + Height * 0.02f + bob;
        float progress = 1f - (StateTimer / (0.55f * (CurrentAttackVariant == AttackVariant.Forward ? 0.85f : (CurrentAttackVariant == AttackVariant.Backward ? 1.2f : 1.0f))));
        progress = Math.Clamp(progress, 0f, 1f);

        for (int i = -1; i <= 1; i += 2)
        {
            float nx = center.X + i * dir * Width * 0.35f;
            float flameReach = 40f + progress * 90f;
            float flameHeight = 25f + progress * 35f;

            Raylib.DrawEllipse((int)(nx + dir * flameReach * 0.5f), (int)nozzleY, flameReach * 0.5f, flameHeight * 0.5f, new Color(255, 60, 0, 200));
            Raylib.DrawEllipse((int)(nx + dir * flameReach * 0.55f), (int)nozzleY, flameReach * 0.35f, flameHeight * 0.35f, new Color(255, 140, 0, 220));
            Raylib.DrawEllipse((int)(nx + dir * flameReach * 0.6f), (int)nozzleY, flameReach * 0.18f, flameHeight * 0.18f, new Color(255, 255, 100, 240));

            for (int j = 0; j < 5; j++)
            {
                float fx = nx + dir * (10f + j * 18f) + (MathF.Sin((float)Raylib.GetTime() * 20f + j * 1.5f) * 6f);
                float fy = nozzleY + MathF.Cos((float)Raylib.GetTime() * 18f + j * 2f) * 10f;
                Raylib.DrawCircle((int)fx, (int)fy, 2f + j * 0.5f, new Color(255, 200, 50, 220));
            }
        }
    }
}
