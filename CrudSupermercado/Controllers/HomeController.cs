using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CrudSupermercado.Models;
using CrudSupermercado.Models.ViewModel;

namespace CrudSupermercado.Controllers;

public class HomeController : BaseController
{
    public IActionResult Index()
    {
        if (ValidaLogin())
        {
            return RedirectToAction("Login");
        }
        else
            return View();
    }

    public IActionResult Privacy()
    {
        if (ValidaLogin())
        {
            return RedirectToAction("Login");
        }
        else
            return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Login(string MensagemErro = null)
    {
        ViewBag.MensagemErro = MensagemErro;
        return View();
    }

    [HttpPost]
    public IActionResult Login(UsuarioViewModel model)
    {
        Repositorio<Usuario> rep = new Repositorio<Usuario>();
        List<Usuario> usuarios = rep.Listar();

        if (model.Login is not null && model.Senha is not null)
        {
            Hash hash = new Hash();
            model.Senha = hash.CriptografarSenha(model.Senha);

            if (usuarios.Any(p => p.Login.Equals(model.Login) && p.Senha.Equals(model.Senha)))
            {
                HttpContext.Session.SetString("UsuarioLogado", model.Login);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "Home", new { MensagemErro = "Verifique o Login e a Senha!" });
            }
        }
        else
        {
            return RedirectToAction("Login", "Home", new { MensagemErro = "Verifique o Login e a Senha!" });
        }

    }
}
