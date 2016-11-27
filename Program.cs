/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.Windows.Forms;

using Clifton.WinForm.ServiceInterfaces;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    static partial class Program
    {
        private static Form form;
        private static IDockingFormService dockingService;
        private static Panel pnlToolbox;
        private static Panel pnlFlowSharp;
        private static PropertyGrid propGrid;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Bootstrap();
            CreateDockingForm();
            Application.Run(form);
        }

        static void CreateDockingForm()
        {
            dockingService = ServiceManager.Get<IDockingFormService>();
            dockingService.ContentLoaded += OnContentLoaded;
            dockingService.ActiveDocumentChanged += (sndr, args) => OnActiveDocumentChanged(sndr);
            form = dockingService.CreateMainForm();
            form.Text = "FlowSharp";
            form.Icon = Properties.Resources.FlowSharp;
            form.Size = new Size(1200, 800);
            form.Shown += OnShown;
            form.FormClosing += OnFormClosing;
            ((IBaseForm)form).ProcessCmdKeyEvent += OnProcessCmdKeyEvent;
        }

        private static void OnContentLoaded(object sender, ContentLoadedEventArgs e)
        {
            switch (e.Metadata)
            {
                case Constants.META_CANVAS:
                    pnlFlowSharp = new Panel() { Dock = DockStyle.Fill, Tag = Constants.META_CANVAS };
                    e.DockContent.Controls.Add(pnlFlowSharp);
                    e.DockContent.Text = "Canvas";
                    ServiceManager.Get<IFlowSharpCanvasService>().CreateCanvas(pnlFlowSharp);
                    BaseController baseController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
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

            if ((e.Metadata == Constants.META_CANVAS || e.Metadata == Constants.META_PROPERTYGRID) && (pnlFlowSharp != null && propGrid != null))
            {
                ServiceManager.Get<IFlowSharpPropertyGridService>().Initialize(propGrid);
            }
        }

        private static void OnProcessCmdKeyEvent(object sender, ProcessCmdKeyEventArgs e)
        {
            e.Handled = ServiceManager.Get<IFlowSharpEditService>().ProcessCmdKey(e.KeyData);
        }

        private static void OnFormClosing(object sender, FormClosingEventArgs e)
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

        private static void OnShown(object sender, EventArgs e)
        {
            dockingService.LoadLayout("defaultLayout.xml");
            Initialize();
        }

        static void Initialize()
        {
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            IFlowSharpMouseControllerService mouseService = ServiceManager.Get<IFlowSharpMouseControllerService>();
            menuService.Initialize(form);
            menuService.Initialize(canvasService.ActiveController);
            canvasService.AddCanvas += (sndr, args) => CreateCanvas();
            mouseService.Initialize(canvasService.ActiveController);
            InformServicesOfNewCanvas(canvasService);
        }

        static void CreateCanvas()
        {
            // Create canvas.
            Panel panel = new Panel() { Dock = DockStyle.Fill, Tag = Constants.META_CANVAS };
            Control dockPanel = ServiceManager.Get<IDockingFormService>().CreateDocument(DockState.Document, Constants.META_CANVAS);
            dockPanel.Controls.Add(panel);
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            canvasService.CreateCanvas(panel);
            InformServicesOfNewCanvas(canvasService);
        }

        static void InformServicesOfNewCanvas(IFlowSharpCanvasService canvasService)
        {
            // Wire up menu for this canvas controller.
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            menuService.Initialize(canvasService.ActiveController);

            // Wire up mouse for this canvas controller.
            IFlowSharpMouseControllerService mouseService = ServiceManager.Get<IFlowSharpMouseControllerService>();
            mouseService.Initialize(canvasService.ActiveController);

            // Debug window needs to know too.
            ServiceManager.Get<IFlowSharpDebugWindowService>().Initialize(canvasService.ActiveController);
        }

        static void OnActiveDocumentChanged(object document)
        {
            Control ctrl = document as Control;

            if (ctrl.Controls.Count == 1)
            {
                System.Diagnostics.Trace.WriteLine("*** Document Changed");
                Control child = ctrl.Controls[0];
                ServiceManager.Get<IFlowSharpMouseControllerService>().ClearState();
                ServiceManager.Get<IFlowSharpCanvasService>().SetActiveController(child);
                ServiceManager.Get<IFlowSharpDebugWindowService>().UpdateDebugWindow();
            }
        }
    }
}

