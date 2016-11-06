/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
	public class UIController
	{
		protected CanvasController canvasController;
		protected ElementProperties elementProperties;
		protected PropertyGrid pgElement;
        protected Action onFocus;

		public UIController(PropertyGrid pgElement, CanvasController canvasController)
		{
			this.pgElement = pgElement;
			this.canvasController = canvasController;
			canvasController.ElementSelected += ElementSelected;
			canvasController.UpdateSelectedElement += UpdateSelectedElement;
			pgElement.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
		}

		protected void ElementSelected(object controller, ElementEventArgs args)
		{
			elementProperties = null;

            // TODO: Get rid of this if statement by having an ElementDeselected event.
			if (args.Element != null)
			{
				elementProperties = args.Element.CreateProperties();
			}

			pgElement.SelectedObject = elementProperties;
			canvasController.Canvas.Focus();
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
            canvasController.SelectedElements.ForEach(sel =>
            {
                canvasController.Redraw(sel, el =>
                {
                    elementProperties.Update(el, e.ChangedItem.Label);
                    el.UpdateProperties();
                    el.UpdatePath();
                });
            });
		}
	}
}
