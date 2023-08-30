using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private string secretKey;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }
        public bool IsUniqueUser(string username)
        {
            LocalUser user = _db.LocalUsers.FirstOrDefault(user => user.UserName == username);
            if (user == null) { return true; }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            LocalUser user = await _db.LocalUsers.FirstOrDefaultAsync(user =>
                user.UserName == loginRequestDTO.UserName && 
                user.Password == loginRequestDTO.Password
            );
            if (user == null) 
            {
                return new LoginResponseDTO()
                {
                    User = null,
                    Token = ""
                };
            }
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO responseDTO = new LoginResponseDTO() 
            {
                User = user,
                Token = tokenHandler.WriteToken(token)
            };
            return responseDTO;
        }

        public async Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            LocalUser user = new LocalUser()
            {
                Name = registrationRequestDTO.Name,
                UserName = registrationRequestDTO.UserName,
                Password = registrationRequestDTO.Password,
                Role = registrationRequestDTO.Role,
            };
            await _db.LocalUsers.AddAsync(user);
            await _db.SaveChangesAsync();
            user.Password = "";
            return user;
        }
    }
}
