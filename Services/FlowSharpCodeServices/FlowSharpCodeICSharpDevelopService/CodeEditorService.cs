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

        public void CreateEditor(Control parent)
        {
            ElementHost host = new ElementHost();
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
        }

        public void AddAssembly(string filename)
        {
            completion.AddAssembly(filename);
        }

        public void AddAssembly(Type t)
        {
            completion.AddAssembly(t.Assembly.Location);
        }

        public void SetText(string text)
        {
            editor.Text = text;
        }

        protected void OnTextChanged(object sender, EventArgs e)
        {
            TextChanged.Fire(this, new TextChangedEventArgs() { Text = editor.Text });
        }
    }

    public class CsCodeEditorReceptor : IReceptor
    {
    }
}
