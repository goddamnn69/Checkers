namespace Checkers;

public class Player
{
    public CheckerColor CurrentPlayer { get; set; }
    public event Action? PlayerSwitched;

    public void SwitchPlayer()
    {
        CurrentPlayer = (CurrentPlayer == CheckerColor.White)
            ? CheckerColor.Black
            : CheckerColor.White;

        PlayerSwitched?.Invoke();
    }
}