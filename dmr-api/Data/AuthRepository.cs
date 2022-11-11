using System.Threading.Tasks;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DMR_API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
                return true;
            }
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        // public bool RevokeToken(string token, string ipAddress)
        // {
        //     var user = _context.Users.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

        //     // return false if no user found with token
        //     if (user == null) return false;

        //     var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        //     // return false if token is not active
        //     if (!refreshToken.IsActive) return false;

        //     // revoke token and save
        //     refreshToken.Revoked = DateTime.UtcNow;
        //     refreshToken.RevokedByIp = ipAddress;
        //     _context.Update(user);
        //     _context.SaveChanges();

        //     return true;
        // }
    }
}