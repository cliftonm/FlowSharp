/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

using FlowSharpLib;

namespace FlowSharpServiceInterfaces
{
    public interface IFlowSharpCanvasService : IService
    {
        event EventHandler<EventArgs> AddCanvas;

        BaseController ActiveController { get; }

        void CreateCanvas(Control parent);
        void SetActiveController(Control parent);
        void RequestNewCanvas();
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
        void Initialize(BaseController controller);
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
        void Initialize(BaseController controller);
        bool SaveOrSaveAs();
    }

    public interface IFlowSharpEditService : IService
    {
        void Copy();
        void Paste();
        void Delete();
        void Undo();
        void Redo();
        void EditText();
        ClosingState CheckForChanges();
        void ResetSavePoint();
        void SetSavePoint();
        bool ProcessCmdKey(Keys keyData);
    }

    public interface IFlowSharpDebugWindowService : IService
    {
        void Initialize(BaseController canvasController);
        void ShowDebugWindow();
        void EditPlugins();     // TODO: Not really a debug window!
        void UpdateDebugWindow();
    }
}
