using System.Threading.Tasks;
using Mnemi.Domain.Entities;

namespace Mnemi.Application.Services;

public interface IUserService
{
    Task<User> GetCurrentUserAsync();
    Task UpdateUserAsync(User user);
}
