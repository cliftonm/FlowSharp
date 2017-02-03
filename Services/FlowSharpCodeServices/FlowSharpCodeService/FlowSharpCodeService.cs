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
        private const string LANGUAGE_CSHARP = "C#";
        private const string LANGUAGE_PYTHON = "Python";
        private const string LANGUAGE_JAVASCRIPT = "JavaScript";
        private const string LANGUAGE_HTML = "HTML";
        private const string LANGUAGE_CSS = "CSS";

        protected ToolStripMenuItem mnuCSharp = new ToolStripMenuItem() { Name = "mnuCSharp", Text = "C#" };
        protected ToolStripMenuItem mnuPython = new ToolStripMenuItem() { Name = "mnuPython", Text = "Python" };
        protected ToolStripMenuItem mnuJavascript = new ToolStripMenuItem() { Name = "mnuJavascript", Text = "JavaScript" };
        protected ToolStripMenuItem mnuHtml = new ToolStripMenuItem() { Name = "mnuHtml", Text = "Html" };
        protected ToolStripMenuItem mnuCss = new ToolStripMenuItem() { Name = "mnuCss", Text = "Css" };
        protected ToolStripMenuItem mnuOutput = new ToolStripMenuItem() { Name = "mnuOutput", Text = "Output" };

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            IFlowSharpService fss = ServiceManager.Get<IFlowSharpService>();
            IFlowSharpCodeEditorService ces = ServiceManager.Get<IFlowSharpCodeEditorService>();
            IFlowSharpScintillaEditorService ses = ServiceManager.Get<IFlowSharpScintillaEditorService>();
            fss.FlowSharpInitialized += OnFlowSharpInitialized;
            fss.ContentResolver += OnContentResolver;
            fss.NewCanvas += OnNewCanvas;
            ces.TextChanged += OnCSharpEditorServiceTextChanged;
            ses.TextChanged += OnScintillaEditorServiceTextChanged;
            InitializeBuildMenu();
            InitializeEditorsMenu();
        }

        public void OutputWindowClosed()
        {
            mnuOutput.Checked = false;
        }

        public void EditorWindowClosed(string language)
        {
            // TODO: Fix this hardcoded language name and generalize with how editors are handled.
            if (language == LANGUAGE_CSHARP)
            {
                mnuCSharp.Checked = false;
            }
            else
            {
                // TODO: Fix this hardcoded language name and generalize with how editors are handled.
                switch (language.ToLower())
                {
                    case "python":
                        mnuPython.Checked = false;
                        break;

                    case "javascript":
                        mnuJavascript.Checked = false;
                        break;

                    case "html":
                        mnuHtml.Checked = false;
                        break;

                    case "css":
                        mnuCss.Checked = false;
                        break;
                }
            }
        }

        protected void InitializeBuildMenu()
        {
            ToolStripMenuItem buildToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem mnuCompile = new ToolStripMenuItem();
            ToolStripMenuItem mnuRun = new ToolStripMenuItem();
            ToolStripMenuItem mnuStop = new ToolStripMenuItem();

            mnuCompile.Name = "mnuCompile";
            mnuCompile.ShortcutKeys = Keys.Alt | Keys.C;
            mnuCompile.Size = new System.Drawing.Size(165, 24);
            mnuCompile.Text = "&Compile";

            mnuRun.Name = "mnuRun";
            mnuRun.ShortcutKeys = Keys.Alt | Keys.R;
            mnuRun.Size = new System.Drawing.Size(165, 24);
            mnuRun.Text = "&Run";

            mnuStop.Name = "mnuStop";
            mnuStop.ShortcutKeys = Keys.Alt | Keys.S;
            // mnuStop.ShortcutKeys = Keys.Alt | Keys.R;
            mnuStop.Size = new System.Drawing.Size(165, 24);
            mnuStop.Text = "&Stop";

            buildToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {mnuCompile, mnuRun, mnuStop});
            buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            buildToolStripMenuItem.Size = new System.Drawing.Size(37, 21);
            buildToolStripMenuItem.Text = "Bu&ild";

            mnuCompile.Click += OnCompile;
            mnuRun.Click += OnRun;
            mnuStop.Click += OnStop;

            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(buildToolStripMenuItem);
        }

        protected void InitializeEditorsMenu()
        {
            // TODO: Make this declarative, and put lexer configuration into an XML file or something.
            ToolStripMenuItem editorsToolStripMenuItem = new ToolStripMenuItem() { Name = "mnuEditors", Text = "Editors" };
            editorsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuCSharp, mnuPython, mnuJavascript, mnuHtml, mnuCss, new ToolStripSeparator(), mnuOutput });
            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(editorsToolStripMenuItem);

            mnuCSharp.Click += OnCreateCSharpEditor;
            mnuPython.Click += OnCreatePythonEditor;
            mnuJavascript.Click += OnCreateJavascriptEditor;
            mnuHtml.Click += OnCreateHtmlEditor;
            mnuCss.Click += OnCreateCssEditor;
            mnuOutput.Click += OnCreateOutputWindow;

            // mnuCSharp.Checked = true;

            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(editorsToolStripMenuItem);
        }

        private void OnCreateCSharpEditor(object sender, EventArgs e)
        {
            mnuCSharp.Checked.Else(() => CreateCSharpEditor());
            mnuCSharp.Checked = true;
        }

        private void OnCreatePythonEditor(object sender, EventArgs e)
        {
            mnuPython.Checked.Else(() => CreateEditor(LANGUAGE_PYTHON));
            mnuPython.Checked = true;
        }

        private void OnCreateJavascriptEditor(object sender, EventArgs e)
        {
            mnuJavascript.Checked.Else(() => CreateEditor(LANGUAGE_JAVASCRIPT));
            mnuJavascript.Checked = true;
        }

        private void OnCreateHtmlEditor(object sender, EventArgs e)
        {
            mnuHtml.Checked.Else(() => CreateEditor(LANGUAGE_HTML));
            mnuHtml.Checked = true;
        }

        private void OnCreateCssEditor(object sender, EventArgs e)
        {
            mnuCss.Checked.Else(() => CreateEditor(LANGUAGE_CSS));
            mnuCss.Checked = true;
        }

        private void OnCreateOutputWindow(object sender, EventArgs e)
        {
            mnuOutput.Checked.Else(() => CreateOutputWindow());
            mnuOutput.Checked = true;
        }

        protected void OnCompile(object sender, EventArgs e)
        {
            ServiceManager.Get<IFlowSharpCodeCompilerService>().Compile();
        }

        protected void OnRun(object sender, EventArgs e)
        {
            ServiceManager.Get<IFlowSharpCodeCompilerService>().Run();
        }

        protected void OnStop(object sender, EventArgs e)
        {
            ServiceManager.Get<IFlowSharpCodeCompilerService>().Stop();
        }

        protected void OnFlowSharpInitialized(object sender, EventArgs args)
        {
             //IDockDocument csEditor = CreateCSharpEditor();
             //CreateEditor("python");
             //CreateOutputWindow();

            // Select C# editor, as it's the first tab in the code editor panel.
            // ServiceManager.Get<IDockingFormService>().SetActiveDocument(csEditor);
        }

        Control csDocEditor;

        protected IDockDocument CreateCSharpEditor()
        {
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            // Panel dock = dockingService.DockPanel;
            Control d = FindDocument(dockingService, FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR);

            if (d == null)
            {
                Control docCanvas = FindDocument(dockingService, FlowSharpServiceInterfaces.Constants.META_CANVAS);

                if (docCanvas == null)
                {
                    csDocEditor = dockingService.CreateDocument(DockState.Document, "C# Editor", FlowSharpCodeServiceInterfaces.Constants.META_CSHARP_EDITOR);
                }
                else
                {
                    csDocEditor = dockingService.CreateDocument(docCanvas, DockAlignment.Bottom, "C# Editor", FlowSharpCodeServiceInterfaces.Constants.META_CSHARP_EDITOR, 0.50);
                }
            }
            else
            {
                csDocEditor = dockingService.CreateDocument(d, DockState.Document, "C# Editor", FlowSharpCodeServiceInterfaces.Constants.META_CSHARP_EDITOR);
            }

            Control pnlCsCodeEditor = new Panel() { Dock = DockStyle.Fill };
            csDocEditor.Controls.Add(pnlCsCodeEditor);

            IFlowSharpCodeEditorService csCodeEditorService = ServiceManager.Get<IFlowSharpCodeEditorService>();
            csCodeEditorService.CreateEditor(pnlCsCodeEditor);
            csCodeEditorService.AddAssembly("Clifton.Core.dll");

            return (IDockDocument)csDocEditor;
        }

        protected void CreateEditor(string language)
        {
            Control docEditor = null;
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            Control d = FindDocument(dockingService, FlowSharpCodeServiceInterfaces.Constants.META_CSHARP_EDITOR);

            if (d == null)
            {
                d = FindDocument(dockingService, FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR);

                if (d == null)
                {
                    d = FindDocument(dockingService, FlowSharpServiceInterfaces.Constants.META_CANVAS);

                    if (d == null)
                    {
                        docEditor = dockingService.CreateDocument(DockState.Document, language + " Editor", FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR);
                    }
                    else
                    {
                        docEditor = dockingService.CreateDocument(d, DockAlignment.Bottom, language + " Editor", FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR, 0.50);
                    }
                }
                else
                {
                    docEditor = dockingService.CreateDocument(d, DockState.Document, language + " Editor", FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR);
                }
            }
            else
            {
                docEditor = dockingService.CreateDocument(d, DockState.Document, language + " Editor", FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR);
            }

            // Panel dock = dockingService.DockPanel;
            // Interestingly, this uses the current document page, which, I guess because the C# editor was created first, means its using that pane.
            //Control pyDocEditor = dockingService.CreateDocument(DockState.Document, "Python Editor", FlowSharpCodeServiceInterfaces.Constants.META_PYTHON_EDITOR);
            Control pnlCodeEditor = new Panel() { Dock = DockStyle.Fill, Tag = language };
            docEditor.Controls.Add(pnlCodeEditor);

            IFlowSharpScintillaEditorService scintillaEditorService = ServiceManager.Get<IFlowSharpScintillaEditorService>();
            scintillaEditorService.CreateEditor(pnlCodeEditor, language);
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
                case FlowSharpCodeServiceInterfaces.Constants.META_CSHARP_EDITOR:
                    Panel pnlEditor = new Panel() { Dock = DockStyle.Fill, Tag = FlowSharpCodeServiceInterfaces.Constants.META_CSHARP_EDITOR};
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
            IFlowSharpScintillaEditorService editorService = ServiceManager.Get<IFlowSharpScintillaEditorService>();

            if (args.Element != null)
            {
                GraphicElement el = args.Element;
                elementProperties = el.CreateProperties();

                string code;
                el.Json.TryGetValue("Code", out code);
                csCodeEditorService.SetText("C#", code ?? String.Empty);

                el.Json.TryGetValue("Python", out code);
                editorService.SetText("python", code ?? String.Empty);
            }
            else
            {
                csCodeEditorService.SetText("C#", String.Empty);
                editorService.SetText("python", String.Empty);
            }
        }

        // TODO: We want to be able to associate many different code types with the same shape.
        // This requires setting the Json dictionary appropriately for whatever editor is generating the event.
        // Are we doing this the right why?

        protected void OnCSharpEditorServiceTextChanged(object sender, TextChangedEventArgs e)
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            if (canvasService.ActiveController.SelectedElements.Count == 1)
            {
                GraphicElement el = canvasService.ActiveController.SelectedElements[0];
                el.Json["Code"] = e.Text;           // Should we call this C# or CSharp?
            }
        }

        protected void OnScintillaEditorServiceTextChanged(object sender, TextChangedEventArgs e)
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            if (canvasService.ActiveController.SelectedElements.Count == 1)
            {
                GraphicElement el = canvasService.ActiveController.SelectedElements[0];
                el.Json[e.Language] = e.Text;         // TODO: Should we call this Script or something else?
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
