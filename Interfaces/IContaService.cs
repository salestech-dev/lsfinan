using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lsfinan.Models;
using lsfinan.ViewModel;

namespace lsfinan.Interfaces
{
    public interface IContaService
    {
        Task<List<Conta>> ListarContas(Usuario usuario);
        Task<Conta> CadastrarConta(ContaViewModel contaViewModel, Usuario usuario);
    }
}