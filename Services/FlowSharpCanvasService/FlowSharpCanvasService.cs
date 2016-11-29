/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
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
        public event EventHandler<FileEventArgs> SaveLayout;
        public event EventHandler<FileEventArgs> LoadLayout;

        public BaseController ActiveController { get { return activeCanvasController; } }
        public List<BaseController> Controllers { get { return documents.Values.ToList(); } }

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

        public void ClearControllers()
        {
            documents.Clear();
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

        public void LoadDiagrams(string filename)
        {
            LoadLayout.Fire(this, new FileEventArgs() { Filename = filename });
        }

        public void SaveDiagramsAndLayout(string filename)
        {
            SaveDiagrams(filename);

            // Callback to app to save layout:
            SaveLayout.Fire(this, new FileEventArgs() { Filename = filename });
        }

        protected void SaveDiagrams(string filename)
        {
            int n = 0;

            foreach (BaseController controller in Controllers)
            {
                string data = Persist.Serialize(controller.Elements);

                // If the "canvas" doesn't have a filename, we need to assign one.  For the first controller, this would be the base name,
                // subsequent unnamed canvases get auto-named "-1", "-2", etc.
                if (String.IsNullOrEmpty(controller.Filename))
                {
                    if (n == 0)
                    {
                        controller.Filename = filename;
                    }
                    else
                    {
                        controller.Filename = Path.GetFileNameWithoutExtension(filename) + "-" + n.ToString() + Path.GetExtension(filename);
                    }

                    ++n;
                }

                File.WriteAllText(controller.Filename, data);
            }
        }
    }

    public class FlowSharpCanvasControllerReceptor : IReceptor
    {

    }
}
