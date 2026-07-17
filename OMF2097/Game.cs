using System.Numerics;
using Raylib_cs;

namespace OMF2097;

public enum GameState
{
    Menu,
    CharacterSelect,
    Fighting,
    RoundOver,
    MatchOver
}

public class Game
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private readonly Arena _arena;
    private Robot _player1;
    private Robot _player2;
    private readonly Camera2D _camera;
    private readonly Menu _menu;
    private readonly CharacterSelect _characterSelect;

    private GameState _state = GameState.Menu;
    private int _round = 1;
    private int _p1Wins = 0;
    private int _p2Wins = 0;
    private const int WinsNeeded = 2;
    private float _roundOverTimer = 0f;
    private string _roundResultText = "";

    public Game(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _arena = new Arena(screenWidth, screenHeight);

        _player1 = RobotFactory.Create(RobotType.Jaguar, true, new Vector2(300, _arena.FloorY));
        _player2 = RobotFactory.Create(RobotType.Shadow, false, new Vector2(screenWidth - 300, _arena.FloorY));
        _p1LastGroundX = _player1.Position;
        _p2LastGroundX = _player2.Position;
        _characterSelect = new CharacterSelect(screenWidth, screenHeight);

        _camera = new Camera2D
        {
            target = new Vector2(screenWidth / 2f, screenHeight / 2f),
            offset = new Vector2(screenWidth / 2f, screenHeight / 2f),
            rotation = 0f,
            zoom = 1.0f
        };

        _menu = new Menu(screenWidth, screenHeight);
    }

    public void Update(float dt)
    {
        switch (_state)
        {
            case GameState.Menu:
                _menu.Update(dt);
                if (_menu.StartGame)
                {
                    _state = GameState.CharacterSelect;
                    _menu.StartGame = false;
                }
                break;

            case GameState.CharacterSelect:
                _characterSelect.Update(dt);
                if (_characterSelect.Confirmed)
                {
                    ResetMatch(_characterSelect.Player1Type, _characterSelect.Player2Type);
                    _state = GameState.Fighting;
                    _characterSelect.Confirmed = false;
                }
                break;

            case GameState.Fighting:
                UpdateFight(dt);
                break;

            case GameState.RoundOver:
                _roundOverTimer -= dt;
                if (_roundOverTimer <= 0f)
                {
                    if (_p1Wins >= WinsNeeded || _p2Wins >= WinsNeeded)
                    {
                        _state = GameState.MatchOver;
                    }
                    else
                    {
                        _round++;
                        ResetRound();
                        _state = GameState.Fighting;
                    }
                }
                break;

            case GameState.MatchOver:
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                {
                    _state = GameState.Menu;
                    _p1Wins = 0;
                    _p2Wins = 0;
                    _round = 1;
                }
                break;
        }
    }

    private void UpdateFight(float dt)
    {
        _player1.HandleInput(true);
        _player2.HandleInput(false);

        _player1.Update(dt);
        _player2.Update(dt);

        KeepInBounds(_player1);
        KeepInBounds(_player2);

        ResolveCollision(_player1, _player2);

        AutoTurnAfterJumpOver(_player1, _player2, ref _p1LastGroundX);
        AutoTurnAfterJumpOver(_player2, _player1, ref _p2LastGroundX);

        if (_player1.IsAttacking && _player1.HitboxActive && _player1.AttackHitbox.Intersects(_player2.Hurtbox))
        {
            _player2.TakeDamage(_player1.AttackDamage, _player1.FacingRight ? 1 : -1, _player1.FacingRight);
            _player1.HitboxActive = false;
        }

        if (_player2.IsAttacking && _player2.HitboxActive && _player2.AttackHitbox.Intersects(_player1.Hurtbox))
        {
            _player1.TakeDamage(_player2.AttackDamage, _player2.FacingRight ? 1 : -1, _player2.FacingRight);
            _player2.HitboxActive = false;
        }

        if (_player1.Health <= 0 || _player2.Health <= 0)
        {
            EndRound();
        }
    }

    private void KeepInBounds(Robot robot)
    {
        robot.Position = new Vector2(
            Math.Clamp(robot.Position.X, 80, _screenWidth - 80),
            robot.Position.Y
        );
    }

    private void ResolveCollision(Robot a, Robot b)
    {
        bool aAirborne = a.Position.Y < _arena.FloorY - 1f;
        bool bAirborne = b.Position.Y < _arena.FloorY - 1f;

        // Wenn ein Roboter springt und horizontal über dem anderen ist,
        // wird keine horizontale Kollision aufgelöst, damit er übergehen kann.
        if (aAirborne || bAirborne)
        {
            float horizontalDistance = Math.Abs(a.Position.X - b.Position.X);
            float minHorizontalDistance = a.Width / 2f + b.Width / 2f;

            if (horizontalDistance < minHorizontalDistance)
            {
                bool aAboveB = a.Position.Y < b.Position.Y;
                bool bAboveA = b.Position.Y < a.Position.Y;

                if ((aAirborne && aAboveB) || (bAirborne && bAboveA))
                    return;
            }
        }

        float distance = a.Position.X - b.Position.X;
        float minDistance = a.Width / 2f + b.Width / 2f;

        if (Math.Abs(distance) < minDistance)
        {
            float overlap = minDistance - Math.Abs(distance);
            float direction = distance > 0 ? 1 : -1;
            float shift = overlap * 0.5f * direction;

            a.Position = new Vector2(a.Position.X + shift, a.Position.Y);
            b.Position = new Vector2(b.Position.X - shift, b.Position.Y);
        }

        a.FacingRight = a.Position.X < b.Position.X;
        b.FacingRight = b.Position.X < a.Position.X;
    }

    private Vector2 _p1LastGroundX;
    private Vector2 _p2LastGroundX;

    private void AutoTurnAfterJumpOver(Robot jumper, Robot other, ref Vector2 lastGroundX)
    {
        if (jumper.State == RobotState.Jump)
        {
            bool wasLeftOfOther = lastGroundX.X < other.Position.X;
            bool nowRightOfOther = jumper.Position.X > other.Position.X;

            if (wasLeftOfOther && nowRightOfOther)
            {
                jumper.FacingRight = false;
            }
            else if (!wasLeftOfOther && !nowRightOfOther)
            {
                jumper.FacingRight = true;
            }
        }
        else if (jumper.Position.Y >= _arena.FloorY - 1f)
        {
            lastGroundX = jumper.Position;
        }
    }

    private void EndRound()
    {
        if (_player1.Health <= 0)
        {
            _p2Wins++;
            _roundResultText = "PLAYER 2 WINS ROUND";
        }
        else
        {
            _p1Wins++;
            _roundResultText = "PLAYER 1 WINS ROUND";
        }

        _roundOverTimer = 2.5f;
        _state = GameState.RoundOver;
    }

    private void ResetMatch(RobotType p1Type, RobotType p2Type)
    {
        _p1Wins = 0;
        _p2Wins = 0;
        _round = 1;
        _player1 = RobotFactory.Create(p1Type, true, new Vector2(300, _arena.FloorY));
        _player2 = RobotFactory.Create(p2Type, false, new Vector2(_screenWidth - 300, _arena.FloorY));
        _p1LastGroundX = _player1.Position;
        _p2LastGroundX = _player2.Position;
    }

    private void ResetRound()
    {
        _player1.Reset(new Vector2(300, _arena.FloorY));
        _player2.Reset(new Vector2(_screenWidth - 300, _arena.FloorY));
        _p1LastGroundX = _player1.Position;
        _p2LastGroundX = _player2.Position;
    }

    public void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.BLACK);

        switch (_state)
        {
            case GameState.Menu:
                _menu.Draw();
                break;

            case GameState.CharacterSelect:
                _characterSelect.Draw();
                break;

            case GameState.Fighting:
            case GameState.RoundOver:
                DrawFight();
                break;

            case GameState.MatchOver:
                DrawFight();
                DrawMatchOver();
                break;
        }

        Raylib.EndDrawing();
    }

    private void DrawFight()
    {
        Raylib.BeginMode2D(_camera);
        _arena.Draw();
        _player1.Draw();
        _player2.Draw();
        Raylib.EndMode2D();

        DrawHud();

        if (_state == GameState.RoundOver)
        {
            int textWidth = Raylib.MeasureText(_roundResultText, 40);
            Raylib.DrawText(_roundResultText, (_screenWidth - textWidth) / 2, _screenHeight / 2 - 20, 40, Color.YELLOW);
        }
    }

    private void DrawHud()
    {
        const int barWidth = 400;
        const int barHeight = 24;
        const int y = 20;

        int p1HealthWidth = (int)(_player1.Health / 100f * barWidth);
        int p2HealthWidth = (int)(_player2.Health / 100f * barWidth);

        Raylib.DrawRectangle(40, y, barWidth, barHeight, Color.DARKGRAY);
        Raylib.DrawRectangle(40 + barWidth - p1HealthWidth, y, p1HealthWidth, barHeight, Color.RED);
        Raylib.DrawRectangleLines(40, y, barWidth, barHeight, Color.WHITE);

        Raylib.DrawRectangle(_screenWidth - 40 - barWidth, y, barWidth, barHeight, Color.DARKGRAY);
        Raylib.DrawRectangle(_screenWidth - 40 - barWidth, y, p2HealthWidth, barHeight, Color.RED);
        Raylib.DrawRectangleLines(_screenWidth - 40 - barWidth, y, barWidth, barHeight, Color.WHITE);

        string p1Name = RobotName(_player1.Type);
        string p2Name = RobotName(_player2.Type);
        int p1NameWidth = Raylib.MeasureText(p1Name, 20);
        int p2NameWidth = Raylib.MeasureText(p2Name, 20);

        Raylib.DrawText(p1Name, 40 + (barWidth - p1NameWidth) / 2, y + 30, 20, Color.WHITE);
        Raylib.DrawText(p2Name, _screenWidth - 40 - barWidth + (barWidth - p2NameWidth) / 2, y + 30, 20, Color.WHITE);

        Raylib.DrawText("P1", 40, y + 54, 18, Color.GRAY);
        Raylib.DrawText("P2", _screenWidth - 70, y + 54, 18, Color.GRAY);

        string roundText = $"ROUND {_round}";
        int roundWidth = Raylib.MeasureText(roundText, 24);
        Raylib.DrawText(roundText, (_screenWidth - roundWidth) / 2, y + 30, 24, Color.WHITE);

        string winsText = $"{_p1Wins} - {_p2Wins}";
        int winsWidth = Raylib.MeasureText(winsText, 30);
        Raylib.DrawText(winsText, (_screenWidth - winsWidth) / 2, y + 60, 30, Color.YELLOW);
    }

    private void DrawMatchOver()
    {
        string winnerText = _p1Wins >= WinsNeeded ? "PLAYER 1 WINS THE MATCH" : "PLAYER 2 WINS THE MATCH";
        int textWidth = Raylib.MeasureText(winnerText, 50);

        Raylib.DrawRectangle(0, _screenHeight / 2 - 60, _screenWidth, 120, new Color(0, 0, 0, 200));
        Raylib.DrawText(winnerText, (_screenWidth - textWidth) / 2, _screenHeight / 2 - 25, 50, Color.GOLD);

        string continueText = "PRESS ENTER TO CONTINUE";
        int continueWidth = Raylib.MeasureText(continueText, 24);
        Raylib.DrawText(continueText, (_screenWidth - continueWidth) / 2, _screenHeight / 2 + 40, 24, Color.WHITE);
    }

    private static string RobotName(RobotType type) => type switch
    {
        RobotType.Jaguar => "JAGUAR",
        RobotType.Shadow => "SHADOW",
        RobotType.Thorn => "THORN",
        RobotType.Flail => "FLAIL",
        RobotType.Pyros => "PYROS",
        _ => "UNKNOWN"
    };
}
