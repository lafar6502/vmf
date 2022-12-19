using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMF.Core;
using Sooda;

namespace VMF.BusinessObjects.Services
{
    public class UserRepository : IUserRepository
    {
        public AppUser GetUserByUserId(string userId)
        {
            var su = SysUser.Linq(Sooda.SoodaSnapshotOptions.NoWriteObjects).Where(x => x.Active && x.Login == userId).FirstOrDefault();
            if (su == null) return null;
            return new AppUser
            {
                Id = su.Id,
                Login = su.Login,
                Name = su.Name,
                Email = su.Email
            };
        }

        public bool PasswordAuthenticate(string userId, string password)
        {
            var su = SysUser.Linq(Sooda.SoodaSnapshotOptions.NoWriteObjects).Where(x => x.Active && x.Login == userId).FirstOrDefault();
            if (su == null) return false;
            return true;
        }
    }
}
