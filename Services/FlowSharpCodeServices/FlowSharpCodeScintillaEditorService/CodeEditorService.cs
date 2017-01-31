using System;
using System.Drawing;
using System.Windows.Forms;

using ScintillaNET;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

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

    public class ScintillaCodeEditorService : ServiceBase, IFlowSharpScintillaEditorService
    {
        public event EventHandler<TextChangedEventArgs> TextChanged;

        protected Scintilla editor;

        public void CreateEditor(Control parent)
        {
            editor = new Scintilla();
            editor.Margins[0].Width = 16;
            editor.Dock = DockStyle.Fill;
            editor.Lexer = Lexer.Python;
            ConfigureLexer(editor);
            editor.TextChanged += OnTextChanged;
            // ConfigureLexer(editor);
            parent.Controls.Add(editor);
        }

        public void SetText(string text)
        {
            editor.Text = text;
        }

        protected void OnTextChanged(object sender, EventArgs e)
        {
            TextChanged.Fire(this, new TextChangedEventArgs() { Text = editor.Text });
        }

        protected void ConfigureLexer(Scintilla editor)
        {
            // Reset the styles
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 10;
            editor.StyleClearAll(); // i.e. Apply to all

            // Set the lexer
            editor.Lexer = Lexer.Python;

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
            editor.SetProperty("tab.timmy.whinge.level", "1");
            editor.SetProperty("fold", "1");

            // Use margin 2 for fold markers
            editor.Margins[2].Type = MarginType.Symbol;
            editor.Margins[2].Mask = Marker.MaskFolders;
            editor.Margins[2].Sensitive = true;
            editor.Margins[2].Width = 20;

            // Reset folder markers
            for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
            {
                editor.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                editor.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Style the folder markers
            editor.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            editor.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
            editor.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            editor.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            editor.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
            editor.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            editor.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            editor.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            editor.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            editor.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

            // Set the styles
            editor.Styles[Style.Python.Default].ForeColor = Color.FromArgb(0x80, 0x80, 0x80);
            editor.Styles[Style.Python.CommentLine].ForeColor = Color.FromArgb(0x00, 0x7F, 0x00);
            editor.Styles[Style.Python.CommentLine].Italic = true;
            editor.Styles[Style.Python.Number].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            editor.Styles[Style.Python.String].ForeColor = Color.FromArgb(0x7F, 0x00, 0x7F);
            editor.Styles[Style.Python.Character].ForeColor = Color.FromArgb(0x7F, 0x00, 0x7F);
            editor.Styles[Style.Python.Word].ForeColor = Color.FromArgb(0x00, 0x00, 0x7F);
            editor.Styles[Style.Python.Word].Bold = true;
            editor.Styles[Style.Python.Triple].ForeColor = Color.FromArgb(0x7F, 0x00, 0x00);
            editor.Styles[Style.Python.TripleDouble].ForeColor = Color.FromArgb(0x7F, 0x00, 0x00);
            editor.Styles[Style.Python.ClassName].ForeColor = Color.FromArgb(0x00, 0x00, 0xFF);
            editor.Styles[Style.Python.ClassName].Bold = true;
            editor.Styles[Style.Python.DefName].ForeColor = Color.FromArgb(0x00, 0x7F, 0x7F);
            editor.Styles[Style.Python.DefName].Bold = true;
            editor.Styles[Style.Python.Operator].Bold = true;
            // editor.Styles[Style.Python.Identifier] ... your keywords styled here
            editor.Styles[Style.Python.CommentBlock].ForeColor = Color.FromArgb(0x7F, 0x7F, 0x7F);
            editor.Styles[Style.Python.CommentBlock].Italic = true;
            editor.Styles[Style.Python.StringEol].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
            editor.Styles[Style.Python.StringEol].BackColor = Color.FromArgb(0xE0, 0xC0, 0xE0);
            editor.Styles[Style.Python.StringEol].FillLine = true;
            editor.Styles[Style.Python.Word2].ForeColor = Color.FromArgb(0x40, 0x70, 0x90);
            editor.Styles[Style.Python.Decorator].ForeColor = Color.FromArgb(0x80, 0x50, 0x00);

            // Important for Python
            editor.ViewWhitespace = WhitespaceMode.VisibleAlways;

            // Keyword lists:
            // 0 "Keywords",
            // 1 "Highlighted identifiers"

            var python2 = "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield";
            // var python3 = "False None True and as assert break class continue def del elif else except finally for from global if import in is lambda nonlocal not or pass raise return try while with yield";
            var cython = "cdef cimport cpdef";

            editor.SetKeywords(0, python2 + " " + cython);
        }
    }
}
