namespace Checkers;

/// <summary>
/// Представляет один ход в игре.
/// </summary>
/// <param name="FromRow">Строка начала</param>
/// <param name="FromCol">Колонка начала</param>
/// <param name="ToRow">Строка назначения</param>
/// <param name="ToCol">Колонка назначения</param>
/// <param name="IsCapture">Является ли ход взятием</param>
/// <param name="CapturedRow">Строка съеденной фигуры (если есть)</param>
/// <param name="CapturedCol">Колонка съеденной фигуры (если есть)</param>
public record Move(int FromRow, int FromCol, int ToRow, int ToCol, bool IsCapture, int? CapturedRow = null, int? CapturedCol = null)
{
    public int MiddleRow => CapturedRow ?? (FromRow + ToRow) / 2;
    public int MiddleCol => CapturedCol ?? (FromCol + ToCol) / 2;
}

/// <summary>
/// Результат выполнения хода для обработки во ViewModel.
/// </summary>
/// <param name="Success">Успешен ли ход</param>
/// <param name="Message">Сообщение об ошибке или статусе</param>
/// <param name="Move">Данные выполненного хода</param>
/// <param name="IsChainCapturePossible">Нужно ли продолжать бить той же фигурой</param>
public record MoveResult(bool Success, string Message = "", Move? Move = null, bool IsChainCapturePossible = false);

