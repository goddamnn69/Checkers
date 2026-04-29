using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Avalonia.Threading;

namespace Checkers.Network;

/// <summary>
/// Управляет P2P соединением по TCP в локальной сети.
/// Хост слушает порт, клиент подключается по IP.
/// Ходы передаются как JSON-строки.
/// </summary>
public class NetworkManager
{
    private TcpListener? _listener;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public bool IsHost { get; private set; }
    public bool IsConnected { get; private set; }
    public string RemoteName { get; private set; } = "";

    /// <summary>
    /// Вызывается в UI-потоке когда приходит ход от соперника.
    /// </summary>
    public event Action<Move>? MoveReceived;

    /// <summary>
    /// Вызывается при разрыве соединения.
    /// </summary>
    public event Action<string>? Disconnected;

    /// <summary>
    /// Возвращает локальный IP для отображения хосту.
    /// </summary>
    public static string GetLocalIp()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 80);
            return ((IPEndPoint)socket.LocalEndPoint!).Address.ToString();
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// Запускает сервер и ждёт подключения одного клиента.
    /// </summary>
    public async Task StartHostAsync(string localName, int port = 5555)
    {
        IsHost = true;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        _client = await _listener.AcceptTcpClientAsync();
        SetupStream();
        IsConnected = true;

        await ExchangeNamesAsync(localName);
        StartReceiving();
    }

    /// <summary>
    /// Подключается к хосту по IP.
    /// </summary>
    public async Task ConnectAsync(string ip, string localName, int port = 5555)
    {
        IsHost = false;
        _client = new TcpClient();
        await _client.ConnectAsync(ip, port);
        SetupStream();
        IsConnected = true;

        await ExchangeNamesAsync(localName);
        StartReceiving();
    }

    /// <summary>
    /// Отправляет ход сопернику.
    /// </summary>
    public async Task SendMoveAsync(Move move)
    {
        if (_writer == null || !IsConnected) return;

        var json = JsonSerializer.Serialize(new MoveDto(move));
        await _writer.WriteLineAsync(json);
        await _writer.FlushAsync();
    }

    /// <summary>
    /// Закрывает соединение.
    /// </summary>
    public void Disconnect()
    {
        IsConnected = false;
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Dispose();
        _listener?.Stop();
    }

    private void SetupStream()
    {
        var stream = _client!.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream);
    }

    private async Task ExchangeNamesAsync(string localName)
    {
        var json = JsonSerializer.Serialize(new HandshakeDto { Name = localName });
        await _writer!.WriteLineAsync(json);
        await _writer.FlushAsync();

        var line = await _reader!.ReadLineAsync();
        if (line != null)
        {
            var dto = JsonSerializer.Deserialize<HandshakeDto>(line);
            RemoteName = dto?.Name ?? "Противник";
        }
    }

    /// <summary>
    /// Фоновый цикл чтения ходов от соперника.
    /// </summary>
    private async void StartReceiving()
    {
        try
        {
            while (IsConnected && _reader != null)
            {
                var line = await _reader.ReadLineAsync();
                if (line == null) break; // соединение закрыто

                var dto = JsonSerializer.Deserialize<MoveDto>(line);
                if (dto == null) continue;

                var move = dto.ToMove();

                // Вызываем событие в UI-потоке
                Dispatcher.UIThread.Invoke(() => MoveReceived?.Invoke(move));
            }
        }
        catch (Exception)
        {
            // Соединение оборвалось
        }

        IsConnected = false;
        Dispatcher.UIThread.Invoke(() => Disconnected?.Invoke("Соединение потеряно"));
    }
}

/// <summary>
/// DTO для сериализации Move в JSON.
/// Record нельзя десериализовать напрямую из-за конструктора, поэтому простой класс.
/// </summary>
public class MoveDto
{
    public int FromRow { get; set; }
    public int FromCol { get; set; }
    public int ToRow { get; set; }
    public int ToCol { get; set; }
    public bool IsCapture { get; set; }
    public int? CapturedRow { get; set; }
    public int? CapturedCol { get; set; }

    public MoveDto() { }

    public MoveDto(Move move)
    {
        FromRow = move.FromRow;
        FromCol = move.FromCol;
        ToRow = move.ToRow;
        ToCol = move.ToCol;
        IsCapture = move.IsCapture;
        CapturedRow = move.CapturedRow;
        CapturedCol = move.CapturedCol;
    }

    public Move ToMove() => new(FromRow, FromCol, ToRow, ToCol, IsCapture, CapturedRow, CapturedCol);
}

public class HandshakeDto
{
    public string Name { get; set; } = "";
}
