/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Linq;
using System.Reflection;

using Clifton.Core.Utils;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpRestService
{
    public class CommandProcessor : IReceptor
    {
        // Ex: localhost:8001:flowsharp?cmd=CmdUpdateProperty&ShapeName=btnTest&PropertyName=Text&Value=Foobar
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdUpdateProperty cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;

            GraphicElement el = controller.Elements.SingleOrDefault(e => e.Name == cmd.ShapeName);
            PropertyInfo pi = el.GetType().GetProperty(cmd.PropertyName);
            object cval = Converter.Convert(cmd.Value, pi.PropertyType);

            el?.Canvas.Invoke(() =>
            {
                pi.SetValue(el, cval);
                controller.Redraw(el);
            });
        }
    }
}