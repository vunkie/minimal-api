using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();


builder.Services.AddScoped<iAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<iVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Insira o token JWT :"

    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new string[]{}
        }
    });
});

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
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores

string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim("Email", administrador.Email),
        new Claim(ClaimTypes.Role, administrador.Perfil),
        new Claim("Perfil", administrador.Perfil)
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServico administradorServico) =>
{
    var admin = administradorServico.Login(loginDTO);
    if (admin != null)
    {
        string token = GerarTokenJwt(admin);
        return Results.Ok(new AdmLogado
        {
            Token = token,
            Email = admin.Email,
            Perfil = admin.Perfil
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administradores");

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
}).RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute
{
    Roles = "Adm"
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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute
{
    Roles = "Adm"
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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute
{
    Roles = "Adm"
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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, [FromServices] iVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, [FromServices] iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm,Editor"})
.WithTags("Veiculos");

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
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, [FromServices] iVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    veiculoServico.Apagar(veiculo);
    return Results.NoContent();
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute{ Roles = "Adm"})
.WithTags("Veiculos");

#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion


