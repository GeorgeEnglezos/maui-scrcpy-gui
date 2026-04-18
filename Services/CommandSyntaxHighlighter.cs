namespace ScrcpyGUI.Services;

/// <summary>
/// Shared syntax highlighting logic for Scrcpy command strings.
/// Provides color mappings and FormattedString building used by both the home and favorites screens.
/// </summary>
public static class CommandSyntaxHighlighter
{
    public static Dictionary<string, Color> GetCompleteColorMappings() => new()
    {
        { "--fullscreen",             ResourceColor("General") },
        { "--turn-screen-off",        ResourceColor("General") },
        { "--crop=",                  ResourceColor("General") },
        { "--capture-orientation=",   ResourceColor("General") },
        { "--stay-awake",             ResourceColor("General") },
        { "--window-title=",          ResourceColor("General") },
        { "--video-bit-rate=",        ResourceColor("General") },
        { "--window-borderless",      ResourceColor("General") },
        { "--always-on-top",          ResourceColor("General") },
        { "--disable-screensaver",    ResourceColor("General") },
        { "--video-codec=",           ResourceColor("General") },
        { "--video-encoder=",         ResourceColor("General") },
        { "--audio-bit-rate=",        ResourceColor("Audio") },
        { "--audio-buffer=",          ResourceColor("Audio") },
        { "--audio-codec-options=",   ResourceColor("Audio") },
        { "--audio-codec=",           ResourceColor("Audio") },
        { "--audio-encoder=",         ResourceColor("Audio") },
        { "--audio-dup",              ResourceColor("Audio") },
        { "--no-audio",               ResourceColor("Audio") },
        { "--new-display",            ResourceColor("VirtualDisplay") },
        { "--no-vd-destroy-content",  ResourceColor("VirtualDisplay") },
        { "--no-vd-system-decorations", ResourceColor("VirtualDisplay") },
        { "--max-size=",              ResourceColor("Recording") },
        { "--max-fps=",               ResourceColor("Recording") },
        { "--record-format=",         ResourceColor("Recording") },
        { "--record=",                ResourceColor("Recording") },
        { "--start-app",              ResourceColor("PackageSelector") },
    };

    public static Dictionary<string, Color> GetPartialColorMappings() => new()
    {
        { "--fullscreen",      ResourceColor("General") },
        { "--turn-screen-off", ResourceColor("General") },
        { "--video-bit-rate=", ResourceColor("General") },
        { "--audio-bit-rate=", ResourceColor("Audio") },
        { "--audio-buffer=",   ResourceColor("Audio") },
        { "--no-audio",        ResourceColor("Audio") },
        { "--new-display",     ResourceColor("VirtualDisplay") },
        { "--record-format=",  ResourceColor("Recording") },
        { "--record=",         ResourceColor("Recording") },
        { "--start-app",       ResourceColor("PackageSelector") },
    };

    public static Dictionary<string, Color> GetPackageOnlyColorMapping() => new()
    {
        { "--start-app", ResourceColor("PackageSelector") },
    };

    public static FormattedString BuildFormattedString(string commandText, Dictionary<string, Color> colorMapping)
    {
        var formattedString = new FormattedString();
        var parts = commandText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var span = new Span { Text = part };

            foreach (var mapping in colorMapping)
            {
                if (part.StartsWith(mapping.Key))
                {
                    span.TextColor = mapping.Value;
                    break;
                }
            }

            formattedString.Spans.Add(span);

            if (i < parts.Length - 1)
                formattedString.Spans.Add(new Span { Text = " " });
        }

        return formattedString;
    }

    private static Color ResourceColor(string key) =>
        (Color)Application.Current.Resources[key];
}
