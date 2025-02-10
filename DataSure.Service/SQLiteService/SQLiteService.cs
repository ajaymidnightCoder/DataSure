using SQLite;

namespace DataSure.Service.SQLiteService;

public class EntityState
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string SelectedEntityFileName { get; set; }
}

public class SQLiteService
{
    private readonly SQLiteAsyncConnection db;

    public SQLiteService(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
        db.CreateTableAsync<EntityState>().Wait();
    }

    // Save or update entity state
    public async Task SaveEntityStateAsync(EntityState state)
    {
        await db.InsertOrReplaceAsync(state);
    }

    // Get entity state
    public async Task<EntityState> GetEntityStateAsync()
    {
        return await db.Table<EntityState>().FirstOrDefaultAsync();
    }
}