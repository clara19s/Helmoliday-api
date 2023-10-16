using HELMoliday.Models;

namespace HELMoliday.Services.JwtToken;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
