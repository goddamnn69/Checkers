using System.Collections.ObjectModel;
using System.Windows.Input;
using Checkers;
using Checkers.Network;
using CommunityToolkit.Mvvm.Input;

namespace Checkers.ViewModels;

public class MainViewModel : BaseViewModel
{
    private Board _board;
    private CellViewModel? _selectedCell;
    private CheckerColor _currentPlayer;
    private string _statusMessage = "";
    private bool _isGameOver;
    private bool _isBotMode = false;
    private bool _isOnlineMode = false;
    private CheckerColor _myColor;
    private NetworkManager? _network;
    private readonly Player _player = new();

    public Player Player => _player;
    public ObservableCollection<CellViewModel> Cells { get; } = new();

    // Команда для нажатия на клетку
    public ICommand HandleCellClickCommand => new RelayCommand<object>(o => {
        if (o is CellViewModel cell) HandleCellClick(cell);
    });

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public bool IsGameOver
    {
        get => _isGameOver;
        set => SetField(ref _isGameOver, value);
    }

    public bool IsBotMode
    {
        get => _isBotMode;
        set => SetField(ref _isBotMode, value);
    }

    public MainViewModel()
    {
        _board = GameEngine.InitializeBoard(8, 8);

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var cellModel = new CellViewModel(r, c);
                var boardCell = _board.GetCell(r, c);
                if (boardCell != null)
                    cellModel.Checker = boardCell.Checker;
                Cells.Add(cellModel);
            }
        }
    }

    public MainViewModel(bool isBotMode, CheckerColor currentPlayer, string playerName = "", string enemyName = "") : this()
    {
        _isBotMode = isBotMode;
        _currentPlayer = currentPlayer;
        _myColor = currentPlayer;
        _player.CurrentPlayer = _currentPlayer;

        if (isBotMode)
        {
            _player.WhiteName = !string.IsNullOrEmpty(playerName) ? playerName : "Игрок";
            _player.BlackName = "Бот";
        }
        else
        {
            _player.WhiteName = currentPlayer == CheckerColor.White
                ? (!string.IsNullOrEmpty(playerName) ? playerName : "Белые")
                : (!string.IsNullOrEmpty(enemyName) ? enemyName : "Белые");
            _player.BlackName = currentPlayer == CheckerColor.Black
                ? (!string.IsNullOrEmpty(playerName) ? playerName : "Черные")
                : (!string.IsNullOrEmpty(enemyName) ? enemyName : "Черные");
        }

        UpdateStatus();
    }

    /// <summary>
    /// Конструктор для онлайн-игры.
    /// </summary>
    public MainViewModel(CheckerColor myColor, NetworkManager network, string playerName, string enemyName) : this()
    {
        _isOnlineMode = true;
        _myColor = myColor;
        _network = network;
        _currentPlayer = CheckerColor.White;
        _player.CurrentPlayer = _currentPlayer;

        _player.WhiteName = myColor == CheckerColor.White ? playerName : enemyName;
        _player.BlackName = myColor == CheckerColor.Black ? playerName : enemyName;

        UpdateStatus();

        _network.MoveReceived += OnOpponentMove;
        _network.Disconnected += msg => StatusMessage = msg;
    }

    public void Disconnect() => _network?.Disconnect();

    private bool IsMyTurn() => _currentPlayer == _myColor;

    // Метод для обработки клика по клетке
    public void HandleCellClick(CellViewModel clickedCell)
    {
        var winner = GameEngine.CheckWinner(_board);
        if (winner != null)
        {
            StatusMessage = $"Победил: {_player.GetName(winner.Value)}";
            IsGameOver = true;
            return;
        }

        // Запрещаем выбирать шашки бота
        if (_isBotMode && clickedCell.Checker != null
            && clickedCell.Checker.Color == CheckerColor.Black
            && !clickedCell.IsTarget)
        {
            return;
        }

        // Онлайн: блокируем клики не в свой ход
        if (_isOnlineMode && !IsMyTurn() && !clickedCell.IsTarget)
            return;

        // 1. Если кликнули по своей шашке — выбираем её и подсвечиваем ходы
        if (clickedCell.Checker != null && clickedCell.Checker.Color == _currentPlayer)
        {
            ClearSelection();
            _selectedCell = clickedCell;
            _selectedCell.IsSelected = true;

            var validMoves = GameEngine.GetValidMovesForPiece(_board, clickedCell.Row, clickedCell.Col);
            foreach (var move in validMoves)
            {
                var target = GetCell(move.ToRow, move.ToCol);
                if (target != null) target.IsTarget = true;
            }
        }
        // 2. Если кликнули по подсвеченной цели
        else if (clickedCell.IsTarget && _selectedCell != null)
        {
            var move = GameEngine.GetValidMovesForPiece(_board, _selectedCell.Row, _selectedCell.Col)
                                 .FirstOrDefault(m => m.ToRow == clickedCell.Row && m.ToCol == clickedCell.Col);
            if (move != null)
            {
                var result = GameEngine.ExecuteMove(_board, move, _player);
                if (result.Success)
                {
                    SyncBoard();

                    if (_isOnlineMode)
                        _ = _network!.SendMoveAsync(move);

                    if (!result.IsChainCapturePossible)
                    {
                        _currentPlayer = _player.CurrentPlayer;
                        UpdateStatus();

                        // Бот
                        if (_isBotMode && _currentPlayer == CheckerColor.Black)
                        {
                            Bot.MakeBotMove(_board, _currentPlayer, _player);
                            SyncBoard();

                            _currentPlayer = _player.CurrentPlayer;

                            var botWinner = GameEngine.CheckWinner(_board);
                            if (botWinner != null)
                            {
                                StatusMessage = $"Победил: {_player.GetName(botWinner.Value)}";
                                IsGameOver = true;
                                return;
                            }

                            UpdateStatus();
                        }

                        ClearSelection();
                    }
                    else
                    {
                        StatusMessage = "Бейте дальше этой же шашкой!";
                        HandleCellClick(GetCell(move.ToRow, move.ToCol)!);
                    }
                }
            }
        }
        else
        {
            ClearSelection();
        }
    }

    /// <summary>
    /// Обработка хода от сетевого соперника.
    /// </summary>
    private void OnOpponentMove(Move move)
    {
        var result = GameEngine.ExecuteMove(_board, move, _player);
        if (!result.Success) return;

        SyncBoard();

        if (!result.IsChainCapturePossible)
        {
            _currentPlayer = _player.CurrentPlayer;
            UpdateStatus();
            ClearSelection();
        }
        // Если цепное взятие — ждём следующий ход от соперника
    }

    private void UpdateStatus()
    {
        var winner = GameEngine.CheckWinner(_board);
        if (winner != null)
        {
            StatusMessage = $"Победил: {_player.GetName(winner.Value)}";
            IsGameOver = true;
            return;
        }

        StatusMessage = $"Ходит: {_player.CurrentPlayerName}";
    }

    private void SyncBoard()
    {
        foreach (var cellVM in Cells)
        {
            var boardCell = _board.GetCell(cellVM.Row, cellVM.Col);
            cellVM.Checker = boardCell?.Checker;
            cellVM.RefreshAll();
        }
    }

    private void ClearSelection()
    {
        foreach (var cell in Cells)
        {
            cell.IsSelected = false;
            cell.IsTarget = false;
        }
        _selectedCell = null;
    }

    private CellViewModel? GetCell(int row, int col)
        => Cells.FirstOrDefault(c => c.Row == row && c.Col == col);
}
