using System.Numerics;
using Raylib_cs;

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
    private readonly string[] _names = { "JAGUAR", "SHADOW", "THORN", "FLAIL" };
    private readonly string[] _descriptions =
    {
        "Balanced striker with quick claws",
        "Fast ninja with shadow strikes",
        "Heavy bruiser with thorny armor",
        "Slow powerhouse with brutal flails"
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

        string hint = "P1: A/D choose, W confirm, SPACE punch, F kick  |  P2: LEFT/RIGHT choose, UP confirm, ENTER punch, RSHIFT kick";
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

        DrawRobotPreview(x + w / 2, y + 180, type);

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
        Color body = type switch
        {
            RobotType.Jaguar => new Color(255, 100, 30, 255),
            RobotType.Shadow => new Color(60, 60, 80, 255),
            RobotType.Thorn => new Color(40, 140, 60, 255),
            RobotType.Flail => new Color(180, 160, 40, 255),
            _ => Color.GRAY
        };

        Color accent = type switch
        {
            RobotType.Jaguar => new Color(80, 80, 80, 255),
            RobotType.Shadow => new Color(180, 40, 220, 255),
            RobotType.Thorn => new Color(160, 200, 40, 255),
            RobotType.Flail => new Color(120, 60, 20, 255),
            _ => Color.WHITE
        };

        // Body
        Raylib.DrawRectangle(cx - 40, cy - 60, 80, 100, body);
        Raylib.DrawRectangleLines(cx - 40, cy - 60, 80, 100, accent);

        // Head
        switch (type)
        {
            case RobotType.Jaguar:
                Raylib.DrawCircle(cx, cy - 80, 25, accent);
                Raylib.DrawCircle(cx, cy - 80, 20, body);
                Raylib.DrawTriangle(new Vector2(cx + 10, cy - 95), new Vector2(cx + 2, cy - 88), new Vector2(cx + 18, cy - 86), accent);
                break;
            case RobotType.Shadow:
                Raylib.DrawRectangle(cx - 25, cy - 105, 50, 50, accent);
                Raylib.DrawRectangle(cx - 20, cy - 100, 40, 40, body);
                break;
            case RobotType.Thorn:
                Raylib.DrawCircle(cx, cy - 80, 25, accent);
                Raylib.DrawCircle(cx, cy - 80, 20, body);
                Raylib.DrawTriangle(new Vector2(cx + 20, cy - 94), new Vector2(cx + 10, cy - 86), new Vector2(cx + 26, cy - 86), accent);
                Raylib.DrawTriangle(new Vector2(cx - 20, cy - 94), new Vector2(cx - 10, cy - 86), new Vector2(cx - 26, cy - 86), accent);
                break;
            case RobotType.Flail:
                Raylib.DrawRectangle(cx - 28, cy - 108, 56, 56, accent);
                Raylib.DrawRectangle(cx - 22, cy - 102, 44, 44, body);
                break;
        }

        // Visor
        Raylib.DrawRectangle(cx - 15, cy - 85, 30, 10, Color.SKYBLUE);

        // Chest core
        Raylib.DrawCircle(cx, cy - 20, 12, accent);
        Raylib.DrawCircle(cx, cy - 20, 6, Color.WHITE);

        // Type detail
        switch (type)
        {
            case RobotType.Jaguar:
                Raylib.DrawTriangle(new Vector2(cx, cy - 28), new Vector2(cx - 8, cy - 8), new Vector2(cx + 8, cy - 8), new Color(255, 200, 50, 255));
                break;
            case RobotType.Shadow:
                Raylib.DrawRectangle(cx - 6, cy - 24, 12, 16, new Color(120, 40, 160, 255));
                break;
            case RobotType.Thorn:
                Raylib.DrawRectangle(cx - 8, cy - 26, 16, 20, new Color(100, 80, 40, 255));
                break;
            case RobotType.Flail:
                Raylib.DrawRectangle(cx - 10, cy - 22, 20, 14, new Color(80, 60, 30, 255));
                break;
        }
    }
}
