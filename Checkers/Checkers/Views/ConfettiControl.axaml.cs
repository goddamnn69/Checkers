using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Checkers.Views;

public partial class ConfettiControl : UserControl
{
    private readonly DispatcherTimer _timer = new();
    private readonly Random _rng = new();
    private readonly List<(TextBlock tb, double vx, double vy, double rotation, double rotSpeed)> _particles = new();

    public ConfettiControl()
    {
        InitializeComponent();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnTick;
    }

    public void Start(string emoji)
    {
        _particles.Clear();
        ConfettiCanvas.Children.Clear();

        double w = Bounds.Width > 0 ? Bounds.Width : 500;
        double h = Bounds.Height > 0 ? Bounds.Height : 600;

        for (int i = 0; i < 160; i++)
        {
            var size = 14 + _rng.NextDouble() * 18;
            var tb = new TextBlock
            {
                Text = emoji,
                FontSize = size,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new RotateTransform(_rng.NextDouble() * 360)
            };

            double x = _rng.NextDouble() * w;
            double y = -_rng.NextDouble() * h * 0.5;
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            ConfettiCanvas.Children.Add(tb);

            double vx = (_rng.NextDouble() - 0.5) * 4;
            double vy = 2 + _rng.NextDouble() * 5;
            double rotSpeed = (_rng.NextDouble() - 0.5) * 10;

            _particles.Add((tb, vx, vy, 0, rotSpeed));
        }

        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        double h = Bounds.Height > 0 ? Bounds.Height : 600;
        int offScreen = 0;

        for (int i = 0; i < _particles.Count; i++)
        {
            var (tb, vx, vy, rotation, rotSpeed) = _particles[i];

            double x = Canvas.GetLeft(tb) + vx;
            double y = Canvas.GetTop(tb) + vy;
            rotation += rotSpeed;
            vy += 0.15;

            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            ((RotateTransform)tb.RenderTransform!).Angle = rotation;

            _particles[i] = (tb, vx, vy, rotation, rotSpeed);

            if (y > h + 50) offScreen++;
        }

        if (offScreen >= _particles.Count)
        {
            _timer.Stop();
            ConfettiCanvas.Children.Clear();
            _particles.Clear();
        }
    }
}
