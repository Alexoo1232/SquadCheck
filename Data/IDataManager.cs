using SquadCheck.Models;

namespace SquadCheck.Data;

/// <summary>
/// Defines the methods used to load and save the application data.
/// </summary>
public interface IDataManager
{
    /// <summary>
    /// Loads all saved application data.
    /// </summary>
    /// <returns>The saved application data, or empty data if no valid file exists.</returns>
    AppData LoadData();

    /// <summary>
    /// Saves all application data.
    /// </summary>
    /// <param name="data">The data object to save.</param>
    void SaveData(AppData data);

    bool DataFileExists();
}
