/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpPropertyGridService
{
    public class PropertyGridController
    {
        protected ElementProperties elementProperties;
        protected PropertyGrid pgElement;
        protected Action onFocus;
        protected IServiceManager serviceManager;

        public PropertyGridController(IServiceManager serviceManager, PropertyGrid pgElement)
        {
            this.serviceManager = serviceManager;
            this.pgElement = pgElement;
            pgElement.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
        }

        public void HookEvents(BaseController canvasController)
        {
            canvasController.ElementSelected += ElementSelected;
            canvasController.UpdateSelectedElement += UpdateSelectedElement;
        }

        public void UnhookEvents(BaseController canvasController)
        {
            canvasController.ElementSelected -= ElementSelected;
            canvasController.UpdateSelectedElement -= UpdateSelectedElement;
        }

        public void Show(IPropertyObject obj)
        {
            pgElement.SelectedObject = obj;
            serviceManager.Get<IFlowSharpCanvasService>().ActiveController.Canvas.Focus();
        }

        protected void ElementSelected(object controller, ElementEventArgs args)
        {
            elementProperties = null;

            // TODO: Get rid of this if statement by having an ElementDeselected event.
            if (args.Element != null)
            {
                elementProperties = args.Element.CreateProperties();
                pgElement.SelectedObject = elementProperties;
            }

            serviceManager.Get<IFlowSharpCanvasService>().ActiveController.Canvas.Focus();
        }

        protected void UpdateSelectedElement(object controller, ElementEventArgs args)
        {
            // TODO: For some reason this event was being fired for either a null element or elementProperties was null.
            // It would be nice to figure out why this happened and correct the root problem so we can remove this if statement.
            if (args.Element != null && elementProperties != null)
            {
                elementProperties.UpdateFrom(args.Element);
                pgElement.Refresh();
            }
        }

        protected void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            BaseController canvasController = serviceManager.Get<IFlowSharpCanvasService>().ActiveController;
            string label = e.ChangedItem.Label;

            // Updating a shape.
            if (pgElement.SelectedObject is ElementProperties)
            {
                canvasController.SelectedElements.ForEach(sel =>
                {
                    PropertyInfo piElProps = elementProperties.GetType().GetProperty(label);
                    object oldVal = e.OldValue;
                    object newVal = piElProps.GetValue(elementProperties);

                    canvasController.UndoStack.UndoRedo("Update " + label,
                        () =>
                        {
                            canvasController.Redraw(sel, el =>
                            {
                                piElProps.SetValue(elementProperties, newVal);
                                elementProperties.Update(el, label);
                                el.UpdateProperties();
                                el.UpdatePath();
                                pgElement.Refresh();
                            });
                        },
                        () =>
                        {
                            canvasController.Redraw(sel, el =>
                            {
                                piElProps.SetValue(elementProperties, oldVal);
                                elementProperties.Update(el, label);
                                el.UpdateProperties();
                                el.UpdatePath();
                                pgElement.Refresh();
                            });
                        }, false);
                });

                canvasController.UndoStack.FinishGroup();

                // Return focus to the canvas so that keyboard actions, like copy/paste, undo/redo, are intercepted
                // TODO: Seems really kludgy.
                Task.Delay(250).ContinueWith(t =>
                    pgElement.FindForm().BeginInvoke(() => canvasController.Canvas.Focus())
                );
            }
            else
            {
                // Updating canvas properties
                (pgElement.SelectedObject as IPropertyObject).Update(label);
            }
        }
    }
}
