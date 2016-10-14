/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
    public class TraceListener : ConsoleTraceListener
    {
        public DlgDebugWindow DebugWindow { get; set; }

        public override void WriteLine(string msg)
        {
            if (DebugWindow != null)
            {
                DebugWindow.Trace(msg + "\r\n");
            }
        }
    }

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

        protected DlgDebugWindow debugWindow;
        protected TraceListener traceListener;

		public FlowSharpUI()
        {
            InitializeComponent();
            traceListener = new TraceListener();
            Trace.Listeners.Add(traceListener);
            Shown += OnShown;
			UpdateMenu(false);

            // We have to initialize the menu event handlers here, rather than in the designer,
            // so that we can move the menu handlers to the MenuController partial class.
            mnuNew.Click += mnuNew_Click;
            mnuOpen.Click += mnuOpen_Click;
            mnuImport.Click += (sndr, args) =>
            {
                canvasController.DeselectCurrentSelectedElements();
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

			keyActions[Keys.Control | Keys.C] = Copy;
			keyActions[Keys.Control | Keys.V] = Paste;
			keyActions[Keys.Delete] = Delete;
            keyActions[Keys.Up] = () => canvasController.DragSelectedElements(new Point(0, -1));
            keyActions[Keys.Down] = () => canvasController.DragSelectedElements(new Point(0, 1));
            keyActions[Keys.Left] = () => canvasController.DragSelectedElements(new Point(-1, 0));
            keyActions[Keys.Right] = () => canvasController.DragSelectedElements(new Point(1, 0));
		}

        public void OnShown(object sender, EventArgs e)
        {
			InitializeCanvas();
			InitializeToolbox();
			InitializeControllers();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Action act;
            bool ret = false;

            if (canvas.Focused && keyActions.TryGetValue(keyData, out act))
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
            if (canvasController.SelectedElements.Any())
            {
                string copyBuffer = Persist.Serialize(canvasController.SelectedElements);
                Clipboard.SetData("FlowSharp", copyBuffer);
            }
            else
            {
                MessageBox.Show("Please select one or more shape(s).", "Nothing to copy.", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    List<GraphicElement> els = Persist.Deserialize(canvas, copyBuffer);
                    canvasController.DeselectCurrentSelectedElements();
                    els.ForEach(el =>
                    {
                        el.Move(new Point(20, 20));
                        el.UpdateProperties();
                        el.UpdatePath();
                    });

                    List<GraphicElement> intersections = new List<GraphicElement>();

                    els.ForEach(el =>
                    {
                        intersections.AddRange(canvasController.FindAllIntersections(el));
                    });

                    IEnumerable<GraphicElement> distinctIntersections = intersections.Distinct();
                    canvasController.EraseTopToBottom(distinctIntersections);
                    els.ForEach(el => elements.Insert(0, el));
                    canvasController.DrawBottomToTop(distinctIntersections);
                    canvasController.UpdateScreen(distinctIntersections);
                    els.ForEach(el => canvasController.SelectElement(el));
                }
                catch (Exception ex)
				{
					MessageBox.Show("Error pasting shape:\r\n"+ex.Message, "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		protected void Delete()
		{
			canvasController.DeleteSelectedElements();
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

        private void mnuDebugWindow_Click(object sender, EventArgs e)
        {
            if (debugWindow == null)
            {
                debugWindow = new DlgDebugWindow(canvasController);
                debugWindow.Show();
                traceListener.DebugWindow = debugWindow;
                debugWindow.FormClosed += (sndr, args) =>
                {
                    debugWindow = null;
                    traceListener.DebugWindow = null;
                };
            }
        }
    }
}
