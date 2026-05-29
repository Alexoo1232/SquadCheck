using System.Text.Json;
using SquadCheck.Models;

namespace SquadCheck.Data;

public class DataManager : IDataManager
{
    private readonly string _dataFolderPath;
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

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

        EnsureDataFolderExists();
    }

    public AppData LoadData()
    {
        try
        {
            if (!DataFileExists())
            {
                return new AppData();
            }

            string json = File.ReadAllText(_dataFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppData();
            }

            AppData? data = JsonSerializer.Deserialize<AppData>(json, _jsonOptions);

            return data ?? new AppData();
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

    public void SaveData(AppData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        try
        {
            EnsureDataFolderExists();

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

    private void EnsureDataFolderExists()
    {
        if (!Directory.Exists(_dataFolderPath))
        {
            Directory.CreateDirectory(_dataFolderPath);
        }
    }
}
