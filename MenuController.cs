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
			if (canvasController.SelectedElement != null)
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

		private void mnuSave_Click(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(filename))
			{
				mnuSaveAs_Click(sender, e);
			}

			string data = Persist.Serialize(elements);
			File.WriteAllText(filename, data);
			UpdateCaption();
		}

		private void mnuSaveAs_Click(object sender, EventArgs e)
		{
			SaveFileDialog ofd = new SaveFileDialog();
			DialogResult res = ofd.ShowDialog();

			if (res == DialogResult.OK)
			{
				filename = ofd.FileName;
			}
			else
			{
				return;
			}

			string data = Persist.Serialize(elements);
			File.WriteAllText(filename, data);
			UpdateCaption();
		}

		private void mnuExit_Click(object sender, EventArgs e)
		{
			// TODO: Check for changes before closing.
			Close();
		}
	}
}
