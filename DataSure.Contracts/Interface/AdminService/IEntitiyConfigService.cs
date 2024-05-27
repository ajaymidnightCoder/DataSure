using DataSure.Contracts.Models.AdminModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSure.Contracts.Interface.AdminService
{
    public interface IEntitiyConfigService
    {
        List<EntityConfigModel> GetAll();
        List<PropertyConfigModel> GetPropertyByEntitiy(int entityId);
    }
}
