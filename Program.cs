using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enums;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;


#region Builder
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

#endregion

var app = builder.Build();


#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administraores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login efetuado com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, iAdministradorServico administradorServico) =>
{

    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };


    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("Email é obrigatório");
    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("Senha é obrigatório");
    if (administradorDTO.Perfil == null)
        validacao.Mensagens.Add("Perfil é obrigatório");

    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador
    {
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(),
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, [FromServices] iAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, [FromServices] iAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).WithTags("Administradores");

#endregion

#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };
    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("Nome é obrigatório");
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("Marca é obrigatório");
    if (veiculoDTO.Ano < 1900 || veiculoDTO.Ano > DateTime.Now.Year + 1)
        validacao.Mensagens.Add("Ano inválido");
    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, iVeiculoServico veiculoServico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }


    var veiculo = new Veiculo
    {
        Marca = veiculoDTO.Marca,
        Nome = veiculoDTO.Nome,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, [FromServices] iVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, [FromServices] iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, [FromServices] iVeiculoServico veiculoServico) =>
{



    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, [FromServices] iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    veiculoServico.Apagar(veiculo);
    return Results.NoContent();
}).WithTags("Veiculos");

#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion


