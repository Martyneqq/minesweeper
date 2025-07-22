using System.IO;
using minesweeper;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Controls;

public class GameSaver
{
    private readonly string savePath = "savegame.json";
    private readonly GameBoard _gameBoard;
    private readonly Label _score;
    public GameSaver(GameBoard gameBoard, Label score)
    {
        _gameBoard = gameBoard;
        _score = score;
    }

    public void SaveGame()
    {
        var gameState = new GameState()
        {
            Board = _gameBoard.Board,
            Score = Score.Value
        };

        string json = JsonConvert.SerializeObject(gameState, Formatting.Indented);
        File.WriteAllText(savePath, json);

        Debug.WriteLine("Game saved to " + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.WriteLine("No save file found.");
            return;
        }

        string json = File.ReadAllText(savePath);
        var loadedGameState = JsonConvert.DeserializeObject<GameState>(json);

        if (loadedGameState == null)
        {
            Debug.WriteLine("Failed to load save.");
            return;
        }

        _gameBoard.SetBoard(loadedGameState.Board);
        Score.Value = loadedGameState.Score;
        _score.Content = "Score: " + Score.Value;
    }

    public bool SaveExists() => File.Exists(savePath);

    public class GameState
    {
        public Cell[,] Board { get; set; }
        public int Score { get; set; }
    }
}
