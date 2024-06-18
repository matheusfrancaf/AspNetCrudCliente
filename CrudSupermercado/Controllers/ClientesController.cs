using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CrudSupermercado.Models;
using CrudSupermercado.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text.Json;

namespace CrudSupermercado.Controllers;
public class ClientesController : BaseController
{
    private readonly ILogger<ClientesController> _logger;
    private readonly IWebHostEnvironment _appEnvironment;

    public ClientesController(ILogger<ClientesController> logger, IWebHostEnvironment appEnvironment)
    {
        _logger = logger;
        _appEnvironment = appEnvironment;
    }

    public FileContentResult Exportar(int id)
    {
        Repositorio<Cliente> rep = new Repositorio<Cliente>();
        Cliente model = rep.Buscar(id);
        String json = JsonSerializer.Serialize(model);
        return File(new System.Text.UTF8Encoding().GetBytes(json), "text/json", "DadosCliente" + id.ToString() + ".json");
    }

    public FileContentResult ExportarTodos()
    {
        Repositorio<Cliente> rep = new Repositorio<Cliente>();
        List<Cliente> lista = rep.Listar();
        String json = JsonSerializer.Serialize(lista);
        return File(new System.Text.UTF8Encoding().GetBytes(json), "text/json", "DadosClientes.json");
    }

    public IActionResult Cliente()
    {
        return View();
    }
    public IActionResult Listar(string MensagemErro = null)
    {
        if (ValidaLogin())
            return RedirectToAction("Login", "Home");

        Repositorio<Cliente> rep = new Repositorio<Cliente>();
        List<ClienteViewModel> lista = rep.Listar()
        .Select(p => new ClienteViewModel(p)).ToList();
        ViewBag.MensagemErro = MensagemErro;

        return View(lista);
    }

    private List<Estado> ObterEstadosECidades()
    {
        string caminho = Path.Combine(_appEnvironment.ContentRootPath, "EstadosCidades.json");
        var rep = new RepositorioEstadosCidades(caminho);
        var estadosCidades = rep.Listar();
        return estadosCidades.Estados ?? new List<Estado>();
    }

    [HttpGet]
    public IActionResult Cliente(int id = 0)
    {

        if (ValidaLogin())
            return RedirectToAction("Login", "Home");

        Repositorio<Cliente> rep = new Repositorio<Cliente>();
        Cliente model = rep.Buscar(id);
        List<Cliente> clientes = rep.Listar();

        var estados = ObterEstadosECidades();

        ViewBag.Estados = estados.Select(e => new SelectListItem
        {
            Value = e.Sigla,
            Text = e.Nome
        }).ToList();

        ViewBag.CidadesPorEstado = estados.ToDictionary(
            e => e.Sigla,
            e => e.Cidades
        );

        if (!ModelState.IsValid)
        {
            ClienteViewModel viewModel = new ClienteViewModel(model);
            return View(viewModel);
        }
        else
        {
            if (id > 0)
            {
                ClienteViewModel viewModel = new ClienteViewModel(model);
                return View(viewModel);
            }
            else
                return View(model);
        }

    }

    [HttpGet]
    public IActionResult ObterCidades(string estado)
    {
        string caminhoArquivo = Path.Combine(_appEnvironment.ContentRootPath, "EstadosCidades.json");
        var rep = new RepositorioEstadosCidades(caminhoArquivo);
        var estadosCidades = rep.Listar();

        var estadoSelecionado = estadosCidades.Estados.FirstOrDefault(e => e.Sigla == estado);

        if (estadoSelecionado != null)
        {
            return Json(estadoSelecionado.Cidades);
        }

        return Json(new List<string>());
    }

    [HttpGet]
    public IActionResult Excluir(int id)
    {
        Repositorio<Cliente> rep = new Repositorio<Cliente>();
        var cliente = rep.Buscar(id);
        rep.Remover(id);

        return RedirectToAction("Listar", "Clientes");
    }

    [HttpPost]
    public IActionResult Cliente(ClienteViewModel model, IFormFile anexo)
    {
        ModelState.Remove("Id");
        ModelState.Remove("Imagem");
        ModelState.Remove("anexo");
        if (!ModelState.IsValid)
        {
            var estados = ObterEstadosECidades();
            ViewBag.Estados = estados.Select(e => new SelectListItem
            {
                Value = e.Sigla,
                Text = e.Nome
            }).ToList();

            ViewBag.CidadesPorEstado = estados.ToDictionary(
                e => e.Sigla,
                e => e.Cidades
            );

            return View(model);
        }

        if (anexo != null)
        {
            string caminho = _appEnvironment.WebRootPath + "\\imagens\\" + anexo.FileName;
            using (FileStream stream = new FileStream(caminho, FileMode.Create))
            {
                anexo.CopyTo(stream);
            }
            model.Imagem = caminho;
        }

        model.Estado = Request.Form["Estado"];
        model.Cidade = Request.Form["Cidade"];
        Repositorio<Cliente> rep = new Repositorio<Cliente>();

        bool cpfExistente = rep.Listar().Any(c => c.Cpf == model.Cpf && c.Id != model.Id);
        bool inscricaoExistente = rep.Listar().Any(c => c.InscricaoEstadual == model.InscricaoEstadual && c.Id != model.Id);

        if (cpfExistente)
        {
            ModelState.AddModelError("Cpf", "CPF já existente.");
        }

        if (inscricaoExistente)
        {
            ModelState.AddModelError("InscricaoEstadual", "Inscrição Estadual já existente.");
        }

        if (cpfExistente || inscricaoExistente)
        {
            var estados = ObterEstadosECidades();
            ViewBag.Estados = estados.Select(e => new SelectListItem
            {
                Value = e.Sigla,
                Text = e.Nome
            }).ToList();

            ViewBag.CidadesPorEstado = estados.ToDictionary(
                e => e.Sigla,
                e => e.Cidades
            );
            return View(model);
        }


        if (model.Id == 0)
        {
            rep.Adicionar(model.Parse());
        }
        else
        {
            rep.Atualizar(model.Parse());
        }

        return RedirectToAction("Listar", "Clientes");
    }
}