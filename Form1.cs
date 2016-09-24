using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlowSharp
{
    public partial class Form1 : Form
    {
		protected CanvasController controller;
		protected ElementProperties elementProperties;

		public Form1()
        {
            InitializeComponent();
            Shown += OnShown;
        }

        public void OnShown(object sender, EventArgs e)
        { 
            Canvas canvas = Canvas.Initialize(pnlCanvas);
			List<GraphicElement> elements = new List<GraphicElement>();
			controller = new CanvasController(canvas, elements);
            elements.Add(new Box(canvas) { DisplayRectangle = new Rectangle(25, 50, 200, 25) });
            elements.Add(new Box(canvas) { DisplayRectangle = new Rectangle(225, 250, 100, 25) });
            elements.Add(new Ellipse(canvas) { DisplayRectangle = new Rectangle(125, 100, 100, 75) });
			elements.Add(new Diamond(canvas) { DisplayRectangle = new Rectangle(325, 100, 40, 40) });
			elements.ForEach(el => el.UpdatePath());
			controller.ElementSelected += ElementSelected;
			controller.UpdateSelectedElement += UpdateSelectedElement;
		}

		protected void ElementSelected(object controller, ElementEventArgs args)
		{
			elementProperties = null;

			if (args.Element != null)
			{
				elementProperties = new ElementProperties(args.Element);
			}

			pgElement.SelectedObject = elementProperties;
		}

		protected void UpdateSelectedElement(object controller, ElementEventArgs args)
		{
			elementProperties.UpdateFrom(args.Element);
			pgElement.Refresh();
		}

		protected void pgElement_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			controller.Redraw(controller.SelectedElement, el=>
			{
				elementProperties.Update(el);
				el.UpdatePath();
			});
		}


	}
}
