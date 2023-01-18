using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGinnBPM.DSLServices;
using System.IO;
using NLog;
using Boo.Lang.Compiler;

namespace Unitech.CutLists.V2
{
    /// <summary>
    /// this maintains 2 subdirectories of the base folder:
    /// - Global - for global products
    /// - Private - for customer's private products.
    /// 
    /// WARNING: in both global and private folders products should be stored in subdirectories
    /// corresponding to company names. All subdirectories will be scanned
    /// so remember not to publish everything to the private folder
    /// 
    /// The product Id is generated as
    /// [Company]_[Product]_[Version] where Version is an integer increasing nr
    /// Vema_PlisseDuette_3
    /// 
    /// Boo files: PlisseDuette.3.boo, PlisseDuette.2.boo etc
    /// But should we compile all versions? We'll see. Maybe not necessary
    /// but there should be a default version for each of the products
    /// and this one we should compile for sure. To be decided
    /// 
    /// 
    /// </summary>
    public class ScriptStorageWithFolder : ScriptStorage
    {
        private string folder;

        public ScriptStorageWithFolder(string bdir) : base(bdir)
        {
            folder = new DirectoryInfo(bdir).Name;
        }

        public override string GetTypeNameFromUrl(string url)
        {
            return base.GetTypeNameFromUrl(url);
        }

        
    }
}
