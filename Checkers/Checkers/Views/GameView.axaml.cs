using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Checkers.ViewModels;

namespace Checkers.Views;

public partial class GameView : UserControl
{
    public event EventHandler? BackToLobbyRequested;
    public event EventHandler? BackToGameRequested;

    private MainViewModel? _currentVm;
    private static readonly IBrush DefaultBackground = SolidColorBrush.Parse("#2c3e50");

    public GameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_currentVm != null)
        {
            _currentVm.PropertyChanged -= OnViewModelPropertyChanged;
            _currentVm.Player.PlayerSwitched -= OnPlayerSwitched;
        }

        _currentVm = DataContext as MainViewModel;

        if (_currentVm != null)
        {
            _currentVm.PropertyChanged += OnViewModelPropertyChanged;
            _currentVm.Player.PlayerSwitched += OnPlayerSwitched;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsGameOver) && sender is MainViewModel vm && vm.IsGameOver)
            Confetti.Start("🌟");
    }
    private void OnKal(object? sender, RoutedEventArgs routedEventArgs)
    {
        Confetti.Start("💩");
    }

    private async void OnPlayerSwitched()
    {
        Background = new SolidColorBrush(Colors.Yellow);
        await Task.Delay(200);
        Background = DefaultBackground;
    }

    private void OnBackToLobby(object? sender, RoutedEventArgs e)
    {
        BackToLobbyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnBackToGame(object? sender, RoutedEventArgs e)
    {
        BackToGameRequested?.Invoke(this, EventArgs.Empty);
    }
}
