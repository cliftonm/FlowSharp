using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
    public partial class FlowSharpUI : Form
    {
		protected CanvasController canvasController;
		protected ToolboxController toolboxController;
		protected UIController uiController;
		protected Canvas canvas;
		protected List<GraphicElement> elements = new List<GraphicElement>();

		protected Canvas toolboxCanvas;
		protected List<GraphicElement> toolboxElements = new List<GraphicElement>();

		public FlowSharpUI()
        {
            InitializeComponent();
            Shown += OnShown;
        }

		public void OnShown(object sender, EventArgs e)
        {
			InitializeCanvas();
			InitializeToolbox();
			InitializeControllers();
			CreateSampleElements();
		}

		protected void InitializeCanvas()
		{
			canvas = new Canvas();
			canvas.Initialize(pnlCanvas);
		}

		protected void InitializeControllers()
		{ 
			canvasController = new CanvasController(canvas, elements);
			toolboxController = new ToolboxController(toolboxCanvas, toolboxElements, canvasController);
			uiController = new UIController(pgElement, canvasController);
		}

		protected void CreateSampleElements()
		{
			elements.Add(new Box(canvas) { DisplayRectangle = new Rectangle(25, 50, 200, 100) });
			// elements.Add(new HorizontalLine(canvas) { DisplayRectangle = new Rectangle(325, 100, 75, 20) });
			elements.Add(new DynamicConnectorLR(canvas, new Point(325, 100), new Point(325 + 75, 100 + 20)));
			elements.ForEach(el => el.UpdatePath());
		}

		protected void InitializeToolbox()
		{
			toolboxCanvas = new ToolboxCanvas();
			toolboxCanvas.Initialize(pnlToolbox);
			int x = pnlToolbox.Width / 2 - 12;
			toolboxElements.Add(new Box(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 15, 25, 25) });
			toolboxElements.Add(new Ellipse(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 60, 25, 25) });
			toolboxElements.Add(new Diamond(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 105, 25, 25) });
			toolboxElements.Add(new HorizontalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 25, 150, 30, 20) });
			toolboxElements.Add(new VerticalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x+25, 145, 20, 30) });
			toolboxElements.Add(new ToolboxDynamicConnectorLR(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 25, 185, 25, 25)});
			toolboxElements.Add(new ToolboxDynamicConnectorUD(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 25, 185, 25, 25) });
			toolboxElements.Add(new ToolboxText(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 240, 25, 25) });
			toolboxElements.ForEach(el => el.UpdatePath());
		}

		// Menus

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

		}

		private void mnuPaste_Click(object sender, EventArgs e)
		{

		}

		private void mnuDelete_Click(object sender, EventArgs e)
		{
			canvasController.DeleteElement();
		}

		private void mnuNew_Click(object sender, EventArgs e)
		{
			// TODO: Check for changes before closing.
			elements.Clear();
			canvas.Invalidate();
		}

		private void mnuOpen_Click(object sender, EventArgs e)
		{
			// TODO: Check for changes before closing.

		}

		private void mnuSave_Click(object sender, EventArgs e)
		{

		}

		private void mnuSaveAs_Click(object sender, EventArgs e)
		{

		}

		private void mnuExit_Click(object sender, EventArgs e)
		{
			// TODO: Check for changes before closing.
			Close();
		}

	}
}
