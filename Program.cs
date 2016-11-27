/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Clifton.WinForm.ServiceInterfaces;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    static partial class Program
    {
        private const string META_CANVAS = "Canvas";
        private const string META_TOOLBOX = "Toolbox";
        private const string META_PROPERTYGRID = "PropertyGrid";

        private static Form form;
        private static IDockingFormService dockingService;

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
                case META_CANVAS:
                    Panel pnlFlowSharp = new Panel() { Dock = DockStyle.Fill };
                    e.DockContent.Controls.Add(pnlFlowSharp);
                    ServiceManager.Get<IFlowSharpCanvasService>().CreateCanvas(pnlFlowSharp);
                    break;

                case META_TOOLBOX:
                    Panel pnlToolbox = new Panel() { Dock = DockStyle.Fill };
                    e.DockContent.Controls.Add(pnlToolbox);
                    BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().Controller;
                    IFlowSharpToolboxService toolboxService = ServiceManager.Get<IFlowSharpToolboxService>();
                    toolboxService.CreateToolbox(pnlToolbox);
                    toolboxService.InitializeToolbox();
                    toolboxService.InitializePluginsInToolbox();
                    toolboxService.UpdateToolboxPaths();
                    break;

                case META_PROPERTYGRID:
                    PropertyGrid propGrid = new PropertyGrid() { Dock = DockStyle.Fill };
                    ServiceManager.Get<IFlowSharpPropertyGridService>().Initialize(propGrid);
                    e.DockContent.Controls.Add(propGrid);
                    break;
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
        }

        private static void OnShown(object sender, EventArgs e)
        {
            if (File.Exists("layout.xml"))
            {
                dockingService.LoadLayout("layout.xml");
            }
            else
            {
                dockingService.LoadLayout("defaultLayout.xml");
            }

            InitializeMenu();
        }

        static void InitializeMenu()
        {
            ServiceManager.Get<IFlowSharpMenuService>().Initialize(form);
        }
    }
}
