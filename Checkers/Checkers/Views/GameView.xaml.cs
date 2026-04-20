using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Checkers.ViewModels;

namespace Checkers.Views;

public partial class GameView : UserControl
{
    public event EventHandler? BackToLobbyRequested;

    public GameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyPropertyChanged oldVm)
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        if (e.NewValue is INotifyPropertyChanged newVm)
            newVm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsGameOver) && sender is MainViewModel vm && vm.IsGameOver)
            Confetti.Start();
    }

    private void OnBackToLobby(object sender, RoutedEventArgs e)
    {
        BackToLobbyRequested?.Invoke(this, EventArgs.Empty);
    }
}
