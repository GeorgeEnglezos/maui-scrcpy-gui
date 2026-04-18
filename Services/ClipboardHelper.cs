using System.Diagnostics;

namespace ScrcpyGUI.Services;

/// <summary>
/// Utility class for clipboard operations with platform-specific error handling.
/// </summary>
public static class ClipboardHelper
{
    /// <summary>
    /// Copies text to the system clipboard and displays a confirmation dialog.
    /// Handles platform-specific clipboard errors gracefully.
    /// </summary>
    /// <param name="text">The text to copy to clipboard.</param>
    /// <returns>Empty string on completion.</returns>
    public static async Task<string> CopyToClipboardAsync(string text)
    {
        if (text != null && !string.IsNullOrEmpty(text))
        {
            try
            {
                await Clipboard.SetTextAsync(text.ToString());
                await Application.Current.MainPage.DisplayAlert("Copied!", "Text copied to clipboard.", "OK");
            }
            catch (FeatureNotSupportedException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Clipboard functionality not supported.", "OK");
                Debug.WriteLine($"Clipboard not supported: {ex.Message}");
            }
            catch (PermissionException ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Clipboard permission denied.", "OK");
                Debug.WriteLine($"Clipboard permission denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
                Debug.WriteLine($"Clipboard error: {ex.Message}");
            }
        }
        return "";
    }
}
