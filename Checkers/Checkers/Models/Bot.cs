namespace Checkers;

public class Bot
{
    /// <summary>
    /// Выбирает лучший ход для указанного цвета и выполняет его.
    /// Обрабатывает цепные взятия в цикле.
    /// </summary>
    public static void MakeBotMove(Board board, CheckerColor color)
    {
        var allMoves = GameEngine.GetAllAvailableMoves(board, color);
        if (allMoves.Count == 0 || !board.HasCheckersColor(color)) return;

        var move = ChooseBestMove(allMoves, board, color);
        var result = GameEngine.ExecuteMove(board, move);

        // Завершаем цепные взятия
        while (result.IsChainCapturePossible)
        {
            var nextCaptures = GameEngine.GetValidMovesForPiece(board, move.ToRow, move.ToCol)
                                         .Where(m => m.IsCapture).ToList();
            if (nextCaptures.Count == 0) break;
            move = nextCaptures[new Random().Next(nextCaptures.Count)];
            result = GameEngine.ExecuteMove(board, move);
        }
    }

    /// <summary>
    /// Выбирает лучший ход по приоритетам:
    /// 1. Взятия (уже обеспечены GetAllAvailableMoves)
    /// 2. Ходы, ведущие к промоушену в дамку
    /// 3. Ходы дамками (активное использование)
    /// 4. Продвижение вперёд ближе к центру
    /// </summary>
    private static Move ChooseBestMove(List<Move> moves, Board board, CheckerColor color)
    {
        var random = new Random();

        // Приоритет 1: ходы, превращающие в дамку
        int promotionRow = color == CheckerColor.White ? 0 : board.Rows - 1;
        var promotionMoves = moves.Where(m => m.ToRow == promotionRow).ToList();
        if (promotionMoves.Count > 0)
            return promotionMoves[random.Next(promotionMoves.Count)];

        // Приоритет 2: взятия дамками (они мощнее)
        var kingCaptures = moves.Where(m => m.IsCapture &&
            board.GetCell(m.FromRow, m.FromCol)?.Checker?.IsKing == true).ToList();
        if (kingCaptures.Count > 0)
            return kingCaptures[random.Next(kingCaptures.Count)];

        // Приоритет 3: любые взятия
        var captures = moves.Where(m => m.IsCapture).ToList();
        if (captures.Count > 0)
            return captures[random.Next(captures.Count)];

        // Приоритет 4: ходы дамками
        var kingMoves = moves.Where(m =>
            board.GetCell(m.FromRow, m.FromCol)?.Checker?.IsKing == true).ToList();
        if (kingMoves.Count > 0)
            return kingMoves[random.Next(kingMoves.Count)];

        // Приоритет 5: продвижение вперёд, предпочтение центру
        int direction = color == CheckerColor.White ? -1 : 1;
        var forwardMoves = moves
            .Where(m => (m.ToRow - m.FromRow) * direction > 0)
            .OrderBy(m => Math.Abs(m.ToCol - 3.5)) // ближе к центру
            .ToList();
        if (forwardMoves.Count > 0)
            return forwardMoves[random.Next(Math.Min(3, forwardMoves.Count))]; // из топ-3 центральных

        return moves[random.Next(moves.Count)];
    }
}