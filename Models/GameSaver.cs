using System.IO;
using minesweeper;
using Newtonsoft.Json;
using System.Diagnostics;

public class GameSaver
{
    private readonly string savePath = "savegame.json";
    private readonly GameBoard _gameBoard;
    public GameSaver(GameBoard gameBoard)
    {
        _gameBoard = gameBoard;
    }
    public void SaveGame()
    {
        var gameState = new GameState
        {
            Board = _gameBoard.Board,
            Score = Score.Value
        };

        File.WriteAllText(savePath, JsonConvert.SerializeObject(gameState));
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        var json = File.ReadAllText(savePath);

        var loadedGameState = JsonConvert.DeserializeObject<GameState>(json);
        if (loadedGameState == null || loadedGameState.Board == null) return;

        _gameBoard.SetBoard(loadedGameState.Board);
        Score.Value = loadedGameState.Score;
    }

    public bool SaveExists() => File.Exists(savePath);
}

public class GameState
{
    [JsonProperty(Required = Required.Always)]
    public int[,] Board { get; set; }
    // public int[,] Revealed { get; set;}
    public int Score { get; set; }
}
