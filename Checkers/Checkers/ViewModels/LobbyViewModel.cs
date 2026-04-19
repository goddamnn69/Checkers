using System;
using System.Reflection;
using System.Windows.Input;
using Checkers.Network;
using Checkers.Services;
using CommunityToolkit.Mvvm.Input;
using Velopack;

namespace Checkers.ViewModels;

public class LobbyViewModel : BaseViewModel
{
    private string _selectedGameMode = "PvP";
    private string _selectedPlayerColor = "White";
    private string _remoteIp = "";
    private string _connectionStatus = "";
    private bool _isConnecting;
    private NetworkManager? _networkManager;
    private string _updateStatus = "";
    private bool _isUpdateAvailable;
    private UpdateInfo? _updateInfo;

    public ICommand StartGameCommand { get; }
    public ICommand HostGameCommand { get; }
    public ICommand JoinGameCommand { get; }
    public ICommand UpdateCommand { get; }
    public event EventHandler<GameStartEventArgs>? StartGameRequested;

    public string VersionText =>
        $"v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "?"}";

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
        UpdateCommand = new RelayCommand(ApplyUpdate);

        _ = CheckForUpdatesAsync();
    }

    private void StartGame()
    {
        bool isBotMode = SelectedGameMode == "PvB";

        if (!Enum.TryParse(_selectedPlayerColor, out CheckerColor colorEnum))
            colorEnum = CheckerColor.White;

        StartGameRequested?.Invoke(this, new GameStartEventArgs(isBotMode, colorEnum));
    }

    private async void HostGame()
    {
        if (IsConnecting) return;
        IsConnecting = true;

        _networkManager = new NetworkManager();
        var localIp = NetworkManager.GetLocalIp();
        ConnectionStatus = $"Ваш IP: {localIp}\nОжидание игрока...";

        try
        {
            await _networkManager.StartHostAsync();
            ConnectionStatus = "Игрок подключился!";

            // Хост играет белыми
            StartGameRequested?.Invoke(this, new GameStartEventArgs(
                isBotMode: false,
                playerColor: CheckerColor.White,
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
            await _networkManager.ConnectAsync(RemoteIp.Trim());
            ConnectionStatus = "Подключено!";

            // Клиент играет чёрными
            StartGameRequested?.Invoke(this, new GameStartEventArgs(
                isBotMode: false,
                playerColor: CheckerColor.Black,
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

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            _updateInfo = await UpdateService.CheckForUpdatesAsync();
            if (_updateInfo != null)
            {
                IsUpdateAvailable = true;
                UpdateStatus = $"Доступна версия {_updateInfo.TargetFullRelease.Version}";
            }
        }
        catch
        {
            // Silently ignore update check failures (no network, no repo yet, etc.)
        }
    }

    private async void ApplyUpdate()
    {
        if (_updateInfo == null) return;

        UpdateStatus = "Загрузка обновления...";
        try
        {
            await UpdateService.DownloadAndApplyAsync(_updateInfo);
        }
        catch (Exception ex)
        {
            UpdateStatus = $"Ошибка обновления: {ex.Message}";
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

    public GameStartEventArgs(bool isBotMode, CheckerColor playerColor,
        bool isOnlineMode = false, NetworkManager? network = null, bool isHost = false)
    {
        IsBotMode = isBotMode;
        PlayerColor = playerColor;
        IsOnlineMode = isOnlineMode;
        Network = network;
        IsHost = isHost;
    }
}
