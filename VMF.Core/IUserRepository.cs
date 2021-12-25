using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public interface IUserRepository
    {
        AppUser GetUserByUserId(string userId);
        bool PasswordAuthenticate(string userId, string password);
    }
}
