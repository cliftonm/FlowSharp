/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

using FlowSharpLib;

namespace FlowSharpServiceInterfaces
{
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

    public interface IFlowSharpMenuService : IService
    {
        void Initialize(Form mainForm);
    }

    public interface IFlowSharpEditService : IService
    {
        void Copy();
        void Paste();
        void Delete();
        void Undo();
        void Redo();
    }
}
