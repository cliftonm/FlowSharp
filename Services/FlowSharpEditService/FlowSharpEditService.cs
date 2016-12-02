/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpEditService
{
    public class FlowSharpEditModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpEditService, FlowSharpEditService>();
        }
    }

    public class FlowSharpEditService : ServiceBase, IFlowSharpEditService
    {
        protected Dictionary<Keys, Action> keyActions = new Dictionary<Keys, Action>();
        protected TextBox editBox;
        protected Dictionary<BaseController, int> savePoints = new Dictionary<BaseController, int>();
        protected GraphicElement shapeBeingEdited;

        public override void Initialize(IServiceManager svcMgr)
        {
            base.Initialize(svcMgr);
        }

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            InitializeKeyIntercepts();
        }

        public void ClearSavePoints()
        {
            savePoints.Clear();
        }

        public void Copy()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (editBox != null)
            {
                Clipboard.SetText(editBox.SelectedText);
                return;
            }

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

        public void Paste()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

            // TODO: This seems klunky.
            if (editBox != null && Clipboard.ContainsText())
            {
                editBox.SelectedText = Clipboard.GetText();
                return;
            }

            string copyBuffer = Clipboard.GetData("FlowSharp")?.ToString();

            if (copyBuffer == null)
            {
                MessageBox.Show("Clipboard does not contain a FlowSharp shape", "Paste Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    List<GraphicElement> els = Persist.Deserialize(canvasController.Canvas, copyBuffer);
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

        public void Delete()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (canvasController.Canvas.Focused)
            {
                List<ZOrderMap> originalZOrder = canvasController.GetZOrderOfSelectedElements();
                List<GraphicElement> selectedElements = canvasController.SelectedElements.ToList();

                // TODO: Better implementation would be for the mouse controller to hook a shape deleted event?
                IFlowSharpMouseControllerService mouseController = ServiceManager.Get<IFlowSharpMouseControllerService>();
                canvasController.SelectedElements.ForEach(el => mouseController.ShapeDeleted(el));

                canvasController.UndoStack.UndoRedo("Delete",
                    () =>
                    {
                        canvasController.DeleteSelectedElementsHierarchy(false);
                    },
                    () =>
                    {
                        canvasController.RestoreZOrderWithHierarchy(originalZOrder);
                        RestoreConnections(originalZOrder);
                        canvasController.DeselectCurrentSelectedElements();
                        canvasController.SelectElements(selectedElements);
                    });
            }
        }

        public void Undo()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            canvasController.Undo();
        }

        public void Redo()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            canvasController.Redo();
        }

        public ClosingState CheckForChanges()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            ClosingState ret = ClosingState.NoChanges;

            if (GetSavePoint(canvasController) != canvasController.UndoStack.UndoStackSize)
            {
                DialogResult res = MessageBox.Show("Do you wish to save changes to this drawing?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (res)
                {
                    case DialogResult.Cancel:
                        ret = ClosingState.CancelClose;
                        break;

                    case DialogResult.Yes:
                        ret = ClosingState.SaveChanges;
                        break;

                    case DialogResult.No:
                        ret = ClosingState.ExitWithoutSaving;
                        break;
                }
            }

            return ret;
        }

        public void ResetSavePoint()
        {
            List<BaseController> controllers = new List<BaseController>();
            controllers.AddRange(savePoints.Keys);
            controllers.ForEach(c => savePoints[c] = 0);
        }

        public void SetSavePoint()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            SetSavePoint(canvasController);
        }

        public bool ProcessCmdKey(Keys keyData)
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            Action act;
            bool ret = false;

            if (editBox == null)
            {
                if (canvasController.Canvas.Focused && keyActions.TryGetValue(keyData, out act))
                {
                    act();
                    ret = true;
                }
                else
                {
                    if (canvasController.Canvas.Focused &&
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
                }
            }

            return ret;
        }

        public void EditText()
        {
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (canvasController.SelectedElements.Count == 1)
            {
                // TODO: At the moment, connectors do not support text.
                if (!canvasController.SelectedElements[0].IsConnector)
                {
                    shapeBeingEdited = canvasController.SelectedElements[0];
                    editBox = shapeBeingEdited.CreateTextBox(Cursor.Position);
                    canvasController.Canvas.Controls.Add(editBox);
                    editBox.Visible = true;
                    editBox.Focus();
                    editBox.KeyPress += OnEditBoxKey;
                    editBox.LostFocus += (sndr, args) => TerminateEditing();
                }
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

        protected void RestoreConnections(List<ZOrderMap> zomList)
        {
            foreach (ZOrderMap zom in zomList)
            {
                GraphicElement el = zom.Element;
                List<Connection> connections = zom.Connections;

                foreach (Connection conn in connections)
                {
                    conn.ToElement.SetConnection(conn.ToConnectionPoint.Type, el);
                }

                if (el.IsConnector)
                {
                    Connector connector = el as Connector;
                    connector.StartConnectedShape = zom.StartConnectedShape;
                    connector.EndConnectedShape = zom.EndConnectedShape;

                    if (connector.StartConnectedShape != null)
                    {
                        connector.StartConnectedShape.SetConnection(GripType.Start, connector);
                        connector.StartConnectedShape.Connections.Add(zom.StartConnection);
                    }

                    if (connector.EndConnectedShape != null)
                    {
                        connector.EndConnectedShape.SetConnection(GripType.End, connector);
                        connector.EndConnectedShape.Connections.Add(zom.EndConnection);
                    }
                }
            }
        }

        protected void InitializeKeyIntercepts()
        {
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
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

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
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
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
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
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

        protected bool CanStartEditing(Keys keyData)
        {
            bool ret = false;

            if (((keyData & Keys.Control) != Keys.Control) &&              // any control + key is not valid
                 ((keyData & Keys.Alt) != Keys.Alt) &&                       // any alt + key is not valid
                 ((keyData & Keys.Delete) != Keys.Delete))                  // DEL key is not valid, as it's assigned to deleting a shape
            {
                Keys k2 = (keyData & ~(Keys.Control | Keys.Shift | Keys.ShiftKey | Keys.Alt | Keys.Menu));

                if ((k2 != Keys.None) && (k2 < Keys.F1 || k2 > Keys.F12))
                {
                    // Here we assume we have a viable character.
                    // TODO: Probably more logic is required here.
                    ret = true;
                }
            }

            return ret;
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
            BaseController canvasController = ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

            if (editBox != null)
            {
                editBox.KeyPress -= OnEditBoxKey;
                string oldVal = shapeBeingEdited.Text;
                string newVal = editBox.Text;
                TextBox tb = editBox;
                editBox = null;     // set editBox to null so the remove, which fires a LoseFocus event, doesn't call into TerminateEditing again!
                shapeBeingEdited.EndEdit(newVal, oldVal);
                canvasController.Canvas.Controls.Remove(tb);
            }
        }

        protected int GetSavePoint(BaseController controller)
        {
            int ret;

            if (!savePoints.TryGetValue(controller, out ret))
            {
                savePoints[controller] = 0;
                ret = 0;
            }

            return ret;
        }

        protected void SetSavePoint(BaseController controller)
        {
            savePoints[controller] = controller.UndoStack.UndoStackSize;
        }
    }

    public class FlowSharpEditReceptor : IReceptor
    {
    }
}
