/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.Linq;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpHopeCommon;
using FlowSharpHopeShapeInterfaces;
using FlowSharpHopeServiceInterfaces;

using HopeShapes.PropertyGridHelpers;

namespace HopeShapes
{
    public class SemanticTypeShape : Box, ISemanticTypeShape
    {
        public SemanticTypeShape(Canvas canvas) : base(canvas)
        {
            Text = "SemType";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            FillBrush.Color = Color.LightGreen;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.Text = "SemType";
            FillBrush.Color = Color.LightGreen;

            return el;
        }

        public override void RightClick()
        {
            IServiceManager serviceManager = canvas.ServiceManager;
            IHigherOrderProgrammingService hope = serviceManager.Get<IHigherOrderProgrammingService>();
            hope.UnloadHopeAssembly();
            hope.LoadHopeAssembly();
            hope.InstantiateReceptors();
            PropertyContainer pc = hope.DescribeSemanticType(Text);
            CustomClass cls = new CustomClass();
            AddProperties(cls, pc);
            PublishSemanticType pst = new PublishSemanticType(Text, pc, cls, hope);
            pst.Show();
        }

        /// <summary>
        /// Creates a flat view of all value types and strings.
        /// Any PropertyData that has a non-null ChildType is a reference type.
        /// These are added to "CustomClass", which is used to create the PropertyGrid properties at runtime.
        /// </summary>
        protected void AddProperties(CustomClass cls, PropertyContainer pc, string root = "")
        {
            pc.Types.ForEach(pd =>
            {
                if (pd.ChildType == null)
                {
                    // Other interesting solutions for getting the default value:
                    // https://stackoverflow.com/questions/325426/programmatic-equivalent-of-defaulttype
                    Type type = Type.GetType(pd.TypeName);
                    object val = null;

                    if (type.Name != "String")
                    {
                        val = Activator.CreateInstance(type);
                    }

                    // TODO: Resolve property names that are the same.
                    cls.Add(new CustomProperty(root, pd.Name, val, pd.Description, pd.Category, type, false, true));
                }
                else
                {
                    AddProperties(cls, pd.ChildType, pd.Name);
                }
            });
        }
    }
}
