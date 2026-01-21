using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace TesseraeBlocks
{
    internal static class App
    {
        private const int Width = 10;
        private const int Height = 20;
        private const string HighScoreKey = "tss-blocks-highscore";

        private static string[,] _grid = new string[Width, Height];
        private static Piece _currentPiece;
        private static double _gameInterval;
        private static int _score;
        private static int _highScore;
        private static bool _isPaused;
        private static bool _isGameOver;

        private static SettableObservable<int> _scoreObservable = new SettableObservable<int>(0);
        private static SettableObservable<bool> _gameStatusObservable = new SettableObservable<bool>(false);

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            _highScore = int.TryParse(localStorage.getItem(HighScoreKey), out var h) ? h : 0;

            var gameBoard = Raw();

            void Render()
            {
                var boardEl = Div(_("game-board"));
                boardEl.style.display = "grid";
                boardEl.style.gridTemplateColumns = $"repeat({Width}, 30px)";
                boardEl.style.gridTemplateRows = $"repeat({Height}, 30px)";
                boardEl.style.gap = "1px";
                boardEl.style.backgroundColor = "#edebe9";
                boardEl.style.border = "4px solid #323130";

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var cell = Div(_("game-cell"));
                        cell.style.width = "30px";
                        cell.style.height = "30px";

                        var color = _grid[x, y];
                        if (_currentPiece != null && _currentPiece.IsAt(x, y))
                        {
                            color = _currentPiece.Color;
                        }

                        cell.style.backgroundColor = string.IsNullOrEmpty(color) ? "white" : color;
                        if (!string.IsNullOrEmpty(color)) cell.classList.add("filled");
                        boardEl.appendChild(cell);
                    }
                }
                gameBoard.Content(Raw(boardEl));
            }

            void Tick()
            {
                if (_isPaused || _isGameOver) return;

                if (!MoveDown())
                {
                    LockPiece();
                    ClearLines();
                    if (!SpawnPiece())
                    {
                        GameOver();
                    }
                }
                Render();
            }

            void StartGame()
            {
                _grid = new string[Width, Height];
                _score = 0;
                _scoreObservable.Value = 0;
                _isGameOver = false;
                _gameStatusObservable.Value = true;
                SpawnPiece();
                Render();

                if (_gameInterval != 0) window.clearInterval(_gameInterval);
                _gameInterval = window.setInterval(_ => Tick(), 500);
            }

            window.onkeydown += e => {
                if (_isGameOver) return;
                var key = e.key;
                if (key == "ArrowLeft" || key == "a") { MoveLeft(); Render(); }
                if (key == "ArrowRight" || key == "d") { MoveRight(); Render(); }
                if (key == "ArrowDown" || key == "s") { MoveDown(); Render(); }
                if (key == "ArrowUp" || key == "w") { Rotate(); Render(); }
                if (key == " ") { HardDrop(); Render(); }
                if (key == "p") _isPaused = !_isPaused;
            };

            var statsArea = VStack().W(200).P(16).Class("game-stats").Children(
                TextBlock("Score").Small().Secondary().Bold(),
                Defer(_scoreObservable, s => Task.FromResult((IComponent)TextBlock(s.ToString()).XXLarge().Bold())),
                TextBlock("High Score").Small().Secondary().Bold().MT(16),
                TextBlock(_highScore.ToString()).Large().SemiBold(),
                Button("New Game").Primary().MT(32).OnClick((_, __) => StartGame()),
                TextBlock("Controls").Small().Secondary().Bold().MT(32),
                TextBlock("Arrows / WASD: Move & Rotate").Small(),
                TextBlock("Space: Hard Drop").Small(),
                TextBlock("P: Pause").Small()
            );

            var page = HStack().S().AlignCenter().JustifyCenter().Children(
                gameBoard.W(310).H(Height * 31),
                statsArea.ML(32)
            );

            document.body.appendChild(page.Render());
            Render(); // Initial empty render
        }

        private static bool SpawnPiece()
        {
            _currentPiece = Piece.CreateRandom();
            if (IsCollision(_currentPiece.X, _currentPiece.Y, _currentPiece.Shape))
            {
                return false;
            }
            return true;
        }

        private static void LockPiece()
        {
            foreach (var pos in _currentPiece.GetAbsolutePositions())
            {
                if (pos.y >= 0) _grid[pos.x, pos.y] = _currentPiece.Color;
            }
        }

        private static void ClearLines()
        {
            int linesCleared = 0;
            for (int y = Height - 1; y >= 0; y--)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                {
                    if (string.IsNullOrEmpty(_grid[x, y])) { full = false; break; }
                }

                if (full)
                {
                    linesCleared++;
                    for (int moveY = y; moveY > 0; moveY--)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            _grid[x, moveY] = _grid[x, moveY - 1];
                        }
                    }
                    for (int x = 0; x < Width; x++) _grid[x, 0] = null;
                    y++; // Check the same row again
                }
            }

            if (linesCleared > 0)
            {
                _score += (int)(Math.Pow(2, linesCleared - 1) * 100);
                _scoreObservable.Value = _score;
                if (_score > _highScore)
                {
                    _highScore = _score;
                    localStorage.setItem(HighScoreKey, _highScore.ToString());
                }
            }
        }

        private static bool MoveDown()
        {
            if (!IsCollision(_currentPiece.X, _currentPiece.Y + 1, _currentPiece.Shape))
            {
                _currentPiece.Y++;
                return true;
            }
            return false;
        }

        private static void MoveLeft()
        {
            if (!IsCollision(_currentPiece.X - 1, _currentPiece.Y, _currentPiece.Shape)) _currentPiece.X--;
        }

        private static void MoveRight()
        {
            if (!IsCollision(_currentPiece.X + 1, _currentPiece.Y, _currentPiece.Shape)) _currentPiece.X++;
        }

        private static void Rotate()
        {
            var newShape = _currentPiece.GetRotatedShape();
            if (!IsCollision(_currentPiece.X, _currentPiece.Y, newShape))
            {
                _currentPiece.Shape = newShape;
            }
        }

        private static void HardDrop()
        {
            while (MoveDown()) { }
        }

        private static bool IsCollision(int nx, int ny, int[,] shape)
        {
            for (int sy = 0; sy < shape.GetLength(0); sy++)
            {
                for (int sx = 0; sx < shape.GetLength(1); sx++)
                {
                    if (shape[sy, sx] != 0)
                    {
                        int gx = nx + sx;
                        int gy = ny + sy;
                        if (gx < 0 || gx >= Width || gy >= Height) return true;
                        if (gy >= 0 && !string.IsNullOrEmpty(_grid[gx, gy])) return true;
                    }
                }
            }
            return false;
        }

        private static void GameOver()
        {
            _isGameOver = true;
            window.clearInterval(_gameInterval);
            _gameStatusObservable.Value = false;
            Dialog("Game Over", $"Your final score is {_score}").Ok(() => { });
        }
    }

    public class Piece
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; }
        public int[,] Shape { get; set; }

        public static Piece CreateRandom()
        {
            var random = new Random();
            var index = random.Next(Shapes.Length);
            return new Piece
            {
                X = 3,
                Y = 0,
                Color = Colors[index],
                Shape = Shapes[index]
            };
        }

        public bool IsAt(int gx, int gy)
        {
            for (int sy = 0; sy < Shape.GetLength(0); sy++)
            {
                for (int sx = 0; sx < Shape.GetLength(1); sx++)
                {
                    if (Shape[sy, sx] != 0 && X + sx == gx && Y + sy == gy) return true;
                }
            }
            return false;
        }

        public IEnumerable<(int x, int y)> GetAbsolutePositions()
        {
            for (int sy = 0; sy < Shape.GetLength(0); sy++)
            {
                for (int sx = 0; sx < Shape.GetLength(1); sx++)
                {
                    if (Shape[sy, sx] != 0) yield return (X + sx, Y + sy);
                }
            }
        }

        public int[,] GetRotatedShape()
        {
            int r = Shape.GetLength(0);
            int c = Shape.GetLength(1);
            int[,] newShape = new int[c, r];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    newShape[j, r - 1 - i] = Shape[i, j];
                }
            }
            return newShape;
        }

        private static readonly string[] Colors = { "#00f0f0", "#0000f0", "#f0a000", "#f0f000", "#00f000", "#a000f0", "#f00000" };
        private static readonly int[][,] Shapes = {
            new int[,] { { 1, 1, 1, 1 } }, // I
            new int[,] { { 1, 0, 0 }, { 1, 1, 1 } }, // J
            new int[,] { { 0, 0, 1 }, { 1, 1, 1 } }, // L
            new int[,] { { 1, 1 }, { 1, 1 } }, // O
            new int[,] { { 0, 1, 1 }, { 1, 1, 0 } }, // S
            new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }, // T
            new int[,] { { 1, 1, 0 }, { 0, 1, 1 } }  // Z
        };
    }
}
