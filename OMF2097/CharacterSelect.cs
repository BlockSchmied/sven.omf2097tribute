using Raylib_cs;
using System.Numerics;

namespace OMF2097;

/// <summary>
/// Zeigt die animierte Charakterauswahl.
/// Statt eigener Vorschau-Methoden werden echte Robot-Instanzen verwendet,
/// sodass Änderungen an den Robotermodellen automatisch im Auswahlbild sichtbar werden.
/// </summary>
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

    private Robot _p1PreviewRobot;
    private Robot _p2PreviewRobot;

    public CharacterSelect(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _p1PreviewRobot = RobotFactory.Create(RobotType.Jaguar, true, new Vector2(0, 0));
        _p2PreviewRobot = RobotFactory.Create(RobotType.Shadow, false, new Vector2(0, 0));
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

        DrawPanel(120, 140, 420, 460, "PLAYER 1", _p1Index, _p1Ready, Color.BLUE, true);
        DrawPanel(_screenWidth - 540, 140, 420, 460, "PLAYER 2", _p2Index, _p2Ready, Color.RED, false);

        string hint = "P1: A/D choose, W confirm, F punch, SPACE kick  |  P2: LEFT/RIGHT choose, UP confirm, ENTER punch, RCTRL kick";
        int hintSize = 20;
        int hintWidth = Raylib.MeasureText(hint, hintSize);
        Raylib.DrawText(hint, (_screenWidth - hintWidth) / 2, 640, hintSize, Color.LIGHTGRAY);
    }

    private void DrawPanel(int x, int y, int w, int h, string label, int index, bool ready, Color accent, bool isPlayer1)
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

        DrawRobotPreview(x + w / 2, y + 190, type, isPlayer1);

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

    private void DrawRobotPreview(int cx, int cy, RobotType type, bool isPlayer1)
    {
        Robot previewRobot = isPlayer1 ? _p1PreviewRobot : _p2PreviewRobot;

        if (previewRobot.Type != type)
        {
            previewRobot = RobotFactory.Create(type, isPlayer1, new Vector2(cx, cy + previewRobot.Height / 2f + 40f));
            if (isPlayer1)
                _p1PreviewRobot = previewRobot;
            else
                _p2PreviewRobot = previewRobot;
        }

        // Position und Ausrichtung für die Vorschau setzen
        previewRobot.Position = new Vector2(cx, cy + previewRobot.Height / 2f + 40f);
        previewRobot.FacingRight = isPlayer1;
        previewRobot.Reset(previewRobot.Position);

        // Animation: leichtes Laufen/Bobbing simulieren
        previewRobot.Update(0.016f);

        // Zeichne den echten Roboter
        previewRobot.Draw();
    }
}
