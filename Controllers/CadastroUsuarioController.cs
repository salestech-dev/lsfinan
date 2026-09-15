using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace lsfinan.Controllers;

[Route("[controller]")]
public class CadastroUsuarioController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public CadastroUsuarioController( IUsuarioService usuarioService)
    {
        
        _usuarioService = usuarioService;
    }

    [HttpGet("Cadastro")]
    public IActionResult Cadastro()
    {
        return View();
    }

    [HttpPost("Cadastro")]
    public async Task<IActionResult> Cadastro([FromForm] UsuarioCadastroViewModel usuario)
    {
        if(!ModelState.IsValid)
        {
            return View(usuario);
        }

        var resultado = await _usuarioService.CadastrarUsuario(usuario);
        if (resultado.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        else
        {
            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, erro.Description);
            }
            return View(usuario);
        }

    }
}

