using System.Text.Json;
using SquadCheck.Models;

namespace SquadCheck.Data;

/// <summary>
/// Saves and loads the app data from a JSON file.
/// </summary>
public class DataManager : IDataManager
{
    private readonly string _dataFolderPath;
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Sets up the path to the JSON data file.
    /// </summary>
    /// <param name="environment">The web host environment used to find the project folder.</param>
    public DataManager(IWebHostEnvironment environment)
    {
        if (environment == null)
        {
            throw new ArgumentNullException(nameof(environment));
        }

        _dataFolderPath = Path.Combine(environment.ContentRootPath, "App_Data");
        _dataFilePath = Path.Combine(_dataFolderPath, "squadCheckData.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        CreateDataFolderIfMissing();
    }

    /// <summary>
    /// Reads the saved JSON data.
    /// </summary>
    /// <returns>The saved application data, or a new empty data object if loading fails.</returns>
    public AppData LoadData()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                return new AppData();
            }

            string json = File.ReadAllText(_dataFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppData();
            }

            AppData? data = JsonSerializer.Deserialize<AppData>(json, _jsonOptions);

            if (data == null)
            {
                return new AppData();
            }

            return data;
        }
        catch (JsonException)
        {
            return new AppData();
        }
        catch (IOException)
        {
            return new AppData();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppData();
        }
    }

    /// <summary>
    /// Writes the current data to the JSON file.
    /// </summary>
    /// <param name="data">The application data to save.</param>
    public void SaveData(AppData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        try
        {
            CreateDataFolderIfMissing();

            string json = JsonSerializer.Serialize(data, _jsonOptions);

            File.WriteAllText(_dataFilePath, json);
        }
        catch (IOException exception)
        {
            throw new InvalidOperationException("The application data could not be saved.", exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new InvalidOperationException("The application does not have permission to save data.", exception);
        }
    }

    public bool DataFileExists()
    {
        return File.Exists(_dataFilePath);
    }

    private void CreateDataFolderIfMissing()
    {
        bool folderExists = Directory.Exists(_dataFolderPath);

        if (!folderExists)
        {
            Directory.CreateDirectory(_dataFolderPath);
        }
    }
}
