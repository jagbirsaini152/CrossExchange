using System;
using System.Threading.Tasks;
using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Moq;

namespace CrossExchange.Tests
{
    public class PortfolioConrollerTests
    {
        private readonly Mock<IPortfolioRepository> _portfolioRepositoryMock = new Mock<IPortfolioRepository>();
        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();
        private readonly Mock<ITradeRepository> _tradeRepositoryMock = new Mock<ITradeRepository>();

        private readonly PortfolioController _portfolioController;

        public PortfolioConrollerTests()
        {
            _portfolioController = new PortfolioController(_shareRepositoryMock.Object,_tradeRepositoryMock.Object, _portfolioRepositoryMock.Object);
        }

        [Test]
        public async Task Post_ShouldInsertPortfolio()
        {
            var obj = new Portfolio
            {
                Name="Jagbir"
            };

            // Arrange

            // Act
            var result = await _portfolioController.Post(obj);

            // Assert
            Assert.NotNull(result);

             var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }
        [Test]
        public async Task Get_ShouldGetPortfolio()
        {
            var result = await _portfolioController.GetPortfolioInfo(1);
            Assert.NotNull(result);
        }
    }
}
