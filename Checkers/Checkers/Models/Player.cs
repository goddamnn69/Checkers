namespace Checkers;

public class Player
{
    public CheckerColor CurrentPlayer { get; set; }
    public string WhiteName { get; set; } = "Белые";
    public string BlackName { get; set; } = "Черные";
    public event Action? PlayerSwitched;

    public string CurrentPlayerName => GetName(CurrentPlayer);

    public string GetName(CheckerColor color) =>
        color == CheckerColor.White ? WhiteName : BlackName;

    public void SwitchPlayer()
    {
        CurrentPlayer = (CurrentPlayer == CheckerColor.White)
            ? CheckerColor.Black
            : CheckerColor.White;

        PlayerSwitched?.Invoke();
    }
}