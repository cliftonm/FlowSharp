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

			// We have to initialize the menu event handlers here, rather than in the designer,
			// so that we can move the menu handlers to the MenuController partial class.
			mnuNew.Click += new EventHandler(mnuNew_Click);
			mnuOpen.Click += new EventHandler(mnuOpen_Click);
			mnuSave.Click += new EventHandler(mnuSave_Click);
			mnuSaveAs.Click += new EventHandler(mnuSaveAs_Click);
			mnuExit.Click += new EventHandler(mnuExit_Click);
			mnuCopy.Click += new EventHandler(mnuCopy_Click);
			mnuPaste.Click += new EventHandler(mnuPaste_Click);
			mnuDelete.Click += new EventHandler(mnuDelete_Click);
			mnuTopmost.Click += new EventHandler(mnuTopmost_Click);
			mnuBottommost.Click += new EventHandler(mnuBottommost_Click);
			mnuMoveUp.Click += new EventHandler(mnuMoveUp_Click);
			mnuMoveDown.Click += new EventHandler(mnuMoveDown_Click);

		}

		public void OnShown(object sender, EventArgs e)
        {
			InitializeCanvas();
			InitializeToolbox();
			InitializeControllers();
			// CreateSampleElements();
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
			toolboxElements.Add(new ToolboxDynamicConnectorLR(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 185, 25, 25)});
			toolboxElements.Add(new ToolboxDynamicConnectorLD(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 185, 25, 25) });
			toolboxElements.Add(new ToolboxDynamicConnectorUD(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 50, 185, 25, 25) });
			toolboxElements.Add(new ToolboxText(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 230, 25, 25) });
			toolboxElements.ForEach(el => el.UpdatePath());
		}

		protected void UpdateCaption()
		{
			Text = "FlowSharp" + (String.IsNullOrEmpty(filename) ? "" : " - ") + filename;
		}
	}
}
