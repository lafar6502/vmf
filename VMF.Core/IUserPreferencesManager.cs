using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMF.Core
{
    public interface IUserPreferencesManager
    {
        /// <summary>
        /// get current user's preferences with specd key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);
        T Get<T>(string key, T defVal);

        void Set<T>(string key, T val);
        void Set<T>(string userId, string key, T val);

        
        T Get<T>(string userId, string key, T defVal);
    }

    
}
