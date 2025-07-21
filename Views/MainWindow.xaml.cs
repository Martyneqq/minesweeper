using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool gameOver = false;
        private int[,] board;
        private int numberOfMines = 50;
        private int numberOfAtomBombs = 0;
        private int[,] adjacentFields =
        {
            { -1, 1 },{ 0, 1 },{ 1, 1 },
            { -1, 0 },         { 1, 0 },
            { -1, -1 },{ 0, -1 },{ 1, -1 }
        };
        private int[,] adjacentFieldsAtom =
        {
            { -2, 2 }, { -1, 2 }, { 0, 2 }, { 1, 2 }, { 2, 2 },
            { -2, 1 }, { -1, 1 },{ 0, 1 },{ 1, 1 }, { 2, 1 },
            { -2, 0 }, { -1, 0 },         { 1, 0 }, { 2, 0 },
            { -2, -1 }, { -1, -1 },{ 0, -1 },{ 1, -1 }, { 2, -1 },
            { -2, -2 }, { -1, -2 }, { 0, -2 }, { 1, -2 }, { 2, -2 }
        };

        private ScoreHolder scoreHolder = new ScoreHolder();
        public MainWindow()
        {
            InitializeComponent();

            board = new int[GameGrid.Rows, GameGrid.Columns];
            scoreHolder.Value = 0;
            LabelScore.Content = "Score: " + scoreHolder.Value;

            CreateGrid();
            CreateField(numberOfMines);
        }

        void CreateGrid()
        {
            for (int i = 0; i < GameGrid.Rows; i++)
            {
                for (int j = 0; j < GameGrid.Columns; j++)
                {
                    Button button = new Button();
                    button.Content = "";
                    button.FontSize = 16;
                    button.Tag = (i, j);
                    button.Click += Cell_Click;
                    button.MouseRightButtonUp += Cell_RightClick;

                    GameGrid.Children.Add(button);

                    LabelRowCol.Content = GameGrid.Rows + "x" + GameGrid.Columns;
                }
            }
        }
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (gameOver) return;

            if (sender is Button btn)
            {
                var (r, c) = ((int, int))btn.Tag;

                switch (board[r, c])
                {
                    case -1:
                        PropagateMineCell(r, c);
                        break;
                    case 0:
                        btn.Content = "0";
                        PropagateNormalCell(r, c);
                        break;
                    default:
                        RevealNormalCell(r, c);
                        break;
                }

        btn.IsEnabled = false;
            }
        }
        private void Cell_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (gameOver) return;

            if (sender is Button btn)
            {
                btn.Content = btn.Content?.ToString() == "🚩" ? "" : "🚩";
            }
        }
        private void CreateField(int numberOfMines)
        {
            Random r = new Random();
            int placed = 0;
            while (placed < numberOfMines)
            {
                int mine_row = r.Next(GameGrid.Rows);
                int mine_col = r.Next(GameGrid.Columns);
                if (board[mine_row, mine_col] != (int)CellType.MINE)
                {
                    board[mine_row, mine_col] = (int)CellType.MINE;
                    placed++;
                }
            }

            for (int i = 0; i < GameGrid.Rows; i++)
            {
                for (int j = 0; j < GameGrid.Columns; j++)
                {
                    if (board[i, j] == (int)CellType.MINE)
                    {
                        continue;
                    }

                    int sum = 0;

                    for (int k = 0; k < adjacentFields.GetLength(0); k++)
                    {
                        int ni = i + adjacentFields[k, 0];
                        int nj = j + adjacentFields[k, 1];
                        if (ni >= 0 && ni < GameGrid.Rows && nj >= 0 && nj < GameGrid.Columns)
                        {
                            if (board[ni, nj] == (int)CellType.MINE)
                                sum++;
                        }
                    }
                    board[i, j] = sum;
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

                if (board[cx, cy] != (int)CellType.EMPTY)
                {
                    continue;
                }
                for (int k = 0; k < adjacentFields.GetLength(0); k++)
                {
                    int ni = cx + adjacentFields[k, 0];
                    int nj = cy + adjacentFields[k, 1];
                    if (ni >= 0 && ni < GameGrid.Rows && nj >= 0 && nj < GameGrid.Columns)
                    {
                        if (!visited.Contains((ni, nj)) && board[ni, nj] != (int)CellType.MINE)
                        {
                            int buttonIndex = ni * GameGrid.Columns + nj;
                            Button neighborButton = (Button)GameGrid.Children[buttonIndex];

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
            int buttonIndex = x * GameGrid.Columns + y;
            Button btn = (Button)GameGrid.Children[buttonIndex];
            if (board[x, y] == (int)CellType.EMPTY)
            {
                btn.Content = "";
            }
            else
            {
                btn.Content = board[x, y];
                DeductScoreForExplosion(board[x, y]);
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

                if (board[cx, cy] != (int)CellType.MINE)
                {
                    continue;
                }
                for (int k = 0; k < adjacentFields.GetLength(0); k++)
                {
                    int ni = cx + adjacentFields[k, 0];
                    int nj = cy + adjacentFields[k, 1];
                    if (ni >= 0 && ni < GameGrid.Rows && nj >= 0 && nj < GameGrid.Columns)
                    {
                        int buttonIndex = ni * GameGrid.Columns + nj;
                        Button neighborButton = (Button)GameGrid.Children[buttonIndex];

                        if (!visited.Contains((ni, nj)))
                        {
                            if (board[ni, nj] == (int)CellType.MINE)
                            {
                                queue.Enqueue((ni, nj));
                                visited.Add((ni, nj));
                            }
                            else if (board[ni, nj] == (int)CellType.ATOM_BOMB)
                            {
                                queue.Enqueue((ni, nj));
                                visited.Add((ni, nj));
                            }
                            else
                            {
                                neighborButton.Content = "X";
                                neighborButton.IsEnabled = false;

                                DeductScoreForExplosion(board[ni, nj], false); // TEST
                                visited.Add((ni, nj));
                            }
                        }

                    }
                }
            }
        }
        public void RevealMineCell(int x, int y)
        {
            int buttonIndex = x * GameGrid.Columns + y;
            Button btn = (Button)GameGrid.Children[buttonIndex];

            if (!btn.IsEnabled) return;

            DeductScoreForExplosion(board[x, y]);

            if (board[x, y] == (int)CellType.MINE)
            {
                btn.Content = "💣";
            }
            else if (board[x, y] == (int)CellType.ATOM_BOMB)
            {
                btn.Content = "☢️";
            }


            btn.IsEnabled = false;
        }
        private void DeductScoreForExplosion(int cellValue, bool increase = true)
        {
            if (increase) {
                scoreHolder.Value += cellValue;
            }
            else {
                scoreHolder.Value -= cellValue;
            }

            LabelScore.Content = "Score: " + scoreHolder.Value;
        }
    }
}