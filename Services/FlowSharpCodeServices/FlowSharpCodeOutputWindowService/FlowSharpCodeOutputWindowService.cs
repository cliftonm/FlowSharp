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
            Control outputContainer = null;
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            // Panel dock = dockingService.DockPanel;

            // If an editor is open, create the output window to the right of the editor.
            // If no editor is open, create the output window below the canvas.
            // If no canvas is even open, create the output window as the primary document.
            // We need to implement this as a basic pattern of rules for how to position new windows.
            Control ctrl = FindDocument(dockingService, Constants.META_CSHARP_EDITOR); // create output window relative to the editor window.

            if (ctrl == null)
            {
                ctrl = FindDocument(dockingService, FlowSharpServiceInterfaces.Constants.META_CANVAS);

                if (ctrl == null)
                {
                    outputContainer = dockingService.CreateDocument(DockState.Document, "Output", Constants.META_OUTPUT);
                }
                else
                {
                    outputContainer = dockingService.CreateDocument(ctrl, DockAlignment.Bottom, "Output", Constants.META_OUTPUT, 0.50);
                }
            }
            else
            {
                outputContainer = dockingService.CreateDocument(ctrl, DockAlignment.Right, "Output", Constants.META_OUTPUT, 0.50);
            }

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

        public void Write(string text)
        {
            if (text != null)
            {
                Application.OpenForms[0].BeginInvoke(() =>
                {
                    CreateOutputWindowIfNeeded();
                    outputWindow.AppendText(text ?? "");
                });
            }
        }

        public void WriteLine(string line)
        {
            Application.OpenForms[0].BeginInvoke(() =>
            {
                CreateOutputWindowIfNeeded();
                outputWindow.AppendText((line ?? "") + "\r\n");
            });
        }

        public void Clear()
        {
            Application.OpenForms[0].BeginInvoke(() =>
            {
                CreateOutputWindowIfNeeded();
                outputWindow.Clear();
            });
        }

        protected void Closed()
        {
            parent.Controls.Remove(outputWindow);
            outputWindow = null;
            ServiceManager.Get<IFlowSharpCodeService>().OutputWindowClosed();
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