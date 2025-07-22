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
using static System.Formats.Asn1.AsnWriter;

namespace minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool gameOver = false;
        private Cell[,] board;
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

        private GameBoard gameBoard;
        private GameSaver gameSaver;

        public MainWindow()
        {
            InitializeComponent();

            board = new Cell[GameGrid.Rows, GameGrid.Columns];

            gameBoard = new GameBoard(board, adjacentFields, GameGrid, Cell_Click, Cell_RightClick, LabelRowCol, LabelScore);
            gameSaver = new GameSaver(gameBoard, LabelScore);

            gameBoard.CreateGrid();
            gameBoard.CreateEmptyBoard();

            if (gameSaver.SaveExists())
            {
                var result = MessageBox.Show("Load previous game?", "Load Game",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    gameSaver.LoadGame();
                    gameBoard.RestoreVisitedState();
                }
                else
                {
                    gameBoard.CreateField(numberOfMines);
                }
            }
            else
            {
                gameBoard.CreateField(numberOfMines);
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            gameSaver.SaveGame();
            base.OnClosing(e);
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (gameOver) return;

            if (sender is Button btn)
            {
                var (r, c) = ((int, int))btn.Tag;
                var cell = gameBoard.Board[r, c];

                if (cell.IsMine)
                {
                    gameBoard.Propagate(r, c, isExplosion: true);
                    return;
                }

                if (cell.Type == CellType.ATOM)
                {
                    return;
                }

                if (cell.AdjacentMines == 0)
                {
                    gameBoard.Propagate(r, c, isExplosion: false);
                    return;
                }

                gameBoard.RevealCellGameplay(r, c);
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
    }
}