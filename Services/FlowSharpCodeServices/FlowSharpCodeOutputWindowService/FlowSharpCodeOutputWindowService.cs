using System.Linq;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceManagement;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpCodeServiceInterfaces;

namespace FlowSharpCodeOutputWindowService
{
    public class FlowSharpCodeOutputModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCodeOutputWindowService, FlowSharpCodeOutputWindowService>();
        }
    }

    public class FlowSharpCodeOutputWindowService : ServiceBase, IFlowSharpCodeOutputWindowService
    {
        protected TextBox outputWindow;
        protected Control parent;

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            dockingService.DocumentClosing += (sndr, args) => OnDocumentClosing(sndr);
        }

        public void CreateOutputWindow()
        {
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            Panel dock = dockingService.DockPanel;
            Control docCanvas = FindDocument(dockingService, FlowSharpServiceInterfaces.Constants.META_CANVAS); // create output window relative to the canvas.

            Control outputContainer = dockingService.CreateDocument(docCanvas, DockAlignment.Bottom, "Output", Constants.META_OUTPUT, 0.50);
            Control pnlOutputWindow = new Panel() { Dock = DockStyle.Fill };
            outputContainer.Controls.Add(pnlOutputWindow);
            CreateOutputWindow(pnlOutputWindow);
        }

        public void CreateOutputWindow(Control parent)
        {
            outputWindow = new TextBox();
            outputWindow.Multiline = true;
            outputWindow.Dock = DockStyle.Fill;
            outputWindow.ReadOnly = true;
            outputWindow.BackColor = Color.White;
            outputWindow.ScrollBars = ScrollBars.Both;
            parent.Controls.Add(outputWindow);
            this.parent = parent;
        }

        public void WriteLine(string line)
        {
            CreateOutputWindowIfNeeded();
            outputWindow.AppendText(line + "\r\n");
        }

        public void Clear()
        {
            CreateOutputWindowIfNeeded();
            outputWindow.Clear();
        }

        public void Closed()
        {
            parent.Controls.Remove(outputWindow);
            outputWindow = null;
        }

        protected void CreateOutputWindowIfNeeded()
        {
            if (outputWindow == null)
            {
                CreateOutputWindow();
            }
        }

        // TODO: Duplicate code in FlowSharpCodeService::FlowSharpCodeService.cs
        protected Control FindDocument(IDockingFormService dockingService, string tag)
        {
            return (Control)dockingService.Documents.SingleOrDefault(d => d.Metadata.LeftOf(",") == tag);
        }

        protected void OnDocumentClosing(object document)
        {
            Control ctrl = document as Control;

            if ( (ctrl != null && ctrl.Controls.Count == 1) && ((IDockDocument)document).Metadata.LeftOf(",") == Constants.META_OUTPUT)
            {
                Closed();
            }
        }
    }
}