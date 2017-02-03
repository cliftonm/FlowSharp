using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.CodeCompletion;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpCodeServiceInterfaces;

namespace FlowSharpCodeICSharpDevelopService
{
    public class CodeEditorModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCodeEditorService, CsCodeEditorService>();
        }
    }

    public class CsCodeEditorService : ServiceBase, IFlowSharpCodeEditorService
    {
        public event EventHandler<TextChangedEventArgs> TextChanged;

        protected CodeTextEditor editor;
        protected CSharpCompletion completion;
        protected Control parent;
        protected ElementHost host;

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            dockingService.DocumentClosing += (sndr, args) => OnDocumentClosing(sndr);
        }

        public void CreateEditor(Control parent)
        {
            host = new ElementHost();
            host.Dock = DockStyle.Fill;

            completion = new CSharpCompletion(new ScriptProvider());

            editor = new CodeTextEditor();
            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 12;
            editor.Completion = completion;
            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            editor.TextChanged += OnTextChanged;

            // Very important!  The code completer throws exceptions if the document does not have a filename.
            editor.Document.FileName = "foo.cs";

            host.Child = editor;
            parent.Controls.Add(host);
            this.parent = parent;
        }

        public void AddAssembly(string filename)
        {
            completion.AddAssembly(filename);
        }

        public void AddAssembly(Type t)
        {
            completion.AddAssembly(t.Assembly.Location);
        }

        public void SetText(string language, string text)
        {
            // We ignore language as the ICSharp editor is only used for C# at the moment.
            // Editor may not exist.
            if (editor != null)
            {
                editor.Text = text;
            }
        }

        protected void OnTextChanged(object sender, EventArgs e)
        {
            // TODO: Fix this hardcoded language name and generalize with how editors are handled.
            TextChanged.Fire(this, new TextChangedEventArgs() { Language = "C#", Text = editor.Text });
        }

        protected void Closed()
        {
            parent.Controls.Remove(host);
            host = null;
            editor = null;
            // TODO: Fix this hardcoded language name and generalize with how editors are handled.
            ServiceManager.Get<IFlowSharpCodeService>().EditorWindowClosed("C#");
        }

        protected void OnDocumentClosing(object document)
        {
            Control ctrl = document as Control;

            if ((ctrl != null && ctrl.Controls.Count == 1) && ((IDockDocument)document).Metadata.LeftOf(",") == Constants.META_CSHARP_EDITOR)
            {
                Closed();
            }
        }
    }

    public class CsCodeEditorReceptor : IReceptor
    {
    }
}
