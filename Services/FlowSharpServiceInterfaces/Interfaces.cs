using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

using FlowSharpLib;

namespace FlowSharpServiceInterfaces
{
    public static class Constants
    {
        public const string PLUGIN_FILE_LIST = "plugins.txt";
    }

    public interface IFlowSharpCanvasService : IService
    {
        BaseController Controller { get; }

        void CreateCanvas(Control parent);
    }

    public interface IFlowSharpToolboxService : IService
    {
        BaseController Controller { get; }

        void CreateToolbox(Control parent);
        void ResetDisplacement();
        void InitializeToolbox();
        void InitializePluginsInToolbox();
        void UpdateToolboxPaths();
    }

    public interface IFlowSharpMouseControllerService : IService
    {
        void ClearState();
        void ShapeDeleted(GraphicElement el);
    }

    public interface IFlowSharpPropertyGridService : IService
    {
        void Initialize(PropertyGrid propertyGrid);
    }
}
