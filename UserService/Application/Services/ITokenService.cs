namespace UserService.Application.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(Guid userId);
    }
}
