using SquadCheck.Models;

namespace SquadCheck.Data;

public interface IDataManager
{
    AppData LoadData();

    void SaveData(AppData data);

    bool DataFileExists();
}
