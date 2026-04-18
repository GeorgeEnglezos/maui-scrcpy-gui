using ScrcpyGUI.Models;

namespace ScrcpyGUI.Services;

/// <summary>
/// Singleton service holding the current device session state.
/// Registered via MAUI's DI container; accessible statically via <see cref="Instance"/>.
/// </summary>
public class DeviceService
{
    public static DeviceService Instance { get; private set; } = new();

    public DeviceService()
    {
        Instance = this;
    }

    public List<ConnectedDevice> ConnectedDevices { get; set; } = new();
    public ConnectedDevice SelectedDevice { get; set; } = new();
    public string ScrcpyPath { get; set; } = "";
    public string RecordingsPath { get; set; } = "";
}
