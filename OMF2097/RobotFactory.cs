using System.Numerics;

namespace OMF2097;

/// <summary>
/// Erzeugt die passende Robotersubklasse anhand des gewählten RobotType.
/// Zentrale Stelle, um neue Roboter hinzuzufügen, ohne den Rest der Logik anzufassen.
/// </summary>
public static class RobotFactory
{
    public static Robot Create(RobotType type, bool isPlayer1, Vector2 startPosition) => type switch
    {
        RobotType.Jaguar => new Robots.JaguarRobot(isPlayer1, startPosition),
        RobotType.Shadow => new Robots.ShadowRobot(isPlayer1, startPosition),
        RobotType.Thorn => new Robots.ThornRobot(isPlayer1, startPosition),
        RobotType.Flail => new Robots.FlailRobot(isPlayer1, startPosition),
        RobotType.Pyros => new Robots.PyrosRobot(isPlayer1, startPosition),
        _ => new Robots.JaguarRobot(isPlayer1, startPosition)
    };
}
