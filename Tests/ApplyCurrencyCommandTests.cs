using Common.Repository;
using CurrencyService.Application.Command;
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
    public class ApplyCurrencyCommandTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
        private readonly ApplyCurrencyCommand _command;

        public ApplyCurrencyCommandTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currencyRepositoryMock = new Mock<ICurrencyRepository>();
            _command = new ApplyCurrencyCommand(
                _unitOfWorkMock.Object,
                _currencyRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Execute_WithMixedCurrencies_ShouldUpdateExistingAndCreateNew()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = "11111111-1111-1111-1111-111111111111", Name = "USD Updated", Rate = 1.05m },
            new ApplyCurrencyRequest { Id = "22222222-2222-2222-2222-222222222222", Name = "EUR Updated", Rate = 1.10m },
            new ApplyCurrencyRequest { Id = "33333333-3333-3333-3333-333333333333", Name = "GBP New", Rate = 1.25m }
        };

            var existingCurrencies = new List<Currency>
        {
            new Currency { Id = "11111111-1111-1111-1111-111111111111", Name = "USD Old", Rate = 1.00m },
            new Currency { Id = "22222222-2222-2222-2222-222222222222", Name = "EUR Old", Rate = 1.08m }
        };

            var requestIds = request.Select(x => x.Id).ToList();

            _currencyRepositoryMock
                .Setup(x => x.Get(requestIds))
                .ReturnsAsync(existingCurrencies);

            _currencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<Currency>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            // Проверяем обновление существующих валют
            var updatedUsd = existingCurrencies.First(c => c.Id == "11111111-1111-1111-1111-111111111111");
            updatedUsd.Name.Should().Be("USD Updated");
            updatedUsd.Rate.Should().Be(1.05m);

            var updatedEur = existingCurrencies.First(c => c.Id == "22222222-2222-2222-2222-222222222222");
            updatedEur.Name.Should().Be("EUR Updated");
            updatedEur.Rate.Should().Be(1.10m);

            // Проверяем создание новой валюты
            _currencyRepositoryMock.Verify(
                x => x.Add(It.Is<Currency>(c =>
                    c.Id == "33333333-3333-3333-3333-333333333333" &&
                    c.Name == "GBP New" &&
                    c.Rate == 1.25m)),
                Times.Once
            );

            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WithAllExistingCurrencies_ShouldOnlyUpdateNoCreate()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = "11111111-1111-1111-1111-111111111111", Name = "USD Updated", Rate = 1.05m },
            new ApplyCurrencyRequest { Id = "22222222-2222-2222-2222-222222222222", Name = "EUR Updated", Rate = 1.10m }
        };

            var existingCurrencies = new List<Currency>
        {
            new Currency { Id = "11111111-1111-1111-1111-111111111111", Name = "USD Old", Rate = 1.00m },
            new Currency { Id = "22222222-2222-2222-2222-222222222222", Name = "EUR Old", Rate = 1.08m }
        };

            var requestIds = request.Select(x => x.Id).ToList();

            _currencyRepositoryMock
                .Setup(x => x.Get(requestIds))
                .ReturnsAsync(existingCurrencies);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            // Проверяем, что метод Add не вызывался
            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Never);

            // Проверяем обновление
            var updatedUsd = existingCurrencies.First(c => c.Id == "11111111-1111-1111-1111-111111111111");
            updatedUsd.Name.Should().Be("USD Updated");
            updatedUsd.Rate.Should().Be(1.05m);

            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WithAllNewCurrencies_ShouldOnlyCreateNoUpdate()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = "11111111-1111-1111-1111-111111111111", Name = "USD New", Rate = 1.05m },
            new ApplyCurrencyRequest { Id = "22222222-2222-2222-2222-222222222222", Name = "EUR New", Rate = 1.10m }
        };

            // Репозиторий возвращает пустой список (нет существующих валют)
            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Currency>());

            _currencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<Currency>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            // Проверяем, что метод Add вызван для каждой валюты
            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Exactly(2));

            // Проверяем, что Update не было (нет существующих для обновления)
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WithEmptyRequest_ShouldNotUpdateOrCreateAnything()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>();

            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Currency>());

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            // Проверяем, что Get был вызван с пустой коллекцией
            _currencyRepositoryMock.Verify(x => x.Get(It.Is<IEnumerable<string>>(ids => !ids.Any())), Times.Once);

            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenGetThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = "id", Name = "USD", Rate = 1.05m }
        };

            var expectedException = new InvalidOperationException("Database connection failed");

            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _currencyRepositoryMock.Verify(x => x.Get(It.IsAny<IEnumerable<string>>()), Times.Once);
            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenAddThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = "id", Name = "USD New", Rate = 1.05m }
        };

            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Currency>());

            var expectedException = new Exception("Failed to add currency");
            _currencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<Currency>()))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Failed to add currency");

            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenSaveThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = "id", Name = "USD", Rate = 1.05m }
        };

            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Currency>());

            _currencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<Currency>()))
                .Returns(Task.CompletedTask);

            var expectedException = new Exception("Save failed");
            _unitOfWorkMock
                .Setup(x => x.Save())
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Save failed");

            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WithDuplicateIdsInRequest_ShouldHandleCorrectly()
        {
            // Arrange
            var duplicateId = "11111111-1111-1111-1111-111111111111";
            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = duplicateId, Name = "First", Rate = 1.05m },
            new ApplyCurrencyRequest { Id = duplicateId, Name = "Second", Rate = 1.10m }
        };

            var existingCurrencies = new List<Currency>
        {
            new Currency { Id = duplicateId, Name = "Old", Rate = 1.00m }
        };

            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(existingCurrencies);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            // Должен использоваться первый элемент (First())
            var updatedCurrency = existingCurrencies.First();
            updatedCurrency.Name.Should().Be("First");
            updatedCurrency.Rate.Should().Be(1.05m);

            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Act
            Func<Task> act = async () => await _command.Execute(null);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Execute_PartialUpdate_ShouldCorrectlyIdentifyMissingCurrencies()
        {
            // Arrange
            var existingId1 = "11111111-1111-1111-1111-111111111111";
            var existingId2 = "22222222-2222-2222-2222-222222222222";
            var newId = "33333333-3333-3333-3333-333333333333";

            var request = new List<ApplyCurrencyRequest>
        {
            new ApplyCurrencyRequest { Id = existingId1, Name = "USD Updated", Rate = 1.05m },
            new ApplyCurrencyRequest { Id = existingId2, Name = "EUR Updated", Rate = 1.10m },
            new ApplyCurrencyRequest { Id = newId, Name = "GBP New", Rate = 1.25m },
            new ApplyCurrencyRequest { Id = "44444444-4444-4444-4444-444444444444", Name = "JPY New", Rate = 0.009m }
        };

            var existingCurrencies = new List<Currency>
        {
            new Currency { Id = existingId1, Name = "USD Old", Rate = 1.00m },
            new Currency { Id = existingId2, Name = "EUR Old", Rate = 1.08m }
        };

            _currencyRepositoryMock
                .Setup(x => x.Get(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(existingCurrencies);

            _currencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<Currency>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            // Проверяем обновление 2 существующих
            _currencyRepositoryMock.Verify(x => x.Add(It.IsAny<Currency>()), Times.Exactly(2));

            // Проверяем, что новые валюты были созданы
            _currencyRepositoryMock.Verify(
                x => x.Add(It.Is<Currency>(c => c.Id == newId && c.Name == "GBP New")),
                Times.Once
            );

            _currencyRepositoryMock.Verify(
                x => x.Add(It.Is<Currency>(c => c.Id == "44444444-4444-4444-4444-444444444444" && c.Name == "JPY New")),
                Times.Once
            );

            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }
    }
}
