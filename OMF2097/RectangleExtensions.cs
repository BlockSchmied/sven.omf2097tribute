using Raylib_cs;

namespace OMF2097;

public static class RectangleExtensions
{
    public static bool Intersects(this Rectangle a, Rectangle b)
    {
        return a.x < b.x + b.width &&
               a.x + a.width > b.x &&
               a.y < b.y + b.height &&
               a.y + a.height > b.y;
    }
}
