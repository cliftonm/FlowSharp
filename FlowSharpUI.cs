/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

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

		protected Dictionary<Keys, Action> keyActions = new Dictionary<Keys, Action>();

		public FlowSharpUI()
        {
            InitializeComponent();
            Shown += OnShown;
			UpdateMenu(false);

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

			keyActions[Keys.Control | Keys.C] = Copy;
			keyActions[Keys.Control | Keys.V] = Paste;
			keyActions[Keys.Delete] = Delete;
            keyActions[Keys.Up] = () => canvasController.DragSelectedElement(new Point(0, -1));
            keyActions[Keys.Down] = () => canvasController.DragSelectedElement(new Point(0, 1));
            keyActions[Keys.Left] = () => canvasController.DragSelectedElement(new Point(-1, 0));
            keyActions[Keys.Right] = () => canvasController.DragSelectedElement(new Point(1, 0));
		}

        public void OnShown(object sender, EventArgs e)
        {
			InitializeCanvas();
			InitializeToolbox();
			InitializeControllers();
			// CreateSampleElements();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Action act;
            bool ret = false;

			if (canvas.Focused && canvasController.SelectedElement != null && keyActions.TryGetValue(keyData, out act))
            {
				act();
                ret = true;
			}
            else
            {
                ret = base.ProcessCmdKey(ref msg, keyData);
            }

            return ret;
		}

		protected void Copy()
		{
			if (canvasController.SelectedElement != null)
			{
				string copyBuffer = Persist.Serialize(canvasController.SelectedElement);
				Clipboard.SetData("FlowSharp", copyBuffer);
			}
		}

		protected void Paste()
		{
			string copyBuffer = Clipboard.GetData("FlowSharp")?.ToString();

			if (copyBuffer == null)
			{
				MessageBox.Show("Clipboard does not contain a FlowSharp shape", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				try
				{
					GraphicElement el = Persist.DeserializeElement(canvas, copyBuffer);
                    el.Move(new Point(20, 20));
                    el.UpdateProperties();
					el.UpdatePath();
					canvasController.Insert(el);
					canvasController.DeselectCurrentSelectedElement();
					canvasController.SelectElement(el);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error pasting shape:\r\n"+ex.Message, "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		protected void Delete()
		{
			canvasController.DeleteElement();
		}

		protected void InitializeCanvas()
		{
			canvas = new Canvas();
			canvas.Initialize(pnlCanvas);
            // Once the user clicks on the canvas, the displacement for copying elements from the toolbox onto the canvas is reset.
            canvas.MouseClick += (sndr, args) => toolboxController.ResetDisplacement();
        }

		protected void InitializeControllers()
		{ 
			canvasController = new CanvasController(canvas, elements);
			canvasController.ElementSelected+=(snd, args) => UpdateMenu(args.Element != null);
			toolboxController = new ToolboxController(toolboxCanvas, toolboxElements, canvasController);
			uiController = new UIController(pgElement, canvasController);
		}


		protected void UpdateMenu(bool elementSelected)
		{
			mnuBottommost.Enabled = elementSelected;
			mnuTopmost.Enabled = elementSelected;
			mnuMoveUp.Enabled = elementSelected;
			mnuMoveDown.Enabled = elementSelected;
			mnuCopy.Enabled = elementSelected;
			mnuDelete.Enabled = elementSelected;
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
			toolboxElements.Add(new Box(toolboxCanvas) { DisplayRectangle = new Rectangle(x-50, 15, 25, 25) });
			toolboxElements.Add(new Ellipse(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 15, 25, 25) });
			toolboxElements.Add(new Diamond(toolboxCanvas) { DisplayRectangle = new Rectangle(x+50, 15, 25, 25) });

            toolboxElements.Add(new LeftTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x -60, 60, 25, 25) });
            toolboxElements.Add(new RightTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 20, 60, 25, 25) });
            toolboxElements.Add(new UpTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x+20, 60, 25, 25) });
            toolboxElements.Add(new DownTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x+60, 60, 25, 25) });

            toolboxElements.Add(new HorizontalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 25, 130, 30, 20) });
			toolboxElements.Add(new VerticalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x+25, 125, 20, 30) });

            // toolboxElements.Add(new ToolboxDynamicConnectorLR(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 185, 25, 25)});
            toolboxElements.Add(new DynamicConnectorLR(toolboxCanvas, new Point(x - 50, 175), new Point(x - 50 + 25, 175 + 25)));
            toolboxElements.Add(new DynamicConnectorLD(toolboxCanvas, new Point(x, 175), new Point(x + 25, 175 + 25)));
            toolboxElements.Add(new DynamicConnectorUD(toolboxCanvas, new Point(x + 50, 175), new Point(x + 50 + 25, 175 + 25)));

			toolboxElements.Add(new ToolboxText(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 230, 25, 25) });
			toolboxElements.ForEach(el => el.UpdatePath());
		}

		protected void UpdateCaption()
		{
			Text = "FlowSharp" + (String.IsNullOrEmpty(filename) ? "" : " - ") + filename;
		}
	}
}
