using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculosTeste
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange
            var veiculo = new Veiculo();

            // Act
            veiculo.Id = 1;
            veiculo.Marca = "Fiat";
            veiculo.Nome = "Uno";
            veiculo.Ano = 2020;

            // Assert
            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual("Fiat", veiculo.Marca);
            Assert.AreEqual("Uno", veiculo.Nome);
            Assert.AreEqual(2020, veiculo.Ano);
        }
    }
}