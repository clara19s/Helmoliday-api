using HELMoliday.Models;

namespace HELMoliday.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
