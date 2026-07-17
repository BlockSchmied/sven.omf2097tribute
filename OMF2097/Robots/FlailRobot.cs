using Raylib_cs;
using System.Numerics;

namespace OMF2097.Robots;

/// <summary>
/// FLAIL – großer Katzenkopf auf einer langen Stange, unten mit zwei kleinen Stachelrädern.
/// Zustände: Idle, Walk, Jump, Attack, Hit, Block.
/// Besonderheiten: Katzenkopf-Korpus, dünne Arme mit riesigen Fäusten direkt am Kopf,
/// genau zwei Ketten, die beim Kick nach vorne in Richtung Gegner fliegen,
/// Räder rotieren beim Laufen, boxt mit den Fäusten beim Punch.
/// </summary>
public class FlailRobot : Robot
{
    public FlailRobot(bool isPlayer1, Vector2 startPosition) : base(RobotType.Flail, isPlayer1, startPosition) { }

    protected override void InitializeColors()
    {
        Color = new Color(180, 160, 40, 255);
        Accent = new Color(120, 60, 20, 255);
        Dark = new Color(110, 95, 20, 255);
        Light = new Color(230, 210, 80, 255);
    }

    protected override float GetAttackDuration(AttackType attackType) =>
        attackType == AttackType.Kick ? 0.55f : 0.35f;

    protected override float GetAttackDurationMultiplier(AttackType attackType) =>
        CurrentAttackVariant switch
        {
            AttackVariant.Forward => 0.8f,
            AttackVariant.Backward => 1.45f,
            _ => 1.0f
        };

    protected override int CalculateDamage(AttackType attackType)
    {
        int baseDamage = attackType == AttackType.Kick ? 18 : 14;
        int variantDamage = CurrentAttackVariant switch
        {
            AttackVariant.Forward => -2,
            AttackVariant.Backward => 4,
            _ => 0
        };
        return Math.Max(1, baseDamage + 6 + variantDamage);
    }

    protected override void DrawRobot(Vector2 center, float bob, float time)
    {
        // Flail nach unten verschieben, bis er auf Jaguar-Höhe steht
        float verticalShift = Height * 0.20f;
        Vector2 shiftedCenter = new Vector2(center.X, center.Y + verticalShift);

        // Ketten bleiben auf ihrer ursprünglichen Höhe
        DrawChains(center, bob, time);

        // Rest des Körpers wird nach unten verschoben
        DrawSpikedWheels(shiftedCenter, bob, time);
        DrawBody(shiftedCenter, bob);
        DrawArms(shiftedCenter, bob, time);
        DrawHead(shiftedCenter, bob, time);
    }

    // ========================
    // KÖRPER: Lange Stange, Achse mit Rädern ganz unten
    // ========================

    private void DrawBody(Vector2 center, float bob)
    {
        float torsoY = center.Y + bob;
        float poleW = 14f;
        float poleH = Height * 0.55f;

        // Kürzere vertikale Stange, Kopf sitzt höher
        DrawRoundedRect(center.X - poleW / 2f, torsoY - Height * 0.25f, poleW, poleH, 4f, Dark, Accent, 2f);

        // Querachse mit Stachelrädern ganz unten (bleibt an derselben Position)
        float axleY = torsoY + Height * 0.40f;
        Raylib.DrawRectangle((int)(center.X - Width * 0.35f), (int)(axleY - 3f), (int)(Width * 0.7f), 6, Accent);
    }

    private void DrawHead(Vector2 center, float bob, float time)
    {
        float headSize = 58f;
        // Kopf deutlich höher, Schwungrad auf Höhe der Radachse
        Vector2 headPos = new Vector2(center.X, center.Y - Height * 0.28f + bob);
        float dir = FacingRight ? 1f : -1f;

        // Katzenkopf-Korpus
        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f, Color);
        Raylib.DrawCircleLines((int)headPos.X, (int)headPos.Y, headSize / 2f, Accent);

        // Schnauze
        Raylib.DrawEllipse((int)(headPos.X + dir * 16f), (int)(headPos.Y + 7f), 20f, 14f, Light);
        Raylib.DrawCircle((int)(headPos.X + dir * 25f), (int)(headPos.Y + 7f), 5f, Dark);

        // Grimmiges Gesicht
        DrawGrimFace(headPos, dir);

        // Visor (Katzenaugen)
        DrawVisor(headPos);

