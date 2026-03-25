using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MinhaApi.Services;

public class TokenService
{
    private readonly IConfiguration _confituration;
    public TokenService(IConfiguration configuration) => _confituration = configuration;

    public string GerarToken(string username)
    {
        var key = Encoding.ASCII.GetBytes(_confituration["JwtSettings:Key"] 
                                     ?? throw new InvalidOperationException("A chave JWT não foi configurada nos User Secrets!"));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
          Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
          Expires = DateTime.UtcNow.AddHours(2),
          SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
          Issuer = _confituration["JwtSettings:Issuer"],
          Audience = _confituration["JwtSettings:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}