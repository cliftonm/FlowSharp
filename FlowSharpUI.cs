/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
    public partial class FlowSharpUI : Form
    {
        public const string PLUGIN_FILE_LIST = "plugins.txt";

        protected MouseController mouseController;
        protected CanvasController canvasController;
        protected ToolboxController toolboxController;
        protected UIController uiController;
        protected Canvas canvas;

        protected Canvas toolboxCanvas;
        protected Dictionary<Keys, Action> keyActions = new Dictionary<Keys, Action>();

        protected DlgDebugWindow debugWindow;
        protected TraceListener traceListener;
        protected PluginManager pluginManager;

        protected TextBox editBox;
        protected GraphicElement shapeBeingEdited;

        public FlowSharpUI()
        {
            InitializeComponent();
            traceListener = new TraceListener();
            Trace.Listeners.Add(traceListener);
            Shown += OnShown;
            FormClosing += OnFormClosing;

            // We have to initialize the menu event handlers here, rather than in the designer,
            // so that we can move the menu handlers to the MenuController partial class.
            mnuNew.Click += mnuNew_Click;
            mnuOpen.Click += mnuOpen_Click;
            mnuImport.Click += (sndr, args) =>
            {
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
            mnuGroup.Click += mnuGroup_Click;
            mnuUngroup.Click += mnuUngroup_Click;
            mnuPlugins.Click += mnuPlugins_Click;
            mnuUndo.Click += mnuUndo_Click;
            mnuRedo.Click += mnuRedo_Click;
            mnuEdit.Click += (sndr, args) => EditText();

            //keyActions[Keys.Control | Keys.C] = Copy;
            //keyActions[Keys.Control | Keys.V] = Paste;
            //keyActions[Keys.Control | Keys.Z] = Undo;
            //keyActions[Keys.Control | Keys.Y] = Redo;
            keyActions[Keys.Delete] = Delete;
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
            bool ignoreSnapCheck = canvasController.IsSnapToBeIgnored;      // preserve ignore snap state
            canvasController.UndoStack.UndoRedo(
            "Move",
            () => canvasController.DragSelectedElements(dir, true),
            () =>
            {
                canvasController.UndoRedoIgnoreSnapCheck = ignoreSnapCheck;
                canvasController.DragSelectedElements(dir.ReverseDirection(), true);
            }
            );
        }

        public void OnShown(object sender, EventArgs e)
        {
            InitializePlugins();
            InitializeCanvas();
            InitializeControllers();
            InitializeToolbox();
            InitializePluginsInToolbox();
            UpdateToolboxPaths();
            UpdateMenu(false);
        }

        protected void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CheckForChanges();
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

        protected void Copy()
        {
            if (canvasController.SelectedElements.Any())
            {
                List<GraphicElement> elementsToCopy = new List<GraphicElement>();
                // Include child elements of any groupbox, otherwise, on deserialization,
                // the ID's for the child elements aren't found.
                elementsToCopy.AddRange(canvasController.SelectedElements);
                elementsToCopy.AddRange(IncludeChildren(elementsToCopy));
                string copyBuffer = Persist.Serialize(elementsToCopy.OrderByDescending(el => canvasController.Elements.IndexOf(el)));
                Clipboard.SetData("FlowSharp", copyBuffer);
            }
            else
            {
                MessageBox.Show("Please select one or more shape(s).", "Nothing to copy.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected List<GraphicElement> IncludeChildren(List<GraphicElement> parents)
        {
            List<GraphicElement> els = new List<GraphicElement>();

            parents.ForEach(p =>
            {
                els.AddRange(p.GroupChildren);
                els.AddRange(IncludeChildren(p.GroupChildren));
            });

            return els;
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
                    List<GraphicElement> selectedElements = canvasController.SelectedElements.ToList();

                    // After deserialization, only move and select elements without parents -
                    // children of group boxes should not be moved, as their parent will handle this,
                    // and children of group boxes cannot be selected.
                    List<GraphicElement> noParentElements = els.Where(e => e.Parent == null).ToList();

                    noParentElements.ForEach(el =>
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

                    canvasController.UndoStack.UndoRedo("Paste",
                        () =>
                        {
                            canvasController.DeselectCurrentSelectedElements();

                            canvasController.EraseTopToBottom(distinctIntersections);

                            els.ForEach(el =>
                            {
                                canvasController.Insert(0, el);
                                ElementCache.Instance.Remove(el);
                            });

                            canvasController.DrawBottomToTop(distinctIntersections);
                            canvasController.UpdateScreen(distinctIntersections);
                            noParentElements.ForEach(el => canvasController.SelectElement(el));
                        }
                        ,
                        () =>
                        {
                            canvasController.DeselectCurrentSelectedElements();

                            els.ForEach(el =>
                            {
                                canvasController.DeleteElement(el, false);
                                ElementCache.Instance.Add(el);
                            });

                            canvasController.SelectElements(selectedElements);
                        });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error pasting shape:\r\n" + ex.Message, "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected void Delete()
        {
            if (canvasController.Canvas.Focused)
            {
                // TODO: Better implementation would be for the mouse controller to hook a shape deleted event?
                canvasController.SelectedElements.ForEach(el => mouseController.ShapeDeleted(el));
                canvasController.DeleteSelectedElements();
            }
        }

        protected void Undo()
        {
            canvasController.Undo();
        }

        protected void Redo()
        {
            canvasController.Redo();
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
                        shapeBeingEdited.Text = newVal;
                        canvasController.Redraw(shapeBeingEdited);
                        // Updates PropertyGrid:
                        canvasController.ElementSelected.Fire(this, new ElementEventArgs() { Element = shapeBeingEdited });
                    },
                    () =>
                    {
                        shapeBeingEdited.Text = oldVal;
                        canvasController.Redraw(shapeBeingEdited);
                        // Updates PropertyGrid:
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

        protected void InitializePlugins()
        {
            pluginManager = new PluginManager();
            pluginManager.InitializePlugins();
        }

        protected void InitializeCanvas()
		{
			canvas = new Canvas();
			canvas.Initialize(pnlCanvas);
            // Once the user clicks on the canvas, the displacement for copying elements from the toolbox onto the canvas is reset.
            canvas.MouseClick += (sndr, args) => toolboxController.ResetDisplacement();
            toolboxCanvas = new ToolboxCanvas();
        }

        protected void InitializeControllers()
		{ 
			canvasController = new CanvasController(canvas);
            mouseController = new MouseController(canvasController);
            // No longer needed, as editbox LostFocus event handles terminating itself now.
            // mouseController.MouseClick += (sndr, args) => TerminateEditing();
            canvasController.ElementSelected += (snd, args) => UpdateMenu(args.Element != null);
            canvasController.UndoStack.AfterAction += (sndr, args) => UpdateDebugWindowUndoStack();
			toolboxController = new ToolboxController(toolboxCanvas, canvasController);
            toolboxCanvas.Controller = toolboxController;
            uiController = new UIController(pgElement, canvasController);
            mouseController.HookMouseEvents();
            mouseController.InitializeBehavior();
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

        protected void InitializeToolbox()
        {
            toolboxCanvas.Initialize(pnlToolbox);
            int x = pnlToolbox.Width / 2 - 12;
            toolboxController.AddElement(new Box(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 15, 25, 25) });
            toolboxController.AddElement(new Ellipse(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 15, 25, 25) });
            toolboxController.AddElement(new Diamond(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 50, 15, 25, 25) });

            toolboxController.AddElement(new LeftTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 60, 60, 25, 25) });
            toolboxController.AddElement(new RightTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 20, 60, 25, 25) });
            toolboxController.AddElement(new UpTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 20, 60, 25, 25) });
            toolboxController.AddElement(new DownTriangle(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 60, 60, 25, 25) });

            toolboxController.AddElement(new HorizontalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 130, 30, 20) });
            toolboxController.AddElement(new VerticalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 125, 20, 30) });
            toolboxController.AddElement(new DiagonalConnector(toolboxCanvas, new Point(x + 50, 125), new Point(x + 50 + 25, 125 + 25)));

            // toolboxElements.Add(new ToolboxDynamicConnectorLR(toolboxCanvas) { DisplayRectangle = new Rectangle(x - 50, 185, 25, 25)});
            toolboxController.AddElement(new DynamicConnectorLR(toolboxCanvas, new Point(x - 50, 175), new Point(x - 50 + 25, 175 + 25)));
            toolboxController.AddElement(new DynamicConnectorLD(toolboxCanvas, new Point(x, 175), new Point(x + 25, 175 + 25)));
            toolboxController.AddElement(new DynamicConnectorUD(toolboxCanvas, new Point(x + 50, 175), new Point(x + 50 + 25, 175 + 25)));

            toolboxController.AddElement(new ToolboxText(toolboxCanvas) { DisplayRectangle = new Rectangle(x, 230, 25, 25) });
            // toolboxElements.Add(new DiagonalLine(toolboxCanvas) { DisplayRectangle = new Rectangle(x + 25, 230, 25, 25) });
        }

        protected void InitializePluginsInToolbox()
        {
            int x = pnlToolbox.Width / 2 - 12;
            List<Type> pluginShapes = pluginManager.GetShapeTypes();

            // Plugin shapes
            int n = x - 60;
            int y = 260;

            foreach (Type t in pluginShapes)
            {
                GraphicElement pluginShape = Activator.CreateInstance(t, new object[] { toolboxCanvas }) as GraphicElement;
                pluginShape.DisplayRectangle = new Rectangle(n, y, 25, 25);
                toolboxController.AddElement(pluginShape);

                // Next toolbox shape position:
                n += 40;

                if (n > x + 60)
                {
                    n = x - 60;
                    y += 40;
                }
            }
		}

        protected void UpdateToolboxPaths()
        {
            toolboxController.Elements.ForEach(el => el.UpdatePath());
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
