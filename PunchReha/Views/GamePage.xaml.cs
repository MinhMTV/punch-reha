using PunchReha.ViewModels;
using PunchReha.Services;
using PunchReha.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PunchReha.Views;

public partial class GamePage : ContentPage
{
    private GameViewModel? _vm;
    private IDispatcherTimer? _renderTimer;
    private readonly GameDrawable _drawable;

    public GamePage(GameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
        _drawable = new GameDrawable(vm);
        GameCanvas.Drawable = _drawable;

        // Touch handling — covers both tap and press-and-hold
        var touchEffect = new TouchEffect();
        touchEffect.TouchAction += OnTouchAction;
        GameArea.Effects.Add(touchEffect);

        // Render loop for targets
        _renderTimer = Dispatcher.CreateTimer();
        _renderTimer.Interval = TimeSpan.FromMilliseconds(50); // 20 FPS
        _renderTimer.Tick += OnRenderTick;
        _renderTimer.Start();

        // Listen for state changes
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GameViewModel.GameState))
            {
                MainThread.BeginInvokeOnMainThread(UpdateOverlays);
            }
        };
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (_vm != null)
        {
            _vm.ScreenWidth = (float)width;
            _vm.ScreenHeight = (float)(height - 120); // Subtract stats/power bars
        }
    }

    private void OnRenderTick(object? sender, EventArgs e)
    {
        GameCanvas.Invalidate();

        // Update countdown display
        if (_vm?.GameState == GameState.Countdown)
        {
            CountdownLabel.Text = _vm.CountdownValue > 0 ? _vm.CountdownValue.ToString() : "LOS!";
        }

        // Update waiting message visibility
        WaitingLabel.IsVisible = _vm?.GameState == GameState.Playing && _vm.Targets.Count == 0;
    }

    private void UpdateOverlays()
    {
        if (_vm == null) return;

        CountdownOverlay.IsVisible = _vm.GameState == GameState.Countdown;
        PausedOverlay.IsVisible = _vm.GameState == GameState.Paused;
        WaitingLabel.IsVisible = _vm.GameState == GameState.Playing && _vm.Targets.Count == 0;
    }

    private void OnTouchAction(object? sender, TouchActionEventArgs args)
    {
        if (_vm == null) return;

        switch (args.Type)
        {
            case TouchActionType.Pressed:
                _vm.OnTouchDown((float)args.Location.X, (float)args.Location.Y);
                break;
            case TouchActionType.Released:
            case TouchActionType.Cancelled:
                _vm.OnTouchUp();
                break;
        }
    }

    private void OnResumeClicked(object? sender, EventArgs e)
    {
        _vm?.ResumeCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _renderTimer?.Stop();
    }
}

/// <summary>
/// Draws targets and hit effects on the game canvas.
/// </summary>
public class GameDrawable : IDrawable
{
    private readonly GameViewModel _vm;

    private static readonly Dictionary<PunchDirection, Color> DirectionColors = new()
    {
        [PunchDirection.Left] = Colors.Red,
        [PunchDirection.Right] = Colors.DodgerBlue,
        [PunchDirection.High] = Colors.LimeGreen,
        [PunchDirection.Low] = Colors.Orange,
        [PunchDirection.Straight] = Colors.MediumPurple
    };

    private static readonly Dictionary<PunchDirection, string> DirectionLabels = new()
    {
        [PunchDirection.Left] = "⬅️",
        [PunchDirection.Right] = "➡️",
        [PunchDirection.High] = "⬆️",
        [PunchDirection.Low] = "⬇️",
        [PunchDirection.Straight] = "🎯"
    };

    public GameDrawable(GameViewModel vm) => _vm = vm;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Background
        canvas.FillColor = Color.FromArgb("#0A0A1A");
        canvas.FillRectangle(dirtyRect);

        // Draw grid lines (subtle)
        canvas.StrokeColor = Color.FromArgb("#1A1A2E");
        canvas.StrokeSize = 1;
        for (int i = 1; i < 4; i++)
        {
            canvas.DrawLine(dirtyRect.Width * i / 4, 0, dirtyRect.Width * i / 4, dirtyRect.Height);
            canvas.DrawLine(0, dirtyRect.Height * i / 4, dirtyRect.Width, dirtyRect.Height * i / 4);
        }

        // Draw targets
        foreach (var target in _vm.Targets)
        {
            DrawTarget(canvas, target, dirtyRect);
        }

