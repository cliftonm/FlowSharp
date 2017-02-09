using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using ScintillaNET;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpServiceInterfaces;
using FlowSharpCodeServiceInterfaces;

namespace FlowSharpCodeScintillaEditorService
{
    public class CodeEditorModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpScintillaEditorService, ScintillaCodeEditorService>();
        }
    }

    public class ScintillaCodeEditorReceptor : IReceptor
    {
    }

    public abstract class ScintillaEditor : Scintilla
    {
        public string Language { get; set; }
        public Control ContainerParent { get; set; }

        public abstract void ConfigureLexer();
    }

    public class ScintillaCodeEditorService : ServiceBase, IFlowSharpScintillaEditorService
    {
        public event EventHandler<TextChangedEventArgs> TextChanged;

        // Only one editor per language is allowed.
        // TODO: How would we handle multiple editors of the same language, associated with two or more shapes?
        protected Dictionary<string, ScintillaEditor> editors;      

        public ScintillaCodeEditorService()
        {
            editors = new Dictionary<string, ScintillaEditor>();
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            IDockingFormService dockingService = ServiceManager.Get<IDockingFormService>();
            dockingService.DocumentClosing += (sndr, args) => OnDocumentClosing(sndr);
        }

        public void CreateEditor(Control parent, string language)
        {
            ScintillaEditor editor = null;

            switch (language.ToLower())
            {
                case "python":
                    editor = CreateEditor<PythonEditor>(parent);
                    break;

                case "javascript":
                    editor = CreateEditor<JavascriptEditor>(parent);
                    break;

                case "html":
                    editor = CreateEditor<HtmlEditor>(parent);
                    break;

                case "css":
                    editor = CreateEditor<CssEditor>(parent);
                    break;                
            }

            editor.Language = language.ToLower();
            editors[editor.Language] = editor;
        }

        public void SetText(string language, string text)
        {
            // TODO: Set the focus to the appropriate editor.
            // If the editor doesn't exist, create it.

            ScintillaEditor editor;

            if (editors.TryGetValue(language.ToLower(), out editor))
            {
                editor.Text = text;
            }
        }

        protected ScintillaEditor CreateEditor<T>(Control parent) where T: ScintillaEditor, new()
        {
            ScintillaEditor editor = new T();
            editor.Margins[0].Width = 16;
            editor.Dock = DockStyle.Fill;
            editor.Lexer = Lexer.Python;
            editor.ConfigureLexer();
            editor.TextChanged += OnTextChanged;
            parent.Controls.Add(editor);
            editor.ContainerParent = parent;

            return editor;
        }

        protected void OnTextChanged(object sender, EventArgs e)
        {
            ScintillaEditor editor = (ScintillaEditor)sender;
            TextChanged.Fire(this, new TextChangedEventArgs() { Language = editor.Language, Text = editor.Text });
        }

        protected void Closed(string language)
        {
            ScintillaEditor editor;

            if (editors.TryGetValue(language.ToLower(), out editor))
            {
                editor.ContainerParent.Controls.Remove(editor);
                editors.Remove(language.ToLower());
                ServiceManager.Get<IFlowSharpCodeService>().EditorWindowClosed(language);
            }
        }

        protected void OnDocumentClosing(object document)
        {
            Control ctrl = document as Control;

            if ((ctrl != null && ctrl.Controls.Count == 1) && 
                (((IDockDocument)document).Metadata.LeftOf(",") == FlowSharpCodeServiceInterfaces.Constants.META_SCINTILLA_EDITOR))
            {
                string language = ((ctrl.Controls[0].Controls[0]) as ScintillaEditor).Language;
                Closed(language);
            }
        }
    }
    public class JavascriptEditor : ScintillaEditor
    {
        public override void ConfigureLexer()
        {
        }
    }

    public class HtmlEditor : ScintillaEditor
    {
        public override void ConfigureLexer()
        {
        }
    }

    public class CssEditor : ScintillaEditor
    {
        public override void ConfigureLexer()
        {
        }
    }

    public class PythonEditor : ScintillaEditor
    {
        public override void ConfigureLexer()
        {
            // Reset the styles
            StyleResetDefault();
            Styles[Style.Default].Font = "Consolas";
            Styles[Style.Default].Size = 10;
            StyleClearAll(); // i.e. Apply to all

            // Set the lexer
            Lexer = Lexer.Python;
            IndentWidth = 2;

            // Known lexer properties:
            // "tab.timmy.whinge.level",
            // "lexer.python.literals.binary",
            // "lexer.python.strings.u",
            // "lexer.python.strings.b",
            // "lexer.python.strings.over.newline",
            // "lexer.python.keywords2.no.sub.identifiers",
            // "fold.quotes.python",
            // "fold.compact",
            // "fold"

            // Some properties we like
            SetProperty("tab.timmy.whinge.level", "1");
            SetProperty("fold", "1");

            // Use margin 2 for fold markers
            Margins[2].Type = MarginType.Symbol;
            Margins[2].Mask = Marker.MaskFolders;
            Margins[2].Sensitive = true;
            Margins[2].Width = 20;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                Markers[i].SetForeColor(SystemColors.ControlLightLight);
                Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Style the folder markers
            Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
            Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
            Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

            // Set the styles
            Styles[Style.Python.Default].ForeColor = Color.FromArgb(0x80, 0x80, 0x80);
            Styles[Style.Python.CommentLine].ForeColor = Color.FromArgb(0x00, 0x7F, 0x00);
            Styles[Style.Python.CommentLine].Italic = true;
            Styles[Style.Python.Number].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            Styles[Style.Python.String].ForeColor = Color.FromArgb(0x7F, 0x00, 0x7F);
            Styles[Style.Python.Character].ForeColor = Color.FromArgb(0x7F, 0x00, 0x7F);
            Styles[Style.Python.Word].ForeColor = Color.FromArgb(0x00, 0x00, 0x7F);
            Styles[Style.Python.Word].Bold = true;
            Styles[Style.Python.Triple].ForeColor = Color.FromArgb(0x7F, 0x00, 0x00);
            Styles[Style.Python.TripleDouble].ForeColor = Color.FromArgb(0x7F, 0x00, 0x00);
            Styles[Style.Python.ClassName].ForeColor = Color.FromArgb(0x00, 0x00, 0xFF);
            Styles[Style.Python.ClassName].Bold = true;
            Styles[Style.Python.DefName].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            Styles[Style.Python.DefName].Bold = true;
            Styles[Style.Python.Operator].Bold = true;
            // Styles[Style.Python.Identifier] ... your keywords styled here
            Styles[Style.Python.CommentBlock].ForeColor = Color.FromArgb(0x7F, 0x7F, 0x7F);
            Styles[Style.Python.CommentBlock].Italic = true;
            Styles[Style.Python.StringEol].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
            Styles[Style.Python.StringEol].BackColor = Color.FromArgb(0xE0, 0xC0, 0xE0);
            Styles[Style.Python.StringEol].FillLine = true;
            Styles[Style.Python.Word2].ForeColor = Color.FromArgb(0x40, 0x70, 0x90);
            Styles[Style.Python.Decorator].ForeColor = Color.FromArgb(0x80, 0x50, 0x00);

            // Important for Python
            ViewWhitespace = WhitespaceMode.VisibleAlways;

            // Keyword lists:
            // 0 "Keywords",
            // 1 "Highlighted identifiers"

            var python2 = "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield";
            // var python3 = "False None True and as assert break class continue def del elif else except finally for from global if import in is lambda nonlocal not or pass raise return try while with yield";
            var cython = "cdef cimport cpdef";

            SetKeywords(0, python2 + " " + cython);
        }
    }
}
