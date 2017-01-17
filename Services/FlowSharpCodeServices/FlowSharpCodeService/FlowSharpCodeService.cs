using System;
using System.Linq;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceManagement;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpLib;
using FlowSharpServiceInterfaces;
using FlowSharpCodeServiceInterfaces;

namespace FlowSharpCodeService
{
    public class FlowSharpCodeModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCodeService, FlowSharpCodeService>();
        }
    }

    public class FlowSharpCodeService : ServiceBase, IFlowSharpCodeService
    {
        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            IFlowSharpService fss = ServiceManager.Get<IFlowSharpService>();
            IFlowSharpCodeEditorService ces = ServiceManager.Get<IFlowSharpCodeEditorService>();
            fss.FlowSharpInitialized += OnFlowSharpInitialized;
            fss.ContentResolver += OnContentResolver;
            fss.NewCanvas += OnNewCanvas;
            ces.TextChanged += OnCodeEditorServiceTextChanged;
            InitializeMenu();
        }

        protected void InitializeMenu()
        {
            ToolStripMenuItem buildToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem mnuCompile = new ToolStripMenuItem();
            ToolStripMenuItem mnuRun = new ToolStripMenuItem();

            mnuRun.Name = "mnuRun";
            mnuRun.ShortcutKeys = Keys.Alt | Keys.R;
            mnuRun.Size = new System.Drawing.Size(165, 24);
            mnuRun.Text = "&Run";

            mnuCompile.Name = "mnuCompile";
            mnuCompile.ShortcutKeys = Keys.Alt | Keys.C;
            mnuCompile.Size = new System.Drawing.Size(165, 24);
            mnuCompile.Text = "&Compile";

            buildToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {mnuCompile, mnuRun});
            buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            buildToolStripMenuItem.Size = new System.Drawing.Size(37, 21);
            buildToolStripMenuItem.Text = "Bu&ild";

            mnuCompile.Click += OnCompile;
            mnuRun.Click += OnRun;

            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(buildToolStripMenuItem);
        }

        protected void OnCompile(object sender, EventArgs e)
        {
            ServiceManager.Get<IFlowSharpCodeCompilerService>().Compile();
        }

        protected void OnRun(object sender, EventArgs e)
        {
            ServiceManager.Get<IFlowSharpCodeCompilerService>().Run();
        }

        protected void OnFlowSharpInitialized(object sender, EventArgs args)
        {
            CreateEditor();
            CreateOutputWindow();
        }

        protected void CreateEditor()
        {
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            Panel dock = dockingService.DockPanel;
            Control docCanvas = FindDocument(dockingService, FlowSharpServiceInterfaces.Constants.META_CANVAS);

            Control csDocEditor = dockingService.CreateDocument(docCanvas, DockAlignment.Bottom, "C# Editor", FlowSharpCodeServiceInterfaces.Constants.META_EDITOR, 0.50);
            Control pnlCsCodeEditor = new Panel() { Dock = DockStyle.Fill };
            csDocEditor.Controls.Add(pnlCsCodeEditor);

            IFlowSharpCodeEditorService csCodeEditorService = ServiceManager.Get<IFlowSharpCodeEditorService>();
            csCodeEditorService.CreateEditor(pnlCsCodeEditor);
            csCodeEditorService.AddAssembly("Clifton.Core.dll");
        }

        protected void CreateOutputWindow()
        {
            //IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            //Panel dock = dockingService.DockPanel;
            //Control docCanvas = FindDocument(dockingService, FlowSharpServiceInterfaces.Constants.META_CANVAS);

            //Control outputWindow = dockingService.CreateDocument(docCanvas, DockAlignment.Bottom, "Output", FlowSharpCodeServiceInterfaces.Constants.META_OUTPUT, 0.50);
            //Control pnlOutputWindow = new Panel() { Dock = DockStyle.Fill };
            //outputWindow.Controls.Add(pnlOutputWindow);

            IFlowSharpCodeOutputWindowService outputWindowService = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            outputWindowService.CreateOutputWindow();
            // outputWindowService.CreateOutputWindow(pnlOutputWindow);
        }

        protected void OnContentResolver(object sender, ContentLoadedEventArgs e)
        {
            switch (e.Metadata.LeftOf(","))
            {
                case FlowSharpCodeServiceInterfaces.Constants.META_EDITOR:
                    Panel pnlEditor = new Panel() { Dock = DockStyle.Fill, Tag = FlowSharpCodeServiceInterfaces.Constants.META_EDITOR};
                    e.DockContent.Controls.Add(pnlEditor);
                    e.DockContent.Text = "Editor";
                    IFlowSharpCodeEditorService csCodeEditorService = ServiceManager.Get<IFlowSharpCodeEditorService>();
                    csCodeEditorService.CreateEditor(pnlEditor);
                    csCodeEditorService.AddAssembly("Clifton.Core.dll");
                    break;

                case FlowSharpCodeServiceInterfaces.Constants.META_OUTPUT:
                    Panel pnlOutputWindow = new Panel() { Dock = DockStyle.Fill, Tag = FlowSharpCodeServiceInterfaces.Constants.META_OUTPUT };
                    e.DockContent.Controls.Add(pnlOutputWindow);
                    e.DockContent.Text = "Output";
                    IFlowSharpCodeOutputWindowService outputWindowService = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
                    outputWindowService.CreateOutputWindow(pnlOutputWindow);
                    break;
            }
        }

        protected void OnNewCanvas(object sender, NewCanvasEventArgs args)
        {
            args.Controller.ElementSelected += OnElementSelected;
        }

        protected void OnElementSelected(object controller, ElementEventArgs args)
        {
            ElementProperties elementProperties = null;
            IFlowSharpCodeEditorService csCodeEditorService = ServiceManager.Get<IFlowSharpCodeEditorService>();

            if (args.Element != null)
            {
                GraphicElement el = args.Element;
                elementProperties = el.CreateProperties();

                string code;
                el.Json.TryGetValue("Code", out code);
                csCodeEditorService.SetText(code ?? String.Empty);
            }
            else
            {
                csCodeEditorService.SetText(String.Empty);
            }
        }

        protected void OnCodeEditorServiceTextChanged(object sender, TextChangedEventArgs e)
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            if (canvasService.ActiveController.SelectedElements.Count == 1)
            {
                GraphicElement el = canvasService.ActiveController.SelectedElements[0];
                el.Json["Code"] = e.Text;
            }
        }

        /// <summary>
        /// Traverse the root docking panel to find the IDockDocument child control with the specified metadata tag.
        /// For example, dock.Controls[1].Controls[1].Controls[2].Metadata is META_TOOLBOX.
        /// </summary>
        protected Control FindPanel(Control ctrl, string tag)
        {
            Control ret = null;

            // dock.Controls[1].Controls[1].Controls[2].Metadata <- this is the toolbox
            if ((ctrl is IDockDocument) && ((IDockDocument)ctrl).Metadata == tag)
            {
                ret = ctrl;
            }
            else
            {
                foreach (Control c in ctrl.Controls)
                {
                    ret = FindPanel(c, tag);

                    if (ret != null)
                    {
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Return the document (or null) that implements IDockDocument with the specified Metadata tag.
        /// </summary>
        protected Control FindDocument(IDockingFormService dockingService, string tag)
        {
            return (Control)dockingService.Documents.SingleOrDefault(d => d.Metadata.LeftOf(",") == tag);
        }
    }
}
