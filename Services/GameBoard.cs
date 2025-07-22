using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace minesweeper
{
    public class GameBoard
    {
        private Cell[,] _board;
        public Cell[,] Board => _board;
        private int[,] _adjacentFields;
        private UniformGrid _gameGrid;
        private System.Windows.RoutedEventHandler _cellClick;
        private System.Windows.Input.MouseButtonEventHandler _cellRightClick;
        private Label _labelRowCol;
        private Label _labelScore;

        public GameBoard(Cell[,] board, int[,] adjacentFields, UniformGrid gameGrid, System.Windows.RoutedEventHandler cellClick, System.Windows.Input.MouseButtonEventHandler cellRightClick, Label labelRowCol, Label labelScore) { 
            _board = board;
            _adjacentFields = adjacentFields;
            _gameGrid = gameGrid;
            _cellClick = cellClick;
            _cellRightClick = cellRightClick;
            _labelRowCol = labelRowCol;
            _labelScore = labelScore;
        }
        public void CreateEmptyBoard()
        {
            _board = new Cell[_gameGrid.Rows, _gameGrid.Columns];
            for (int i = 0; i < _gameGrid.Rows; i++)
            {
                for (int j = 0; j < _gameGrid.Columns; j++)
                {
                    _board[i, j] = new Cell();
                }
            }
        }

        public void CreateGrid()
        {
            for (int i = 0; i < _gameGrid.Rows; i++)
            {
                for (int j = 0; j < _gameGrid.Columns; j++)
                {
                    Button button = new Button();
                    button.Content = "";
                    button.FontSize = 16;
                    button.Tag = (i, j);
                    button.Click += _cellClick;
                    button.MouseRightButtonUp += _cellRightClick;

                    _gameGrid.Children.Add(button);

                    _labelRowCol.Content = _gameGrid.Rows + "x" + _gameGrid.Columns;
                }
            }
        }
        public void CreateField(int numberOfMines)
        {
            Random r = new Random();
            int placed = 0;
            while (placed < numberOfMines)
            {
                int mine_row = r.Next(_gameGrid.Rows);
                int mine_col = r.Next(_gameGrid.Columns);
                if (!_board[mine_row, mine_col].IsMine)
                {
                    _board[mine_row, mine_col].Type = CellType.MINE;
                    placed++;
                }
            }

            for (int i = 0; i < _gameGrid.Rows; i++)
            {
                for (int j = 0; j < _gameGrid.Columns; j++)
                {
                    if (_board[i, j].IsMine)
                    {
                        continue;
                    }

                    int sum = 0;

                    for (int k = 0; k < _adjacentFields.GetLength(0); k++)
                    {
                        int ni = i + _adjacentFields[k, 0];
                        int nj = j + _adjacentFields[k, 1];
                        if (ni >= 0 && ni < _gameGrid.Rows && nj >= 0 && nj < _gameGrid.Columns)
                        {
                            if (_board[ni, nj].IsMine)
                                sum++;
                        }
                    }
                    _board[i, j].AdjacentMines = sum;
                }
            }
        }
        public async void Propagate(int startX, int startY, bool isExplosion)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((startX, startY));
            _board[startX, startY].IsVisited = true;

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                // Always reveal the current cell
                RevealCellGameplay(cx, cy);
                await Task.Delay(30); // Small delay for explosion animation effect

                if (!isExplosion)
                {
                    // Normal flood-fill stops at numbered cells
                    if (_board[cx, cy].AdjacentMines > 0)
                        continue;
                }

                // EXPLOSION LOGIC:
                if (isExplosion)
                {
                    // Non-mine cells become BOOM (X)
                    if (!_board[cx, cy].IsMine)
                        _board[cx, cy].Type = CellType.BOOM;
                }

                // Explore neighbors
                for (int k = 0; k < _adjacentFields.GetLength(0); k++)
                {
                    int ni = cx + _adjacentFields[k, 0];
                    int nj = cy + _adjacentFields[k, 1];

                    if (ni < 0 || ni >= _gameGrid.Rows || nj < 0 || nj >= _gameGrid.Columns)
                        continue;

                    if (_board[ni, nj].IsVisited) continue;

                    _board[ni, nj].IsVisited = true;

                    if (!isExplosion)
                    {
                        // Normal click flood-fill: only enqueue empty cells
                        if (!_board[ni, nj].IsMine && _board[ni, nj].AdjacentMines == 0)
                            queue.Enqueue((ni, nj));
                    }
                    else
                    {
                        // Explosion BFS:
                        if (_board[ni, nj].IsMine)
                        {
                            // Chain explosion: enqueue this mine so it also explodes
                            queue.Enqueue((ni, nj));
                        }
                        else
                        {
                            // Mark as BOOM but do NOT enqueue further
                            _board[ni, nj].Type = CellType.BOOM;
                        }
                    }

                    RevealCellGameplay(ni, nj);
                }
            }
        }

        private Brush GetNumberColor(int n)
        {
            return n switch
            {
                1 => Brushes.Blue,
                2 => Brushes.Green,
                3 => Brushes.Red,
                4 => Brushes.DarkBlue,
                5 => Brushes.DarkRed,
                6 => Brushes.Turquoise,
                7 => Brushes.Black,
                8 => Brushes.Gray,
                _ => Brushes.Black
            };
        }
        private void RevealCellForUI(int x, int y)
        {
            int buttonIndex = x * _gameGrid.Columns + y;
            Button btn = (Button)_gameGrid.Children[buttonIndex];

            // Show correct symbol/number

            if (_board[x, y].IsMine)
            {
                btn.Content = "💣";
            }
            else if (_board[x, y].Type == CellType.BOOM)
            {
                btn.Content = "X";
            }
            else if (_board[x, y].Type == CellType.ATOM)
            {
                btn.Content = "☢️";
            }
            else if (_board[x, y].AdjacentMines > 0)
            {
                btn.Content = _board[x, y].AdjacentMines.ToString();
                btn.Foreground = GetNumberColor(_board[x, y].AdjacentMines);
            }
            else
            {
                btn.Content = "";
            }

            btn.IsEnabled = false;
            _board[x, y].IsVisited = true;
        }
        public void RevealCellGameplay(int x, int y)
        {
            var cell = _board[x, y];

            if (cell.IsMine)
            {
                Score.Value -= 1;
            }
            else if (cell.Type == CellType.BOOM)
            {
                Score.Value -= 1;
            }
            else if (cell.Type == CellType.ATOM)
            {
                Score.Value -= 5;
            }
            else if (cell.AdjacentMines > 0)
            {
                Score.Value += cell.AdjacentMines;
            }

            _labelScore.Content = "Score: " + Score.Value;
            RevealCellForUI(x, y);
        }

        public void RestoreVisitedState()
        {
            for (int i = 0; i < _gameGrid.Rows; i++)
            {
                for (int j = 0; j < _gameGrid.Columns; j++)
                {
                    if (_board[i, j].IsVisited)
                    {
                        RevealCellForUI(i, j);
                    }
                }
            }
        }
        public void SetBoard(Cell[,] newBoard)
        {
            _board = newBoard;
        }
    }
}
