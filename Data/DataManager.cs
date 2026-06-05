using System.Text.Json;
using SquadCheck.Models;

namespace SquadCheck.Data;

/// <summary>
/// Loads and saves the application data as JSON in the App_Data folder.
/// </summary>
public class DataManager : IDataManager
{
    private readonly string _dataFolderPath;
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a data manager and prepares the path to the JSON data file.
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
    /// Loads the saved JSON data from disk.
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
    /// Saves the current application data to disk.
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
