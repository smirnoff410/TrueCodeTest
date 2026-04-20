using CurrencyService.Application.Query;
using CurrencyService.Application.Repository;
using CurrencyService.Application.Request;
using CurrencyService.Domain.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class GetUserCurrencyQueryTests
    {
        private readonly Mock<IUserCurrencyRepository> _userCurrencyRepositoryMock;
        private readonly GetUserCurrencyQuery _query;

        public GetUserCurrencyQueryTests()
        {
            _userCurrencyRepositoryMock = new Mock<IUserCurrencyRepository>();
            _query = new GetUserCurrencyQuery(_userCurrencyRepositoryMock.Object);
        }

        [Fact]
        public async Task Execute_UserHasCurrencies_ShouldReturnCurrenciesList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = userId };

            var expectedCurrencies = new List<Currency>
        {
            new Currency { Id = "id1", Name = "USD", Rate = 1.05m },
            new Currency { Id = "id2", Name = "EUR", Rate = 1.10m },
            new Currency { Id = "id3", Name = "GBP", Rate = 1.25m }
        };

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(expectedCurrencies);

            // Act
            var result = await _query.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response.Should().BeEquivalentTo(expectedCurrencies);
            result.Response.Count.Should().Be(3);

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Execute_UserHasNoCurrencies_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = userId };

            var emptyList = new List<Currency>();

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _query.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response.Should().BeEmpty();
            result.Response.Count.Should().Be(0);

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenRepositoryReturnsNull_ShouldReturnNullResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = userId };

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ReturnsAsync((List<Currency>)null);

            // Act
            var result = await _query.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().BeNull();

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Execute_WithValidUserId_ShouldCallRepositoryWithCorrectUserId()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = expectedUserId };

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Currency>());

            // Act
            await _query.Execute(request);

            // Assert
            _userCurrencyRepositoryMock.Verify(
                x => x.GetByUserId(It.Is<Guid>(id => id == expectedUserId)),
                Times.Once);
        }

        [Fact]
        public async Task Execute_WithDifferentUserIds_ShouldPassCorrectIdToRepository()
        {
            // Arrange
            var testCases = new[]
            {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

            foreach (var userId in testCases)
            {
                var request = new GetUserCurrencyRequest { UserId = userId };

                _userCurrencyRepositoryMock
                    .Setup(x => x.GetByUserId(userId))
                    .ReturnsAsync(new List<Currency>());

                // Act
                await _query.Execute(request);

                // Assert
                _userCurrencyRepositoryMock.Verify(
                    x => x.GetByUserId(userId),
                    Times.Once);
            }
        }

        [Fact]
        public async Task Execute_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = userId };

            var expectedException = new InvalidOperationException("Database connection failed");

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _query.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenRepositoryThrowsArgumentException_ShouldPropagateException()
        {
            // Arrange
            var userId = Guid.Empty; // Пустой Guid может вызвать ошибку
            var request = new GetUserCurrencyRequest { UserId = userId };

            var expectedException = new ArgumentException("UserId cannot be empty");

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _query.Execute(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("UserId cannot be empty");

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Execute_WithDefaultUserId_ShouldPassToRepository()
        {
            // Arrange
            var userId = Guid.Empty;
            var request = new GetUserCurrencyRequest { UserId = userId };

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(new List<Currency>());

            // Act
            var result = await _query.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldReturnCommandResultWithSuccessTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = userId };

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(new List<Currency>());

            // Act
            var result = await _query.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNull(); // Предполагая, что ErrorMessage может быть null
        }

        [Fact]
        public async Task Execute_WithManyCurrencies_ShouldReturnAllCurrencies()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetUserCurrencyRequest { UserId = userId };

            var manyCurrencies = new List<Currency>();
            for (int i = 0; i < 100; i++)
            {
                manyCurrencies.Add(new Currency
                {
                    Id = "id",
                    Name = $"Currency{i}",
                    Rate = 1.00m + i
                });
            }

            _userCurrencyRepositoryMock
                .Setup(x => x.GetByUserId(userId))
                .ReturnsAsync(manyCurrencies);

            // Act
            var result = await _query.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().NotBeNull();
            result.Response.Count.Should().Be(100);
            result.Response.Should().BeEquivalentTo(manyCurrencies);

            _userCurrencyRepositoryMock.Verify(x => x.GetByUserId(userId), Times.Once);
        }
    }
}
