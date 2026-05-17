using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ProiectWeb.Services
{
    public class EmailTwoFactorTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : class
    {
        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            
            var code = new Random().Next(100000, 999999).ToString();
            return await Task.FromResult(code);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }
    }
}
