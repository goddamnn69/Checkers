using System.Windows;
using Checkers.ViewModels;

namespace Checkers;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
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
    }

    private void StartGame(GameStartEventArgs args)
    {
        MainViewModel mainViewModel;

        if (args.IsOnlineMode && args.Network != null)
        {
            // Онлайн-игра: хост = белые, клиент = чёрные
            mainViewModel = new MainViewModel(args.PlayerColor, args.Network);
        }
        else
        {
            // Оффлайн: PvP или PvB
            mainViewModel = new MainViewModel(args.IsBotMode, args.PlayerColor);
        }

        GameView.DataContext = mainViewModel;

        LobbyView.Visibility = Visibility.Collapsed;
        GameView.Visibility = Visibility.Visible;
    }

    private void ShowLobby()
    {
        GameView.Visibility = Visibility.Collapsed;
        LobbyView.Visibility = Visibility.Visible;
    }
}
