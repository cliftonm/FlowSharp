/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpMenuService
{
    public class NavigateToShape : IComparable
    {
        public GraphicElement Shape { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo(((NavigateToShape)obj).Name);
        }
    }

    public partial class MenuController
    {
        private const string MRU_FILENAME = "FlowSharp.mru";

        public string Filename { get { return filename; } }

        protected string filename;
        protected IServiceManager serviceManager;
        protected Form mainForm;
        protected List<string> mru;

        public MenuController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
            Initialize();
        }

        public void Initialize(Form mainForm)
        {
            this.mainForm = mainForm;
            mru = new List<string>();
            InitializeMenuHandlers();
            PopulateMostRecentFiles();
        }

        public void Initialize(BaseController canvasController)
        {
            canvasController.ElementSelected += (snd, args) => UpdateMenu(args.Element != null);
            canvasController.UndoStack.AfterAction += (snd, args) => UpdateMenu(canvasController.SelectedElements.Any());
            UpdateMenu(false);
        }

        // TODO: The save/load operations might be best moved to the edit service?
        public bool SaveOrSaveAs(bool forceSaveAs = false)
        {
            bool ret = true;

            if (String.IsNullOrEmpty(filename) || forceSaveAs)
            {
                ret = SaveAs();
            }
            else
            {
                SaveDiagram(filename);
            }

            return ret;
        }

        public void UpdateMenu(bool elementSelected)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            mnuBottommost.Enabled = elementSelected;
            mnuTopmost.Enabled = elementSelected;
            mnuMoveUp.Enabled = elementSelected;
            mnuMoveDown.Enabled = elementSelected;
            mnuCopy.Enabled = elementSelected;
            mnuDelete.Enabled = elementSelected;
            mnuGroup.Enabled = elementSelected && !canvasController.SelectedElements.Any(el => el.Parent != null);
            mnuUngroup.Enabled = canvasController.SelectedElements.Count == 1 && canvasController.SelectedElements[0].GroupChildren.Any();
            mnuUndo.Enabled = canvasController.UndoStack.CanUndo;
            mnuRedo.Enabled = canvasController.UndoStack.CanRedo;
        }

        /// <summary>
        /// Adds a top-level menu tree, appended to the end of the default menu strip items.
        /// </summary>
        public void AddMenu(ToolStripMenuItem menuItem)
        {
            menuStrip.Items.Add(menuItem);
        }

        protected void InitializeMenuHandlers()
        {
            mnuClearCanvas.Click += mnuNew_Click;
            mnuOpen.Click += mnuOpen_Click;
            mnuImport.Click += (sndr, args) =>
            {
                mnuImport_Click(sndr, args);
            };
            mnuSave.Click += mnuSave_Click;
            mnuSaveAs.Click += mnuSaveAs_Click;
            mnuExit.Click += mnuExit_Click;
            mnuCopy.Click += mnuCopy_Click;
            mnuPaste.Click += mnuPaste_Click;
            mnuDelete.Click += mnuDelete_Click;
            mnuTopmost.Click += mnuTopmost_Click;
            mnuBottommost.Click += mnuBottommost_Click;
            mnuMoveUp.Click += mnuMoveUp_Click;
            mnuMoveDown.Click += mnuMoveDown_Click;
            mnuGroup.Click += mnuGroup_Click;
            mnuUngroup.Click += mnuUngroup_Click;
            mnuUndo.Click += mnuUndo_Click;
            mnuRedo.Click += mnuRedo_Click;
            mnuEdit.Click += (sndr, args) => serviceManager.Get<IFlowSharpEditService>().EditText();
            mnuDebugWindow.Click += (sndr, args) => serviceManager.Get<IFlowSharpDebugWindowService>().ShowDebugWindow();
            mnuPlugins.Click += (sndr, args) => serviceManager.Get<IFlowSharpDebugWindowService>().EditPlugins();
            // mnuLoadLayout.Click += (sndr, args) => serviceManager.Get<IDockingFormService>().LoadLayout("layout.xml");
            // mnuSaveLayout.Click += (sndr, args) => serviceManager.Get<IDockingFormService>().SaveLayout("layout.xml");
            // TODO: Decouple dependency - see canvas controller
            // Instead, fire an event or publish on subscriber an action?
            mnuAddCanvas.Click += (sndr, args) => serviceManager.Get<IFlowSharpCanvasService>().RequestNewCanvas();

            mnuGoToShape.Click += GoToShape;
            mnuGoToBookmark.Click += GoToBookmark;
            mnuToggleBookmark.Click += ToogleBookmark;
            mnuClearBookmarks.Click += ClearBookmarks;

        }

        private void GoToShape(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            List<NavigateToShape> navShapes = canvasController.Elements.
                Where(el => !el.IsConnector).
                Select(el => new NavigateToShape() { Shape = el, Name = el.NavigateName }).
                OrderBy(s => s.Name).
                ToList();
            ShowNavigateDialog(canvasController, navShapes);
        }

        private void GoToBookmark(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            List<NavigateToShape> navShapes = canvasController.Elements.
                Where(el=>el.IsBookmarked).
                Select(el => new NavigateToShape() { Shape = el, Name = el.NavigateName }).
                OrderBy(s=>s).
                ToList();
            ShowNavigateDialog(canvasController, navShapes);
        }

        private void ToogleBookmark(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            canvasController.SelectedElements?.ForEach(el =>
            {
                el.ToggleBookmark();
                canvasController.Redraw(el);
            });
        }

        private void ClearBookmarks(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            canvasController.ClearBookmarks();
        }

        protected void ShowNavigateDialog(BaseController canvasController, List<NavigateToShape> navShapes)
        {
            new NavigateDlg(serviceManager, navShapes).ShowDialog();
        }

        protected void PopulateMostRecentFiles()
        {
            if (File.Exists(MRU_FILENAME))
            {
                mru = File.ReadAllLines(MRU_FILENAME).ToList();

                foreach (string f in mru)
                {
                    ToolStripItem tsi = new ToolStripMenuItem(f);
                    tsi.Click += OnRecentFileSelected;
                    mnuRecentFiles.DropDownItems.Add(tsi);
                }
            }
        }

        protected void UpdateMru(string filename)
        {
            // Any existing MRU, remove, and regardless, insert at beginning of list.
            mru.Remove(filename);
            mru.Insert(0, filename);
            File.WriteAllLines(MRU_FILENAME, mru);
        }

        private void OnRecentFileSelected(object sender, EventArgs e)
        {
            if (CheckForChanges()) return;
            ToolStripItem tsi = sender as ToolStripItem;
            filename = tsi.Text;
            IFlowSharpCanvasService canvasService = serviceManager.Get<IFlowSharpCanvasService>();
            canvasService.LoadDiagrams(filename);
            UpdateCaption();
        }

        private void mnuTopmost_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            List<ZOrderMap> originalZOrder = canvasController.GetZOrderOfSelectedElements();

            canvasController.UndoStack.UndoRedo("Z-Top",
                () =>
                {
                    canvasController.Topmost();
                },
                () =>
                {
                    canvasController.RestoreZOrder(originalZOrder);
                });
        }

        private void mnuBottommost_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            List<ZOrderMap> originalZOrder = canvasController.GetZOrderOfSelectedElements();

            canvasController.UndoStack.UndoRedo("Z-Bottom",
                () =>
                {
                    canvasController.Bottommost();
                },
                () =>
                {
                    canvasController.RestoreZOrder(originalZOrder);
                });
        }

        private void mnuMoveUp_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            List<ZOrderMap> originalZOrder = canvasController.GetZOrderOfSelectedElements();

            canvasController.UndoStack.UndoRedo("Z-Up",
                () =>
                {
                    canvasController.MoveSelectedElementsUp();
                },
                () =>
                {
                    canvasController.RestoreZOrder(originalZOrder);
                });
        }

        private void mnuMoveDown_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            List<ZOrderMap> originalZOrder = canvasController.GetZOrderOfSelectedElements();

            canvasController.UndoStack.UndoRedo("Z-Down",
                () =>
                {
                    canvasController.MoveSelectedElementsDown();
                },
                () =>
                {
                    canvasController.RestoreZOrder(originalZOrder);
                });
        }

        private void mnuCopy_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            if (canvasController.SelectedElements.Count > 0)
            {
                serviceManager.Get<IFlowSharpEditService>().Copy();
            }
        }

        private void mnuPaste_Click(object sender, EventArgs e)
        {
            serviceManager.Get<IFlowSharpEditService>().Paste();
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            serviceManager.Get<IFlowSharpEditService>().Delete();
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            if (CheckForChanges()) return;
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            serviceManager.Get<IFlowSharpEditService>().ResetSavePoint();
            canvasController.Clear();
            canvasController.UndoStack.ClearStacks();
            // ElementCache.Instance.ClearCache();
            serviceManager.Get<IFlowSharpMouseControllerService>().ClearState();
            canvasController.Canvas.Invalidate();
            filename = String.Empty;
            canvasController.Filename = String.Empty;
            UpdateCaption();
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            if (CheckForChanges()) return;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FlowSharp (*.fsd)|*.fsd";
            DialogResult res = ofd.ShowDialog();

            if (res == DialogResult.OK)
            {
                filename = ofd.FileName;
            }
            else
            {
                return;
            }

            IFlowSharpCanvasService canvasService = serviceManager.Get<IFlowSharpCanvasService>();
            canvasService.LoadDiagrams(filename);
            UpdateCaption();
            UpdateMru(filename);
        }

        private void mnuImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FlowSharp (*.fsd)|*.fsd";
            DialogResult res = ofd.ShowDialog();

            if (res == DialogResult.OK)
            {
                BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                string importFilename = ofd.FileName;
                string data = File.ReadAllText(importFilename);
                List<GraphicElement> els = Persist.Deserialize(canvasController.Canvas, data);
                List<GraphicElement> selectedElements = canvasController.SelectedElements.ToList();

                canvasController.UndoStack.UndoRedo("Import",
                    () =>
                    {
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.AddElements(els);
                        canvasController.Elements.ForEach(el => el.UpdatePath());
                        canvasController.SelectElements(els);
                        canvasController.Canvas.Invalidate();
                    },
                    () =>
                    {
                        canvasController.DeselectCurrentSelectedElements();
                        els.ForEach(el => canvasController.DeleteElement(el));
                        canvasController.SelectElements(selectedElements);
                    });
            }
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (canvasController.Elements.Count > 0)
            {
                SaveOrSaveAs();
                UpdateCaption();
            }
            else
            {
                MessageBox.Show("Nothing to save.", "Empty Canvas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (canvasController.Elements.Count > 0)
            {
                SaveOrSaveAs(true);
                UpdateCaption();
            }
            else
            {
                MessageBox.Show("Nothing to save.", "Empty Canvas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (CheckForChanges()) return;
            mainForm.Close();
        }

        private void mnuGroup_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (canvasController.SelectedElements.Any())
            {
                List<GraphicElement> selectedShapes = canvasController.SelectedElements.ToList();
                FlowSharpLib.GroupBox groupBox = new FlowSharpLib.GroupBox(canvasController.Canvas);

                canvasController.UndoStack.UndoRedo("Group",
                    () =>
                    {
                        // ElementCache.Instance.Remove(groupBox);
                        canvasController.GroupShapes(groupBox);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElement(groupBox);
                    },
                    () =>
                    {
                        // ElementCache.Instance.Add(groupBox);
                        canvasController.UngroupShapes(groupBox, false);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElements(selectedShapes);
                    });
            }
        }

        private void mnuUngroup_Click(object sender, EventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;

            // At this point, we can only ungroup one group.
            if (canvasController.SelectedElements.Count == 1)
            {
                FlowSharpLib.GroupBox groupBox = canvasController.SelectedElements[0] as FlowSharpLib.GroupBox;

                if (groupBox != null)
                {
                    List<GraphicElement> groupedShapes = new List<GraphicElement>(groupBox.GroupChildren);

                    canvasController.UndoStack.UndoRedo("Ungroup",
                    () =>
                    {
                        // ElementCache.Instance.Add(groupBox);
                        canvasController.UngroupShapes(groupBox, false);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElements(groupedShapes);
                    },
                    () =>
                    {
                        // ElementCache.Instance.Remove(groupBox);
                        canvasController.GroupShapes(groupBox);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElement(groupBox);
                    });
                }
            }
        }

        private void mnuUndo_Click(object sender, EventArgs e)
        {
            serviceManager.Get<IFlowSharpEditService>().Undo();
        }

        private void mnuRedo_Click(object sender, EventArgs e)
        {
            serviceManager.Get<IFlowSharpEditService>().Redo();
        }

        /// <summary>
        /// Return true if operation should be cancelled.
        /// </summary>
        protected bool CheckForChanges()
        {
            bool ret = true;

            ClosingState state = serviceManager.Get<IFlowSharpEditService>().CheckForChanges();

            if (state == ClosingState.SaveChanges)
            {
                ret = !SaveOrSaveAs();   // override because of possible cancel in save operation.
            }
            else if (state != ClosingState.CancelClose)
            {
                BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
                canvasController.UndoStack.ClearStacks();       // Prevents second "are you sure" when exiting with Ctrl+X
                ret = false;
            }

            return ret;
        }

        protected bool SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "FlowSharp (*.fsd)|*.fsd|PNG (*.png)|*.png";
            DialogResult res = sfd.ShowDialog();
            string ext = ".fsd";
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (res == DialogResult.OK)
            {
                ext = Path.GetExtension(sfd.FileName).ToLower();

                if (ext == ".png")
                {
                    canvasController.SaveAsPng(sfd.FileName);
                }
                else
                {
                    filename = sfd.FileName;
                    // Let canvas controller assign filenames.
                    SaveDiagram(filename);
                    UpdateCaption();
                    UpdateMru(filename);
                }
            }

            return res == DialogResult.OK && ext != ".png";
        }

        protected void SaveDiagram(string filename)
        {
            IFlowSharpCanvasService canvasService = serviceManager.Get<IFlowSharpCanvasService>();
            canvasService.SaveDiagramsAndLayout(filename);
            //BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            //string data = Persist.Serialize(canvasController.Elements);
            //File.WriteAllText(filename, data);
            serviceManager.Get<IFlowSharpEditService>().SetSavePoint();
        }

        protected void UpdateCaption()
        {
            mainForm.Text = "FlowSharp" + (String.IsNullOrEmpty(filename) ? "" : " - ") + filename;
        }
    }
}
