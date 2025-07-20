using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Threading;

namespace minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool gameOver = false;
        private CellType[,] board;
        private int numberOfMines = 50;
        private int rows = 20;
        private int cols = 20;
        private int[,] adjacentFields =
        {
            { -1, 1 },{ 0, 1 },{ 1, 1 },
            { -1, 0 },         { 1, 0 },
            { -1, -1 },{ 0, -1 },{ 1, -1 }
        };
        public MainWindow()
        {
            board = new CellType[rows, cols];

            InitializeComponent();
            CreateGrid(rows, cols);
            CreateField(numberOfMines);
        }

        void CreateGrid(int rows, int cols)
        {
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button button = new Button();
                    button.Content = "";
                    button.FontSize = 16;
                    button.Tag = (i, j);
                    button.Click += Cell_Click;
                    button.MouseRightButtonUp += Cell_RightClick;

                    GameGrid.Children.Add(button);
                }
            }
        }
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (gameOver) return;

            if (sender is Button btn)
            {
                var (r, c) = ((int, int))btn.Tag;

                Options(btn, r, c);

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
                int mine_row = r.Next(rows);
                int mine_col = r.Next(cols);
                if (board[mine_row, mine_col] != CellType.MINE)
                {
                    board[mine_row, mine_col] = CellType.MINE;
                    placed++;
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (board[i, j] == CellType.MINE)
                    {
                        continue;
                    }

                    CellType sum = 0;

                    for (int k = 0; k < adjacentFields.GetLength(0); k++)
                    {
                        int ni = i + adjacentFields[k, 0];
                        int nj = j + adjacentFields[k, 1];
                        if (ni >= 0 && ni < rows && nj >= 0 && nj < cols)
                        {
                            if (board[ni, nj] == CellType.MINE)
                                sum++;
                        }
                    }
                    board[i, j] = sum;
                }
            }
        }
        private void Options(Button btn, int x, int y)
        {
            switch (board[x, y])
            {
                case CellType.MINE:
                    RevealAllMines();
                    break;
                case CellType.EMPTY:
                    btn.Content = "0";
                    RevealFields(x, y);

                    break;
                default:
                    btn.Content = board[x, y];
                    break;
            }
        }
        private void RevealFields(int x, int y)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));


            // BFS
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                RevealCell(cx, cy);

                if (board[cx, cy] != 0)
                {
                    continue;
                }
                for (int k = 0; k < adjacentFields.GetLength(0); k++)
                {
                    int ni = cx + adjacentFields[k, 0];
                    int nj = cy + adjacentFields[k, 1];
                    if (ni >= 0 && ni < rows && nj >= 0 && nj < cols)
                    {
                        int buttonIndex = ni * cols + nj;
                        Button neighborButton = (Button)GameGrid.Children[buttonIndex];
                        if (neighborButton.IsEnabled && board[ni, nj] != CellType.MINE)
                        {
                            queue.Enqueue((ni, nj));
                        }
                    }
                }
            }
        }
        private void RevealCell(int x, int y)
        {
            int buttonIndex = x * cols + y;
            Button btn = (Button)GameGrid.Children[buttonIndex];
            if (board[x, y] == 0)
                btn.Content = "0";
            else
                btn.Content = board[x, y];

            btn.IsEnabled = false;
        }
        private void RevealAllMines()
        {
            gameOver = true;

            Thread th = new Thread(() =>
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (board[i, j] == CellType.MINE)
                        {
                            int buttonIndex = i * cols + j;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Button btn = (Button)GameGrid.Children[buttonIndex];
                                btn.IsEnabled = false;
                                btn.Content = "💣";
                            });
                            Thread.Sleep(50);
                        }
                    }
                }
            });
            th.Start();
            th.Join();
        }
    }
}