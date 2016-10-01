/* The MIT License (MIT)
* 
* Copyright (c) 2016 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

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

