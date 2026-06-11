using SquadCheck.Models;

namespace SquadCheck.Data;

/// <summary>
/// Describes how app data is loaded and saved.
/// </summary>
public interface IDataManager
{
    /// <summary>
    /// Loads the saved app data.
    /// </summary>
    /// <returns>The saved application data, or empty data if no valid file exists.</returns>
    AppData LoadData();

    /// <summary>
    /// Saves the app data.
    /// </summary>
    /// <param name="data">The data object to save.</param>
    void SaveData(AppData data);

    bool DataFileExists();
}
