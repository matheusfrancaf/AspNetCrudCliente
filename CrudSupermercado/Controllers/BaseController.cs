using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CrudSupermercado.Models;
using CrudSupermercado.Models.ViewModel;

namespace CrudSupermercado.Controllers;

public class BaseController : Controller
{
    public bool ValidaLogin()
    {
        var login = HttpContext.Session.GetString("UsuarioLogado");

        if (login == null)
            return true;
        return false;
    }
}
