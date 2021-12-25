using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Collections.Generic;

namespace VMF.Core
{
    public class AppUser : IPrincipal
    {
        public static readonly string AnonymousUserId = "anonymous";

        public static string SystemUserId
        {
            get
            {
                return AppGlobal.Config.Get("SystemUserId", AnonymousUserId);
            }
        }
        
        public IIdentity Identity
        {
            get { return new GenericIdentity(this.Login, "app"); }
        }

        public bool IsInRole(string role)
        {
            return _roles.Contains(role);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// business unit name
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// department id (or whatever)
        /// </summary>
        public int? UnitId { get; set; }
        public string DefaultLanguage { get; set; }

        private HashSet<string> _roles = new System.Collections.Generic.HashSet<string>();
        private HashSet<int> _groupIds = new System.Collections.Generic.HashSet<int>();

        public IEnumerable<string> Roles
        {
            get { return _roles; }
            set { _roles = new System.Collections.Generic.HashSet<string>(value); }
        }

        public IEnumerable<int> GroupIds
        {
            get { return _groupIds; }
            set { _groupIds = new System.Collections.Generic.HashSet<int>(value); }
        }

        public bool HasPermission(string permissionName)
        {
            return IsInRole(permissionName);
        }

        public bool IsAnonymous
        {
            get
            {
                return string.Equals(this.Login, AnonymousUserId, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// cookie expiration date, used to force logout for example when permissions change
        /// </summary>
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// login of the original user who did impersonate the current user
        /// </summary>
        public string ImpersonatingUserId
        {
            get;set;
        }

        public static AppUser Current
        {
            get { return System.Threading.Thread.CurrentPrincipal as AppUser; }
            set { System.Threading.Thread.CurrentPrincipal = value; }
        }

        
    }
}
