using DataSure.Contracts.Interface.AdminService;
using DataSure.Contracts.Models.AdminModel;

namespace DataSure.Service.AdminService
{
    public class EntitiyConfigService : IEntitiyConfigService
    {
        public List<EntityConfigModel> GetAll()
        {
            var list = new List<EntityConfigModel>();
            return list;
        }

        public List<PropertyConfigModel> GetPropertyByEntitiy(int entityId)
        {
            var list = new List<PropertyConfigModel>();
            return list;
        }
    }
}
