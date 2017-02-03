using System;
using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

namespace FlowSharpCodeServiceInterfaces
{
    public class TextChangedEventArgs : EventArgs
    {
        public string Language { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// Master controller for FlowSharpCode
    /// </summary>
    public interface IFlowSharpCodeService : IService
    {
        void OutputWindowClosed();
        void EditorWindowClosed(string language);
    }

    public interface IFlowSharpCodeOutputWindowService : IService
    {
        void CreateOutputWindow();
        void CreateOutputWindow(Control parent);
        void Write(string text);
        void WriteLine(string line);
        void Clear();
    }

    public interface IFlowSharpCodeCompilerService : IService
    {
        void Compile();
        void Run();
        void Stop();
    }

    public interface IFlowSharpCodeEditor
    {
        event EventHandler<TextChangedEventArgs> TextChanged;

        void SetText(string language, string text);
    }

    public interface IFlowSharpCodeEditorService : IFlowSharpCodeEditor, IService
    {
        void CreateEditor(Control parent);
        void AddAssembly(string filename);
        void AddAssembly(Type t);
    }

    public interface IFlowSharpScintillaEditorService : IFlowSharpCodeEditor, IService
    {
        void CreateEditor(Control parent, string language);
        // void CreateEditor<T>(Control parent) where T : IGenericEditor, new();
    }

    //public interface IPythonCodeEditorService : IService
    //{
    //    event EventHandler<TextChangedEventArgs> TextChanged;

    //    void CreateEditor(Control parent);
    //    void SetText(string text);
    //}
}
