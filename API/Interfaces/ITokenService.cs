using API.Entities;

namespace API.Interfaces
{
    public interface ITokenService
    {
        string CraeteToken(AppUser appUser);
    }
}
