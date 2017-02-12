/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Windows.Forms;

using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpMenuService
{
    public class FlowSharpMenuModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpMenuService, FlowSharpMenuService>();
        }
    }

    public class FlowSharpMenuService : ServiceBase, IFlowSharpMenuService
    {
        public string Filename { get { return menuController.Filename; } }
        protected MenuController menuController;
        protected Form mainForm;

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
            menuController = new MenuController(ServiceManager);
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
        }

        public void Initialize(Form mainForm)
        {
            this.mainForm = mainForm;
            menuController.Initialize(mainForm);
            mainForm.Controls.Add(menuController.MenuStrip);
        }

        public void Initialize(BaseController controller)
        {
            menuController.Initialize(controller);
        }

        public void UpdateMenu()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            menuController.UpdateMenu(canvasController.SelectedElements.Count > 0);
        }

        public bool SaveOrSaveAs()
        {
            return menuController.SaveOrSaveAs();
        }

        public void AddMenu(ToolStripMenuItem menuItem)
        {
            menuController.AddMenu(menuItem);
        }

        public void EnableCopyPasteDel(bool state)
        {
            menuController.EnableCopyPasteDel(state);
        }
    }

    public class FlowSharpMenuReceptor : IReceptor
    {
    }
}
