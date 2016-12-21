/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Diagnostics;

using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpDebugWindowService
{
    public class FlowSharpDebugWindowModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpDebugWindowService, FlowSharpDebugWindowService>();
        }
    }

    public class FlowSharpDebugWindowService : ServiceBase, IFlowSharpDebugWindowService
    {
        protected DlgDebugWindow dlgDebugWindow;
        protected TraceListener traceListener;

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
            traceListener = new TraceListener();
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
        }

        public void ShowDebugWindow()
        {
            if (dlgDebugWindow == null)
            {
                dlgDebugWindow = new DlgDebugWindow(ServiceManager);

                dlgDebugWindow.FormClosed += (sndr, args) =>
                {
                    dlgDebugWindow = null;
                    traceListener.DebugWindow = null;
                    Trace.Listeners.Remove(traceListener);
                };

                dlgDebugWindow.Show();
                traceListener.DebugWindow = dlgDebugWindow;
                Trace.Listeners.Add(traceListener);

                dlgDebugWindow.UpdateUndoStack();
            }
        }

        public void Initialize(BaseController canvasController)
        {
            canvasController.UndoStack.AfterAction += (sndr, args) => UpdateStackTrace();
            UpdateShapeTree();
        }

        public void EditPlugins()
        {
            new DlgPlugins().ShowDialog();
        }

        public void UpdateDebugWindow()
        {
            UpdateStackTrace();
            UpdateShapeTree();
        }

        public void UpdateStackTrace()
        {
            dlgDebugWindow?.UpdateUndoStack();
        }

        public void UpdateShapeTree()
        {
            dlgDebugWindow?.UpdateShapeTree();
        }

        public void FindShape(GraphicElement shape)
        {
            dlgDebugWindow?.FindShape(shape);
        }
    }

    public class FlowSharpDebugWindowReceptor : IReceptor
    {
    }
}
