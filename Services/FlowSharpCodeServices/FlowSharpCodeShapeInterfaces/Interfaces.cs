/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

namespace FlowSharpCodeShapeInterfaces
{
    public enum TruePath
    {
        Down,
        LeftOrRight
    }

    public interface IFileBox
    {
        string Filename { get; }
    }

    public interface IAssemblyBox : IFileBox
    {
    }

    public interface IAssemblyReferenceBox : IFileBox
    {
    }

    public interface IPythonClass : IFileBox
    {
    }

    public interface IWorkflowBox
    {
    }

    // Interfaces are required and must be separate from FlowSharpCodeDrakonShapes because the shapes are dynamically loaded,
    // as a result, statements like "el is IDrakonShape" returns false if the interface is defined in FlowSharpCodeDrakonShapes.  
    // Crazily, the debugger returns true in this situation.
    public interface IDrakonShape { }

    // We don't name these "Drakon" so that we could associate other kinds of flowcharting shapes with these statements.
    public interface IIfBox
    {
        TruePath TruePath { get; set; }
    }

    public interface IBeginForLoopBox { }
    public interface IEndForLoopBox { }
    public interface IOutputBox { }
    public interface IInputBox { }
}
