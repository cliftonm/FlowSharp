using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowSharpCodeShapeInterfaces
{
    public interface IAssemblyBox
    {
        string Filename { get; }
    }

    public interface IFileBox
    {
        string Filename { get; }
    }

    public interface IWorkflowBox
    {
    }
}
