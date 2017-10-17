/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Clifton.Core.ServiceManagement;
using Clifton.WinForm.ServiceInterfaces;

using FlowSharpLib;

namespace FlowSharpServiceInterfaces
{
    public interface IFlowSharpForm
    {
        IServiceManager ServiceManager { get; set; }
    }

    /// <summary>
    /// The master controller for FlowSharp
    /// </summary>
    public interface IFlowSharpService : IService
    {
        event EventHandler<ContentLoadedEventArgs> ContentResolver;
        event EventHandler<EventArgs> FlowSharpInitialized;
        event EventHandler<NewCanvasEventArgs> NewCanvas;

        Form CreateDockingForm(Icon icon);
    }

    public interface IFlowSharpCanvasService : IService
    {
        event EventHandler<EventArgs> AddCanvas;
        event EventHandler<FileEventArgs> LoadLayout;
        event EventHandler<FileEventArgs> SaveLayout;

        BaseController ActiveController { get; }
        List<BaseController> Controllers { get; }

        void CreateCanvas(Control parent);
        void DeleteCanvas(Control parent);
        void SetActiveController(Control parent);
        void RequestNewCanvas();
        void LoadDiagrams(string filename);
        void SaveDiagramsAndLayout(string filename, bool selectionOnly = false);
        void ClearControllers();
    }

    public interface IFlowSharpToolboxService : IService
    {
        BaseController Controller { get; }

        void CreateToolbox(Control parent);
        void ResetDisplacement();
        void InitializeToolbox();
        void InitializePluginsInToolbox();
        void UpdateToolboxPaths();

        List<Type> ShapeList { get; }
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
        void Initialize(BaseController controller);
        void Terminate(BaseController controller);
        void ShowProperties(IPropertyObject propObject);        // Really used for showing the canvas properties.
    }

    public interface IFlowSharpMenuService : IService
    {
        string Filename { get; }

        void Initialize(Form mainForm);
        void Initialize(BaseController controller);
        bool SaveOrSaveAs();
        void UpdateMenu();
        void AddMenu(ToolStripMenuItem menuItem);
        void EnableCopyPasteDel(bool state);
    }

    public interface IFlowSharpEditService : IService
    {
        void NewCanvas(BaseController controller);
        void Copy();
        void Paste();
        void Delete();
        void Undo();
        void Redo();
        void EditText();
        ClosingState CheckForChanges();
        void ResetSavePoint();
        void ClearSavePoints();
        void SetSavePoint();
        bool ProcessCmdKey(Keys keyData);
        void FocusOnShape(GraphicElement el);
    }

    public interface IFlowSharpDebugWindowService : IService
    {
        void Initialize(BaseController canvasController);
        void ShowDebugWindow();
        void EditPlugins();     // TODO: Not really a debug window!
        void UpdateDebugWindow(); // Updates both shape tree and stack trace.
        void UpdateShapeTree();
        void UpdateStackTrace();
        void FindShape(GraphicElement shape);
    }

    public interface IFlowSharpRestService : IService
    {
        string HttpGet(string url, string data);
        string HttpGet(string url, Dictionary<string, string> data);
    }

    public interface IFlowSharpWebSocketService : IService { }

    public interface IHasResponse
    {
        string SerializeResponse();
    }
}
