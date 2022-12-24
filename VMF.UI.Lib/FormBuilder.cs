using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VMF.Core;

namespace VMF.UI.Lib
{
    public class FormBuilder : IDisposable
    {
        public FormBuilder(HtmlHelper h)
        {
            Init(null, h);
        }

        public FormBuilder(object model, HtmlHelper h)
        {
            Init(model, h);
        }

        protected void Init(object mdl, HtmlHelper html)
        {
            this.Html = html;
            this.FormModel = new FormViewModel
            {
                RootRef = EntityResolver.GetObjectRef(mdl).ToString(),
            };
        }

        protected void RenderBeginScope()
        {
            
        }
        protected void RenderEndScope()
        {

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
    }
}
