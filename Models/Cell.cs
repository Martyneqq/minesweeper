using minesweeper;

public class Cell
{
    public CellType Type { get; set; }
    public int AdjacentMines { get; set; }
    public bool IsVisited { get; set; }
    public bool IsFlagged { get; set; }

    public Cell(CellType type = CellType.EMPTY)
    {
        Type = type;
        AdjacentMines = 0;
        IsVisited = false;
        IsFlagged = false;
    }

    public bool IsMine => Type == CellType.MINE;
    public bool IsEmpty => Type == CellType.EMPTY && AdjacentMines == 0;

    public override string ToString()
    {
        return IsMine ? "💣" : (AdjacentMines > 0 ? AdjacentMines.ToString() : "");
    }
}
