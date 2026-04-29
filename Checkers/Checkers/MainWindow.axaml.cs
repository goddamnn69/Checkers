using Avalonia.Controls;
using Checkers.ViewModels;

namespace Checkers;

public partial class MainWindow : Window
{
    private GameStartEventArgs? _lastGameArgs;

    public MainWindow()
    {
        InitializeComponent();

        var lobbyViewModel = new LobbyViewModel();

        lobbyViewModel.StartGameRequested += (sender, e) =>
        {
            if (e is GameStartEventArgs gameArgs)
            {
                StartGame(gameArgs);
            }
        };

        LobbyView.DataContext = lobbyViewModel;

        GameView.BackToLobbyRequested += (_, _) => ShowLobby();
        GameView.BackToGameRequested += (_, _) =>
        {
            if (_lastGameArgs != null)
                StartGame(_lastGameArgs);
        };
    }

    private void StartGame(GameStartEventArgs args)
    {
        _lastGameArgs = args;
        MainViewModel mainViewModel;

        if (args.IsOnlineMode && args.Network != null)
        {
            mainViewModel = new MainViewModel(args.PlayerColor, args.Network, args.Name, args.EnemyName);
        }
        else
        {
            mainViewModel = new MainViewModel(args.IsBotMode, args.PlayerColor, args.Name, args.EnemyName);
        }

        GameView.DataContext = mainViewModel;

        LobbyView.IsVisible = false;
        GameView.IsVisible = true;
    }

    private void ShowLobby()
    {
        if (GameView.DataContext is MainViewModel vm)
            vm.Disconnect();

        GameView.IsVisible = false;
        LobbyView.IsVisible = true;
    }
}
