/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpLib
{
    [ExcludeFromToolbox]
    public class GroupBox : Box
    {
        public enum CollapseState
        {
            Collapsed,
            Expanded,
        }

        public CollapseState State { get; protected set; }

        public Size ExpandedSize { get; protected set; }

        public GroupBox(Canvas canvas) : base(canvas)
		{
            FillBrush.Color = Color.FromArgb(240, 240, 240);
            State = CollapseState.Expanded;
        }

        public override List<ShapeAnchor> GetAnchors()
        {
            // GroupBox doesn't have anchors - it can't be resized.
            return new List<ShapeAnchor>();
        }

        public void SetCollapsedState()
        {
            State = CollapseState.Collapsed;
        }

        public void SaveExpandedSize()
        {
            ExpandedSize = DisplayRectangle.Size;
        }

        public void SetExpandedState()
        {
            State = CollapseState.Expanded;
        }

        public override void Move(Point delta)
        {
            base.Move(delta);

            GroupChildren.ForEach(g =>
            {
                g.Move(delta);
                g.UpdatePath();

                // TODO: Kludgy workaround for issue #34.
                if (!canvas.Controller.IsCanvasDragging)
                {
                    //g.Connections.Where(c => c.ToElement.Parent == null).ForEach(c => c.ToElement.MoveAnchor(c.ToConnectionPoint.Type, delta));
                    // Issue #56
                    g.Connections.Where(c => c.ToElement.Parent == null).ForEach(c => canvas.Controller.MoveLineOrAnchor(c, delta));
                }
            });
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
        }
    }
}
