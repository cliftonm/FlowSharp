/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.IO;
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
			canvasController.MoveUp();
		}

		private void mnuMoveDown_Click(object sender, EventArgs e)
		{
			canvasController.MoveDown();
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
			// TODO: Check for changes before closing.
			elements.Clear();
			canvas.Invalidate();
			filename = String.Empty;
			UpdateCaption();
		}

		private void mnuOpen_Click(object sender, EventArgs e)
		{
			// TODO: Check for changes before closing.
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
				if (String.IsNullOrEmpty(filename))
				{
					mnuSaveAs_Click(sender, e);
				}

				string data = Persist.Serialize(elements);
				File.WriteAllText(filename, data);
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
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.Filter = "FlowSharp (*.fsd)|*.fsd|PNG (*.png)|*.png";
				DialogResult res = sfd.ShowDialog();

				if (res == DialogResult.OK)
				{
					if (Path.GetExtension(sfd.FileName).ToLower() == ".png")
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
			}
			else
			{
				MessageBox.Show("Nothing to save.", "Empty Canvas", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void mnuExit_Click(object sender, EventArgs e)
		{
			// TODO: Check for changes before closing.
			Close();
		}
	}
}
