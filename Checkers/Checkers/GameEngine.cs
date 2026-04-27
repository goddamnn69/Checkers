using Checkers;

namespace Checkers;

public static class GameEngine
{
    public static Board InitializeBoard(int rows, int columns)
    {
        var board = new Board(rows, columns);

        for (var row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if ((row + col) % 2 != 0)
                {
                    var cell = new Cell(row, col);
                    board.SetCell(row, col, cell);

                    if (row < 3) // Обычно 3 ряда
                    {
                        cell.Checker = new Checker(CheckerColor.Black);
                    }
                    else if (row > 4)
                    {
                        cell.Checker = new Checker(CheckerColor.White);
                    }
                }
            }
        }
        return board;
    }

    /// <summary>
    /// Возвращает все допустимые ходы для игрока с учетом правила обязательного взятия.
    /// </summary>
    public static List<Move> GetAllAvailableMoves(Board board, CheckerColor color)
    {
        var captureMoves = new List<Move>();
        var regularMoves = new List<Move>();

        for (int r = 0; r < board.Rows; r++)
        {
            for (int c = 0; c < board.Columns; c++)
            {
                var cell = board.GetCell(r, c);
                if (cell?.Checker != null && cell.Checker.Color == color)
                {
                    var moves = GetMovesForPiece(board, r, c);
                    captureMoves.AddRange(moves.Where(m => m.IsCapture));
                    regularMoves.AddRange(moves.Where(m => !m.IsCapture));
                }
            }
        }

        // Если есть возможность бить — возвращаем только взятия (Enforce mandatory capture)
        return captureMoves.Any() ? captureMoves : regularMoves;
    }

    /// <summary>
    /// Возвращает ходы только для конкретной выбранной шашки (для подсветки в WPF).
    /// </summary>
    public static List<Move> GetValidMovesForPiece(Board board, int row, int col)
    {
        var cell = board.GetCell(row, col);
        if (cell?.Checker == null) return new List<Move>();

        var allValidMoves = GetAllAvailableMoves(board, cell.Checker.Color);
        return allValidMoves.Where(m => m.FromRow == row && m.FromCol == col).ToList();
    }
    /// <summary>
    /// Выполняет ход и возвращает результат.
    /// </summary>
    public static MoveResult ExecuteMove(Board board, Move move, Player player)
    {
        var cellFrom = board.GetCell(move.FromRow, move.FromCol);
        if (cellFrom?.Checker == null) return new MoveResult(false, "Нет шашки");

        var validMoves = GetValidMovesForPiece(board, move.FromRow, move.FromCol);
        if (!validMoves.Any(m => m.ToRow == move.ToRow && m.ToCol == move.ToCol))
            return new MoveResult(false, "Недопустимый ход!");

        var checker = cellFrom.Checker;
        var cellTo = board.GetCell(move.ToRow, move.ToCol);

        // Совершаем перемещение
        cellTo!.Checker = checker;
        cellFrom.Checker = null;

        // Превращение в дамку СРАЗУ при достижении последнего ряда (по правилам русских шашек)
        if (!checker.IsKing)
        {
            if (checker.Color == CheckerColor.White && move.ToRow == 0)
                checker.IsKing = true;
            else if (checker.Color == CheckerColor.Black && move.ToRow == board.Rows - 1)
                checker.IsKing = true;
        }

        bool isChainPossible = false;

        if (move.IsCapture)
        {
            // Удаляем съеденную шашку
            board.ClearCell(move.MiddleRow, move.MiddleCol);

            // Проверяем, может ли эта же шашка бить дальше (уже как дамка, если была превращена)
            var nextCaptures = GetMovesForPiece(board, move.ToRow, move.ToCol).Where(m => m.IsCapture).ToList();
            if (nextCaptures.Any())
            {
                isChainPossible = true;
            }
        }

        if (!isChainPossible)
        {
            player.SwitchPlayer();
        }

        return new MoveResult(true, isChainPossible ? "Бейте дальше!" : "Ход выполнен", move, isChainPossible);
    }

    /// <summary>
    /// Проверяет состояние игры и определяет победителя.
    /// </summary>
    /// <param name="board">Игровая доска.</param>
    /// <returns>Цвет победителя или null, если игра продолжается.</returns>
    public static CheckerColor? CheckWinner(Board board)
    {
        bool whiteHasPieces = false;
        bool blackHasPieces = false;
        bool whiteHasMoves = false;
        bool blackHasMoves = false;

        for (int r = 0; r < board.Rows; r++)
        {
            for (int c = 0; c < board.Columns; c++)
            {
                var cell = board.GetCell(r, c);
                if (cell?.Checker != null)
                {
                    if (cell.Checker.Color == CheckerColor.White)
                    {
                        whiteHasPieces = true;
                        if (!whiteHasMoves && GetValidMovesForPiece(board, r, c).Any())
                            whiteHasMoves = true;
                    }
                    else
                    {
                        blackHasPieces = true;
                        if (!blackHasMoves && GetValidMovesForPiece(board, r, c).Any())
                            blackHasMoves = true;
                    }
                }
            }
        }

        // Если у игрока нет фигур или нет ходов — он проиграл
        if (!whiteHasPieces || !whiteHasMoves) return CheckerColor.Black;
        if (!blackHasPieces || !blackHasMoves) return CheckerColor.White;

        return null;
    }

    private static List<Move> GetMovesForPiece(Board board, int row, int col)
    {
        var moves = new List<Move>();
        var cell = board.GetCell(row, col);
        if (cell?.Checker == null) return moves;

        var checker = cell.Checker;

        // Направления (вверх-лево, вверх-право, вниз-лево, вниз-право)
        int[] dr = { -1, -1, 1, 1 };
        int[] dc = { -1, 1, -1, 1 };

        if (checker.IsKing)
        {
            // Логика Дамки: летает по диагоналям на любое расстояние
            for (int i = 0; i < 4; i++)
            {
                int tr = row + dr[i];
                int tc = col + dc[i];
                bool pieceFound = false;

                while (board.GetCell(tr, tc) != null)
                {
                    var targetCell = board.GetCell(tr, tc);
                    if (targetCell?.Checker == null)
                    {
                        if (!pieceFound)
                        {
                            // Обычный ход дамки по пустой клетке
                            moves.Add(new Move(row, col, tr, tc, false));
                        }
                        else
                        {
                            // Клетки приземления после взятия
                            // (уже нашли врага, это пустые клетки за ним)
                            // Мы не выходим из цикла, чтобы добавить ВСЕ клетки приземления
                        }
                    }
                    else
                    {
                        if (pieceFound) break; // Вторая фигура на пути - путь закрыт

                        if (targetCell.Checker.Color == checker.Color) break; // Своя фигура - путь закрыт

                        // Нашли врага - проверяем клетку ЗА ним
                        int nr = tr + dr[i];
                        int nc = tc + dc[i];
                        var landingCell = board.GetCell(nr, nc);

                        if (landingCell != null && landingCell.Checker == null)
                        {
                            // Нашли взятие!
                            pieceFound = true; // Помечаем, что нашли фигуру для взятия
                            int cr = tr;
                            int cc = tc;

                            // Добавляем все возможные клетки приземления за этой фигурой
                            while (landingCell != null && landingCell.Checker == null)
                            {
                                moves.Add(new Move(row, col, nr, nc, true, cr, cc));
                                nr += dr[i];
                                nc += dc[i];
                                landingCell = board.GetCell(nr, nc);
                            }
                        }
                        break; // Дамка не может перепрыгнуть через две фигуры или через врага без пустой клетки
                    }
                    tr += dr[i];
                    tc += dc[i];
                }
            }
        }
        else
        {
            // Логика обычной шашки
            for (int i = 0; i < 4; i++)
            {
                // 1. Обычные ходы (на 1 клетку)
                int tr1 = row + dr[i];
                int tc1 = col + dc[i];
                if (IsValidSimpleMove(board, checker, row, col, tr1, tc1))
                {
                    moves.Add(new Move(row, col, tr1, tc1, false));
                }

                // 2. Взятия (на 2 клетки через противника)
                int tr2 = row + dr[i] * 2;
                int tc2 = col + dc[i] * 2;
                int mr = row + dr[i];
                int mc = col + dc[i];
                if (IsValidCaptureMove(board, checker, tr2, tc2, mr, mc))
                {
                    moves.Add(new Move(row, col, tr2, tc2, true, mr, mc));
                }
            }
        }

        return moves;
    }

    private static bool IsValidSimpleMove(Board board, Checker checker, int fr, int fc, int tr, int tc)
    {
        var target = board.GetCell(tr, tc);
        if (target == null || target.Checker != null) return false;

        if (!checker.IsKing)
        {
            if (checker.Color == CheckerColor.White && tr >= fr) return false;
            if (checker.Color == CheckerColor.Black && tr <= fr) return false;
        }
        return true;
    }

    private static bool IsValidCaptureMove(Board board, Checker attacker, int tr, int tc, int mr, int mc)
    {
        var target = board.GetCell(tr, tc);
        var middle = board.GetCell(mr, mc);

        if (target == null || target.Checker != null) return false;
        if (middle?.Checker == null || middle.Checker.Color == attacker.Color) return false;

        return true;
    }
}
