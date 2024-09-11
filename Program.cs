using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using minimal_api.Infraestrutura.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});


var app = builder.Build();

app.MapGet("/", () => "Hello World!!");

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) => {
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login efetuado com sucesso");
    } else
    {
        return Results.Unauthorized();
    }
});

app.Run();



