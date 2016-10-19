/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;

namespace FlowSharpLib
{
    public class GroupBox : Box
    {
        public GroupBox(Canvas canvas) : base(canvas)
		{
        }

        public override List<ShapeAnchor> GetAnchors()
        {
            // GroupBox doesn't have anchors - it can't be resized.
            return new List<ShapeAnchor>();
        }

        public override void Move(Point delta)
        {
            base.Move(delta);

            GroupChildren.ForEach(g =>
            {
                g.Move(delta);
                g.UpdatePath();
            });
        }
    }
}
