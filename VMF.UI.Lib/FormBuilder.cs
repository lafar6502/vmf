using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using VMF.Core;

namespace VMF.UI.Lib
{
    public class FormBuilder : IDisposable
    {
        public FormBuilder(HtmlHelper h)
        {
            Init(null, h, null);
        }

        public FormBuilder(object model, HtmlHelper h)
        {
            Init(model, h, null);
        }

        protected void Init(object mdl, HtmlHelper html, string scopeId)
        {
            EntityResolver = VMFGlobal.ResolveService<IEntityResolver>();
            this.Html = html;
            this.FormModel = new FormViewModel
            {
                FormScopeId = scopeId ?? Guid.NewGuid().ToString("N"),
                RootRef = EntityResolver.GetObjectRef(mdl).ToString(),
            };
            RenderBeginScope();
        }

        protected void RenderBeginScope()
        {
            Html.RenderPartial("UI/FormScopeStart", FormModel);
        }
        protected void RenderEndScope()
        {
            Html.RenderPartial("UI/FormScopeEnd", FormModel);
        }
        public void Dispose()
        {
            RenderEndScope();
        }

        public IEntityResolver EntityResolver { get; set; }

        public FormViewModel FormModel { get; set; }
        public HtmlHelper Html { get; set; }
        
        private bool _ownScope = true;

        /// <summary>
        /// entity -for RootRef
        /// </summary>
        public object ScopeContext { get; set; }

        protected void Init(HtmlHelper html, string scopeId, object model)
        {
            var parentForm = html.ViewBag.FaParentForm as FormViewModel;

            if (string.IsNullOrEmpty(scopeId))
            {
                scopeId = "fsc_" + Guid.NewGuid().ToString("N");
            }
            Html = html;
            if (parentForm != null)
            {
                _ownScope = false;
                FormModel = parentForm;
                scopeId = parentForm.FormScopeId;
            }
            else
            {
                FormModel.FormScopeId = scopeId;
                FormModel.Tid = SessionContext.Current.Transaction.Tid;
                FormModel.Dirty = SessionContext.Current.Transaction.HasModifications;
                if (model != null && EntityResolver.KnowsEntityType(model.GetType()))
                {
                    FormModel.RootRef = model == null ? null : EntityResolver.GetObjectRef(model).ToString();
                }
                /*if (html.ViewContext.ViewBag.FormViewUrl != null)
                {
                    FormModel.ViewUrl = html.ViewContext.ViewBag.FormViewUrl;
                }
                if (html.ViewContext.ViewBag.PostbackUrl != null)
                {
                    FormModel.PostbackUrl = html.ViewContext.ViewBag.PostbackUrl;
                }
                if (html.ViewContext.ViewBag.SaveMode != null)
                {
                    FormModel.SaveMode = html.ViewContext.ViewBag.SaveMode;
                }*/
            }
            ScopeContext = model;
            if (_ownScope && html.ViewBag.FaParentForm == null)
            {
                //html.ViewBag.FaParentForm = this.FormModel;
                //html.ViewData["V1"] = "S_" + this.ScopeId;
                //html.ViewBag.V2 = "Q_" + this.ScopeId;
                //html.ViewContext.TempData["FaParentForm"] = this.ViewModel;
            }
        }
        public FormBuilder AddModelValue(string name, object value)
        {
            this.FormModel.AddEntityFieldValue(null, name, value);
            return this;
        }


        /// <summary>
        /// add value from an entity, together with UI control context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="entity"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public FormBuilder AddValueWithContext(string name, object value, object entity, WidgetContext ctx)
        {
            var er = EntityResolver.GetObjectRef(entity);
            var fid = FormModel.AddEntityFieldValue(er, name, value);
            if (string.IsNullOrEmpty(ctx.FieldName)) ctx.FieldName = name;

            if (ctx != null)
            {
                var cid = FormModel.AddWidgetContext(ctx, fid);
            }

            
            return this;
        }
    }
}
