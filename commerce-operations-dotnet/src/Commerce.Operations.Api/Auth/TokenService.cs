using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Commerce.Operations.Api.Auth;

public sealed class TokenService(JwtOptions options)
{
    public LoginResponse Create(OperatorAccount account)
    {
        var expires = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Name, account.DisplayName),
            new Claim(ClaimTypes.Role, account.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var token = new JwtSecurityToken(options.Issuer, options.Audience, claims, expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)), SecurityAlgorithms.HmacSha256));
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires,
            new OperatorProfile(account.Id, account.Email, account.DisplayName, account.Role));
    }
}

