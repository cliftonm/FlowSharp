using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
    public partial class Form1 : Form
    {
		protected CanvasController canvasController;
		protected ToolboxController toolboxController;
		protected UIController uiController;
		protected Canvas canvas;
		protected List<GraphicElement> elements = new List<GraphicElement>();

		protected Canvas toolboxCanvas;
		protected List<GraphicElement> toolboxElements = new List<GraphicElement>();

		public Form1()
        {
            InitializeComponent();
            Shown += OnShown;
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
			elements.Add(new HorizontalLine(canvas) { DisplayRectangle = new Rectangle(325, 100, 75, 20) });
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
			toolboxElements.Add(new HorizontalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 150, 30, 20) });
			toolboxElements.Add(new VerticalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x+50, 145, 20, 30) });
			toolboxElements.Add(new ToolboxDynamicConnector(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 185, 25, 25)});
			toolboxElements.ForEach(el => el.UpdatePath());
		}
	}
}
