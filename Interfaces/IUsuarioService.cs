using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace lsfinan.Interfaces
{
    public interface IUsuarioService
    {
        Task<IdentityResult> CadastrarUsuario(UsuarioCadastroViewModel usuario);
        
    }
}