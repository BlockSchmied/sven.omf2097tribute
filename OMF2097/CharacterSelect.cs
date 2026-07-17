using Raylib_cs;
using System.Numerics;

namespace OMF2097;

public class CharacterSelect
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    public RobotType Player1Type { get; private set; } = RobotType.Jaguar;
    public RobotType Player2Type { get; private set; } = RobotType.Shadow;
    public bool Confirmed { get; set; } = false;

    private int _p1Index = 0;
    private int _p2Index = 1;
    private bool _p1Ready = false;
    private bool _p2Ready = false;

    private readonly RobotType[] _types = (RobotType[])Enum.GetValues(typeof(RobotType));
    private readonly string[] _names = { "JAGUAR", "SHADOW", "THORN", "FLAIL", "PYROS" };
    private readonly string[] _descriptions =
    {
        "Balanced striker with quick claws",
        "Fast ninja with shadow strikes",
        "Heavy bruiser with thorny armor",
        "Slow powerhouse with brutal flails",
        "Hovering wasp with twin flamethrowers"
    };

    public CharacterSelect(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void Update(float dt)
    {
        if (!_p1Ready)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_A)) _p1Index = (_p1Index - 1 + _types.Length) % _types.Length;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_D)) _p1Index = (_p1Index + 1) % _types.Length;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_W)) _p1Ready = true;
        }

        if (!_p2Ready)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT)) _p2Index = (_p2Index - 1 + _types.Length) % _types.Length;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT)) _p2Index = (_p2Index + 1) % _types.Length;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP)) _p2Ready = true;
        }

        Player1Type = _types[_p1Index];
        Player2Type = _types[_p2Index];

        if (_p1Ready && _p2Ready)
        {
            Confirmed = true;
            _p1Ready = false;
            _p2Ready = false;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
        {
            _p1Ready = false;
            _p2Ready = false;
        }
    }

    public void Draw()
    {
        Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(15, 15, 25, 255));

        string title = "SELECT YOUR ROBOT";
        int titleSize = 48;
        int titleWidth = Raylib.MeasureText(title, titleSize);
        Raylib.DrawText(title, (_screenWidth - titleWidth) / 2, 40, titleSize, Color.GOLD);

        DrawPanel(120, 140, 420, 460, "PLAYER 1", _p1Index, _p1Ready, Color.BLUE);
        DrawPanel(_screenWidth - 540, 140, 420, 460, "PLAYER 2", _p2Index, _p2Ready, Color.RED);

        string hint = "P1: A/D choose, W confirm, F punch, SPACE kick  |  P2: LEFT/RIGHT choose, UP confirm, ENTER punch, RCTRL kick";
        int hintSize = 20;
        int hintWidth = Raylib.MeasureText(hint, hintSize);
        Raylib.DrawText(hint, (_screenWidth - hintWidth) / 2, 640, hintSize, Color.LIGHTGRAY);
    }

    private void DrawPanel(int x, int y, int w, int h, string label, int index, bool ready, Color accent)
    {
        Color panelColor = ready ? new Color(30, 60, 30, 255) : new Color(30, 30, 40, 255);
        Raylib.DrawRectangle(x, y, w, h, panelColor);
        Raylib.DrawRectangleLines(x, y, w, h, accent);

        int labelSize = 32;
        int labelWidth = Raylib.MeasureText(label, labelSize);
        Raylib.DrawText(label, x + (w - labelWidth) / 2, y + 20, labelSize, accent);

        RobotType type = _types[index];
        string name = _names[index];
        string desc = _descriptions[index];

        DrawRobotPreview(x + w / 2, y + 190, type);

        int nameSize = 40;
        int nameWidth = Raylib.MeasureText(name, nameSize);
        Raylib.DrawText(name, x + (w - nameWidth) / 2, y + 300, nameSize, Color.WHITE);

        int descSize = 18;
        int descWidth = Raylib.MeasureText(desc, descSize);
        Raylib.DrawText(desc, x + (w - descWidth) / 2, y + 360, descSize, Color.LIGHTGRAY);

        string status = ready ? "READY!" : "SELECTING...";
        int statusSize = 24;
        int statusWidth = Raylib.MeasureText(status, statusSize);
        Color statusColor = ready ? Color.GREEN : Color.YELLOW;
        Raylib.DrawText(status, x + (w - statusWidth) / 2, y + 410, statusSize, statusColor);
    }

    private void DrawRobotPreview(int cx, int cy, RobotType type)
    {
        // Farbpalette exakt wie im Spiel
        var (body, accent, dark, light) = type switch
        {
            RobotType.Jaguar => (new Color(255, 100, 30, 255), new Color(80, 80, 80, 255), new Color(140, 50, 15, 255), new Color(255, 160, 60, 255)),
            RobotType.Shadow => (new Color(60, 60, 80, 255), new Color(180, 40, 220, 255), new Color(30, 30, 45, 255), new Color(100, 100, 130, 255)),
            RobotType.Thorn => (new Color(40, 140, 60, 255), new Color(160, 200, 40, 255), new Color(25, 90, 35, 255), new Color(80, 190, 90, 255)),
            RobotType.Flail => (new Color(180, 160, 40, 255), new Color(120, 60, 20, 255), new Color(110, 95, 20, 255), new Color(230, 210, 80, 255)),
            RobotType.Pyros => (new Color(255, 140, 20, 255), new Color(139, 0, 0, 255), new Color(80, 20, 10, 255), new Color(255, 220, 80, 255)),
            _ => (Color.GRAY, Color.WHITE, Color.DARKGRAY, Color.LIGHTGRAY)
        };

        float time = (float)Raylib.GetTime();
        float bob = MathF.Sin(time * 3f) * 3f;

        // Schatten
        Raylib.DrawEllipse(cx, cy + 58, 55, 10, new Color(0, 0, 0, 80));

        // Rücken-Details
        DrawPreviewBackDetail(cx, cy, bob, type, accent, light);

        if (type == RobotType.Pyros)
        {
            DrawPyrosPreview(cx, cy, body, accent, dark, light);
            return;
        }

        if (type == RobotType.Flail)
        {
            DrawFlailPreview(cx, cy, body, accent, dark, light);
            return;
        }

        // Beine
        DrawPreviewLeg(cx - 18, (int)(cy + 35), 18, 45, bob, type, body, accent, dark);
        DrawPreviewLeg(cx + 18, (int)(cy + 35), 18, 45, -bob, type, body, accent, dark);

        // Körper
        DrawRoundedRectCharSelect(cx - 36, (int)(cy - 25 + bob), 72, 70, 10, body, accent, 3);
        DrawRoundedRectCharSelect(cx - 28, (int)(cy - 18 + bob), 56, 20, 6, light, Color.WHITE, 1);

        // Reaktor-Kern
        Raylib.DrawCircle(cx, (int)(cy + 5 + bob), 10, accent);
        Raylib.DrawCircle(cx, (int)(cy + 5 + bob), 6, dark);
        Raylib.DrawCircle(cx, (int)(cy + 5 + bob), 3, Color.WHITE);

        // Brust-Detail
        DrawPreviewChestDetail(cx, (int)(cy + bob), type, body, accent, dark, light);

        // Schultergelenke
        Raylib.DrawCircle(cx - 40, (int)(cy - 18 + bob), 10, accent);
        Raylib.DrawCircle(cx + 40, (int)(cy - 18 + bob), 10, accent);

        // Arme
        DrawPreviewArm(cx - 40, (int)(cy - 18 + bob), 14, 40, bob * 0.5f, type, body, accent, dark, light, true);
        DrawPreviewArm(cx + 40, (int)(cy - 18 + bob), 14, 40, -bob * 0.5f, type, body, accent, dark, light, false);

        // Kopf
        DrawPreviewHead(cx, (int)(cy - 42 + bob), type, body, accent, dark, light);

        // Waffe
        DrawPreviewWeapon(cx + 42, (int)(cy - 10 + bob), type, accent, light);

        // Vorder-Details
        DrawPreviewFrontDetail(cx, cy, bob, type, accent, dark, light);
    }

    private void DrawFlailPreview(int cx, int cy, Color body, Color accent, Color dark, Color light)
    {
        float time = (float)Raylib.GetTime();
        float bob = MathF.Sin(time * 3f) * 3f;
        int headSize = 58;
        int verticalShift = 22; // Kopf nach unten, wie im Spiel
        int headY = (int)(cy - 22 + bob + verticalShift);

        // Schatten
        Raylib.DrawEllipse(cx, (int)(cy + 58 + bob + verticalShift), 55, 10, new Color(0, 0, 0, 80));

        // Stachelräder unten
        int axleY = cy + 45 + verticalShift;
        Raylib.DrawRectangle(cx - 25, axleY - 3, 50, 6, accent);
        DrawPreviewSpikedWheel(cx - 18, axleY, 14, time * 12f, light, accent);
        DrawPreviewSpikedWheel(cx + 18, axleY, 14, time * 12f + MathF.PI, light, accent);

        // Ketten (zwei, weiter außen, Seiten vertauscht) – bleiben auf ursprünglicher Höhe
        DrawPreviewFlailChain(cx - 32, headY, 1, time, accent, light);
        DrawPreviewFlailChain(cx + 32, headY, -1, time, accent, light);

        // Stange, nach unten verschoben bis sie mit dem Kopf-Kreis kollidiert
        DrawRoundedRectCharSelect(cx - 7, headY, 14, 70, 4, dark, accent, 2);

        // Arme am Kopf
        DrawPreviewFlailArm(cx - 22, headY, body, accent, dark, light, time, true);
        DrawPreviewFlailArm(cx + 22, headY, body, accent, dark, light, time, false);

        // Katzenkopf
        Raylib.DrawCircle(cx, headY, headSize / 2, body);
        Raylib.DrawCircleLines(cx, headY, headSize / 2, accent);

        // Schnauze
        Raylib.DrawEllipse(cx + 16, headY + 7, 20, 14, light);
        Raylib.DrawCircle(cx + 25, headY + 7, 5, dark);

        // Grimmiges Gesicht
        // Augenbrauen
        Raylib.DrawLineEx(new Vector2(cx - 16, headY - 6), new Vector2(cx - 4, headY), 4f, accent);
        Raylib.DrawLineEx(new Vector2(cx + 16, headY - 6), new Vector2(cx + 4, headY), 4f, accent);
        // Zähne
        for (int i = -2; i <= 2; i++)
        {
            int tx = cx + 22 + i * 5;
            Raylib.DrawTriangle(new Vector2(tx, headY + 10), new Vector2(tx - 2, headY + 16), new Vector2(tx + 2, headY + 16), Color.WHITE);
            Raylib.DrawTriangle(new Vector2(tx, headY + 16), new Vector2(tx - 2, headY + 10), new Vector2(tx + 2, headY + 10), Color.WHITE);
        }
        // Narbe
        Raylib.DrawLine(cx - 10, headY - 8, cx - 4, headY + 4, new Color(80, 60, 30, 255));

        // Augen
        Raylib.DrawEllipse(cx - 5, headY - 4, 6, 4, new Color(255, 60, 60, 255));
        Raylib.DrawEllipse(cx + 5, headY - 4, 6, 4, new Color(255, 60, 60, 255));

        // Schnurrhaare
        Raylib.DrawLine(cx + 23, headY + 5, cx + 39, headY + 7, Color.WHITE);
        Raylib.DrawLine(cx + 23, headY + 9, cx + 35, headY + 9, Color.WHITE);
    }

    private void DrawPreviewSpikedWheel(int cx, int cy, int radius, float rotation, Color light, Color accent)
    {
        Raylib.DrawCircle(cx, cy, radius, new Color(110, 95, 20, 255));
        Raylib.DrawCircleLines(cx, cy, radius, accent);
        Raylib.DrawCircle(cx, cy, (int)(radius * 0.3f), accent);

        for (int i = 0; i < 8; i++)
        {
            float angle = rotation + i * MathF.PI * 2f / 8f;
            int sx = (int)(cx + MathF.Cos(angle) * radius);
            int sy = (int)(cy + MathF.Sin(angle) * radius);
            int tx = (int)(cx + MathF.Cos(angle) * (radius + 8));
            int ty = (int)(cy + MathF.Sin(angle) * (radius + 8));
            Raylib.DrawTriangle(new Vector2(tx, ty), new Vector2(sx - 3, sy), new Vector2(sx + 3, sy), light);
        }
    }

    private void DrawPreviewFlailChain(int cx, int cy, int side, float time, Color accent, Color light)
    {
        int postEndX = cx + side * 18;
        int postEndY = cy - 8;

        // Festes Gelenk am Kopf (inneres Ende der Stange)
        Raylib.DrawCircle(cx, cy, 6, accent);

        // Dünne Stange, die wie ein Ohr aus dem Kopf ragt
        DrawRoundedRectCharSelect(cx - 2, cy - 8, 4, 40, 3, accent, Color.WHITE, 1);

        // Außeres Gelenk, an dem die Kette hängt
        Raylib.DrawCircle(postEndX, postEndY, 4, accent);

        float chainLength = 80f;
        float idleSway = MathF.Sin(time * 3f + side) * 5f;
        float chainEndX = postEndX + idleSway * 0.5f;
        float chainEndY = postEndY + chainLength;

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
            Raylib.DrawCircle((int)chain[i].X, (int)chain[i].Y, 3, Color.GRAY);
        }
        Raylib.DrawCircle((int)chainEndX, (int)chainEndY, 6, accent);
        Raylib.DrawCircle((int)chainEndX, (int)chainEndY, 3, light);
    }

    private void DrawPreviewFlailArm(int cx, int cy, Color body, Color accent, Color dark, Color light, float time, bool isLeft)
    {
        float armBob = MathF.Sin(time * 15f + (isLeft ? 0f : MathF.PI)) * 4f;
        int elbowX = (int)cx;
        int elbowY = (int)(cy + 18 + armBob);

        Raylib.DrawCircle(cx, cy, 8, accent);
        DrawRoundedRectCharSelect(cx - 3, cy, 6, 18, 3, body, accent, 1);
        Raylib.DrawCircle(elbowX, elbowY, 5, accent);
        DrawRoundedRectCharSelect(elbowX - 3, elbowY, 6, 14, 3, body, accent, 1);

        // Faust
        int handX = elbowX;
        int handY = elbowY + 14;
        DrawRoundedRectCharSelect(handX - 12, handY - 10, 24, 20, 7, dark, accent, 3);
        Raylib.DrawCircle(handX - 6, handY - 6, 4, accent);
        Raylib.DrawCircle(handX, handY - 6, 4, accent);
        Raylib.DrawCircle(handX + 6, handY - 6, 4, accent);
    }

    private void DrawPyrosPreview(int cx, int cy, Color body, Color accent, Color dark, Color light)
    {
        int bob = (int)(MathF.Sin((float)Raylib.GetTime() * 6f) * 4f);

        // Gewand
        DrawRoundedRectCharSelect(cx - 30, cy + 10 + bob, 60, 75, 10, dark, accent, 2);
        // Hüfte
        DrawRoundedRectCharSelect(cx - 34, cy - 5 + bob, 68, 22, 8, body, accent, 2);
        // Düsen
        Raylib.DrawCircle(cx - 22, cy + 6 + bob, 5, Color.ORANGE);
        Raylib.DrawCircle(cx + 22, cy + 6 + bob, 5, Color.ORANGE);
        // Brustkorb
        DrawRoundedRectCharSelect(cx - 36, cy - 55 + bob, 72, 55, 10, body, accent, 2);
        DrawRoundedRectCharSelect(cx - 26, cy - 50 + bob, 52, 22, 6, light, Color.WHITE, 1);
        // Reaktor
        Raylib.DrawCircle(cx, cy - 30 + bob, 10, accent);
        Raylib.DrawCircle(cx, cy - 30 + bob, 6, Color.ORANGE);
        Raylib.DrawCircle(cx, cy - 30 + bob, 3, Color.WHITE);
        // Schultern
        Raylib.DrawCircle(cx - 40, cy - 45 + bob, 9, accent);
        Raylib.DrawCircle(cx + 40, cy - 45 + bob, 9, accent);
        // Arme
        DrawPyrosPreviewArm(cx - 40, cy - 45 + bob, body, accent, dark, light);
        DrawPyrosPreviewArm(cx + 40, cy - 45 + bob, body, accent, dark, light);
        // Kopf
        DrawPyrosPreviewHead(cx, cy - 70 + bob, body, accent, dark, light);
    }

    private void DrawPyrosPreviewHead(int cx, int cy, Color body, Color accent, Color dark, Color light)
    {
        // Nacken
        DrawRoundedRectCharSelect(cx - 8, cy + 8, 16, 12, 4, dark, accent, 1);
        // Kopf
        Raylib.DrawEllipse(cx, cy, 18, 22, body);
        Raylib.DrawEllipseLines(cx, cy, 18, 22, accent);
        // Augen
        Raylib.DrawCircle(cx + 8, cy - 4, 5, new Color(255, 200, 0, 255));
        Raylib.DrawCircle(cx + 4, cy - 10, 3, new Color(255, 200, 0, 255));
        // Mandibeln
        Raylib.DrawTriangle(new Vector2(cx + 12, cy + 6), new Vector2(cx + 4, cy + 2), new Vector2(cx + 4, cy + 10), accent);
        Raylib.DrawTriangle(new Vector2(cx + 12, cy + 14), new Vector2(cx + 4, cy + 10), new Vector2(cx + 4, cy + 18), accent);
        // Stachel
        Raylib.DrawTriangle(new Vector2(cx - 18, cy), new Vector2(cx - 6, cy - 6), new Vector2(cx - 6, cy + 6), light);
        // Fühler
        float sway = MathF.Sin((float)Raylib.GetTime() * 10f) * 3f;
        Raylib.DrawLine(cx - 2, cy - 18, (int)(cx + 6 + sway), cy - 30, accent);
        Raylib.DrawCircle((int)(cx + 6 + sway), cy - 30, 2, light);
    }

    private void DrawPyrosPreviewArm(int cx, int cy, Color body, Color accent, Color dark, Color light)
    {
        // Schulter
        Raylib.DrawCircle(cx, cy, 7, accent);
        // Oberarm
        DrawRoundedRectCharSelect(cx - 4, cy, 8, 18, 3, body, accent, 1);
        // Ellenbogen
        Raylib.DrawCircle(cx, cy + 18, 5, accent);
        // Unterarm
        DrawRoundedRectCharSelect(cx - 7, cy + 16, 14, 22, 4, body, accent, 1);
        // Klaue
        DrawRoundedRectCharSelect(cx - 6, cy + 36, 12, 12, 3, dark, accent, 1);
        for (int i = -1; i <= 1; i++)
        {
            int clawBaseX = cx + i * 3;
            int clawTipX = clawBaseX + 4;
            Raylib.DrawTriangle(new Vector2(clawTipX, cy + 50), new Vector2(clawBaseX, cy + 44), new Vector2(clawBaseX, cy + 52), light);
        }
    }

    private void DrawPreviewHead(int cx, int cy, RobotType type, Color body, Color accent, Color dark, Color light)
    {
        int headSize = type == RobotType.Flail ? 34 : 30;

        // Nacken
        DrawRoundedRectCharSelect(cx - 10, cy + 10, 20, 14, 5, dark, accent, 1);

        switch (type)
        {
            case RobotType.Jaguar:
                // Katzenkopf
                Raylib.DrawCircle(cx, cy, headSize / 2, accent);
                Raylib.DrawCircle(cx, cy, headSize / 2 - 3, body);
                // Schnauze
                Raylib.DrawEllipse(cx + 6, cy + 5, 10, 7, light);
                Raylib.DrawCircle(cx + 10, cy + 5, 3, dark);
                // Ohren
                Raylib.DrawTriangle(new Vector2(cx + 6, cy - 14), new Vector2(cx + 1, cy - 6), new Vector2(cx + 12, cy - 4), accent);
                Raylib.DrawTriangle(new Vector2(cx - 6, cy - 14), new Vector2(cx - 1, cy - 6), new Vector2(cx - 12, cy - 4), accent);
                break;

            case RobotType.Shadow:
                // Ninja-Kapuze
                DrawRoundedRectCharSelect(cx - headSize / 2, cy - headSize / 2, headSize, headSize, 6, accent, dark, 2);
                DrawRoundedRectCharSelect(cx - headSize / 2 + 4, cy - headSize / 2 + 4, headSize - 8, headSize - 8, 4, body, accent, 1);
                Raylib.DrawRectangle(cx - headSize / 2, cy - 4, headSize, 6, accent);
                break;

            case RobotType.Thorn:
                // Dornenhelm
                Raylib.DrawCircle(cx, cy, headSize / 2, accent);
                Raylib.DrawCircle(cx, cy, headSize / 2 - 4, body);
                Raylib.DrawTriangle(new Vector2(cx, cy - 16), new Vector2(cx - 3, cy - 8), new Vector2(cx + 3, cy - 8), accent);
                Raylib.DrawTriangle(new Vector2(cx + 12, cy - 10), new Vector2(cx + 7, cy - 4), new Vector2(cx + 15, cy - 4), accent);
                Raylib.DrawTriangle(new Vector2(cx - 12, cy - 10), new Vector2(cx - 7, cy - 4), new Vector2(cx - 15, cy - 4), accent);
                break;

            case RobotType.Flail:
                // Boxiger Helm
                DrawRoundedRectCharSelect(cx - headSize / 2, cy - headSize / 2, headSize, headSize, 4, accent, new Color(220, 180, 60, 255), 2);
                DrawRoundedRectCharSelect(cx - headSize / 2 + 5, cy - headSize / 2 + 5, headSize - 10, headSize - 10, 3, body, accent, 1);
                DrawRoundedRectCharSelect(cx - headSize / 2 + 3, cy - headSize / 2 + 3, headSize - 6, 12, 2, dark, new Color(220, 180, 60, 255), 1);
                break;
        }

        // Visor
        Color visorColor = type == RobotType.Shadow ? new Color(200, 50, 255, 255) : Color.SKYBLUE;
        Raylib.DrawRectangle(cx - 12, cy - 4, 24, 8, dark);
        Raylib.DrawRectangle(cx - 10, cy - 3, 20, 6, visorColor);
        Raylib.DrawRectangle(cx - 6, cy - 2, 6, 2, Color.WHITE);
    }

    private void DrawPreviewChestDetail(int cx, int cy, RobotType type, Color body, Color accent, Color dark, Color light)
    {
        switch (type)
        {
            case RobotType.Jaguar:
                Raylib.DrawTriangle(new Vector2(cx, cy - 18), new Vector2(cx - 10, cy + 4), new Vector2(cx + 10, cy + 4), new Color(255, 200, 50, 255));
                Raylib.DrawTriangle(new Vector2(cx, cy - 10), new Vector2(cx - 5, cy + 2), new Vector2(cx + 5, cy + 2), body);
                break;
            case RobotType.Shadow:
                DrawRoundedRectCharSelect(cx - 6, cy - 14, 12, 26, 3, new Color(120, 40, 160, 255), accent, 1);
                for (int i = -2; i <= 2; i++)
                    Raylib.DrawLine(cx - 4, cy + i * 4, cx + 4, cy + i * 4, Color.WHITE);
                break;
            case RobotType.Thorn:
                DrawRoundedRectCharSelect(cx - 8, cy - 14, 16, 26, 4, new Color(100, 80, 40, 255), accent, 1);
                Raylib.DrawCircle(cx, cy, 5, new Color(160, 200, 40, 255));
                break;
            case RobotType.Flail:
                DrawRoundedRectCharSelect(cx - 10, cy - 10, 20, 18, 3, new Color(80, 60, 30, 255), new Color(220, 180, 60, 255), 1);
                for (int i = -1; i <= 1; i += 2)
                    for (int j = -1; j <= 1; j += 2)
                        Raylib.DrawCircle(cx + i * 5, cy + j * 4, 2, new Color(220, 180, 60, 255));
                break;
        }
    }

    private void DrawPreviewArm(int cx, int cy, float w, float h, float bob, RobotType type, Color body, Color accent, Color dark, Color light, bool isLeft)
    {
        // Schulter
        Raylib.DrawCircle(cx, cy, 9, accent);
        // Oberarm
        DrawRoundedRectCharSelect(cx - w / 2, cy, w, h / 2, 5, body, accent, 1);
        // Ellenbogen
        Raylib.DrawCircle(cx, (int)(cy + h / 2), 7, accent);
        // Unterarm
        DrawRoundedRectCharSelect((int)(cx - w * 0.4f), (int)(cy + h / 2 - 2), (int)(w * 0.8f), (int)(h / 2 + 4), 4, body, accent, 1);
        // Hand
        DrawRoundedRectCharSelect(cx - 7, (int)(cy + h - 6), 14, 14, 4, dark, accent, 1);
        // Finger
        for (int i = -1; i <= 1; i++)
        {
            Raylib.DrawRectangle((int)(cx + i * 4 - 1), (int)(cy + h + 4), 2, 6, accent);
            Raylib.DrawTriangle(new Vector2(cx + i * 4, cy + h + 12), new Vector2(cx + i * 4 - 2, cy + h + 8), new Vector2(cx + i * 4 + 2, cy + h + 8), light);
        }
    }

    private void DrawPreviewLeg(int cx, int cy, float w, float h, float bob, RobotType type, Color body, Color accent, Color dark)
    {
        // Hüftgelenk
        Raylib.DrawCircle(cx, cy, 8, accent);
        // Oberschenkel
        DrawRoundedRectCharSelect(cx - w / 2, cy, w, h / 2, 5, body, accent, 1);
        // Knie
        Raylib.DrawCircle(cx, (int)(cy + h / 2), 7, accent);
        // Unterschenkel
        DrawRoundedRectCharSelect((int)(cx - w * 0.4f), (int)(cy + h / 2 - 2), (int)(w * 0.8f), (int)(h / 2 + 4), 4, body, accent, 1);
        // Fuß
        DrawRoundedRectCharSelect((int)(cx - w / 2 - 2), (int)(cy + h - 4), (int)(w + 6), 12, 4, dark, accent, 1);
        // Sohle
        Raylib.DrawRectangle((int)(cx - w / 2), (int)(cy + h + 4), (int)w + 2, 4, accent);
    }

    private void DrawPreviewWeapon(int cx, int cy, RobotType type, Color accent, Color light)
    {
        Color weaponColor = type switch
        {
            RobotType.Jaguar => Color.ORANGE,
            RobotType.Shadow => Color.PURPLE,
            RobotType.Thorn => Color.GREEN,
            RobotType.Flail => Color.GOLD,
            _ => Color.WHITE
        };

        // Griff
        DrawRoundedRectCharSelect(cx - 4, cy - 4, 8, 16, 2, new Color(60, 60, 60, 255), accent, 1);

        // Waffenkörper
        DrawRoundedRectCharSelect(cx, (int)(cy - 6), 45, 12, 3, weaponColor, Color.WHITE, 1);
        Raylib.DrawRectangle((int)(cx + 12), (int)(cy - 4), 18, 8, light);

        switch (type)
        {
            case RobotType.Jaguar:
                Raylib.DrawTriangle(new Vector2(cx + 45, cy), new Vector2(cx + 58, cy - 6), new Vector2(cx + 58, cy + 6), Color.ORANGE);
                Raylib.DrawTriangle(new Vector2(cx + 50, cy), new Vector2(cx + 62, cy - 4), new Vector2(cx + 62, cy + 4), light);
                break;
            case RobotType.Shadow:
                for (int i = 0; i < 3; i++)
                    Raylib.DrawLine((int)(cx + 42 + i * 7), (int)(cy - 4 + i * 2), (int)(cx + 52 + i * 7), (int)(cy + 4 + i * 2), Color.WHITE);
                break;
            case RobotType.Thorn:
                for (int i = 0; i < 4; i++)
                {
                    float sx = cx + 8 + i * 10;
                    Raylib.DrawTriangle(new Vector2(sx, cy - 8), new Vector2(sx - 3, cy + 2), new Vector2(sx + 3, cy + 2), new Color(160, 200, 40, 255));
                    Raylib.DrawTriangle(new Vector2(sx, cy + 10), new Vector2(sx - 3, cy), new Vector2(sx + 3, cy), new Color(160, 200, 40, 255));
                }
                break;
            case RobotType.Flail:
                for (int i = 0; i < 3; i++)
                    Raylib.DrawCircle((int)(cx + 42 + i * 5), cy, 3, Color.GRAY);
                Raylib.DrawCircle((int)(cx + 62), cy, 9, Color.GOLD);
                Raylib.DrawCircle((int)(cx + 62), cy, 9, Color.WHITE);
                Raylib.DrawCircle((int)(cx + 62), cy, 6, light);
                break;
        }
    }

    private void DrawPreviewBackDetail(int cx, int cy, float bob, RobotType type, Color accent, Color light)
    {
        switch (type)
        {
            case RobotType.Jaguar:
                float sway = MathF.Sin((float)Raylib.GetTime() * 8f) * 8f;
                Raylib.DrawLineEx(new Vector2(cx - 28, cy + 30 + bob), new Vector2(cx - 45 + sway, cy + 50 + bob), 5, accent);
                Raylib.DrawLineEx(new Vector2(cx - 45 + sway, cy + 50 + bob), new Vector2(cx - 52 + sway * 1.3f, cy + 68 + bob), 4, accent);
                Raylib.DrawTriangle(new Vector2(cx - 52 + sway * 1.3f, cy + 68 + bob), new Vector2(cx - 58 + sway * 1.3f, cy + 62 + bob), new Vector2(cx - 48 + sway * 1.3f, cy + 62 + bob), light);
                break;
            case RobotType.Shadow:
                float scarfSway = MathF.Sin((float)Raylib.GetTime() * 10f) * 10f;
                Raylib.DrawLineEx(new Vector2(cx - 30, cy - 8 + bob), new Vector2(cx - 55 + scarfSway, cy + 12 + bob), 5, new Color(180, 40, 220, 180));
                Raylib.DrawLineEx(new Vector2(cx - 55 + scarfSway, cy + 12 + bob), new Vector2(cx - 70 + scarfSway * 1.2f, cy + 28 + bob), 4, new Color(180, 40, 220, 140));
                break;
        }
    }

    private void DrawPreviewFrontDetail(int cx, int cy, float bob, RobotType type, Color accent, Color dark, Color light)
    {
        switch (type)
        {
            case RobotType.Thorn:
                for (int i = -1; i <= 1; i += 2)
                {
                    float sx = cx + i * 28f;
                    float sy = cy - 22f + bob;
                    Raylib.DrawTriangle(new Vector2(sx + i * 14f, sy - 8f), new Vector2(sx + i * 6f, sy + 4f), new Vector2(sx - i * 4f, sy - 2f), accent);
                    Raylib.DrawTriangle(new Vector2(sx + i * 12f, sy + 12f), new Vector2(sx + i * 4f, sy + 2f), new Vector2(sx - i * 6f, sy + 8f), accent);
                }
                break;
            case RobotType.Flail:
                for (int i = -1; i <= 1; i += 2)
                {
                    float sx = cx + i * 30f;
                    float sy = cy - 26f + bob;
                    DrawRoundedRectCharSelect(sx - 12f, sy - 10f, 24f, 36f, 6f, accent, new Color(220, 180, 60, 255), 2f);
                    Raylib.DrawCircle((int)(sx - 6f), (int)(sy + 4f), 3f, new Color(220, 180, 60, 255));
                    Raylib.DrawCircle((int)(sx + 6f), (int)(sy + 4f), 3f, new Color(220, 180, 60, 255));
                }
                DrawRoundedRectCharSelect(cx - 27f, cy + 6f + bob, 54f, 16f, 5f, dark, new Color(220, 180, 60, 255), 2f);
                Raylib.DrawCircle((int)cx, (int)(cy + 14f + bob), 10f, new Color(220, 180, 60, 255));
                Raylib.DrawCircle((int)cx, (int)(cy + 14f + bob), 6f, dark);
                break;
        }
    }

    private void DrawRoundedRectCharSelect(float x, float y, float w, float h, float radius, Color fill, Color border, float borderThickness)
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
}
