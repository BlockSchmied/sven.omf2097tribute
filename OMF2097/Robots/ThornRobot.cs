using Raylib_cs;
using System.Numerics;

namespace OMF2097.Robots;

/// <summary>
/// THORN – schwerer, gepanzerter Roboter mit Dornen und Kristall-Kern.
/// Zustände: Idle, Walk, Jump, Attack, Hit, Block.
/// Besonderheiten: hoher Schaden, langsame Bewegung, Dornen an Rücken/Schultern.
/// </summary>
public class ThornRobot : Robot
{
    public ThornRobot(bool isPlayer1, Vector2 startPosition) : base(RobotType.Thorn, isPlayer1, startPosition) { }

    protected override void InitializeColors()
    {
        Color = new Color(40, 140, 60, 255);
        Accent = new Color(160, 200, 40, 255);
        Dark = new Color(25, 90, 35, 255);
        Light = new Color(80, 190, 90, 255);
    }

    protected override int CalculateDamage(AttackType attackType)
    {
        int baseDamage = attackType == AttackType.Kick ? 16 : 12;
        int variantDamage = CurrentAttackVariant switch
        {
            AttackVariant.Forward => -2,
            AttackVariant.Backward => 4,
            _ => 0
        };
        return Math.Max(1, baseDamage + 4 + variantDamage);
    }

    protected override void DrawRobot(Vector2 center, float bob, float time)
    {
        DrawSpikesBack(center, bob);
        DrawBody(center, bob);
        DrawHead(center, bob);
        DrawLimbs(center, bob, time);
        DrawWeapon(center, bob);
        DrawDetailsFront(center, bob);
    }

    private void DrawSpikesBack(Vector2 center, float bob)
    {
        float dir = FacingRight ? -1f : 1f;
        float x = center.X + dir * Width * 0.4f;
        for (int i = 0; i < 3; i++)
        {
            float dy = center.Y - 20f + i * 18f + bob;
            Raylib.DrawTriangle(
                new Vector2(x + dir * 12f, dy),
                new Vector2(x, dy - 5f),
                new Vector2(x, dy + 5f),
                Accent);
        }
    }

    private void DrawBody(Vector2 center, float bob)
    {
        float torsoW = Width * 0.85f;
        float torsoY = center.Y + bob;

        DrawRoundedRect(center.X - torsoW * 0.35f, torsoY + Height * 0.18f, torsoW * 0.7f, Height * 0.16f, 8f, Dark, Accent, 2f);
        DrawRoundedRect(center.X - torsoW * 0.4f, torsoY + Height * 0.02f, torsoW * 0.8f, Height * 0.22f, 10f, Color, Accent, 2f);
        DrawRoundedRect(center.X - torsoW * 0.45f, torsoY - Height * 0.32f, torsoW * 0.9f, Height * 0.38f, 14f, Color, Accent, 3f);
        DrawRoundedRect(center.X - torsoW * 0.35f, torsoY - Height * 0.28f, torsoW * 0.7f, Height * 0.18f, 10f, Light, Color.WHITE, 1f);

        float coreY = torsoY - Height * 0.12f;
        Raylib.DrawCircle((int)center.X, (int)coreY, 16f, Accent);
        Raylib.DrawCircle((int)center.X, (int)coreY, 12f, Dark);
        Raylib.DrawCircle((int)center.X, (int)coreY, 8f, Color.WHITE);
        Raylib.DrawCircle((int)center.X, (int)coreY, 5f, new Color(255, 255, 200, 220));
        Raylib.DrawCircle((int)(center.X - 2f), (int)(coreY - 2f), 2f, Color.WHITE);

        float shoulderY = torsoY - Height * 0.28f;
        DrawCircle3D(center.X - torsoW * 0.48f, shoulderY, 14f, Accent, Dark);
        DrawCircle3D(center.X + torsoW * 0.48f, shoulderY, 14f, Accent, Dark);

        DrawChestDetail(center, torsoY);
    }

    private void DrawChestDetail(Vector2 center, float torsoY)
    {
        float coreY = torsoY - Height * 0.12f;
        DrawRoundedRect(center.X - 12f, coreY - 18f, 24f, 36f, 6f, new Color(100, 80, 40, 255), Accent, 2f);
        Raylib.DrawCircle((int)center.X, (int)coreY, 8f, new Color(160, 200, 40, 255));
        Raylib.DrawCircle((int)center.X, (int)coreY, 4f, Color.WHITE);
        for (int i = -1; i <= 1; i += 2)
            for (int j = 0; j < 2; j++)
            {
                float sx = center.X + i * 26f;
                float sy = coreY - 6f + j * 16f;
                Raylib.DrawTriangle(
                    new Vector2(sx + i * 14f, sy),
                    new Vector2(sx, sy - 5f),
                    new Vector2(sx, sy + 5f),
                    Accent);
            }
    }

