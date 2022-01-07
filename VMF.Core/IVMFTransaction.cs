using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    /// <summary>
    /// what to do at the end of transaction?
    /// </summary>
    public enum TransactionMode
    {
        Discard,
        SaveState,
        Commit
    }

    public interface IVMFTransaction : IDisposable
    {
        string SerializeState();
        void DeserializeState(string state);
        /// <summary>
        /// transaction id
        /// </summary>
        string Tid { get; set; }
        void Commit();
        /// <summary>
        /// re-enlist the transaction in current transaction scope
        /// need to be used only when playing tricks with transaction scopes
        /// </summary>
        void ReEnlist();
        /// <summary>
        /// true if there are any uncommited changes in the transaction
        /// </summary>
        bool HasModifications { get; }

        void SetData(string key, object value);

        T GetData<T>(string key, T defaultValue);

        bool HasData(string key);
    }
}
