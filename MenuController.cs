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

using FlowSharpLib;

namespace FlowSharp
{
	public partial class FlowSharpUI
	{
		protected string filename;

		private void mnuTopmost_Click(object sender, EventArgs e)
		{
			canvasController.Topmost();
		}

		private void mnuBottommost_Click(object sender, EventArgs e)
		{
			canvasController.Bottommost();
		}

		private void mnuMoveUp_Click(object sender, EventArgs e)
		{
			canvasController.MoveSelectedElementsUp();
		}

		private void mnuMoveDown_Click(object sender, EventArgs e)
		{
			canvasController.MoveSelectedElementsDown();
		}

		private void mnuCopy_Click(object sender, EventArgs e)
		{
			if (canvasController.SelectedElements.Count > 0)
			{
				Copy();
			}
		}

		private void mnuPaste_Click(object sender, EventArgs e)
		{
			Paste();
		}

		private void mnuDelete_Click(object sender, EventArgs e)
		{
			Delete();
		}

		private void mnuNew_Click(object sender, EventArgs e)
		{
            if (CheckForChanges()) return;
            elements.Clear();
			canvas.Invalidate();
			filename = String.Empty;
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

			string data = File.ReadAllText(filename);
			List<GraphicElement> els = Persist.Deserialize(canvas, data);
			elements.Clear();
			elements.AddRange(els);
			elements.ForEach(el => el.UpdatePath());
			canvas.Invalidate();
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
                List<GraphicElement> els = Persist.Deserialize(canvas, data);
                elements.AddRange(els);
                elements.ForEach(el => el.UpdatePath());
                els.ForEach(el => canvas.Controller.SelectElement(el));
                canvas.Invalidate();
            }
        }

        private void mnuSave_Click(object sender, EventArgs e)
		{
			if (elements.Count > 0)
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
			if (elements.Count > 0)
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
            Close();
		}

        private void mnuGroup_Click(object sender, EventArgs e)
        {
            if (canvasController.SelectedElements.Any())
            {
                FlowSharpLib.GroupBox groupBox = canvasController.GroupShapes(canvasController.SelectedElements);
                canvasController.DeselectCurrentSelectedElements();
                canvasController.SelectElement(groupBox);
            }
        }

        private void mnuUngroup_Click(object sender, EventArgs e)
        {
            canvasController.UngroupShapes(canvasController.SelectedElements);
            canvasController.SelectedElements.Clear();
        }

        private void mnuPlugins_Click(object sender, EventArgs e)
        {
            new DlgPlugins().ShowDialog();
            pluginManager.UpdatePlugins();
        }

        private void mnuUndo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void mnuRedo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        /// <summary>
        /// Return true if operation should be cancelled.
        /// </summary>
        protected bool CheckForChanges()
        {
            bool ret = false;

            if (canvasController.UndoStack.HasChanges)
            {
                DialogResult res = MessageBox.Show("Do you wish to save changes to this drawing?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                ret = res == DialogResult.Cancel;

                if (res == DialogResult.Yes)
                {
                    ret = !SaveOrSaveAs();   // override because of possible cancel in save operation.
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
                string data = Persist.Serialize(elements);
                File.WriteAllText(filename, data);
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
                    string data = Persist.Serialize(elements);
                    File.WriteAllText(filename, data);
                    UpdateCaption();
                }
            }

            return res == DialogResult.OK && ext != ".png";
        }
    }
}
