using System.Numerics;
using Raylib_cs;

namespace OMF2097;

public class Arena
{
    public int Width { get; }
    public int Height { get; }
    public float FloorY { get; }

    private readonly Color _skyTop;
    private readonly Color _skyBottom;
    private readonly Color _floorColor;
    private readonly Color _gridColor;

    public Arena(int width, int height)
    {
        Width = width;
        Height = height;
        FloorY = height - 160f;

        _skyTop = new Color(10, 10, 30, 255);
        _skyBottom = new Color(60, 30, 60, 255);
        _floorColor = new Color(40, 40, 50, 255);
        _gridColor = new Color(80, 80, 100, 255);
    }

    public void Draw()
    {
        DrawSky();
        DrawBackgroundCity();
        DrawFloor();
        DrawGrid();
    }

    private void DrawSky()
    {
        for (int y = 0; y < Height / 2; y++)
        {
            float t = y / (Height / 2f);
            Color c = LerpColor(_skyTop, _skyBottom, t);
            Raylib.DrawLine(0, y, Width, y, c);
        }
    }

    private void DrawBackgroundCity()
    {
        // Distant skyline silhouette
        Color cityColor = new Color(20, 20, 30, 255);
        int baseY = (int)FloorY - 120;

        Raylib.DrawRectangle(0, baseY, Width, 120, cityColor);

        // Skyscrapers
        int[] buildingWidths = { 80, 120, 60, 150, 90, 70, 110, 50 };
        int x = -40;
        foreach (int bw in buildingWidths)
        {
            int bh = 60 + (x * 13) % 90;
            Raylib.DrawRectangle(x, baseY - bh, bw, bh + 120, cityColor);
            Raylib.DrawRectangleLines(x, baseY - bh, bw, bh + 120, new Color(40, 40, 55, 255));
            x += bw + 10;
        }

        // Neon rings (OMF 2097 style)
        for (int i = 0; i < 5; i++)
        {
            int ringX = 150 + i * 250;
            int ringY = baseY - 40;
            Raylib.DrawCircleLines(ringX, ringY, 30f, new Color(0, 255, 200, 120));
            Raylib.DrawCircleLines(ringX, ringY, 45f, new Color(255, 0, 100, 100));
        }
    }

    private void DrawFloor()
    {
        Raylib.DrawRectangle(0, (int)FloorY, Width, Height - (int)FloorY, _floorColor);
        Raylib.DrawRectangle(0, (int)FloorY, Width, 8, new Color(120, 120, 140, 255));
    }

    private void DrawGrid()
    {
        // Perspective grid lines
        float horizonY = FloorY - 120;
        int spacing = 80;

        for (int x = 0; x < Width; x += spacing)
        {
            Raylib.DrawLine(x, (int)FloorY, x + (x - Width / 2) / 4, (int)horizonY, _gridColor);
        }

        for (int y = 0; y < 6; y++)
        {
            float t = y / 6f;
            float lineY = Lerp(FloorY, horizonY, t);
            Raylib.DrawLine(0, (int)lineY, Width, (int)lineY, _gridColor);
        }
    }

    private static Color LerpColor(Color a, Color b, float t)
    {
        return new Color(
            (int)(a.r + (b.r - a.r) * t),
            (int)(a.g + (b.g - a.g) * t),
            (int)(a.b + (b.b - a.b) * t),
            255
        );
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
