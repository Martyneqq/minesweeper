using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace minesweeper
{
    public class GameBoard
    {
        private int[,] _board;
        public int[,] Board => _board;
        private int[,] _adjacentFields;
        private UniformGrid _gameGrid;
        private System.Windows.RoutedEventHandler _cellClick;
        private System.Windows.Input.MouseButtonEventHandler _cellRightClick;
        private Label _labelRowCol;
        private Label _labelScore;

        public GameBoard(int[,] board, int[,] adjacentFields, UniformGrid gameGrid, System.Windows.RoutedEventHandler cellClick, System.Windows.Input.MouseButtonEventHandler cellRightClick, Label labelRowCol, Label labelScore) { 
            _board = board;
            _adjacentFields = adjacentFields;
            _gameGrid = gameGrid;
            _cellClick = cellClick;
            _cellRightClick = cellRightClick;
            _labelRowCol = labelRowCol;
            _labelScore = labelScore;
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
                if (_board[mine_row, mine_col] != (int)CellType.MINE)
                {
                    _board[mine_row, mine_col] = (int)CellType.MINE;
                    placed++;
                }
            }

            for (int i = 0; i < _gameGrid.Rows; i++)
            {
                for (int j = 0; j < _gameGrid.Columns; j++)
                {
                    if (_board[i, j] == (int)CellType.MINE)
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
                            if (_board[ni, nj] == (int)CellType.MINE)
                                sum++;
                        }
                    }
                    _board[i, j] = sum;
                }
            }
        }
        public async void PropagateNormalCell(int x, int y)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            queue.Enqueue((x, y));
            visited.Add((x, y));

            // BFS
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                RevealNormalCell(cx, cy);
                await Task.Delay(5);

                if (_board[cx, cy] != (int)CellType.EMPTY)
                {
                    continue;
                }
                for (int k = 0; k < _adjacentFields.GetLength(0); k++)
                {
                    int ni = cx + _adjacentFields[k, 0];
                    int nj = cy + _adjacentFields[k, 1];
                    if (ni >= 0 && ni < _gameGrid.Rows && nj >= 0 && nj < _gameGrid.Columns)
                    {
                        if (!visited.Contains((ni, nj)) && _board[ni, nj] != (int)CellType.MINE)
                        {
                            int buttonIndex = ni * _gameGrid.Columns + nj;
                            Button neighborButton = (Button)_gameGrid.Children[buttonIndex];

                            if (neighborButton.IsEnabled)
                            {
                                queue.Enqueue((ni, nj));
                                visited.Add((ni, nj));
                            }
                        }
                    }
                }
            }
        }
        public void RevealNormalCell(int x, int y)
        {
            int buttonIndex = x * _gameGrid.Columns + y;
            Button btn = (Button)_gameGrid.Children[buttonIndex];
            if (_board[x, y] == (int)CellType.EMPTY)
            {
                btn.Content = "";
            }
            else
            {
                btn.Content = _board[x, y];
                DeductScoreForExplosion(_board[x, y]);
            }

            btn.IsEnabled = false;
        }
        public async void PropagateMineCell(int x, int y)
        {
            //gameOver = true;

            Queue<(int, int)> queue = new Queue<(int, int)>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            queue.Enqueue((x, y));
            visited.Add((x, y));

            // BFS
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                RevealMineCell(cx, cy);
                await Task.Delay(5);

                if (_board[cx, cy] != (int)CellType.MINE)
                {
                    continue;
                }
                for (int k = 0; k < _adjacentFields.GetLength(0); k++)
                {
                    int ni = cx + _adjacentFields[k, 0];
                    int nj = cy + _adjacentFields[k, 1];
                    if (ni >= 0 && ni < _gameGrid.Rows && nj >= 0 && nj < _gameGrid.Columns)
                    {
                        int buttonIndex = ni * _gameGrid.Columns + nj;
                        Button neighborButton = (Button)_gameGrid.Children[buttonIndex];

                        if (!visited.Contains((ni, nj)))
                        {
                            if (_board[ni, nj] == (int)CellType.MINE)
                            {
                                queue.Enqueue((ni, nj));
                                visited.Add((ni, nj));
                            }
                            else if (_board[ni, nj] == (int)CellType.ATOM_BOMB)
                            {
                                queue.Enqueue((ni, nj));
                                visited.Add((ni, nj));
                            }
                            else
                            {
                                neighborButton.Content = "X";
                                neighborButton.IsEnabled = false;

                                DeductScoreForExplosion(_board[ni, nj], false); // TEST
                                visited.Add((ni, nj));
                            }
                        }

                    }
                }
            }
        }
        public void RevealMineCell(int x, int y)
        {
            int buttonIndex = x * _gameGrid.Columns + y;
            Button btn = (Button)_gameGrid.Children[buttonIndex];

            if (!btn.IsEnabled) return;

            DeductScoreForExplosion(_board[x, y]);

            if (_board[x, y] == (int)CellType.MINE)
            {
                btn.Content = "💣";
            }
            else if (_board[x, y] == (int)CellType.ATOM_BOMB)
            {
                btn.Content = "☢️";
            }


            btn.IsEnabled = false;
        }
        public void DeductScoreForExplosion(int cellValue, bool increase = true)
        {
            if (increase)
            {
                Score.Value += cellValue;
            }
            else
            {
                Score.Value -= cellValue;
            }

            _labelScore.Content = "Score: " + Score.Value;
        }
        public void SetBoard(int[,] newBoard)
        {
            _board = newBoard;
        }
    }
}
