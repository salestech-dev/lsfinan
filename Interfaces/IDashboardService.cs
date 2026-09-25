using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lsfinan.Interfaces
{
    public interface IDashboardService
    {
        public Task<DashboardViewModel> ObterDashboardAsync(int usuarioId);
    }
}