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
    public class AddUserCurrencyCommandTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserCurrencyRepository> _userCurrencyRepositoryMock;
        private readonly AddUserCurrencyCommand _command;

        public AddUserCurrencyCommandTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userCurrencyRepositoryMock = new Mock<IUserCurrencyRepository>();
            _command = new AddUserCurrencyCommand(
                _unitOfWorkMock.Object,
                _userCurrencyRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Execute_ValidRequest_ShouldAddUserCurrencyAndReturnSuccess()
        {
            // Arrange
            var request = new AddUserCurrencyRequest
            {
                UserId = Guid.NewGuid(),
                CurrencyId = "USD"
            };

            var expectedUserCurrencyId = Guid.NewGuid();

            _userCurrencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<UserCurrency>()))
                .ReturnsAsync(expectedUserCurrencyId);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().Be(expectedUserCurrencyId);

            _userCurrencyRepositoryMock.Verify(
                x => x.Add(It.Is<UserCurrency>(uc =>
                    uc.UserId == request.UserId &&
                    uc.CurrencyId == request.CurrencyId)),
                Times.Once
            );

            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new AddUserCurrencyRequest
            {
                UserId = Guid.NewGuid(),
                CurrencyId = "USD"
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _userCurrencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<UserCurrency>()))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _userCurrencyRepositoryMock.Verify(x => x.Add(It.IsAny<UserCurrency>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenSaveFails_ShouldPropagateException()
        {
            // Arrange
            var request = new AddUserCurrencyRequest
            {
                UserId = Guid.NewGuid(),
                CurrencyId = "EUR"
            };

            var expectedUserCurrencyId = Guid.NewGuid();

            _userCurrencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<UserCurrency>()))
                .ReturnsAsync(expectedUserCurrencyId);

            var expectedException = new Exception("Save failed");
            _unitOfWorkMock
                .Setup(x => x.Save())
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Save failed");

            _userCurrencyRepositoryMock.Verify(x => x.Add(It.IsAny<UserCurrency>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenAddReturnsEmptyGuid_ShouldReturnEmptyGuidInResponse()
        {
            // Arrange
            var request = new AddUserCurrencyRequest
            {
                UserId = Guid.NewGuid(),
                CurrencyId = "GBP"
            };

            var emptyGuid = Guid.Empty;

            _userCurrencyRepositoryMock
                .Setup(x => x.Add(It.IsAny<UserCurrency>()))
                .ReturnsAsync(emptyGuid);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().Be(emptyGuid);

            _userCurrencyRepositoryMock.Verify(x => x.Add(It.IsAny<UserCurrency>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }
    }
}
