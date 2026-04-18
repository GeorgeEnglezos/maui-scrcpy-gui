using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ScrcpyGUI.Controls;
using ScrcpyGUI.Models;
using ScrcpyGUI.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace ScrcpyGUI
{
    /// <summary>
    /// Commands page for managing favorite Scrcpy commands and viewing recent command history.
    /// Displays saved commands with syntax highlighting, allows execution, copying, deletion,
    /// and exporting commands as .bat files.
    /// </summary>
    public partial class CommandsPage : ContentPage, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the observable collection of saved favorite commands.
        /// </summary>
        public ObservableCollection<string> SavedCommandsList { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Static reference to the application's saved data.
        /// </summary>
        public static ScrcpyGuiData jsonData = new ScrcpyGuiData();

        private string _mostRecentCommandText;

        /// <summary>
        /// Gets or sets the most recently executed command text.
        /// Implements property change notification for UI binding.
        /// </summary>
        public string MostRecentCommandText
        {
            get => _mostRecentCommandText;
            set
            {
                _mostRecentCommandText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the CommandsPage class.
        /// Sets up data binding context.
        /// </summary>
        public CommandsPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }

        /// <summary>
        /// Called when the page is appearing. Reloads saved data and updates the UI
        /// with favorite commands and most recent command.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            jsonData = DataStorage.LoadData();
            ReadSavedCommands();
            ReadLastUsedCommand();
        }

        /// <summary>
        /// Loads and displays the most recently executed command from storage.
        /// </summary>
        private void ReadLastUsedCommand()
        {
            string recentCommand = jsonData.MostRecentCommand ?? "No recent command found";
            MostRecentCommandText = recentCommand;
            Debug.WriteLine($"Recent Command: {recentCommand}");
        }

        /// <summary>
        /// Loads all saved favorite commands from storage into the UI collection.
        /// </summary>
        private void ReadSavedCommands()
        {
            SavedCommandsList.Clear();

            if (jsonData.FavoriteCommands != null)
            {
                foreach (var command in jsonData.FavoriteCommands)
                {
                    SavedCommandsList.Add(command);
                }
            }
        }

        /// <summary>
        /// Handles tap events on saved commands to execute them.
        /// </summary>
        /// <param name="sender">The tapped visual element.</param>
        /// <param name="e">Tap event arguments.</param>
        private async void OnCommandTapped(object sender, TappedEventArgs e)
        {
            if (sender is VisualElement element && element.BindingContext is string text)
            {
                var result = await Task.Run(() => AdbCmdService.RunScrcpyCommand(text));
                DataStorage.SaveMostRecentCommand(text);
                if (!string.IsNullOrEmpty(result.RawError))
                    await DisplayAlert("Error", result.RawError, "OK");
            }
        }

        /// <summary>
        /// Handles tap events on the recent command display to re-execute it.
        /// </summary>
        private async void OnRecentCommandTapped(object sender, EventArgs e)
        {
            var command = MostRecentCommandText ?? "";
            if (!string.IsNullOrEmpty(command))
            {
                var result = await Task.Run(() => AdbCmdService.RunScrcpyCommand(command));
                if (!string.IsNullOrEmpty(result.RawError))
                    await DisplayAlert("Error", result.RawError, "OK");
            }
        }

        /// <summary>
        /// Copies the most recent command to the clipboard.
        /// </summary>
        private async void OnCopyMostRecentCommand(object sender, EventArgs e)
        {
            await ClipboardHelper.CopyToClipboardAsync(MostRecentCommandText ?? "");
        }

        /// <summary>
        /// Copies a saved command to the clipboard.
        /// </summary>
        private async void OnCopyCommand(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is string command)
            {
                await ClipboardHelper.CopyToClipboardAsync(command);
            }
        }

        /// <summary>
        /// Deletes a saved command from favorites.
        /// Displays confirmation on success or error message on failure.
        /// </summary>
        private async void OnDeleteCommand(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is string text)
            {
                var data = DataStorage.LoadData();
                int indexToDelete = data.FavoriteCommands.IndexOf(text);

                if (indexToDelete >= 0)
                {
                    DataStorage.RemoveFavoriteCommandAtIndex(indexToDelete, data);
                    SavedCommandsList.Remove(text);

                    await DisplayAlert("Success", "Command deleted successfully!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Item not found!", "OK");
                }
            }
        }

        /// <summary>
        /// Exports a saved command as a Windows batch (.bat) file.
        /// Automatically names the file based on package name if present, with collision avoidance.
        /// </summary>
        private async void OnDownloadBat(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is string command)
            {
                try
                {
                    string baseFileName = "SavedCommand";
                    if (command.Contains("--start-app="))
                        baseFileName = RenameToPackage(command);

                    if (!string.IsNullOrEmpty(DeviceService.Instance.ScrcpyPath)) command = Path.Combine(DeviceService.Instance.ScrcpyPath, command);

                    string desktopPath = string.IsNullOrEmpty(DataStorage.StaticSavedData.AppSettings.DownloadPath) ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : DataStorage.StaticSavedData.AppSettings.DownloadPath;
                    string fullPath = Path.Combine(desktopPath, baseFileName + ".bat");

                    int counter = 1;
                    while (File.Exists(fullPath))
                    {
                        fullPath = Path.Combine(desktopPath, $"{baseFileName} ({counter}).bat");
                        counter++;
                    }

                    // Write the file asynchronously
                    await File.WriteAllTextAsync(fullPath, command);

                    await DisplayAlert("Success", $"Saved as: {Path.GetFileName(fullPath)} in \n{desktopPath}", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Couldn't save file: {ex.Message}", "OK");
                }
            }
        }

        /// <summary>
        /// Extracts the package name from a command containing --start-app parameter.
        /// Used for generating meaningful filenames when exporting commands as .bat files.
        /// </summary>
        /// <param name="command">The command string to parse.</param>
        /// <returns>The extracted package name, or "SavedCommand" if not found.</returns>
        private static string RenameToPackage(string command)
        {
            string packageName = string.Empty;
            int startIndex = command.IndexOf("--start-app=");

            if (startIndex == -1) return "SavedCommand";

            startIndex += "--start-app=".Length;
            int endIndex = command.IndexOf(" ", startIndex);

            if (endIndex != -1)
            {
                packageName = command.Substring(startIndex, endIndex - startIndex);
            }
            else
            {
                packageName = command.Substring(startIndex);
            }

            return packageName;
        }

        private static Dictionary<string, Color> ChooseColorMapping()
        {
            var colorSetting = jsonData.AppSettings?.FavoritesPageCommandColors ?? "Package Only";

            return colorSetting switch
            {
                "None" => new Dictionary<string, Color>(),
                "Important" => CommandSyntaxHighlighter.GetPartialColorMappings(),
                "Complete" => CommandSyntaxHighlighter.GetCompleteColorMappings(),
                _ => CommandSyntaxHighlighter.GetPackageOnlyColorMapping()
            };
        }

        public static FormattedString CreateColoredCommandText(string commandText)
        {
            return CommandSyntaxHighlighter.BuildFormattedString(commandText, ChooseColorMapping());
        }

        /// <summary>
        /// Event raised when a property value changes (INotifyPropertyChanged implementation).
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for a given property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed (auto-filled by compiler).</param>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Value converter for applying syntax highlighting to command strings in XAML bindings.
    /// Converts plain command text strings to formatted strings with color spans.
    /// </summary>
    public class CommandColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a command string to a formatted string with syntax highlighting.
        /// </summary>
        /// <param name="value">The command string to convert.</param>
        /// <param name="targetType">The target type (unused).</param>
        /// <param name="parameter">Conversion parameter (unused).</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>FormattedString with syntax highlighting applied.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string commandText)
                return new FormattedString();

            return CommandsPage.CreateColoredCommandText(commandText);
        }

        /// <summary>
        /// Not implemented - conversion back is not supported.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}