    private void DrawHead(Vector2 center, float bob)
    {
        float headSize = 34f;
        Vector2 headPos = new Vector2(center.X, center.Y - Height * 0.38f + bob);
        float dir = FacingRight ? 1f : -1f;

        DrawRoundedRect(headPos.X - 11f, headPos.Y + 8f, 22f, 18f, 6f, Dark, Accent, 2f);

        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f, Accent);
        Raylib.DrawCircle((int)headPos.X, (int)headPos.Y, headSize / 2f - 4f, Color);
        Raylib.DrawTriangle(
            new Vector2(headPos.X, headPos.Y - 22f),
            new Vector2(headPos.X - 5f, headPos.Y - 10f),
            new Vector2(headPos.X + 5f, headPos.Y - 10f),
            Accent);
        for (int i = -1; i <= 1; i += 2)
        {
            Raylib.DrawTriangle(
                new Vector2(headPos.X + i * 16f, headPos.Y - 14f),
                new Vector2(headPos.X + i * 10f, headPos.Y - 4f),
                new Vector2(headPos.X + i * 22f, headPos.Y - 4f),
                Accent);
            Raylib.DrawTriangle(
                new Vector2(headPos.X + i * 14f, headPos.Y + 8f),
                new Vector2(headPos.X + i * 8f, headPos.Y + 16f),
                new Vector2(headPos.X + i * 20f, headPos.Y + 16f),
                Accent);
        }
        Raylib.DrawRectangle((int)(headPos.X - 10f), (int)(headPos.Y + 8f), 20, 10, Dark);
        DrawVisor(headPos);
    }

    private void DrawVisor(Vector2 headPos)
    {
        float visorW = 24f;
        float visorH = 10f;
        float visorX = FacingRight ? headPos.X + 2f : headPos.X - visorW - 2f;
        Raylib.DrawRectangle((int)(visorX - 2f), (int)(headPos.Y - 6f), (int)(visorW + 4f), (int)(visorH + 4f), Dark);
        Raylib.DrawRectangle((int)visorX, (int)(headPos.Y - 4f), (int)visorW, (int)visorH, Color.SKYBLUE);
        Raylib.DrawRectangle((int)(visorX + 2f), (int)(headPos.Y - 2f), (int)(visorW - 4f), (int)(visorH - 4f), new Color(100, 200, 255, 120));
        Raylib.DrawRectangle((int)(visorX + 4f), (int)(headPos.Y - 3f), (int)(visorW * 0.3f), 2, Color.WHITE);
    }

    private void DrawLimbs(Vector2 center, float bob, float time)
    {
        float armW = 14f;
        float armH = 46f;
        float legW = 24f;
        float legH = 58f;

        float armExtension = CurrentAttackType == AttackType.Punch ? 45f : (CurrentAttackType == AttackType.Kick ? 22f : 28f);
        float armOffset = State == RobotState.Attack ? (FacingRight ? armExtension : -armExtension) : (FacingRight ? 20f : -20f);
        float armAngle = State == RobotState.Attack ? (FacingRight ? -0.9f : 0.9f) : 0f;

        Vector2 shoulderY = new Vector2(center.X, center.Y - Height * 0.26f + bob);
        DrawArm(new Vector2(shoulderY.X - armOffset, shoulderY.Y), armW, armH, armAngle, true, time);
        DrawArm(new Vector2(shoulderY.X + armOffset, shoulderY.Y), armW, armH, -armAngle, false, time);

        Vector2 hipY = new Vector2(center.X, center.Y + Height * 0.18f + bob);
        float legBob = State == RobotState.Walk ? MathF.Sin(time * 15f + MathF.PI) * 8f : 0f;
        float kickExtension = CurrentAttackType == AttackType.Kick && State == RobotState.Attack ? 28f : 0f;
        float kickDir = FacingRight ? 1f : -1f;

        DrawLeg(new Vector2(hipY.X - 20f, hipY.Y), legW, legH, legBob, false, time);
        DrawLeg(new Vector2(hipY.X + 20f + kickDir * kickExtension, hipY.Y - legBob), legW, legH + kickExtension * 0.3f, -legBob, kickExtension > 0, time);
    }

    private void DrawArm(Vector2 shoulder, float w, float h, float angle, bool isLeft, float time)
    {
        DrawCircle3D(shoulder.X, shoulder.Y, 12f, Accent, Dark);
        Vector2 elbow = new Vector2(shoulder.X, shoulder.Y + h * 0.45f);
        DrawRoundedLimb(shoulder, elbow, w, angle, Color, Accent);
        DrawCircle3D(elbow.X, elbow.Y, 10f, Accent, Dark);
        Vector2 hand = new Vector2(elbow.X + MathF.Sin(angle) * h * 0.4f, elbow.Y + MathF.Cos(angle) * h * 0.4f);
        DrawRoundedLimb(elbow, hand, w * 0.85f, angle, Color, Accent);
        DrawHand(hand);
    }

    private void DrawHand(Vector2 pos)
    {
        float handW = 18f;
        float handH = 16f;
        DrawRoundedRect(pos.X - handW / 2f, pos.Y - handH / 2f, handW, handH, 5f, Dark, Accent, 2f);
        for (int i = -1; i <= 1; i++)
        {
            float fx = pos.X + i * 5f;
            float fy = pos.Y + handH / 2f;
            Raylib.DrawRectangle((int)(fx - 2f), (int)fy, 4, 8, Accent);
            Raylib.DrawTriangle(
                new Vector2(fx, fy + 10f),
                new Vector2(fx - 3f, fy + 6f),
                new Vector2(fx + 3f, fy + 6f),
                Light);
        }
        Raylib.DrawCircle((int)pos.X, (int)(pos.Y - 2f), 4f, Light);
    }

    private void DrawLeg(Vector2 hip, float w, float h, float bob, bool isKicking, float time)
    {
        DrawCircle3D(hip.X, hip.Y, 12f, Accent, Dark);
        Vector2 knee = new Vector2(hip.X + bob * 0.3f, hip.Y + h * 0.5f);
        DrawRoundedLimb(hip, knee, w, bob * 0.01f, Color, Accent);
        DrawCircle3D(knee.X, knee.Y, 11f, Accent, Dark);
        Vector2 foot = new Vector2(knee.X - bob * 0.2f, knee.Y + h * 0.5f);
        if (isKicking)
            foot = new Vector2(knee.X + (FacingRight ? 35f : -35f), knee.Y + 10f);
        DrawRoundedLimb(knee, foot, w * 0.85f, isKicking ? (FacingRight ? -0.4f : 0.4f) : 0f, Color, Accent);
        DrawFoot(foot, isKicking);
    }

    private void DrawFoot(Vector2 pos, bool isKicking)
    {
        float footW = 28f;
        float footH = 14f;
        if (isKicking)
        {
            DrawRoundedRect(pos.X - footW / 2f, pos.Y - footH / 2f, footW, footH, 6f, Accent, Light, 2f);
            for (int i = 0; i < 3; i++)
            {
                float sx = pos.X - footW / 2f + 6f + i * 10f;
                Raylib.DrawRectangle((int)sx, (int)(pos.Y + footH / 2f), 6, 6, Dark);
            }
        }
        else
        {
            DrawRoundedRect(pos.X - footW / 2f, pos.Y - footH / 2f, footW, footH, 5f, Dark, Accent, 2f);
            Raylib.DrawRectangle((int)(pos.X - footW / 2f + 3f), (int)(pos.Y + 2f), (int)(footW - 6f), 5, Accent);
            for (int i = -1; i <= 1; i++)
                Raylib.DrawCircle((int)(pos.X + i * 8f), (int)(pos.Y - 2f), 3f, Light);
        }
    }

    private void DrawWeapon(Vector2 center, float bob)
    {
        if (CurrentAttackType == AttackType.Kick) return;

        float dir = FacingRight ? 1f : -1f;
        float handY = center.Y - Height * 0.05f + bob;
        float handX = FacingRight ? center.X + Width / 2f - 5f : center.X - Width / 2f + 5f;

        DrawRoundedRect(handX - 6f, handY - 6f, 12f, 24f, 3f, Dark, Accent, 1f);

        float weaponX = FacingRight ? handX : handX - 60f;
        float weaponY = handY - 7f;
        DrawRoundedRect(weaponX, weaponY, 60f, 14f, 4f, Color.GREEN, Color.WHITE, 2f);
        Raylib.DrawRectangle((int)(weaponX + 21f), (int)(weaponY + 2f), 18, 10, Light);

        for (int i = 0; i < 5; i++)
        {
            float spikeX = FacingRight ? weaponX + 8f + i * 12f : weaponX + 60f - 8f - i * 12f;
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
    }

    private void DrawDetailsFront(Vector2 center, float bob)
    {
        for (int i = -1; i <= 1; i += 2)
        {
            float sx = center.X + i * Width * 0.48f;
            float sy = center.Y - Height * 0.22f + bob;
            Raylib.DrawTriangle(
                new Vector2(sx + i * 14f, sy - 8f),
                new Vector2(sx + i * 6f, sy + 4f),
                new Vector2(sx - i * 4f, sy - 2f),
                Accent);
            Raylib.DrawTriangle(
                new Vector2(sx + i * 12f, sy + 12f),
                new Vector2(sx + i * 4f, sy + 2f),
                new Vector2(sx - i * 6f, sy + 8f),
                Accent);
        }
        for (int i = -1; i <= 1; i += 2)
        {
            float ax = center.X + i * 28f;
            float ay = center.Y - Height * 0.05f + bob;
            Raylib.DrawTriangle(
                new Vector2(ax + i * 10f, ay),
                new Vector2(ax, ay - 5f),
                new Vector2(ax, ay + 5f),
                Accent);
        }
    }
}
