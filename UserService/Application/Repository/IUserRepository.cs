using UserService.Domain.Models;

namespace UserService.Application.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByNameAndPassword(string name, string password);
        Task<bool> IsExist(string name);
        Task<Guid> Add(User user);
    }
}
