namespace Checkers;

public enum CellColor
{
    Black,
    White, 
    Green,
    Red
}
/// <summary>
/// Представляет клетку на игровой доске.
/// </summary>
public class Cell
{
    /// <summary>
    /// Индекс строки клетки.
    /// </summary>
    public int Row { get; }
    /// <summary>
    /// Индекс колонки клетки.
    /// </summary>
    public int Column { get; }
    /// <summary>
    /// Шашка, находящаяся в этой клетке (если есть).
    /// </summary>
    public Checker? Checker { get; set; }

    /// <summary>
    /// Инициализирует новую клетку с заданными координатами.
    /// </summary>
    /// <param name="row">Индекс строки.</param>
    /// <param name="column">Индекс колонки.</param>
    public Cell(int row, int column)
    {
        Row = row;
        Column = column;
    }
}
