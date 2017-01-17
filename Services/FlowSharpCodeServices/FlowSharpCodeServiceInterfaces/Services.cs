using System;
using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

namespace FlowSharpCodeServiceInterfaces
{
    public class TextChangedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }

    /// <summary>
    /// Master controller for FlowSharpCode
    /// </summary>
    public interface IFlowSharpCodeService : IService
    {
    }

    public interface IFlowSharpCodeOutputWindowService : IService
    {
        void CreateOutputWindow();
        void CreateOutputWindow(Control parent);
        void WriteLine(string line);
        void Clear();
        void Closed();
    }

    public interface IFlowSharpCodeCompilerService : IService
    {
        void Compile();
        void Run();
    }

    public interface IFlowSharpCodeEditorService : IService
    {
        event EventHandler<TextChangedEventArgs> TextChanged;

        void CreateEditor(Control parent);
        void AddAssembly(string filename);
        void AddAssembly(Type t);

        void SetText(string text);
    }

    //public interface IPythonCodeEditorService : IService
    //{
    //    event EventHandler<TextChangedEventArgs> TextChanged;

    //    void CreateEditor(Control parent);
    //    void SetText(string text);
    //}
}
