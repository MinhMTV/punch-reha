namespace PunchReha.Services;

/// <summary>
/// Provides haptic and sound feedback for game events.
/// Uses platform-specific APIs when available.
/// </summary>
public static class FeedbackService
{
    /// <summary>
    /// Trigger haptic feedback for a punch hit.
    /// </summary>
    public static void OnHit(float power)
    {
        try
        {
#if ANDROID
            // Android vibration
            var vibrator = Platform.CurrentActivity?.GetSystemService(Android.Content.Context.VibratorService) as Android.OS.Vibrator;
            if (vibrator != null)
            {
                var amplitude = (int)(power * 255);
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    vibrator.Vibrate(Android.OS.VibrationEffect.CreateOneShot(50, amplitude));
                }
                else
                {
                    vibrator.Vibrate(50);
                }
            }
#elif IOS
            // iOS haptic
            var impact = UIKit.UIImpactFeedbackGenerator(UIImpactFeedbackGenerator.FeedbackStyle.Medium);
            impact.ImpactOccurred();
#elif WINDOWS
            // Windows: no haptic typically
            System.Diagnostics.Debug.WriteLine("[Feedback] Hit (power={power:F2})");
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Feedback] Haptic error: {ex.Message}");
        }
    }

    /// <summary>
    /// Trigger haptic feedback for a miss.
    /// </summary>
    public static void OnMiss()
    {
        try
        {
#if ANDROID
            var vibrator = Platform.CurrentActivity?.GetSystemService(Android.Content.Context.VibratorService) as Android.OS.Vibrator;
            vibrator?.Vibrate(20);
#elif IOS
            var notification = UIKit.UINotificationFeedbackGenerator();
            notification.NotificationOccurred(UINotificationFeedbackGenerator.FeedbackType.Error);
#endif
        }
        catch { }
    }

    /// <summary>
    /// Trigger haptic for combo milestone.
    /// </summary>
    public static void OnCombo(int combo)
    {
        if (combo % 5 == 0) // Every 5 combo
        {
            try
            {
#if ANDROID
                var vibrator = Platform.CurrentActivity?.GetSystemService(Android.Content.Context.VibratorService) as Android.OS.Vibrator;
                if (vibrator != null && Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    vibrator.Vibrate(Android.OS.VibrationEffect.CreateWaveform(new long[] { 0, 30, 50, 30 }, -1));
                }
#elif IOS
                var notification = UIKit.UINotificationFeedbackGenerator();
                notification.NotificationOccurred(UINotificationFeedbackGenerator.FeedbackType.Success);
#endif
            }
            catch { }
        }
    }

    /// <summary>
    /// Placeholder for sound effects.
    /// </summary>
    public static void PlaySound(string soundName)
    {
        // TODO: Implement with Plugin.Maui.Audio or platform MediaElement
        System.Diagnostics.Debug.WriteLine($"[Feedback] Sound: {soundName}");
    }
}
