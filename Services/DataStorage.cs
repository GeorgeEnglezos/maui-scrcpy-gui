using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using ScrcpyGUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ScrcpyGUI.Services;

/// <summary>
/// Static service class for persisting and retrieving application data.
/// Handles JSON serialization/deserialization of user settings, commands, and preferences.
/// </summary>
public static class DataStorage
{
    /// <summary>
    /// Gets or sets the cached in-memory copy of saved application data.
    /// </summary>
    public static ScrcpyGuiData StaticSavedData { get; set; } = new ScrcpyGuiData();

    /// <summary>
    /// Gets the file path where application settings are stored.
    /// </summary>
    public static readonly string settingsPath = Path.Combine(FileSystem.AppDataDirectory, "ScrcpyGui-Data.json");

    /// <summary>
    /// Loads application data from the JSON settings file.
    /// Creates a new settings file with defaults if it doesn't exist.
    /// </summary>
    /// <returns>Loaded ScrcpyGuiData object, or default values if loading fails.</returns>
    public static ScrcpyGuiData LoadData()
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                // File doesn't exist, create it with default data
                SaveData(new ScrcpyGuiData());
                return StaticSavedData;
            }

            var jsonString = File.ReadAllText(settingsPath, Encoding.UTF8);
            StaticSavedData = JsonConvert.DeserializeObject<ScrcpyGuiData>(jsonString) ?? new ScrcpyGuiData();
            return StaticSavedData;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load data: {ex.Message}");
            return new ScrcpyGuiData(); // Fallback
        }
    }

    /// <summary>
    /// Saves application data to the JSON settings file.
    /// Creates necessary directories if they don't exist.
    /// </summary>
    /// <param name="data">The data to save.</param>
    public static void SaveData(ScrcpyGuiData data)
    {
        try
        {
            // Ensure directory exists
            var dir = Path.GetDirectoryName(settingsPath);
            if (!Directory.Exists(dir))
                CreateFile();

            StaticSavedData = data;
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(settingsPath, jsonString, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save data: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates the settings file and its parent directory if they don't exist.
    /// Initializes the file with an empty JSON object.
    /// </summary>
    private static void CreateFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(settingsPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(settingsPath))
            {
                File.WriteAllText(settingsPath, "{}"); // Avoid deserialization issues
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to create file: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Adds a new command to the user's favorite commands list and saves the data.
    /// </summary>
    /// <param name="newCommand">The command string to add to favorites.</param>
    public static void AppendFavoriteCommand(string newCommand)
    {
        var data = LoadData();
        data.FavoriteCommands.Add(newCommand);
        SaveData(data);
    }

    /// <summary>
    /// Removes a favorite command at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the command to remove.</param>
    /// <param name="data">The data object containing the commands.</param>
    /// <returns>True if the command was removed; false if the index was invalid.</returns>
    public static bool RemoveFavoriteCommandAtIndex(int index, ScrcpyGuiData data)
    {
        if (index >= 0 && index < data.FavoriteCommands.Count)
        {
            data.FavoriteCommands.RemoveAt(index);
            SaveData(data);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Saves the most recently executed command to persistent storage.
    /// </summary>
    /// <param name="command">The command string to save.</param>
    public static void SaveMostRecentCommand(string command)
    {
        var data = LoadData();
        data.MostRecentCommand = command;
        SaveData(data);
    }

    /// <summary>
    /// Deletes the settings file, clearing all saved application data.
    /// </summary>
    public static void ClearAll()
    {
        if (File.Exists(settingsPath))
        {
            File.Delete(settingsPath);
        }
    }

    /// <summary>
    /// Validates that a folder path exists, creating it if necessary.
    /// </summary>
    /// <param name="folderPath">The folder path to validate/create.</param>
    /// <param name="fallbackPath">Optional fallback path if validation fails.</param>
    /// <returns>The validated folder path, or the fallback path if validation fails.</returns>
    public static string ValidateAndCreatePath(string folderPath, string fallbackPath = null)
    {
        // If the path is null or empty, use fallback or return empty string
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return fallbackPath ?? string.Empty;
        }
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to create directory '{folderPath}': {ex.Message}");
            return fallbackPath ?? string.Empty;
        }
    }

}