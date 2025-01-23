using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.service
{
    public interface ITokenService
    {
        string GenerateJwtToken(string userId);
        string GenerateRefreshToken(string userId);
        Guid ValidateRefreshToken(string refreshToken);
    }

}
