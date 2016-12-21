using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharpMenuService
{
    public partial class NavigateDlg : Form
    {
        protected BaseController canvasController;

        public NavigateDlg(BaseController canvasController, List<NavigateToShape> navNames)
        {
            this.canvasController = canvasController;
            InitializeComponent();
            lbShapes.Items.AddRange(navNames.ToArray());
        }

        private void lbShapes_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Escape:
                    e.Handled = true;
                    Close();
                    break;

                case (char)Keys.Enter:
                    e.Handled = true;
                    Close();
                    GraphicElement shape = ((NavigateToShape)lbShapes.SelectedItem).Shape;
                    FocusOnShape(shape);
                    break;
            }
        }

        private void lbShapes_MouseClick(object sender, MouseEventArgs e)
        {
            Close();
            GraphicElement shape = ((NavigateToShape)lbShapes.SelectedItem).Shape;
            FocusOnShape(shape);
        }

        private void FocusOnShape(GraphicElement shape)
        {
            // For closure:
            List<GraphicElement> selectedShapes = canvasController.SelectedElements.ToList();
            int cx = (canvasController.Canvas.Width - shape.DisplayRectangle.Width) / 2;
            int cy = (canvasController.Canvas.Height - shape.DisplayRectangle.Height) / 2;
            int dx = -(shape.DisplayRectangle.X - cx);
            int dy = -(shape.DisplayRectangle.Y - cy);

            canvasController.UndoStack.UndoRedo("Focus Shape " + shape.ToString(),
                () =>
                {
                    canvasController.MoveAllElements(new Point(dx, dy));
                    canvasController.DeselectCurrentSelectedElements();
                    canvasController.SelectElement(shape);
                },
                () =>
                {
                    canvasController.DeselectCurrentSelectedElements();
                    canvasController.SelectElements(selectedShapes);
                    canvasController.MoveAllElements(new Point(-dx, -dy));
                });
        }
    }
}
