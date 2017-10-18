using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpHopeShapeInterfaces;
using FlowSharpServiceInterfaces;

namespace FlowSharpHopeService
{
    public class Animator
    {
        protected enum FromShapeType
        {
            SemanticType,
            Receptor,
        }

        protected IServiceManager serviceManager;
        protected List<GraphicElement> carriers;

        public Animator(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
            carriers = new List<GraphicElement>();
        }

        public void Animate(object sender, HopeRunnerAppDomainInterface.ProcessEventArgs args)
        {
            IFlowSharpCanvasService canvasService = serviceManager.Get<IFlowSharpCanvasService>();
            BaseController canvasController = canvasService.ActiveController;
            var (sourceShapeName, shapeType) = GetSourceShapeName(args);
            GraphicElement elSrc = null;
            GraphicElement elDest = null;

            lock (this)
            {
                elSrc = GetElement(canvasController, sourceShapeName.RightOf('.'), shapeType);
                elDest = GetElement(canvasController, args.ToReceptorTypeName.RightOf('.'), FromShapeType.Receptor);
            }

            CarrierShape carrier = new CarrierShape(canvasController.Canvas);
            carrier.DisplayRectangle = new Rectangle(elSrc.DisplayRectangle.Center().X, elSrc.DisplayRectangle.Center().Y, 10, 10);

            canvasController.Canvas.FindForm().BeginInvoke(() =>
            {
                lock (this)
                {
                    carriers.Add(carrier);
                    canvasController.Insert(carrier);
                }
            });

            double dx = elDest.DisplayRectangle.Center().X - elSrc.DisplayRectangle.Center().X;
            double dy = elDest.DisplayRectangle.Center().Y - elSrc.DisplayRectangle.Center().Y;
            double steps = 20;
            double subx = dx / steps;
            double suby = dy / steps;
            double px = elSrc.DisplayRectangle.Center().X;
            double py = elSrc.DisplayRectangle.Center().Y;

            for (int i = 0; i < steps; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(25);
                px += subx;
                py += suby;

                canvasController.Canvas.FindForm().BeginInvoke(() =>
                {
                    lock (this)
                    {
                        Assert.SilentTry(() => canvasController.MoveElementTo(carrier, new Point((int)px, (int)py)));
                    }
                });
            }

            canvasController.Canvas.FindForm().BeginInvoke(() =>
            {
                lock (this)
                {
                    carriers.Remove(carrier);
                    Assert.SilentTry(() => canvasController.DeleteElement(carrier));
                }
            });
        }

        public void RemoveCarriers()
        {
            lock (this)
            {
                IFlowSharpCanvasService canvasService = serviceManager.Get<IFlowSharpCanvasService>();
                BaseController canvasController = canvasService.ActiveController;
                carriers.ForEach(c => canvasController.DeleteElement(c));
                carriers.Clear();
            }
        }

        protected (string name, FromShapeType shapeType) GetSourceShapeName(HopeRunnerAppDomainInterface.ProcessEventArgs args)
        {
            string sourceShapeName;
            FromShapeType shapeType;

            if (String.IsNullOrEmpty(args.FromReceptorTypeName))
            {
                sourceShapeName = args.SemanticTypeTypeName;
                shapeType = FromShapeType.SemanticType;
            }
            else
            {
                sourceShapeName = args.FromReceptorTypeName;
                shapeType = FromShapeType.Receptor;
            }

            return (sourceShapeName, shapeType);
        }

        protected GraphicElement GetElement(BaseController canvasController, string name, FromShapeType shapeType)
        {
            GraphicElement el = null;

            switch (shapeType)
            {
                case FromShapeType.Receptor:
                    el = canvasController.Elements.Single(e => e.Text?.RemoveWhitespace().ToLower() == name.ToLower() && e is IAgentReceptor);
                    break;

                case FromShapeType.SemanticType:
                    el = canvasController.Elements.Single(e => e.Text?.RemoveWhitespace().ToLower() == name.ToLower());
                    break;
            }

            return el;
        }
    }
}
