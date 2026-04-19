namespace Checkers;

/// <summary>
/// Класс управления основным игровым процессом.
/// </summary>
public class Game
{
    public bool isBotMode;
    /// <summary>
    /// Запускает игровой цикл шашек.
    /// </summary>
    public void Shashki()
    {
        var board = GameEngine.InitializeBoard(8, 8);
        Console.WriteLine("Хотите сыграть против бота? ");
        var answer = Console.ReadLine();
        if (answer.ToLower() == "да") isBotMode = true;
        
        while (true)
        {
            var currentPlayer = Player.CurrentPlayer;
            Console.Clear();
    
            // Проверка на победу перед началом хода
            var winner = GameEngine.CheckWinner(board);
            if (winner != null)
            {
                Console.WriteLine("***********************************");
                Console.WriteLine($"* ПОБЕДА! ВЫИГРАЛИ: {winner} *");
                Console.WriteLine("***********************************");
                board.Display();
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                break;
            }

            if (isBotMode && currentPlayer == CheckerColor.Black)
            {
                Bot.MakeBotMove(board, currentPlayer);
                continue;
            }
            
            // Выводим текущего игрока сверху для удобства
            Console.WriteLine($"--- ХОДЯТ: {currentPlayer} ---");

            board.Display();

            Console.WriteLine("\nВведите ход (откуда_ряд откуда_кол куда_ряд куда_кол)");
            Console.WriteLine("Пример: 5 0 4 1 (или 'exit' для выхода)");
            Console.Write("> ");
            //
            // string? input = Console.ReadLine();
            //
            // if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

            try
            {
                // string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // if (parts.Length != 4) throw new Exception("Нужно ввести 4 числа.");
                //
                // int fr = int.Parse(parts[0]);
                // int fc = int.Parse(parts[1]);
                // int tr = int.Parse(parts[2]);
                // int tc = int.Parse(parts[3]);
                //
                // var validMoves = GameEngine.GetValidMovesForPiece(board, fr, fc);
                // var move = validMoves.FirstOrDefault(m => m.ToRow == tr && m.ToCol == tc);
                // if (move == null)
                // {
                //     Console.WriteLine("!!! ОШИБКА: Недопустимый ход !!!");
                //     Console.ReadKey();
                //     continue;
                // } 
                //
                //
                // var result = GameEngine.ExecuteMove(board, move);
                //
                // if (result.IsChainCapturePossible)
                // {
                //     Console.WriteLine("--- НУЖНО БИТЬ ДАЛЬШЕ ЭТОЙ ЖЕ ШАШКОЙ ---");
                //     Console.ReadKey();
                // }


                Move? move = null;
                
                if (isBotMode && currentPlayer == CheckerColor.White)
                {
                    Bot.MakeBotMove(board, currentPlayer);
                }

                Thread.Sleep(500);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! ОШИБКА: {ex.Message} !!!");
                Console.ReadKey();
            }
        }
    }
}