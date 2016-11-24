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

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    public partial class FlowSharpUI
    {
        protected void Copy()
        {
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
                List<ZOrderMap> originalZOrder = canvasController.GetZOrderOfSelectedElements();
                List<GraphicElement> selectedElements = canvasController.SelectedElements.ToList();

                // TODO: Better implementation would be for the mouse controller to hook a shape deleted event?
                IFlowSharpMouseControllerService mouseController = Program.ServiceManager.Get<IFlowSharpMouseControllerService>();
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

        protected void UpdateCaption()
        {
            Text = "FlowSharp" + (String.IsNullOrEmpty(filename) ? "" : " - ") + filename;
        }
    }
}
