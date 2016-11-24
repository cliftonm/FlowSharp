using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

using FlowSharpLib;

namespace FlowSharpServiceInterfaces
{
    public interface IFlowSharpCanvasService : IService
    {
        // MouseController MouseController { get; }
        BaseController Controller { get; }

        void CreateCanvas(Control parent);
    }

    public interface IFlowSharpToolboxService : IService
    {
        void CreateToolbox(Control parent, BaseController canvasController);
    }

    public interface IFlowSharpMouseControllerService : IService
    {
        void ClearState();
        void ShapeDeleted(GraphicElement el);
    }
}