        // Draw direction zone labels (always visible)
        canvas.FontSize = 14;
        canvas.FontColor = Color.FromArgb("#333333");
        canvas.DrawString("LINKS", 10, dirtyRect.Height * 0.5f, HorizontalAlignment.Left);
        canvas.DrawString("RECHTS", dirtyRect.Width - 10, dirtyRect.Height * 0.5f, HorizontalAlignment.Right);
        canvas.DrawString("HOCH", dirtyRect.Width / 2, 30, HorizontalAlignment.Center);
        canvas.DrawString("TIEF", dirtyRect.Width / 2, dirtyRect.Height - 30, HorizontalAlignment.Center);
    }

    private void DrawTarget(ICanvas canvas, Target target, RectF rect)
    {
        var pos = GetPosition(target.Direction, rect);
        var radius = 45f;

        if (target.IsHit)
        {
            // Hit effect — expanding green circle
            var age = (DateTime.UtcNow - target.CreatedAt).TotalMilliseconds;
            var expansion = (float)(age / 300.0) * 60f;
            var alpha = 1f - (float)(age / 300.0).Clamp(0, 1);

            canvas.FillColor = Color.FromArgb($"#{(int)(alpha * 255):X2}4CAF50");
            canvas.FillCircle(pos.X, pos.Y, radius + expansion);
            return;
        }

        // Calculate remaining lifetime percentage for fade effect
        var elapsed = (DateTime.UtcNow - target.CreatedAt).TotalMilliseconds;
        var lifetimePercent = 1.0 - (elapsed / target.LifetimeMs).Clamp(0, 1);
        var alpha2 = (float)lifetimePercent;

        var color = DirectionColors.GetValueOrDefault(target.Direction, Colors.White);

        // Outer glow
        canvas.FillColor = color.WithAlpha(alpha2 * 0.3f);
        canvas.FillCircle(pos.X, pos.Y, radius + 25);

        // Main circle
        canvas.FillColor = color.WithAlpha(alpha2);
        canvas.FillCircle(pos.X, pos.Y, radius);

        // Inner ring
        canvas.StrokeColor = Colors.White.WithAlpha(alpha2);
        canvas.StrokeSize = 3;
        canvas.DrawCircle(pos.X, pos.Y, radius - 10);

        // Center dot
        canvas.FillColor = Colors.White.WithAlpha(alpha2);
        canvas.FillCircle(pos.X, pos.Y, 12);

        // Direction emoji
        canvas.FontSize = 20;
        canvas.FontColor = Colors.White.WithAlpha(alpha2);
        var label = DirectionLabels.GetValueOrDefault(target.Direction, "");
        canvas.DrawString(label, pos.X, pos.Y - 55, HorizontalAlignment.Center);

        // Lifetime bar (shrinking)
        var barWidth = 60f * (float)lifetimePercent;
        canvas.FillColor = Colors.White.WithAlpha(alpha2 * 0.5f);
        canvas.FillRectangle(pos.X - 30, pos.Y + radius + 8, barWidth, 4);
    }

    private static PointF GetPosition(PunchDirection dir, RectF rect) => dir switch
    {
        PunchDirection.Left => new PointF(rect.Width * 0.18f, rect.Height * 0.5f),
        PunchDirection.Right => new PointF(rect.Width * 0.82f, rect.Height * 0.5f),
        PunchDirection.High => new PointF(rect.Width * 0.5f, rect.Height * 0.18f),
        PunchDirection.Low => new PointF(rect.Width * 0.5f, rect.Height * 0.78f),
        _ => new PointF(rect.Width * 0.5f, rect.Height * 0.5f)
    };
}

/// <summary>
/// Simple touch effect for tracking press/release.
/// Use this instead of PanGestureRecognizer for cleaner press-and-hold detection.
/// </summary>
public class TouchActionEventArgs : EventArgs
{
    public TouchActionType Type { get; init; }
    public Point Location { get; init; }
}

public enum TouchActionType { Entered, Pressed, Moved, Released, Exited, Cancelled }

public class TouchEffect : RoutingEffect
{
    public event EventHandler<TouchActionEventArgs>? TouchAction;
    public void OnTouchAction(TouchActionEventArgs args) => TouchAction?.Invoke(this, args);
}

/// <summary>
/// Extension method for clamping values.
/// </summary>
public static class DoubleExtensions
{
    public static double Clamp(this double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));
}
