using API.Entities;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface ITokenService
    {
        Task<string> CraeteTokenAsync(AppUser appUser);
    }
}
