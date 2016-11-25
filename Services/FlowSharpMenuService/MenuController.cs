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

using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpMenuService
{
    public partial class MenuController
    {
        protected string filename;
        protected int savePoint = 0;
        protected BaseController canvasController;
        protected IServiceManager serviceManager;
        protected Form mainForm;

        public MenuController(BaseController canvasController, IServiceManager serviceManager, Form mainForm)
        {
            this.canvasController = canvasController;
            this.serviceManager = serviceManager;
            this.mainForm = mainForm;
            Initialize();
            InitializeMenuHandlers();
        }

        protected void InitializeMenuHandlers()
        {
            mnuNew.Click += mnuNew_Click;
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
            mnuPlugins.Click += mnuPlugins_Click;
            mnuUndo.Click += mnuUndo_Click;
            mnuRedo.Click += mnuRedo_Click;
            // TODO:
            // mnuEdit.Click += (sndr, args) => EditText();
        }

        private void mnuTopmost_Click(object sender, EventArgs e)
        {
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
            savePoint = 0;
            canvasController.Clear();
            canvasController.UndoStack.ClearStacks();
            ElementCache.Instance.ClearCache();
            serviceManager.Get<IFlowSharpMouseControllerService>().ClearState();
            canvasController.Canvas.Invalidate();
            filename = String.Empty;
            canvasController.Filename = filename;
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

            canvasController.Filename = filename;       // set now, in case of relative image files, etc...
            savePoint = 0;
            string data = File.ReadAllText(filename);
            List<GraphicElement> els = Persist.Deserialize(canvasController.Canvas, data);
            canvasController.Clear();
            canvasController.UndoStack.ClearStacks();
            ElementCache.Instance.ClearCache();
            serviceManager.Get<IFlowSharpMouseControllerService>().ClearState();
            canvasController.AddElements(els);
            canvasController.Elements.ForEach(el => el.UpdatePath());
            canvasController.Canvas.Invalidate();
            UpdateCaption();
        }

        private void mnuImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FlowSharp (*.fsd)|*.fsd";
            DialogResult res = ofd.ShowDialog();

            if (res == DialogResult.OK)
            {
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
            if (canvasController.Elements.Count > 0)
            {
                SaveOrSaveAs();
                canvasController.Filename = filename;
                UpdateCaption();
            }
            else
            {
                MessageBox.Show("Nothing to save.", "Empty Canvas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            if (canvasController.Elements.Count > 0)
            {
                SaveOrSaveAs(true);
                canvasController.Filename = filename;
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
            if (canvasController.SelectedElements.Any())
            {
                List<GraphicElement> selectedShapes = canvasController.SelectedElements.ToList();
                FlowSharpLib.GroupBox groupBox = new FlowSharpLib.GroupBox(canvasController.Canvas);

                canvasController.UndoStack.UndoRedo("Group",
                    () =>
                    {
                        ElementCache.Instance.Remove(groupBox);
                        canvasController.GroupShapes(groupBox);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElement(groupBox);
                    },
                    () =>
                    {
                        ElementCache.Instance.Add(groupBox);
                        canvasController.UngroupShapes(groupBox, false);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElements(selectedShapes);
                    });
            }
        }

        private void mnuUngroup_Click(object sender, EventArgs e)
        {
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
                        ElementCache.Instance.Add(groupBox);
                        canvasController.UngroupShapes(groupBox, false);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElements(groupedShapes);
                    },
                    () =>
                    {
                        ElementCache.Instance.Remove(groupBox);
                        canvasController.GroupShapes(groupBox);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElement(groupBox);
                    });
                }
            }
        }

        private void mnuPlugins_Click(object sender, EventArgs e)
        {
            // TODO:
            // new DlgPlugins().ShowDialog();
            // TODO: Toolbox needs to update when plugins change.
            // pluginManager.UpdatePlugins();
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
            bool ret = false;

            if (savePoint != canvasController.UndoStack.UndoStackSize)
            {
                DialogResult res = MessageBox.Show("Do you wish to save changes to this drawing?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                ret = res == DialogResult.Cancel;

                if (res == DialogResult.Yes)
                {
                    ret = !SaveOrSaveAs();   // override because of possible cancel in save operation.
                }
                else
                {
                    if (!ret)       // Not cancelled.
                    {
                        canvasController.UndoStack.ClearStacks();       // Prevents second "are you sure" when exiting with Ctrl+X
                    }
                }
            }

            return ret;
        }

        protected bool SaveOrSaveAs(bool forceSaveAs = false)
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

        protected bool SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "FlowSharp (*.fsd)|*.fsd|PNG (*.png)|*.png";
            DialogResult res = sfd.ShowDialog();
            string ext = ".fsd";

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
                    SaveDiagram(filename);
                    canvasController.Filename = filename;
                    UpdateCaption();
                }
            }

            return res == DialogResult.OK && ext != ".png";
        }

        protected void SaveDiagram(string filename)
        {
            string data = Persist.Serialize(canvasController.Elements);
            File.WriteAllText(filename, data);
            savePoint = canvasController.UndoStack.UndoStackSize;
        }

        protected void UpdateCaption()
        {
            mainForm.Text = "FlowSharp" + (String.IsNullOrEmpty(filename) ? "" : " - ") + filename;
        }
    }
}
