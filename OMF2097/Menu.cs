using Raylib_cs;

namespace OMF2097;

public class Menu
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    public bool StartGame { get; set; } = false;

    public Menu(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
        {
            StartGame = true;
        }
    }

    public void Draw()
    {
        // Background
        Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(10, 10, 20, 255));

        // Title
        string title = "ONE MUST FALL: 2097";
        int titleSize = 72;
        int titleWidth = Raylib.MeasureText(title, titleSize);
        Raylib.DrawText(title, (_screenWidth - titleWidth) / 2, 120, titleSize, Color.GOLD);

        string subtitle = "TRIBUTE EDITION";
        int subtitleSize = 36;
        int subtitleWidth = Raylib.MeasureText(subtitle, subtitleSize);
        Raylib.DrawText(subtitle, (_screenWidth - subtitleWidth) / 2, 210, subtitleSize, Color.RED);

        // Decorative robot silhouettes
        DrawRobotIcon(180, 360, Color.DARKBLUE, true);
        DrawRobotIcon(_screenWidth - 260, 360, Color.DARKPURPLE, false);

        // Menu options
        string start = "PRESS ENTER TO START";
        int startSize = 28;
        int startWidth = Raylib.MeasureText(start, startSize);
        float pulse = MathF.Sin((float)Raylib.GetTime() * 4f) * 0.5f + 0.5f;
        Color pulseColor = new Color(255, 255, 255, (int)(150 + 105 * pulse));
        Raylib.DrawText(start, (_screenWidth - startWidth) / 2, 420, startSize, pulseColor);

        string controls = "P1: WASD + SPACE/F    P2: ARROWS + ENTER/RSHIFT    BLOCK: S / DOWN";
        int controlsSize = 20;
        int controlsWidth = Raylib.MeasureText(controls, controlsSize);
        Raylib.DrawText(controls, (_screenWidth - controlsWidth) / 2, 520, controlsSize, Color.LIGHTGRAY);

        string credits = "A fan tribute to the classic Epic MegaGames fighter";
        int creditsSize = 18;
        int creditsWidth = Raylib.MeasureText(credits, creditsSize);
        Raylib.DrawText(credits, (_screenWidth - creditsWidth) / 2, 620, creditsSize, Color.GRAY);
    }

    private void DrawRobotIcon(int x, int y, Color color, bool facingRight)
    {
        int w = 80;
        int h = 120;
        Raylib.DrawRectangle(x, y, w, h, color);
        Raylib.DrawRectangleLines(x, y, w, h, Color.WHITE);

        int headX = facingRight ? x + w - 25 : x + 5;
        Raylib.DrawRectangle(headX, y + 10, 25, 25, Color.LIGHTGRAY);

        int visorX = facingRight ? headX + 10 : headX;
        Raylib.DrawRectangle(visorX, y + 18, 15, 6, new Color(0, 255, 255, 255));

        Raylib.DrawRectangle(x + 10, y + 45, w - 20, 30, Color.GRAY);
        Raylib.DrawCircle(x + w / 2, y + 60, 8, Color.RED);
    }
}
