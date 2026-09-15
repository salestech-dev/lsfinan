using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using lsfinan.Models;

namespace lsfinan.Data
{
    public class AppDbContext 
        : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Saida> Saidas { get; set; }

        public DbSet<Entrada> Entradas { get; set; }

        public DbSet<Conta> Contas { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}