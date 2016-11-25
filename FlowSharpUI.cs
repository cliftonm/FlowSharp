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
using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    public partial class FlowSharpUI : Form
    {
        // protected MouseController mouseController;
        protected BaseController canvasController;
        protected BaseController toolboxController;
        protected Canvas canvas;

        protected Canvas toolboxCanvas;
        protected Dictionary<Keys, Action> keyActions = new Dictionary<Keys, Action>();

        protected DlgDebugWindow debugWindow;
        protected TraceListener traceListener;

        protected TextBox editBox;
        protected GraphicElement shapeBeingEdited;

        public FlowSharpUI()
        {
            InitializeComponent();
            traceListener = new TraceListener();
            Trace.Listeners.Add(traceListener);
            Shown += OnShown;
            FormClosing += OnFormClosing;

            Icon = Properties.Resources.FlowSharp;

            // We have to initialize the menu event handlers here, rather than in the designer,
            // so that we can move the menu handlers to the MenuController partial class.
            mnuEdit.Click += (sndr, args) => EditText();

            //keyActions[Keys.Control | Keys.C] = Copy;
            //keyActions[Keys.Control | Keys.V] = Paste;
            //keyActions[Keys.Control | Keys.Z] = Undo;
            //keyActions[Keys.Control | Keys.Y] = Redo;
            keyActions[Keys.F2] = EditText;

            // TODO: Don't finish the group until another action other than cursor movement of a shape occurs.

            keyActions[Keys.Up] = () => DoMove(new Point(0, -1));
            keyActions[Keys.Down] = () => DoMove(new Point(0, 1));
            keyActions[Keys.Left] = () => DoMove(new Point(-1, 0));
            keyActions[Keys.Right] = () => DoMove(new Point(1, 0));

            // Also allow keyboard move with Ctrl key pressed, which ignores snap check.
            keyActions[Keys.Control | Keys.Up] = () => DoMove(new Point(0, -1));
            keyActions[Keys.Control | Keys.Down] = () => DoMove(new Point(0, 1));
            keyActions[Keys.Control | Keys.Left] = () => DoMove(new Point(-1, 0));
            keyActions[Keys.Control | Keys.Right] = () => DoMove(new Point(1, 0));
        }

        protected void DoMove(Point dir)
        {
            // Always reset the snap controller before a keyboard move.  This ensures that, among other things, the running delta is zero'd.
            canvasController.SnapController.Reset();

            if (canvasController.SelectedElements.Count == 1 && canvasController.SelectedElements[0].IsConnector)
            {
                // TODO: Duplicate code in FlowSharpToolboxService.ToolboxController.OnMouseMove and MouseController
                // Check both ends of any connector being moved.
                if (!canvasController.SnapController.SnapCheck(GripType.Start, dir, (snapDelta) => canvasController.DragSelectedElements(snapDelta), true))
                {
                    if (!canvasController.SnapController.SnapCheck(GripType.End, dir, (snapDelta) => canvasController.DragSelectedElements(snapDelta), true))
                    {
                        // No snap occurred.
                        DoJustKeyboardMove(dir);
                    }
                    else
                    {
                        // Snapped grip end.
                        DoKeyboardSnapWithMove(dir);
                    }
                }
                else
                {
                    // Snapped grip start.
                    DoKeyboardSnapWithMove(dir);
                }
            }
            else
            {
                // Moving shape, or multiple shapes, not a single connector.
                DoJustKeyboardMove(dir);
            }
        }

        protected void DoKeyboardSnapWithMove(Point dir)
        {
            canvasController.SnapController.DoUndoSnapActions(canvasController.UndoStack);

            if (canvasController.SnapController.RunningDelta != Point.Empty)
            {
                Point delta = canvasController.SnapController.RunningDelta;     // for closure
                bool ignoreSnapCheck = canvasController.IsSnapToBeIgnored;      // for closure

                canvasController.UndoStack.UndoRedo(
                "KeyboardMove",
                () => { },  // Doing is already done.
                () =>
                {
                    canvasController.UndoRedoIgnoreSnapCheck = ignoreSnapCheck;
                    canvasController.DragSelectedElements(delta.ReverseDirection());
                    canvasController.UndoRedoIgnoreSnapCheck = false;
                },
                true,
                () => canvasController.DragSelectedElements(delta)
                );
            }
        }

        protected void DoJustKeyboardMove(Point dir)
        {
            bool ignoreSnapCheck = canvasController.IsSnapToBeIgnored;      // for closure
            canvasController.UndoStack.UndoRedo(
            "KeyboardMove",
            () => canvasController.DragSelectedElements(dir),
            () =>
            {
                canvasController.UndoRedoIgnoreSnapCheck = ignoreSnapCheck;
                canvasController.DragSelectedElements(dir.ReverseDirection());
                canvasController.UndoRedoIgnoreSnapCheck = false;
            }
            );
        }

        public void OnShown(object sender, EventArgs e)
        {
            // InitializePlugins();
            InitializeCanvas();
            InitializeControllers();
            UpdateMenu(false);
        }

        protected void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = CheckForChanges();

            //if (!e.Cancel)
            //{
            //    ElementCache.Instance.ClearCache();
            //    canvasController.Clear();
            //    toolboxController.Clear();
            //}
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Action act;
            bool ret = false;

            if (editBox == null)
            {
                if (canvas.Focused && keyActions.TryGetValue(keyData, out act))
                {
                    act();
                    ret = true;
                }
                else
                {
                    if (canvas.Focused &&
                        canvasController.SelectedElements.Count == 1 &&
                        !canvasController.SelectedElements[0].IsConnector &&
                        CanStartEditing(keyData))
                    {
                        EditText();
                        // TODO: THIS IS SUCH A MESS!

                        // Will return upper case letter always, regardless of shift key....
                        string firstKey = ((char)keyData).ToString();

                        // ... so we have to fix it.  Sigh.
                        if ((keyData & Keys.Shift) != Keys.Shift)
                        {
                            firstKey = firstKey.ToLower();
                        }
                        else
                        {
                            // Handle shift of number keys on main keyboard
                            if (char.IsDigit(firstKey[0]))
                            {
                                // TODO: Probably doesn't handle non-American keyboards!
                                // Note index 0 is ")"
                                string key = ")!@#$%^&*(";
                                int n;

                                if (int.TryParse(firstKey, out n))
                                {
                                    firstKey = key[n].ToString();
                                }
                            }
                            // TODO: This is such a PITA.  Other symbols and shift combinations do not produce the correct first character!
                        }

                        editBox.Text = firstKey;
                        editBox.SelectionStart = 1;
                        editBox.SelectionLength = 0;
                        ret = true;
                    }
                    else
                    {
                        ret = base.ProcessCmdKey(ref msg, keyData);
                    }
                }
            }
            else
            {
                ret = base.ProcessCmdKey(ref msg, keyData);
            }

            return ret;
        }

        protected bool CanStartEditing(Keys keyData)
        {
            bool ret = false;

            if ( ((keyData & Keys.Control) != Keys.Control) &&              // any control + key is not valid
                 ((keyData & Keys.Alt) != Keys.Alt) )                       // any alt + key is not valid
            {
                Keys k2 = (keyData & ~(Keys.Control | Keys.Shift | Keys.ShiftKey | Keys.Alt | Keys.Menu));

                if ((k2 != Keys.None) && (k2 < Keys.F1 || k2 > Keys.F12) )
                {
                    // Here we assume we have a viable character.
                    // TODO: Probably more logic is required here.
                    ret = true;
                }
            }

            return ret;
        }

        protected void EditText()
        {
            if (canvasController.SelectedElements.Count == 1)
            {
                // TODO: At the moment, connectors do not support text.
                if (!canvasController.SelectedElements[0].IsConnector)
                {
                    shapeBeingEdited = canvasController.SelectedElements[0];
                    editBox = CreateTextBox(shapeBeingEdited);
                    canvas.Controls.Add(editBox);
                    editBox.Visible = true;
                    editBox.Focus();
                    editBox.KeyPress += OnEditBoxKey;
                    editBox.LostFocus += (sndr, args) => TerminateEditing();
                }
            }
        }

        protected void OnEditBoxKey(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27 || e.KeyChar == 13)
            {
                TerminateEditing();
                e.Handled = true;       // Suppress beep.
            }
        }

        protected void TerminateEditing()
        {
            if (editBox != null)
            {
                editBox.KeyPress -= OnEditBoxKey;
                string oldVal = shapeBeingEdited.Text;
                string newVal = editBox.Text;
                TextBox tb = editBox;
                editBox = null;     // set editBox to null so the remove, which fires a LoseFocus event, doesn't call into TerminateEditing again!

                canvasController.UndoStack.UndoRedo("Inline edit",
                    () =>
                    {
                        canvasController.Redraw(shapeBeingEdited, (el) => el.Text = newVal);
                        canvasController.ElementSelected.Fire(this, new ElementEventArgs() { Element = shapeBeingEdited });
                    },
                    () =>
                    {
                        canvasController.Redraw(shapeBeingEdited, (el) => el.Text = oldVal);
                        canvasController.ElementSelected.Fire(this, new ElementEventArgs() { Element = shapeBeingEdited });
                    });

                canvas.Controls.Remove(tb);
            }
        }

        protected TextBox CreateTextBox(GraphicElement el)
        {
            TextBox tb = new TextBox();
            tb.Location = el.DisplayRectangle.LeftMiddle().Move(0, -10);
            tb.Size = new Size(el.DisplayRectangle.Width, 20);
            tb.Text = el.Text;

            return tb;
        }

        protected void InitializeCanvas()
		{
            IFlowSharpCanvasService canvasService = Program.ServiceManager.Get<IFlowSharpCanvasService>();
            canvasService.CreateCanvas(pnlCanvas);
            canvasController = canvasService.Controller;
            canvas = canvasController.Canvas;
			// canvas = new Canvas();
			// canvas.Initialize(pnlCanvas);
            // Once the user clicks on the canvas, the displacement for copying elements from the toolbox onto the canvas is reset.
            canvas.MouseClick += (sndr, args) => Program.ServiceManager.Get<IFlowSharpToolboxService>().ResetDisplacement();

            IFlowSharpToolboxService toolboxService = Program.ServiceManager.Get<IFlowSharpToolboxService>();
            toolboxService.CreateToolbox(pnlToolbox);
            toolboxController = toolboxService.Controller;
            toolboxCanvas = toolboxController.Canvas;
        }

        protected void InitializeControllers()
		{ 
			// canvasController = new CanvasController(canvas);
            // mouseController = new MouseController(canvasController);
            // No longer needed, as editbox LostFocus event handles terminating itself now.
            // mouseController.MouseClick += (sndr, args) => TerminateEditing();
            canvasController.ElementSelected += (snd, args) => UpdateMenu(args.Element != null);
            canvasController.UndoStack.AfterAction += (sndr, args) =>
            {
                // after an undo/redo, the save point is reset
                // TODO: This still causes a prompt for save if the diagram is saved, then and undo-redo or redo-undo occurs,
                // which results in an actual diagram hasn't changed.  But for now, this fixes the most common situation of
                // saving then exiting, in which case we don't want to prompt for "save changes?" again because we know they've
                // been saved.
                // savePoint = 0;
                UpdateDebugWindowUndoStack();
            };

            // toolboxController = new ToolboxController(toolboxCanvas, canvasController);
            // toolboxCanvas.Controller = toolboxController;
            IFlowSharpPropertyGridService pgService = Program.ServiceManager.Get<IFlowSharpPropertyGridService>();
            pgService.Initialize(pgElement);
            // uiController = new UIController(pgElement, canvasController);
            // mouseController.HookMouseEvents();
            // mouseController.InitializeBehavior();
		}

		protected void UpdateMenu(bool elementSelected)
		{
			mnuBottommost.Enabled = elementSelected;
			mnuTopmost.Enabled = elementSelected;
			mnuMoveUp.Enabled = elementSelected;
			mnuMoveDown.Enabled = elementSelected;
			mnuCopy.Enabled = elementSelected;
			mnuDelete.Enabled = elementSelected;
            mnuGroup.Enabled = elementSelected && !canvasController.SelectedElements.Any(el => el.Parent != null);
            mnuUngroup.Enabled = canvasController.SelectedElements.Count==1 && canvasController.SelectedElements[0].GroupChildren.Any();
		}

        protected void UpdateDebugWindowUndoStack()
        {
            if (debugWindow != null)
            {
                List<string> undoEvents = canvasController.UndoStack.GetStackInfo();
                debugWindow.UpdateUndoStack(undoEvents);
                debugWindow.UpdateShapeTree();
            }
        }

        private void mnuDebugWindow_Click(object sender, EventArgs e)
        {
            if (debugWindow == null)
            {
                debugWindow = new DlgDebugWindow(canvasController);
                debugWindow.Show();
                List<string> undoEvents = canvasController.UndoStack.GetStackInfo();
                debugWindow.UpdateUndoStack(undoEvents);
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
