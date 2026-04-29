using System;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Checkers.Network;
using Checkers.Services;
using CommunityToolkit.Mvvm.Input;
using Velopack;

namespace Checkers.ViewModels;

public class LobbyViewModel : BaseViewModel
{
    private string _selectedGameMode = "PvP";
    private string _selectedPlayerColor = "White";
    private string _playerName = "Шашечник";
    private string _enemyName = "Противник";
    private string _remoteIp = "";
    private string _connectionStatus = "";
    private bool _isConnecting;
    private NetworkManager? _networkManager;
    private string _updateStatus = "";
    private bool _isUpdateAvailable;
    private UpdateInfo? _updateInfo;
    private string? _localIp;

    public ICommand StartGameCommand { get; }
    public ICommand HostGameCommand { get; }
    public ICommand JoinGameCommand { get; }
    public ICommand CopyIpCommand { get; }
    public event EventHandler<GameStartEventArgs>? StartGameRequested;

    public bool HasIpToCopy => _localIp != null;

    public string VersionText
    {
        get
        {
            var attr = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var version = attr?.InformationalVersion ??
                          Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ??
                          "?";
            // Strip build metadata (e.g. "+sha.abc123") if present
            var plusIndex = version.IndexOf('+');
            if (plusIndex >= 0) version = version[..plusIndex];
            return $"v{version}";
        }
    }

    public string UpdateStatus
    {
        get => _updateStatus;
        set => SetField(ref _updateStatus, value);
    }

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => SetField(ref _isUpdateAvailable, value);
    }

    public string SelectedGameMode
    {
        get => _selectedGameMode;
        set
        {
            if (SetField(ref _selectedGameMode, value))
            {
                OnPropertyChanged(nameof(IsBlackColorEnabled));
                OnPropertyChanged(nameof(IsOnlineMode));
                OnPropertyChanged(nameof(IsOfflineMode));

                if (value == "PvB" && _selectedPlayerColor == "Black")
                    SelectedPlayerColor = "White";

                // Сбрасываем сетевое состояние при смене режима
                _networkManager?.Disconnect();
                _networkManager = null;
                ConnectionStatus = "";
            }
        }
    }

    public string Name
    {
        get => _playerName;
        set => SetField(ref _playerName, value);
    } 
    public string EnemyName
    {
        get => _enemyName;
        set => SetField(ref _enemyName, value);
    } 

    public int SelectedGameModeIndex
    {
        get => _selectedGameMode switch { "PvP" => 0, "PvB" => 1, "Online" => 2, _ => 0 };
        set
        {
            SelectedGameMode = value switch { 0 => "PvP", 1 => "PvB", 2 => "Online", _ => "PvP" };
            OnPropertyChanged();
        }
    }

    public int SelectedPlayerColorIndex
    {
        get => _selectedPlayerColor == "White" ? 0 : 1;
        set
        {
            SelectedPlayerColor = value == 0 ? "White" : "Black";
            OnPropertyChanged();
        }
    }

    public bool IsBlackColorEnabled => _selectedGameMode != "PvB";
    public bool IsOnlineMode => _selectedGameMode == "Online";
    public bool IsOfflineMode => _selectedGameMode != "Online";

    public string SelectedPlayerColor
    {
        get => _selectedPlayerColor;
        set => SetField(ref _selectedPlayerColor, value);
    }

    public string RemoteIp
    {
        get => _remoteIp;
        set => SetField(ref _remoteIp, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetField(ref _connectionStatus, value);
    }

    public bool IsConnecting
    {
        get => _isConnecting;
        set => SetField(ref _isConnecting, value);
    }

    public LobbyViewModel()
    {
        StartGameCommand = new RelayCommand(StartGame);
        HostGameCommand = new RelayCommand(HostGame);
        JoinGameCommand = new RelayCommand(JoinGame);
        CopyIpCommand = new RelayCommand(CopyIp);

        _ = CheckForUpdatesAsync();
    }

    private void StartGame()
    {
        bool isBotMode = SelectedGameMode == "PvB";

        if (!Enum.TryParse(_selectedPlayerColor, out CheckerColor colorEnum))
            colorEnum = CheckerColor.White;

        StartGameRequested?.Invoke(this, new GameStartEventArgs(isBotMode, colorEnum, _playerName, _enemyName));
    }

    private async void HostGame()
    {
        if (IsConnecting) return;
        IsConnecting = true;

        _networkManager = new NetworkManager();
        _localIp = NetworkManager.GetLocalIp();
        OnPropertyChanged(nameof(HasIpToCopy));
        ConnectionStatus = $"Ваш IP: {_localIp}\nОжидание игрока...";

        try
        {
            await _networkManager.StartHostAsync(_playerName);
            ConnectionStatus = "Игрок подключился!";

            StartGameRequested?.Invoke(this, new GameStartEventArgs(
                isBotMode: false,
                playerColor: CheckerColor.White,
                name: _playerName,
                enemyName: _networkManager.RemoteName,
                isOnlineMode: true,
                network: _networkManager,
                isHost: true));
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Ошибка: {ex.Message}";
            _networkManager.Disconnect();
            _networkManager = null;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private async void JoinGame()
    {
        if (IsConnecting || string.IsNullOrWhiteSpace(RemoteIp)) return;
        IsConnecting = true;

        _networkManager = new NetworkManager();
        ConnectionStatus = "Подключение...";

        try
        {
            await _networkManager.ConnectAsync(RemoteIp.Trim(), _playerName);
            ConnectionStatus = "Подключено!";

            StartGameRequested?.Invoke(this, new GameStartEventArgs(
                isBotMode: false,
                playerColor: CheckerColor.Black,
                name: _playerName,
                enemyName: _networkManager.RemoteName,
                isOnlineMode: true,
                network: _networkManager,
                isHost: false));
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Ошибка: {ex.Message}";
            _networkManager.Disconnect();
            _networkManager = null;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private async void CopyIp()
    {
        if (_localIp == null) return;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var clipboard = desktop.MainWindow?.Clipboard;
            if (clipboard != null)
                await clipboard.SetTextAsync(_localIp);
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            _updateInfo = await UpdateService.CheckForUpdatesAsync();
            if (_updateInfo != null)
            {
                UpdateStatus = $"Обновление до {_updateInfo.TargetFullRelease.Version}...";
                await UpdateService.DownloadAndApplyAsync(_updateInfo);
            }
        }
        catch
        {
            // Silently ignore update check failures (no network, no repo yet, etc.)
        }
    }
}

public class GameStartEventArgs : EventArgs
{
    public bool IsBotMode { get; }
    public CheckerColor PlayerColor { get; }
    public bool IsOnlineMode { get; }
    public NetworkManager? Network { get; }
    public bool IsHost { get; }
    public string Name {  get; }
    public string EnemyName {  get; }

    public GameStartEventArgs(bool isBotMode, CheckerColor playerColor, string name, string enemyName,
        bool isOnlineMode = false, NetworkManager? network = null, bool isHost = false)
    {
        IsBotMode = isBotMode;
        PlayerColor = playerColor;
        IsOnlineMode = isOnlineMode;
        Network = network;
        IsHost = isHost;
        Name = name;
        EnemyName = enemyName;
    }
}
