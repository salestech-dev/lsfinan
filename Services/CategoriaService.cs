using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lsfinan.Data;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly AppDbContext _context;

        public CategoriaService(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        
        public async Task<Categoria> CadastrarCategoria(CategoriaViewModel categoria, Usuario usuario)
        {
           var usuarioExistente = await _context.Usuarios.FindAsync(usuario.Id);
           if(usuarioExistente == null)
                throw new Exception("Usuario não encontrado");

           var novaCategoria = new Categoria
           {
               Ativa = categoria.Ativa,
               NomeCategoria = categoria.NomeDaCategoria,
               UsuarioId = usuario.Id
           };

           _context.Add(novaCategoria);
           await _context.SaveChangesAsync();

           return novaCategoria;
        }
       
        public Task<List<Categoria>> ListarCategorias(Usuario usuario)
        {
           var categorias = _context.Categorias
           .Where(x => x.UsuarioId == usuario.Id)
           .ToListAsync();

           return categorias;
        }
    }
}