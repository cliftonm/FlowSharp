/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace FlowSharpLib
{
	public abstract class Connector : GraphicElement
	{
		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public GraphicElement StartConnectedShape { get; set; }
		public GraphicElement EndConnectedShape { get; set; }

        public override bool IsConnector { get { return true; } }
        public bool ShowConnectorAsSelected { get; set; }

        // See CustomLineCap for creating other possible endcaps besides arrows.
        // Note that AdjustableArrowCap derives from CustomLineCap!
        // https://msdn.microsoft.com/en-us/library/system.drawing.drawing2d.customlinecap(v=vs.110).aspx
        protected AdjustableArrowCap adjCapArrow;
        protected AdjustableArrowCap adjCapDiamond;

        protected GripType[] startGrips = new GripType[] { GripType.Start, GripType.TopMiddle, GripType.LeftMiddle };

		public Connector(Canvas canvas) : base(canvas)
		{
            adjCapArrow = new AdjustableArrowCap(BaseController.CAP_WIDTH, BaseController.CAP_HEIGHT, true);
            adjCapDiamond = new AdjustableArrowCap(BaseController.CAP_WIDTH, BaseController.CAP_HEIGHT, true);
            adjCapDiamond.MiddleInset = -BaseController.CAP_WIDTH;
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
		{
			base.Serialize(epb, elementsBeingSerialized);
			epb.StartCap = StartCap;
			epb.EndCap = EndCap;

            // Don't assign connected shape ID to partial copy and paste selection where target is not in elements being serialized.

            if (elementsBeingSerialized.Contains(StartConnectedShape))
            {
                epb.StartConnectedShapeId = StartConnectedShape?.Id ?? Guid.Empty;
            }

            if (elementsBeingSerialized.Contains(EndConnectedShape))
            {
                epb.EndConnectedShapeId = EndConnectedShape?.Id ?? Guid.Empty;
            }
		}

		public override void Deserialize(ElementPropertyBag epb)
		{
			base.Deserialize(epb);
			StartCap = epb.StartCap;
			EndCap = epb.EndCap;
		}

		public override void FinalFixup(List<GraphicElement> elements, ElementPropertyBag epb, Dictionary<Guid, Guid> oldNewGuidMap)
		{
			base.FinalFixup(elements, epb, oldNewGuidMap);

            if (epb.StartConnectedShapeId != Guid.Empty)
            {
                StartConnectedShape = elements.SingleOrDefault(e => e.Id == oldNewGuidMap[epb.StartConnectedShapeId]);
            }

            if (epb.EndConnectedShapeId != Guid.Empty)
            {
                EndConnectedShape = elements.SingleOrDefault(e => e.Id == oldNewGuidMap[epb.EndConnectedShapeId]);
            }
		}

		public override void SetConnection(GripType gt, GraphicElement shape)
		{
			(gt == GripType.Start).If(() => StartConnectedShape = shape).Else(() => EndConnectedShape = shape);
		}

		public override void RemoveConnection(GripType gt)
		{
			(gt.In(startGrips)).If(() => StartConnectedShape = null).Else(() => EndConnectedShape = null);
		}

		public override void DisconnectShapeFromConnector(GripType gt)
		{
			(gt.In(startGrips)).IfElse(
				() => StartConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this)),      // If
				() => EndConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this)));       // Else
		}

		public override void DetachAll()
		{
			StartConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this));
			EndConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this));
            StartConnectedShape = null;
            EndConnectedShape = null;
		}
	}
}

