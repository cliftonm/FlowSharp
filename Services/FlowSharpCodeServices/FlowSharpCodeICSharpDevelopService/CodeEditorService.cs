using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Document;
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
        public string Filename { get; set; }

        protected CodeTextEditor editor;
        protected CSharpCompletion completion;
        protected Control parent;
        protected ElementHost host;
        protected int lastCaretPosition;

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
            editor.LostFocus += OnLostFocus;

            // Very important!  The code completer throws exceptions if the document does not have a filename.
            editor.Document.FileName = "foo.cs";

            host.Child = editor;
            parent.Controls.Add(host);
            this.parent = parent;
        }

        /// <summary>
        /// We have to preserve the last caret position on lost focus because once the focus has been
        /// lost, for some reason the CaretOffset returns to 0!  This would then result in FlowSharpCodeService.cs
        /// always getting a 0 for the last known caret when another shape is selected, and we want to 
        /// be able to restore the last position when we re-select the element with this code.
        /// </summary>
        private void OnLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            lastCaretPosition = editor.CaretOffset;
        }

        public int GetPosition()
        {
            return lastCaretPosition;
        }

        public void SetPosition(int pos)
        {
            lastCaretPosition = pos;
            editor.CaretOffset = pos;
            DocumentLine docLine = editor.Document.GetLineByOffset(pos);
            editor.ScrollToLine(docLine.LineNumber);
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
