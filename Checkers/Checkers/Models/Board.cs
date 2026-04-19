using System.Text;

namespace Checkers;

public class Board
{
    /// <summary>
    /// Представляет игровую доску.
    /// </summary>
    public int Rows { get; }
    /// <summary>
    /// Количество колонок на доске.
    /// </summary>
    public int Columns { get; }
    private readonly Cell?[,] _cells;

    /// <summary>
    /// Инициализирует новую доску с заданным количеством строк и колонок.
    /// </summary>
    /// <param name="rows">Количество строк.</param>
    /// <param name="columns">Количество колонок.</param>
    public Board(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _cells = new Cell[rows, columns];
    }

    /// <summary>
    /// Возвращает клетку по указанным координатам.
    /// </summary>
    /// <param name="row">Индекс строки.</param>
    /// <param name="col">Индекс колонки.</param>
    /// <returns>Клетка или null, если координаты вне доски.</returns>
    public Cell? GetCell(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns) return null;
        return _cells[row, col];
    }

    /// <summary>
    /// Устанавливает клетку по указанным координатам.
    /// </summary>
    /// <param name="row">Индекс строки.</param>
    /// <param name="col">Индекс колонки.</param>
    /// <param name="cell">Объект клетки.</param>
    public void SetCell(int row, int col, Cell? cell)
    {
        if (row >= 0 && row < Rows && col >= 0 && col < Columns)
        {
            _cells[row, col] = cell;
        }
    }
    /// <summary>
    /// Удаляет шашку из клетки по указанным координатам.
    /// </summary>
    /// <param name="row">Индекс строки.</param>
    /// <param name="col">Индекс колонки.</param>
    public void ClearCell(int row, int col)
    {
        var cell = GetCell(row, col);
        if (cell != null)
        {
            cell.Checker = null;
        }
    }

    public bool HasCheckersColor(CheckerColor color)
    {
        foreach (var cell in _cells)
        {
            if (cell?.Checker?.Color == color) return true;
        }
        return false;
    }

    /// <summary>
    /// Выводит текущее состояние доски в консоль.
    /// </summary>
    public void Display()
    {
        var sb = new StringBuilder();
        sb.AppendLine("  0 1 2 3 4 5 6 7 8 9");
        for (int r = 0; r < Rows; r++)
        {
            sb.Append(r + " ");
            for (int c = 0; c < Columns; c++)
            {
                var cell = _cells[r, c];
                if (cell == null)
                {
                    sb.Append(". "); // Неигровая клетка (белая)
                }
                else if (cell.Checker == null)
                {
                    sb.Append(". "); // Пустая игровая клетка (черная)
                }
                else
                {
                    sb.Append(cell.Checker.Color == CheckerColor.White ? "W " : "B ");
                }
            }
            sb.AppendLine();
        }
        Console.Write(sb.ToString());
    }
}
