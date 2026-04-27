using System.Windows;
using Checkers.ViewModels;

namespace Checkers;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
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
            mainViewModel = new MainViewModel(args.PlayerColor, args.Network, args.Name);
        }
        else
        {
            mainViewModel = new MainViewModel(args.IsBotMode, args.PlayerColor, args.Name);
        }

        GameView.DataContext = mainViewModel;

        LobbyView.Visibility = Visibility.Collapsed;
        GameView.Visibility = Visibility.Visible;
    }

    private void ShowLobby()
    {
        if (GameView.DataContext is MainViewModel vm)
            vm.Disconnect();

        GameView.Visibility = Visibility.Collapsed;
        LobbyView.Visibility = Visibility.Visible;
    }
}
