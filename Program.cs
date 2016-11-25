/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.Semantics;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    static partial class Program
    {
        private static Form form;
        private static Control docCanvas;
        private static Control docToolbar;
        private static Control propGridToolbar;
        private static IDockingFormService dockingService;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Bootstrap();
            // Application.Run(new FlowSharpUI());

            CreateDockingForm();
            Application.Run(form);
        }

        static void CreateDockingForm()
        {
            dockingService = ServiceManager.Get<IDockingFormService>();
            form = dockingService.CreateMainForm();
            form.Text = "FlowSharp";
            form.Icon = Properties.Resources.FlowSharp;
            form.Size = new Size(1200, 800);
            form.Shown += OnShown;
            form.FormClosing += OnFormClosing;
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
        }

        private static void OnShown(object sender, EventArgs e)
        {
            CreateCanvas();
            CreateToolbar();
            CreatePropertyGrid();
            InitializeMenu();
        }

        static void CreateCanvas()
        {
            docCanvas = dockingService.CreateDocument(DockState.Document, "Canvas");
            Panel pnlFlowSharp = new Panel() { Dock = DockStyle.Fill };
            docCanvas.Controls.Add(pnlFlowSharp);
            ServiceManager.Get<IFlowSharpCanvasService>().CreateCanvas(pnlFlowSharp);
        }

        static void CreateToolbar()
        {
            docToolbar = dockingService.CreateDocument(DockState.DockLeft, "Toolbar");
            Panel pnlToolbox = new Panel() { Dock = DockStyle.Fill };
            docToolbar.Controls.Add(pnlToolbox);
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().Controller;
            IFlowSharpToolboxService toolboxService = ServiceManager.Get<IFlowSharpToolboxService>();
            toolboxService.CreateToolbox(pnlToolbox);
            toolboxService.InitializeToolbox();
            toolboxService.InitializePluginsInToolbox();
            toolboxService.UpdateToolboxPaths();
        }

        static void CreatePropertyGrid()
        {
            propGridToolbar = dockingService.CreateDocument(docToolbar, DockAlignment.Bottom, "Properties", 0.50);
            PropertyGrid propGrid = new PropertyGrid() { Dock = DockStyle.Fill };
            ServiceManager.Get<IFlowSharpPropertyGridService>().Initialize(propGrid);
            propGridToolbar.Controls.Add(propGrid);
        }

        static void InitializeMenu()
        {
            ServiceManager.Get<IFlowSharpMenuService>().Initialize(form);
        }
    }
}
