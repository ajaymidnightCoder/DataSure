namespace DataSure.Contracts.AdminService
{
    public interface IEntitiyConfigService
    {
        Task<List<T>> GetListByFileName<T>(string fileName);
        Task SaveRawFile(string fileName, string content);
        bool CreatRawFile(string fileName);
    }
}
