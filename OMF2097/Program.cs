using System.Numerics;
using Raylib_cs;

namespace OMF2097;

class Program
{
    static void Main(string[] args)
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;
        const int targetFps = 60;

        Raylib.InitWindow(screenWidth, screenHeight, "One Must Fall: 2097 - Tribute");
        Raylib.SetTargetFPS(targetFps);
        Raylib.InitAudioDevice();

        var game = new Game(screenWidth, screenHeight);

        while (!Raylib.WindowShouldClose())
        {
            game.Update(Raylib.GetFrameTime());
            game.Draw();
        }

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}
