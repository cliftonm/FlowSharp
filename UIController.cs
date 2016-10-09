/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
	public class UIController
	{
		protected CanvasController canvasController;
		protected ElementProperties elementProperties;
		protected PropertyGrid pgElement;

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

			if (args.Element != null)
			{
				elementProperties = args.Element.CreateProperties();
			}

			pgElement.SelectedObject = elementProperties;
			canvasController.Canvas.Focus();
		}

		protected void UpdateSelectedElement(object controller, ElementEventArgs args)
		{
			elementProperties.UpdateFrom(args.Element);
			pgElement.Refresh();
		}

		protected void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			canvasController.Redraw(canvasController.SelectedElement, el =>
			{
				elementProperties.Update(el);
				el.UpdateProperties();
				el.UpdatePath();
			});
		}
	}
}
