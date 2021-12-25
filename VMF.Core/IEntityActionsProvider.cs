using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMF.Core
{
    public enum EntityActionType
    {
        //window popup action
        WindowAction,
        // silent action (without UI)
        BackgroundAction,
        // client side script - additional info needed...
        ClientSideAction
    }

    public class EntityActionInfo
    {
        public string Name { get; set; }
        /// <summary>
        /// section where action belongs
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// section where action belongs
        /// </summary>
        public string Section { get; set; }
        public EntityActionType ActionType { get; set; }
        public bool IsEnabled { get; set; }
        /// <summary>
        /// action config - for extra action info
        /// for example, form name, client function name, URL, 
        /// postback mode etc
        /// </summary>
        public Dictionary<string, object> Config { get; set; }
        /// <summary>
        /// full action object reference (optional)
        /// </summary>
        public string ActionRef { get; set; }
        /// <summary>
        /// action group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// translation Id
        /// </summary>
        public string Id { get; set; }
    }


    /// <summary>
    /// Information about actions available for an entity
    /// </summary>
    public interface IEntityActionsProvider
    {
        /// <summary>
        /// return only the actions that are enabled at the moment of the call
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        IEnumerable<EntityActionInfo> GetEnabledEntityActions(object entity);
        /// <summary>
        /// return all available entity actions, not checking if they are enabled now or not
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        IEnumerable<EntityActionInfo> GetAvailableEntityActions(object entity);
        /// <summary>
        /// single action information
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        EntityActionInfo GetActionInfo(object entity, string actionName);

        /// <summary>
        /// list of all possible actions for given entity type.
        /// this does not include expando actions
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IEnumerable<EntityActionInfo> GetDefinedEntityActions(Type entityType);
    }

    /// <summary>
    /// for overriding default IsEnabled on actions
    /// </summary>
    public interface IEntityActionOverride
    {
        bool? IsActionEnabled(ObjectActionBase ab);
    };


    /* action exec mode
     * nocommit, refresh details
     * commit, refresh details
     * commit, close details
     * nocommit, open another details
     * commit, open another details
     * action form postback(???) 
     * 
     * */

    public enum ActionDisposition
    {
        Nothing,
        RefreshView,
        ExitDetails,
        OpenDetails,
        CallClientScript,
        GotoList,
        GotoUrl,
        FileDownload,
        RenderMvcView,
        /// <summary>
        /// show error message. Param is the error message
        /// </summary>
        ShowError,
        /// <summary>
        /// show a message window. Param is the message.
        /// </summary>
        ShowMessage,
        /// <summary>
        /// show notification/baloon message for a moment. Param is the message.
        /// </summary>
        ShowNotification,
        /// <summary>
        /// close details window and show a message (= ExitDetails + ShowMessage)
        /// </summary>
        ExitDetailsAndShowMessage,
        /// <summary>
        /// show message and refresh view (= ShowMessage + RefreshView)
        /// </summary>
        ShowMessageAndRefreshView,
        /// <summary>
        /// wait for a long-running op to complete. 
        /// Show some waiting window. Return job id in Param field.
        /// </summary>
        MonitorLongRunningOperation
    }

    public class ExecuteActionResult
    {
        /// <summary>
        /// action return value
        /// for file download, this should be 
        /// file path, or byte[] data
        /// </summary>
        public object ReturnValue { get; set; }
        public ActionDisposition Disposition { get; set; }
        /// <summary>
        /// client action parameter, for example client action name
        /// </summary>
        public string Param { get; set; }
        public string Tid { get; set; }

        public ExecuteActionResult()
        {
            Disposition = ActionDisposition.RefreshView;
        }

    }
}
