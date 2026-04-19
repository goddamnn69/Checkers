namespace Checkers;

/// <summary>
/// Цвета шашек.
/// </summary>
public enum CheckerColor
{
    White,
    Black
}

/// <summary>
/// Представляет игровую шашку.
/// </summary>
public class Checker
{
    /// <summary>
    /// Цвет шашки.
    /// </summary>
    public CheckerColor Color { get; }
    /// <summary>
    /// Является ли шашка дамкой.
    /// </summary>
    public bool IsKing { get; set; }

    /// <summary>
    /// Инициализирует новую шашку заданного цвета.
    /// </summary>
    /// <param name="color">Цвет шашки.</param>
    public Checker(CheckerColor color)
    {
        Color = color;
        IsKing = false;
    }
}
