using Checkers;

namespace Checkers.ViewModels;

public class CellViewModel : BaseViewModel
{
    private Checker? _checker;
    private bool _isTarget;
    private bool _isSelected;
    private CellColor _isColor;

    public int Row { get; }
    public int Col { get; }

    public CellColor CellColor
    {
        get => IsTarget ? CellColor.Green  : _isColor;
        set => SetField(ref _isColor, value);
    }
    public Checker? Checker
    {
        get => _checker;
        set
        {
            if (SetField(ref _checker, value))
            {
                OnPropertyChanged(nameof(HasChecker));
                OnPropertyChanged(nameof(CheckerColorName));
                OnPropertyChanged(nameof(IsKing));
            }
        }
    }

    public bool IsTarget
    {
        get => _isTarget;
        set
        {
            if (SetField(ref _isTarget, value))
            {
                OnPropertyChanged(nameof(CellColor)); // Цвет зависит от IsTarget
            }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetField(ref _isSelected, value))
            {
                OnPropertyChanged(nameof(CellColor)); // Цвет может зависеть от выбора
            }
        }
    }

    // Автоматическое определение: темная клетка или светлая
    public bool IsDark => (Row + Col) % 2 != 0;

    // Является ли шашка дамкой
    public bool IsKing => Checker?.IsKing == true;

    // Метод для принудительного обновления всех свойств
    public void RefreshAll()
    {
        OnPropertyChanged(nameof(Checker));
        OnPropertyChanged(nameof(HasChecker));
        OnPropertyChanged(nameof(CheckerColorName));
        OnPropertyChanged(nameof(IsKing));
    }

    // Есть ли шашка в этой клетке?
    public bool HasChecker => Checker != null;

    // Цвет шашки строкой (для XAML триггеров)
    public string CheckerColorName => Checker?.Color.ToString() ?? "None";

    public CellViewModel(int row, int col)
    {
        Row = row;
        Col = col;
        // Можно задать базовый цвет сразу
        _isColor = IsDark ? CellColor.Black : CellColor.White;
    }
}
