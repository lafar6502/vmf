using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace VMF.Core
{
    /// <summary>
    /// all virtual b/c of proxying
    /// </summary>
    public abstract class ObjectActionBase 
    {
        /// <summary>
        /// Action target....
        /// </summary>
        public virtual object TargetObject { get; set; }
        /// <summary>
        /// target object's method name to be exeucted
        /// or null..
        /// </summary>
        public virtual MethodInfo TargetMethod { get; set; }


        private IEnumerable<object> _allTargets;

        public virtual IEnumerable<object> AllTargets
        {
            get
            {
                if (_allTargets == null) return new object[] { this.TargetObject };
                return _allTargets;
            }
            set
            {
                _allTargets = value;
            }
        }
        
        public virtual IEnumerable<string> AllTargetRefs
        {
            get
            {
                IEntityResolver r = AppGlobal.ResolveService<IEntityResolver>();
                return AllTargets == null ? new string[0] : AllTargets.Select(x => r.GetObjectRef(x).ToString());
            }
            set
            {
                if (value == null)
                {
                    AllTargets = new object[0];
                    return;
                }
                IEntityResolver r = AppGlobal.ResolveService<IEntityResolver>();
                AllTargets = value.Select(x => r.Get(x));
            }
        }

        public virtual string Section
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }

        public virtual string ActionName
        {
            get { return TargetMethod == null ? this.GetType().Name : TargetMethod.Name; }
            set { throw new NotImplementedException(); }
        }

        public abstract Type TargetType
        {
            get;

        }

        public virtual EntityActionType ActionType
        {
            get { return EntityActionType.WindowAction; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// multi - enable action on multiple records
        /// nocontext - dont show in the context menu
        /// enableOverride - enable override in custom logic
        /// mode - Savepoint, Discard, Commit
        /// </summary>
        /// <param name="dic"></param>
        public virtual void ProvideActionConfig(IDictionary<string, object> dic)
        {
            if (dic == null) return;
        }

        /// <summary>
        /// initialize - called after action creation to initialize necessary data.
        /// WARNING: keep this function short, it's called very frequently.
        /// If you have a big chunk of work to do before the action window is displayed
        /// do it in BeforeRender function
        /// </summary>
        public virtual void Initialize()
        {

        }

        public virtual bool IsEnabled()
        {
            return true;
        }

        /// <summary>
        /// execute action on single target object - override this rather than 'Execute'.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual ExecuteActionResult Execute()
        {
            if (TargetMethod != null)
            {
                var v = TargetMethod.Invoke(TargetObject, new object[] { this });
                if (v is ExecuteActionResult) return (ExecuteActionResult)v;
                var er = AppGlobal.ResolveService<IEntityResolver>();
                return new ExecuteActionResult
                {
                    ReturnValue = v,
                    Disposition = v != null && er.KnowsEntityType(v.GetType()) ? ActionDisposition.OpenDetails : ActionDisposition.RefreshView,
                    Tid = SessionContext.Current.Transaction.Tid
                };
            }
            return null;
        }
        /// <summary>
        /// execute the action on all targets (AllTargets collection)
        /// </summary>
        /// <returns></returns>
        public virtual ExecuteActionResult ExecuteOnAllTargets()
        {
            var tg = this.TargetObject;
            var v = Execute();
            if (AllTargets != null)
            {
                var er = AppGlobal.ResolveService<IEntityResolver>();
                foreach (var obj in AllTargets)
                {
                    if (obj == tg || obj == null) continue;
                    this.TargetObject = obj;
                    if (this.IsEnabled())
                    {
                        Execute();
                    }
                    else
                    {
                        throw new ApplicationException("Action not possible on " + er.GetObjectRef(obj));
                    }
                }
            }
            this.TargetObject = tg;
            return v;
        }
        /// <summary>
        /// invoked after form postback is done/completed
        /// before rendering the form
        /// </summary>
        public virtual void BeforeRender()
        {

        }
        /// <summary>
        /// action group - for grouping actions according to some purpose
        /// for example: all actions that complete a task. Similar to section but Group does not affect where/how the action is shown
        /// it's totally custom.
        /// </summary>
        public virtual string Group
        {
            get
            {
                ObjectActionAttribute att = null;
                if (this.TargetMethod != null)
                {
                    att = Attribute.GetCustomAttribute(this.TargetMethod, typeof(ObjectActionAttribute)) as ObjectActionAttribute;
                }
                if (att == null)
                {
                    att = Attribute.GetCustomAttribute(this.GetType(), typeof(ObjectActionAttribute)) as ObjectActionAttribute;
                }
                if (att != null && !string.IsNullOrEmpty(att.Group)) return att.Group;
                return null;
            }
        }

        /// <summary>
        /// you can override this to return custom display name
        /// return null to have default translation applied
        /// </summary>
        public virtual string DisplayName { get; set; }
    }

    public abstract class ObjectAction<T> : ObjectActionBase
    {
        public T Target
        {
            get { return (T)TargetObject; }
        }

        public override Type TargetType
        {
            get
            {
                return typeof(T);
            }
        }

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ObjectActionAttribute : System.Attribute
    {
        /// <summary>
        /// dont auto-register the action (has to be manually created, for example in expando)
        /// </summary>
        public bool DontRegister { get; set; }
        /// <summary>
        /// action group name
        /// </summary>
        public string Group { get; set; }
    }
}
