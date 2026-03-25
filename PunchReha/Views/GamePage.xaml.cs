using PunchReha.ViewModels;
using PunchReha.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PunchReha.Views;

public partial class GamePage : ContentPage
{
    private GameViewModel? _vm;
    private IDispatcherTimer? _renderTimer;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;

        // Set up touch handling on the game area
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GameArea.GestureRecognizers.Add(panGesture);

        // Also handle simple taps
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GameArea.GestureRecognizers.Add(tapGesture);

        // Render loop for targets
        _renderTimer = Dispatcher.CreateTimer();
        _renderTimer.Interval = TimeSpan.FromMilliseconds(50); // 20 FPS
        _renderTimer.Tick += (s, e) => GameCanvas.Invalidate();
        _renderTimer.Start();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (_vm != null)
        {
            _vm.ScreenWidth = (float)width;
            _vm.ScreenHeight = (float)height;
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (_vm == null) return;

        var position = e.GetPosition(GameArea);
        if (position.HasValue)
        {
            _vm.OnTouchDown((float)position.Value.X, (float)position.Value.Y);
            // Simulate quick release for tap
            Dispatcher.DispatchDelayed(() => _vm.OnTouchUp(), TimeSpan.FromMilliseconds(100));
        }
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (_vm == null) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _vm.OnTouchDown((float)e.TotalX, (float)e.TotalY);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _vm.OnTouchUp();
                break;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _renderTimer?.Stop();
        _vm?.GoBackCommand.Execute(null);
    }
}

/// <summary>
/// Draws targets on the game canvas.
/// </summary>
public class GameDrawable : IDrawable
{
    private readonly GameViewModel _vm;

    public GameDrawable(GameViewModel vm) => _vm = vm;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(dirtyRect);

        foreach (var target in _vm.Targets)
        {
            var pos = GetPosition(target.Direction, dirtyRect);
            var color = target.Direction switch
            {
                Models.PunchDirection.Left => Colors.Red,
                Models.PunchDirection.Right => Colors.Blue,
                Models.PunchDirection.High => Colors.Green,
                Models.PunchDirection.Low => Colors.Orange,
                _ => Colors.Purple
            };

            canvas.FillColor = color;
            canvas.FillCircle(pos.X, pos.Y, 40);

            canvas.FillColor = Colors.White;
            canvas.FillCircle(pos.X, pos.Y, 10);
        }
    }

    private PointF GetPosition(Models.PunchDirection dir, RectF rect) => dir switch
    {
        Models.PunchDirection.Left => new PointF(rect.Width * 0.2f, rect.Height * 0.5f),
        Models.PunchDirection.Right => new PointF(rect.Width * 0.8f, rect.Height * 0.5f),
        Models.PunchDirection.High => new PointF(rect.Width * 0.5f, rect.Height * 0.2f),
        Models.PunchDirection.Low => new PointF(rect.Width * 0.5f, rect.Height * 0.75f),
        _ => new PointF(rect.Width * 0.5f, rect.Height * 0.5f)
    };
}
