using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CrudSupermercado.Models;
using CrudSupermercado.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Rendering;

namespace CrudSupermercado.Controllers;
public class UsuariosController : BaseController
{
    private readonly ILogger<UsuariosController> _logger;
    private readonly IWebHostEnvironment _appEnvironment;

    public UsuariosController(ILogger<UsuariosController> logger, IWebHostEnvironment appEnvironment)
    {
        _logger = logger;
        _appEnvironment = appEnvironment;
    }

    public IActionResult Usuario(string ErroSenha = null)
    {
        ViewBag.ErroSenha = ErroSenha;
        return View();
    }
    public IActionResult Listar(string MensagemErro = null)
    {
        if (ValidaLogin())
            return RedirectToAction("Login", "Home");

        Repositorio<Usuario> rep = new Repositorio<Usuario>();
        List<UsuarioViewModel> lista = rep.Listar()
        .Select(p => new UsuarioViewModel(p)).ToList(); 
        ViewBag.MensagemErro = MensagemErro;

        return View(lista);
    }

    [HttpGet]
    public IActionResult Usuario(int id = 0)
    {
        try
        {
            if (ValidaLogin())
                return RedirectToAction("Login", "Home");
            
            Repositorio<Usuario> rep = new Repositorio<Usuario>();
            Usuario model = rep.Buscar(id);
            List<Usuario> usuarios = rep.Listar();

            if (!ModelState.IsValid)
            {
                UsuarioViewModel viewModel = new UsuarioViewModel(model);
                return View(viewModel);        
            }
            else
            {
                if (id > 0)
                {
                    model.Senha = "";
                    UsuarioViewModel viewModel = new UsuarioViewModel(model);
                    return View(viewModel);
                }
                else
                    return View(model);    
            }
        }
        finally
        {

        }
    }

    [HttpPost]
    public IActionResult Usuario(UsuarioViewModel model, IFormFile anexo)
    {
        if (anexo is not null)
        {
            string caminho = _appEnvironment.WebRootPath + "\\imagens\\"+anexo.FileName;
            using (FileStream stream = new FileStream(caminho, FileMode.Create))
            {
                anexo.CopyTo(stream);
            }
        }

        Repositorio<Usuario> rep = new Repositorio<Usuario>();
        
        if (model.Id == 0)
        {
            rep.Adicionar(model.Parse());
        }
        else
        {
            Hash hash = new Hash();
            if (hash.validarSenha(model.Senha, hash.CriptografarSenha(model.Senha)))
            {
                model.Senha = hash.CriptografarSenha(model.Senha);
                rep.Atualizar(model.Parse());
            }
            else
            {
                return RedirectToAction("Listar", "Usuarios");
            }
        }

        return RedirectToAction("Listar", "Usuarios");
    }
}