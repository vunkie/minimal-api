
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
    {

        private DbContexto CriarContextoDeTest()
        {

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            return new DbContexto(config);
        }


        [TestMethod]
        public void TestandoSalvarAdministrador()
        {

            // Arrange

            var context = CriarContextoDeTest();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");


            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";
            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);

            // Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count);
        }

        [TestMethod]
        public void TestandoBuscaPorId()
        {

            // Arrange

            var context = CriarContextoDeTest();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");


            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";
            var administradorServico = new AdministradorServico(context);

            // Act
            administradorServico.Incluir(adm);
            var admDb = administradorServico.BuscarPorId(adm.Id);


            // Assert
            Assert.AreEqual(1, admDb.Id);
        }

    }
}