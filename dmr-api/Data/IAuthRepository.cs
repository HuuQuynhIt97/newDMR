using System.Threading.Tasks;
using DMR_API.Models;

namespace DMR_API.Data
{
    public interface IAuthRepository
    {
        Task<User> Login(string username, string password);
    }
}