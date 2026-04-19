namespace Checkers;

/// <summary>
/// Статический класс для управления текущим состоянием игрока.
/// </summary>
public static class Player
{
    /// <summary>
    /// Текущий активный игрок (цвет шашек).
    /// </summary>
    public static CheckerColor CurrentPlayer { get; set; }

    /// <summary>
    /// Переключает ход на следующего игрока.
    /// </summary>
    public static void SwitchPlayer()
    {
        CurrentPlayer = (CurrentPlayer == CheckerColor.White)
            ? CheckerColor.Black
            : CheckerColor.White;
    }
}