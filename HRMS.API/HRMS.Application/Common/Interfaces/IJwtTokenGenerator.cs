using HRMS.Domain.Entities;

namespace HRMS.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, IEnumerable<string> roles);
}
