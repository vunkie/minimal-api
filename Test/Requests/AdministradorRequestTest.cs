
using System.Text;
using System.Text.Json;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    public class AdministradorRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Setup.ClassCleanup();
        }

        [TestMethod]
        public async Task TestarGetSetPropriedades()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await Setup.client.PostAsync("/administrador/login", content);


            // Assert
            Assert.AreEqual(200, (int)response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdmLogado>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
            Assert.IsNotNull(admLogado?.Perfil ?? "");
            Assert.IsNotNull(admLogado?.Token ?? "");
            Assert.IsNotNull(admLogado?.Email ?? "");
        }
    }
}