        // Schnurrhaare
        for (int i = -1; i <= 1; i += 2)
        {
            float hx = headPos.X + dir * 23f;
            float hy = headPos.Y + i * 5f;
            Raylib.DrawLine((int)hx, (int)hy, (int)(hx + dir * 16f), (int)(hy + i * 2f), Color.WHITE);
            Raylib.DrawLine((int)hx, (int)(hy + 3f), (int)(hx + dir * 12f), (int)(hy + 3f), Color.WHITE);
        }
    }

    private void DrawGrimFace(Vector2 headPos, float dir)
    {
        // Runzelnde Stirn
        Raylib.DrawLine(
            (int)(headPos.X - dir * 18f), (int)(headPos.Y - 14f),
            (int)(headPos.X - dir * 6f), (int)(headPos.Y - 8f),
            Accent);
        Raylib.DrawLine(
            (int)(headPos.X + dir * 18f), (int)(headPos.Y - 14f),
            (int)(headPos.X + dir * 6f), (int)(headPos.Y - 8f),
            Accent);

        // Grimmige Augenbrauen
        Raylib.DrawLineEx(
            new Vector2(headPos.X - dir * 16f, headPos.Y - 12f),
            new Vector2(headPos.X - dir * 4f, headPos.Y - 6f),
            4f, Accent);
        Raylib.DrawLineEx(
            new Vector2(headPos.X + dir * 16f, headPos.Y - 12f),
            new Vector2(headPos.X + dir * 4f, headPos.Y - 6f),
            4f, Accent);

        // Geblecktes Maul
        for (int i = -2; i <= 2; i++)
        {
            float tx = headPos.X + dir * 22f + i * 5f;
            float tyTop = headPos.Y + 10f;
            float tyBottom = headPos.Y + 16f;
            Raylib.DrawTriangle(
                new Vector2(tx, tyTop),
                new Vector2(tx - 2f, tyTop + 6f),
                new Vector2(tx + 2f, tyTop + 6f),
                Color.WHITE);
            Raylib.DrawTriangle(
                new Vector2(tx, tyBottom),
                new Vector2(tx - 2f, tyBottom - 6f),
                new Vector2(tx + 2f, tyBottom - 6f),
                Color.WHITE);
        }

        // Narbe über einem Auge
        Raylib.DrawLine(
            (int)(headPos.X - dir * 10f), (int)(headPos.Y - 16f),
            (int)(headPos.X - dir * 4f), (int)(headPos.Y - 4f),
            new Color(80, 60, 30, 255));
    }

    private void DrawVisor(Vector2 headPos)
    {
        float eyeW = 12f;
        float eyeH = 7f;
        float dir = FacingRight ? 1f : -1f;

        for (int i = -1; i <= 1; i += 2)
        {
            float eyeX = headPos.X + dir * 10f + i * 5f;
            float eyeY = headPos.Y - 4f;
            Raylib.DrawEllipse((int)eyeX, (int)eyeY, eyeW, eyeH, new Color(255, 60, 60, 255));
            Raylib.DrawEllipseLines((int)eyeX, (int)eyeY, eyeW, eyeH, Accent);
            Raylib.DrawCircle((int)(eyeX + dir * 2f), (int)(eyeY - 1f), 2f, Color.WHITE);
        }
    }

    // ========================
    // ARME: direkt am Kopf, boxen beim Punch
    // ========================

    private void DrawArms(Vector2 center, float bob, float time)
    {
        float headY = center.Y - Height * 0.28f + bob;
        float armOffset = Width * 0.30f;

        Vector2 leftShoulder = new Vector2(center.X - armOffset, headY);
        Vector2 rightShoulder = new Vector2(center.X + armOffset, headY);

        // Beim Punch schwingen die Fäuste nach vorne
        float punchExtension = CurrentAttackType == AttackType.Punch && State == RobotState.Attack ? 35f : 0f;
        float punchDir = FacingRight ? 1f : -1f;

        DrawArm(leftShoulder, true, time, punchDir * punchExtension);
        DrawArm(rightShoulder, false, time, punchDir * punchExtension);
    }

    private void DrawArm(Vector2 shoulder, bool isLeft, float time, float punchOffset)
    {
        DrawCircle3D(shoulder.X, shoulder.Y, 8f, Accent, Dark);

        float armBob = State == RobotState.Walk ? MathF.Sin(time * 15f + (isLeft ? 0f : MathF.PI)) * 4f : 0f;
        Vector2 elbow = new Vector2(shoulder.X, shoulder.Y + 18f + armBob);
        DrawRoundedLimb(shoulder, elbow, 6f, 0f, Color, Accent);

        DrawCircle3D(elbow.X, elbow.Y, 5f, Accent, Dark);
        Vector2 hand = new Vector2(elbow.X + punchOffset, elbow.Y + 14f);
        DrawRoundedLimb(elbow, hand, 6f, 0f, Color, Accent);

        DrawFist(hand);
    }

    private void DrawFist(Vector2 hand)
    {
        float fistW = 24f;
        float fistH = 20f;
        DrawRoundedRect(hand.X - fistW / 2f, hand.Y - fistH / 2f, fistW, fistH, 7f, Dark, Accent, 3f);
        for (int i = -1; i <= 1; i++)
        {
            float fx = hand.X + i * 6f;
            float fy = hand.Y - fistH / 2f + 4f;
            Raylib.DrawCircle((int)fx, (int)fy, 4f, Accent);
        }
        Raylib.DrawCircle((int)(hand.X - fistW / 2f + 4f), (int)(hand.Y + 2f), 5f, Accent);
    }

    // ========================
    // GENAU ZWEI KETTEN: fliegen beim Kick nach vorne
    // ========================

    private void DrawChains(Vector2 center, float bob, float time)
    {
        float headY = center.Y - Height * 0.16f + bob;

        // Kettenstangen behalten stets dieselbe Ausrichtung wie bei P2 (nach links)
        // und drehen sich nicht mit der Blickrichtung des Roboters
        DrawChain(new Vector2(center.X - 26f, headY - 18f), -1, time);
        DrawChain(new Vector2(center.X + 26f, headY - 18f), 1, time);
    }

    private void DrawChain(Vector2 anchor, int side, float time)
    {
        // Feste Ausrichtung: linke Stange zeigt nach links, rechte nach rechts
        float postDir = side;

        // Stange ragt aus dem Kopf heraus; das innere Ende ist fest am Kopf
        float postEndX = anchor.X + postDir * 18f;
        float postEndY = anchor.Y - 8f;

        // Festes Gelenk am Kopf (inneres Ende der Stange)
        DrawCircle3D(anchor.X, anchor.Y, 6f, Accent, Dark);

        // Dünne Stange vom Kopf zur Außenseite
        DrawRoundedLimb(anchor, new Vector2(postEndX, postEndY), 4f, 0f, Accent, Color.WHITE);

        // Außeres Gelenk, an dem die Kette hängt
        DrawCircle3D(postEndX, postEndY, 4f, Accent, Dark);

        // Kette hängt von der Stangenspitze herab
        float chainLength = 80f;
        float idleSway = MathF.Sin(time * 3f + side) * 5f;
        float swingAngle = idleSway * 0.03f;

        float chainEndX = postEndX + MathF.Sin(swingAngle) * chainLength;
        float chainEndY = postEndY + MathF.Cos(swingAngle) * chainLength;

        // Beim Kick: beide Ketten in dieselbe Richtung auf den Gegner schwingen
        if (CurrentAttackType == AttackType.Kick && State == RobotState.Attack)
        {
            float progress = 1f - (StateTimer / GetAttackDuration(CurrentAttackType));
            progress = Math.Clamp(progress, 0f, 1f);
            float attackDir = FacingRight ? 1f : -1f;
            float sweep = MathF.Sin(progress * MathF.PI) * 95f * attackDir;
            float lift = MathF.Sin(progress * MathF.PI) * -15f;
            chainEndX = postEndX + sweep;
            chainEndY = postEndY - 5f + lift;
        }

        Vector2[] chain = new Vector2[8];
        chain[0] = new Vector2(postEndX, postEndY);
        for (int i = 1; i < chain.Length; i++)
        {
            float t = i / (float)(chain.Length - 1);
            chain[i] = new Vector2(
                postEndX + (chainEndX - postEndX) * t + MathF.Sin(time * 8f + i + side) * 2f,
                postEndY + (chainEndY - postEndY) * t);
        }

        for (int i = 0; i < chain.Length - 1; i++)
        {
            Raylib.DrawLineEx(chain[i], chain[i + 1], 3f, Color.GRAY);
            Raylib.DrawCircle((int)chain[i].X, (int)chain[i].Y, 3f, Color.GRAY);
        }
        Raylib.DrawCircle((int)chainEndX, (int)chainEndY, 6f, Accent);
        Raylib.DrawCircle((int)chainEndX, (int)chainEndY, 3f, Light);
    }

    // ========================
    // STACHELRÄDER: unten, rotieren nur beim Laufen
    // ========================

    private void DrawSpikedWheels(Vector2 center, float bob, float time)
    {
        float axleY = center.Y + Height * 0.40f + bob;
        float wheelRadius = 14f;

        float rotationSpeed = State == RobotState.Walk ? time * 12f : 0f;

        DrawSpikedWheel(center.X - Width * 0.25f, axleY, wheelRadius, rotationSpeed);
        DrawSpikedWheel(center.X + Width * 0.25f, axleY, wheelRadius, rotationSpeed + MathF.PI);
    }

    private void DrawSpikedWheel(float cx, float cy, float radius, float rotation)
    {
        Raylib.DrawCircle((int)cx, (int)cy, radius, Dark);
        Raylib.DrawCircleLines((int)cx, (int)cy, radius, Accent);
        Raylib.DrawCircle((int)cx, (int)cy, radius * 0.3f, Accent);

        int spikeCount = 8;
        for (int i = 0; i < spikeCount; i++)
        {
            float angle = rotation + i * MathF.PI * 2f / spikeCount;
            float sx = cx + MathF.Cos(angle) * radius;
            float sy = cy + MathF.Sin(angle) * radius;
            float tx = cx + MathF.Cos(angle) * (radius + 8f);
            float ty = cy + MathF.Sin(angle) * (radius + 8f);
            Raylib.DrawTriangle(
                new Vector2(tx, ty),
                new Vector2(sx - 3f, sy),
                new Vector2(sx + 3f, sy),
                Light);
        }
    }
}
