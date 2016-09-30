using System;
using System.Collections.Generic;
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

        protected GripType[] startGrips = new GripType[] { GripType.Start, GripType.TopMiddle, GripType.LeftMiddle };

		public Connector(Canvas canvas) : base(canvas)
		{
		}

		public override void Serialize(ElementPropertyBag epb)
		{
			base.Serialize(epb);
			epb.StartCap = StartCap;
			epb.EndCap = EndCap;
			epb.StartConnectedShapeId = StartConnectedShape?.Id ?? Guid.Empty;
			epb.EndConnectedShapeId = EndConnectedShape?.Id ?? Guid.Empty;
		}

		public override void Deserialize(ElementPropertyBag epb)
		{
			base.Deserialize(epb);
			StartCap = epb.StartCap;
			EndCap = epb.EndCap;
		}

		public override void FinalFixup(List<GraphicElement> elements, ElementPropertyBag epb)
		{
			base.FinalFixup(elements, epb);
			StartConnectedShape = elements.SingleOrDefault(e => e.Id == epb.StartConnectedShapeId);
			EndConnectedShape = elements.SingleOrDefault(e => e.Id == epb.EndConnectedShapeId);
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
				() => StartConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this)),
				() => EndConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this)));
		}

		public override void DetachAll()
		{
			StartConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this));
			EndConnectedShape.IfNotNull(el => el.Connections.RemoveAll(c => c.ToElement == this));
		}
	}
}

