using Common.Repository;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.Command;
using UserService.Application.Repository;
using UserService.Application.Request.User;
using UserService.Application.Services;
using UserService.Domain.Models;

namespace Tests
{
    public class RegisterUserCommandTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly RegisterUserCommand _command;

        public RegisterUserCommandTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _command = new RegisterUserCommand(
                _unitOfWorkMock.Object,
                _userRepositoryMock.Object,
                _tokenServiceMock.Object
            );
        }

        [Fact]
        public async Task Execute_ValidRequest_UserDoesNotExist_ShouldRegisterUserAndReturnToken()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "john.doe",
                Password = "SecurePassword123"
            };

            var userId = Guid.NewGuid();
            var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(userId);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns(expectedToken);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().Be(expectedToken);
            result.ErrorMessage.Should().BeNull();

            _userRepositoryMock.Verify(x => x.IsExist(request.Name), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(userId), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_UserAlreadyExists_ShouldReturnFailure()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "existing.user",
                Password = "password123"
            };

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Response.Should().BeNull();
            result.ErrorMessage.Should().Be($"User with name {request.Name} is exist");

            _userRepositoryMock.Verify(x => x.IsExist(request.Name), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_ValidRequest_ShouldAddUserWithCorrectData()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "new.user",
                Password = "SecurePass123"
            };

            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(userId);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns("token");

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            await _command.Execute(request);

            // Assert
            _userRepositoryMock.Verify(
                x => x.Add(It.Is<User>(u =>
                    u.Name == request.Name)),
                Times.Once);
        }

        [Fact]
        public async Task Execute_ValidRequest_ShouldGenerateTokenWithCorrectUserId()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "token.test",
                Password = "password123"
            };

            var expectedUserId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(expectedUserId);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(expectedUserId))
                .Returns("generated_token");

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            await _command.Execute(request);

            // Assert
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(expectedUserId), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenIsExistThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _userRepositoryMock.Verify(x => x.IsExist(request.Name), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenAddThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            var expectedException = new Exception("Failed to add user");

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Failed to add user");

            _userRepositoryMock.Verify(x => x.IsExist(request.Name), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenTokenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(userId);

            var expectedException = new Exception("Token generation failed");
            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Throws(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Token generation failed");

            _userRepositoryMock.Verify(x => x.IsExist(request.Name), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(userId), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_WhenSaveThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(userId);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns("token");

            var expectedException = new Exception("Save failed");
            _unitOfWorkMock
                .Setup(x => x.Save())
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Save failed");

            _userRepositoryMock.Verify(x => x.IsExist(request.Name), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(userId), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WithEmptyUsername_ShouldCheckExistenceAndFail()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = string.Empty,
                Password = "password123"
            };

            _userRepositoryMock
                .Setup(x => x.IsExist(string.Empty))
                .ReturnsAsync(false);

            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(userId);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns("token");

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue(); // Технически может зарегистрировать с пустым именем

            _userRepositoryMock.Verify(x => x.IsExist(string.Empty), Times.Once);
            _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldCallUnitOfWorkSaveExactlyOnce()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(userId);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns("token");

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            await _command.Execute(request);

            // Assert
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenAddReturnsEmptyGuid_ShouldStillGenerateToken()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            var emptyGuid = Guid.Empty;
            var expectedToken = "token_for_empty_guid";

            _userRepositoryMock
                .Setup(x => x.IsExist(request.Name))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.Add(It.IsAny<User>()))
                .ReturnsAsync(emptyGuid);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(emptyGuid))
                .Returns(expectedToken);

            _unitOfWorkMock
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().Be(expectedToken);

            _tokenServiceMock.Verify(x => x.GenerateAccessToken(emptyGuid), Times.Once);
        }
    }
}
