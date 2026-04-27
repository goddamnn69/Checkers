using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Checkers.ViewModels;

namespace Checkers.Views;

public partial class GameView : UserControl
{
    public event EventHandler? BackToLobbyRequested;
    public event EventHandler? BackToGameRequested;

    public GameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            oldVm.Player.PlayerSwitched -= OnPlayerSwitched;
        }
        if (e.NewValue is MainViewModel newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
            newVm.Player.PlayerSwitched += OnPlayerSwitched;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsGameOver) && sender is MainViewModel vm && vm.IsGameOver)
            Confetti.Start();
    }

    private void OnPlayerSwitched()
    {
        var brush = new SolidColorBrush(Color.FromRgb(0x2c, 0x3e, 0x50));
        Background = brush;
        var animation = new ColorAnimation
        {
            To = Colors.Yellow,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true
        };
        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
    }

    private void OnBackToLobby(object sender, RoutedEventArgs e)
    {
        BackToLobbyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnBackToGame(object sender, RoutedEventArgs e)
    {
        BackToGameRequested?.Invoke(this, EventArgs.Empty);
    }
}
