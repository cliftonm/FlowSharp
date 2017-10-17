using System;
using System.Diagnostics;
using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

using FlowSharpCodeShapeInterfaces;
using FlowSharpLib;

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

        GraphicElement FindStartOfWorkflow(BaseController canvasController, GraphicElement wf);
        GraphicElement GetTruePathFirstShape(IIfBox el);
        GraphicElement GetFalsePathFirstShape(IIfBox el);
        GraphicElement NextElementInWorkflow(GraphicElement el);

        Process LaunchProcess(string processName, string arguments, Action<string> onOutput, Action<string> onError = null);
        void LaunchProcessAndWaitForExit(string processName, string arguments, Action<string> onOutput, Action<string> onError = null);
        void TerminateProcess(Process p);

        GraphicElement ParseDrakonWorkflow(DrakonCodeTree dcg, IFlowSharpCodeService codeService, BaseController canvasController, GraphicElement el, bool inCondition = false);
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

    public interface IFlowSharpCodePythonCompilerService : IService
    {
        void Compile();
    }

    public interface IFlowSharpCodeEditorServiceBase : IService
    {
        event EventHandler<TextChangedEventArgs> TextChanged;

        void SetText(string language, string text);
    }

    public interface IFlowSharpCodeEditorService : IFlowSharpCodeEditorServiceBase
    {
        void CreateEditor(Control parent);
        void AddAssembly(string filename);
        void AddAssembly(Type t);
    }

    public interface IFlowSharpScintillaEditorService : IFlowSharpCodeEditorServiceBase
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
