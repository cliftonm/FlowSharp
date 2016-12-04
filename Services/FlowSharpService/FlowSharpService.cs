/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceManagement;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpService
{
    public class FlowSharpServiceModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpService, FlowSharpService>();
        }
    }

    public class FlowSharpService : ServiceBase, IFlowSharpService
    {
        public event EventHandler<ContentLoadedEventArgs> ContentResolver;
        public event EventHandler<EventArgs> FlowSharpInitialized;
        public event EventHandler<NewCanvasEventArgs> NewCanvas;

        private Form form;
        private IDockingFormService dockingService;
        private Panel pnlToolbox;
        private Panel pnlFlowSharp;
        private PropertyGrid propGrid;
        private bool loading;

        public Form CreateDockingForm(Icon icon)
        {
            dockingService = ServiceManager.Get<IDockingFormService>();
            dockingService.ContentLoaded += OnContentLoaded;
            dockingService.ActiveDocumentChanged += (sndr, args) => OnActiveDocumentChanged(sndr);
            form = dockingService.CreateMainForm();
            form.Text = "FlowSharp";
            form.Icon = icon;
            form.Size = new Size(1200, 800);
            form.Shown += OnShown;
            form.FormClosing += OnFormClosing;
            ((IBaseForm)form).ProcessCmdKeyEvent += OnProcessCmdKeyEvent;

            return form;
        }

        protected void OnContentLoaded(object sender, ContentLoadedEventArgs e)
        {
            switch (e.Metadata.LeftOf(","))
            {
                case Constants.META_CANVAS:
                    pnlFlowSharp = new Panel() { Dock = DockStyle.Fill, Tag = Constants.META_CANVAS };
                    e.DockContent.Controls.Add(pnlFlowSharp);
                    e.DockContent.Text = "Canvas";
                    IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
                    canvasService.CreateCanvas(pnlFlowSharp);
                    BaseController baseController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

                    if (e.Metadata.Contains(","))
                    {
                        string filename = e.Metadata.Between(",", ",");
                        string canvasName = e.Metadata.RightOfRightmostOf(",");
                        canvasName = String.IsNullOrWhiteSpace(canvasName) ? "Canvas" : canvasName;
                        e.DockContent.Text = canvasName;
                        LoadFileIntoCanvas(filename, canvasName, baseController);
                    }

                    ServiceManager.Get<IFlowSharpMouseControllerService>().Initialize(baseController);
                    break;

                case Constants.META_TOOLBOX:
                    pnlToolbox = new Panel() { Dock = DockStyle.Fill, Tag = Constants.META_TOOLBOX };
                    e.DockContent.Controls.Add(pnlToolbox);
                    e.DockContent.Text = "Toolbox";
                    break;

                case Constants.META_PROPERTYGRID:
                    propGrid = new PropertyGrid() { Dock = DockStyle.Fill, Tag = Constants.META_PROPERTYGRID };
                    e.DockContent.Controls.Add(propGrid);
                    e.DockContent.Text = "Property Grid";
                    ServiceManager.Get<IFlowSharpPropertyGridService>().Initialize(propGrid);
                    break;

                default:
                    ContentResolver.Fire(this, e);
                    break;
            }

            // Associate the toolbox with a canvas controller after both canvas and toolbox panels are created.
            // !!! This handles the defaultLayout configuration. !!!
            if ((e.Metadata == Constants.META_CANVAS || e.Metadata == Constants.META_TOOLBOX) && (pnlFlowSharp != null && pnlToolbox != null))
            {
                IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
                BaseController canvasController = canvasService.ActiveController;
                IFlowSharpToolboxService toolboxService = ServiceManager.Get<IFlowSharpToolboxService>();
                toolboxService.CreateToolbox(pnlToolbox);
                toolboxService.InitializeToolbox();
                toolboxService.InitializePluginsInToolbox();
                toolboxService.UpdateToolboxPaths();
            }

            //if ((e.Metadata == Constants.META_CANVAS || e.Metadata == Constants.META_PROPERTYGRID) && (pnlFlowSharp != null && propGrid != null))
            //{
            //    ServiceManager.Get<IFlowSharpPropertyGridService>().Initialize(propGrid);
            //}
        }

        protected void OnProcessCmdKeyEvent(object sender, ProcessCmdKeyEventArgs e)
        {
            e.Handled = ServiceManager.Get<IFlowSharpEditService>().ProcessCmdKey(e.KeyData);
        }

        protected void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ClosingState state = ServiceManager.Get<IFlowSharpEditService>().CheckForChanges();

            if (state == ClosingState.SaveChanges)
            {
                if (!ServiceManager.Get<IFlowSharpMenuService>().SaveOrSaveAs())
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = state == ClosingState.CancelClose;
            }

            if (!e.Cancel)
            {
                dockingService.SaveLayout("layout.xml");
            }
        }

        protected void OnShown(object sender, EventArgs e)
        {
            dockingService.LoadLayout("defaultLayout.xml");
            Initialize();
            FlowSharpInitialized.Fire(this);
        }

        protected void Initialize()
        {
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            IFlowSharpMouseControllerService mouseService = ServiceManager.Get<IFlowSharpMouseControllerService>();
            menuService.Initialize(form);
            menuService.Initialize(canvasService.ActiveController);
            canvasService.AddCanvas += (sndr, args) => CreateCanvas();
            canvasService.LoadLayout += OnLoadLayout;
            canvasService.SaveLayout += OnSaveLayout;
            mouseService.Initialize(canvasService.ActiveController);
            InformServicesOfNewCanvas(canvasService.ActiveController);
        }

        protected void OnLoadLayout(object sender, FileEventArgs e)
        {
            string layoutFilename = Path.Combine(Path.GetDirectoryName(e.Filename), Path.GetFileNameWithoutExtension(e.Filename) + "-layout.xml");

            if (File.Exists(layoutFilename))
            {
                // Use the layout file to determine the canvas files.
                ServiceManager.Get<IFlowSharpEditService>().ClearSavePoints();
                IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
                IFlowSharpPropertyGridService pgService = ServiceManager.Get<IFlowSharpPropertyGridService>();
                canvasService.Controllers.ForEach(c => pgService.Terminate(c));
                canvasService.ClearControllers();
                loading = true;
                ServiceManager.Get<IDockingFormService>().LoadLayout(layoutFilename);
                loading = false;

                // Update all services with new controllers.
                canvasService.Controllers.ForEach(c => InformServicesOfNewCanvas(c));
                SelectFirstDocument();
            }
            else
            {
                // Just open the diagram the currently selected canvas.
                BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
                ServiceManager.Get<IFlowSharpEditService>().ClearSavePoints();
                LoadFileIntoCanvas(e.Filename, Constants.META_CANVAS, canvasController);
            }
        }

        protected void LoadFileIntoCanvas(string filename, string canvasName, BaseController canvasController)
        {
            canvasController.Filename = filename;       // set now, in case of relative image files, etc...
            canvasController.CanvasName = canvasName;
            string data = File.ReadAllText(filename);
            List<GraphicElement> els = Persist.Deserialize(canvasController.Canvas, data);
            canvasController.Clear();
            canvasController.UndoStack.ClearStacks();
            ElementCache.Instance.ClearCache();
            ServiceManager.Get<IFlowSharpMouseControllerService>().ClearState();
            canvasController.AddElements(els);
            canvasController.Elements.ForEach(el => el.UpdatePath());
            canvasController.Canvas.Invalidate();
        }

        protected void OnSaveLayout(object sender, FileEventArgs e)
        {
            // Save the layout, which, on an open, will check for a layout file and load the documents from the layout metadata.
            string layoutFilename = Path.Combine(Path.GetDirectoryName(e.Filename), Path.GetFileNameWithoutExtension(e.Filename) + "-layout.xml");
            ServiceManager.Get<IDockingFormService>().SaveLayout(layoutFilename);
        }

        protected void CreateCanvas()
        {
            // Create canvas.
            Panel panel = new Panel() { Dock = DockStyle.Fill, Tag = Constants.META_CANVAS };
            Control dockPanel = ServiceManager.Get<IDockingFormService>().CreateDocument(DockState.Document, "Canvas", Constants.META_CANVAS);
            dockPanel.Controls.Add(panel);
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            canvasService.CreateCanvas(panel);
            InformServicesOfNewCanvas(canvasService.ActiveController);
        }

        protected void InformServicesOfNewCanvas(BaseController controller)
        {
            // Wire up menu for this canvas controller.
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            menuService.Initialize(controller);

            // Wire up mouse for this canvas controller.
            IFlowSharpMouseControllerService mouseService = ServiceManager.Get<IFlowSharpMouseControllerService>();
            mouseService.Initialize(controller);

            // Debug window needs to know too.
            ServiceManager.Get<IFlowSharpDebugWindowService>().Initialize(controller);

            // PropertyGrid service needs to hook controller events.
            ServiceManager.Get<IFlowSharpPropertyGridService>().Initialize(controller);

            // Update document tab when canvas name changes.
            controller.CanvasNameChanged += (sndr, args) =>
            {
                IDockDocument doc = ((IDockDocument)((BaseController)sndr).Canvas.Parent.Parent);
                doc.TabText = controller.CanvasName;

                // Update the metadata for the controller document so the layout contains this info on save.
                doc.Metadata = Constants.META_CANVAS + "," + controller.Filename + "," + doc.TabText;
            };

            // Update the metadata for the controller document so the layout contains this info on save.
            controller.FilenameChanged += (sndr, args) =>
            {
                IDockDocument doc = ((IDockDocument)((BaseController)sndr).Canvas.Parent.Parent);
                doc.Metadata = Constants.META_CANVAS + "," + controller.Filename + "," + doc.TabText;
            };

            // Update any other services needing to know about the new canvas.
            NewCanvas.Fire(this, new NewCanvasEventArgs() { Controller = controller });
        }

        protected void OnActiveDocumentChanged(object document)
        {
            if (!loading)
            {
                Control ctrl = document as Control;

                if (ctrl != null && ctrl.Controls.Count == 1 && ((IDockDocument)document).Metadata.LeftOf(",") == Constants.META_CANVAS)
                {
                    // System.Diagnostics.Trace.WriteLine("*** Document Changed");
                    Control child = ctrl.Controls[0];
                    ServiceManager.Get<IFlowSharpMouseControllerService>().ClearState();
                    ServiceManager.Get<IFlowSharpCanvasService>().SetActiveController(child);
                    ServiceManager.Get<IFlowSharpDebugWindowService>().UpdateDebugWindow();
                    ServiceManager.Get<IFlowSharpMenuService>().UpdateMenu();
                }
            }
        }

        protected void SelectFirstDocument()
        {
            IDockingFormService dockService = ServiceManager.Get<IDockingFormService>();
            List<IDockDocument> docs = dockService.Documents;

            if (docs.Count > 0)
            {
                dockService.SetActiveDocument(docs[0]);
            }
        }
    }
}
