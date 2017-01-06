/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;

namespace FlowSharpLib
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcludeFromToolboxAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class ToolboxShapeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class ToolboxOrderAttribute : Attribute
    {
        public int Order { get; protected set; }

        public ToolboxOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
