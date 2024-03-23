using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplyManagement.Utilities
{
    public interface IRoleSupplyManagement
    {
        Task AddRoleAsync();
        Task CreateRoleAsync();
    }
}
