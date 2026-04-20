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
    public class LoginCommandTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly LoginCommand _command;

        public LoginCommandTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _command = new LoginCommand(
                _userRepositoryMock.Object,
                _tokenServiceMock.Object
            );
        }

        [Fact]
        public async Task Execute_ValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "john.doe",
                Password = "SecurePassword123"
            };

            var userId = Guid.NewGuid();
            var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

            var user = new User
            {
                Id = userId,
                Name = request.Name,
                Password = "hashed_password"
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns(expectedToken);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().Be(expectedToken);
            result.ErrorMessage.Should().BeNull();

            _userRepositoryMock.Verify(
                x => x.GetByNameAndPassword(request.Name, request.Password),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(userId),
                Times.Once);
        }

        [Fact]
        public async Task Execute_InvalidCredentials_UserNotFound_ShouldReturnFailure()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "nonexistent.user",
                Password = "WrongPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Response.Should().BeNull();
            result.ErrorMessage.Should().Be($"User not found with name {request.Name}");

            _userRepositoryMock.Verify(
                x => x.GetByNameAndPassword(request.Name, request.Password),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Execute_WrongPassword_ShouldReturnFailure()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "john.doe",
                Password = "WrongPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("User not found");
            result.ErrorMessage.Should().Contain(request.Name);

            _userRepositoryMock.Verify(
                x => x.GetByNameAndPassword(request.Name, request.Password),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Execute_WithNullUserInRepository_ShouldNotGenerateToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "test.user",
                Password = "password123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Response.Should().BeNull();

            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Execute_ValidCredentials_ShouldGenerateTokenWithCorrectUserId()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();
            var request = new LoginRequest
            {
                Name = "john.doe",
                Password = "SecurePassword123"
            };

            var user = new User
            {
                Id = expectedUserId,
                Name = request.Name
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>()))
                .Returns("generated_token");

            // Act
            await _command.Execute(request);

            // Assert
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(expectedUserId),
                Times.Once);
        }

        [Fact]
        public async Task Execute_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "john.doe",
                Password = "password123"
            };

            var expectedException = new InvalidOperationException("Database connection failed");

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _userRepositoryMock.Verify(
                x => x.GetByNameAndPassword(request.Name, request.Password),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Execute_WhenTokenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "john.doe",
                Password = "SecurePassword123"
            };

            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = request.Name };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync(user);

            var expectedException = new Exception("Token generation failed");
            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Throws(expectedException);

            // Act
            Func<Task> act = async () => await _command.Execute(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Token generation failed");

            _userRepositoryMock.Verify(
                x => x.GetByNameAndPassword(request.Name, request.Password),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(userId),
                Times.Once);
        }

        [Fact]
        public async Task Execute_WithEmptyUsername_ShouldReturnFailure()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = string.Empty,
                Password = "password123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be($"User not found with name {request.Name}");

            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Execute_WithEmptyPassword_ShouldReturnFailure()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "john.doe",
                Password = string.Empty
            };

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain(request.Name);

            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task Execute_ValidCredentials_ShouldReturnNonNullToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "admin",
                Password = "Admin123!"
            };

            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = request.Name };
            var expectedToken = "valid_jwt_token_here";

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns(expectedToken);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().NotBeNullOrWhiteSpace();
            result.Response.Should().Be(expectedToken);
        }

        [Fact]
        public async Task Execute_ValidCredentials_ShouldReturnSuccessWithToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "testuser",
                Password = "TestPass123"
            };

            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = request.Name };
            var token = "jwt_token_string";

            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(userId))
                .Returns(token);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Response.Should().Be(token);
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task Execute_CaseSensitiveUsername_ShouldHandleCorrectly()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "JohnDoe",
                Password = "Password123"
            };

            // Имитируем, что репозиторий чувствителен к регистру
            _userRepositoryMock
                .Setup(x => x.GetByNameAndPassword(request.Name, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _command.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be($"User not found with name {request.Name}");

            _userRepositoryMock.Verify(
                x => x.GetByNameAndPassword(request.Name, request.Password),
                Times.Once);
            _tokenServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<Guid>()),
                Times.Never);
        }
    }
}
