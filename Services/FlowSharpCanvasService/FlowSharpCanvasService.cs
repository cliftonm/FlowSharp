/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpCanvasService
{
    public class FlowSharpCanvasModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCanvasService, FlowSharpCanvasService>();
        }
    }

    public class FlowSharpCanvasService : ServiceBase, IFlowSharpCanvasService
    {
        public event EventHandler<EventArgs> AddCanvas;

        public BaseController ActiveController { get { return activeCanvasController; } }

        protected Dictionary<Control, BaseController> documents;
        protected BaseController activeCanvasController;

        public FlowSharpCanvasService()
        {
            documents = new Dictionary<Control, BaseController>();
        }

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
            ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, FlowSharpCanvasControllerReceptor>();
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
        }

        public void CreateCanvas(Control parent)
        {
            Canvas canvas = new Canvas();
            CanvasController canvasController = new CanvasController(canvas);
            documents[parent] = canvasController;
            // Canvas.Initialize requires that the parent be attached to the form!
            canvas.Initialize(parent);
            activeCanvasController = canvasController;
        }

        public void SetActiveController(Control parent)
        {
            activeCanvasController = documents[parent];
        }

        public void RequestNewCanvas()
        {
            AddCanvas.Fire(this);
        }
    }

    public class FlowSharpCanvasControllerReceptor : IReceptor
    {

    }
}
