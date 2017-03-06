﻿/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using Clifton.Core.Utils;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;

using FlowSharpLib;
using FlowSharpServiceInterfaces;
using FlowSharpCodeServiceInterfaces;
using FlowSharpCodeShapeInterfaces;

namespace FlowSharpRestService
{
    public class CommandProcessor : IReceptor
    {
        // Ex: localhost:8001/flowsharp?cmd=CmdUpdateProperty&Name=btnTest&PropertyName=Text&Value=Foobar
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdUpdateProperty cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            var els = controller.Elements.Where(e => e.Name == cmd.Name);

            els.ForEach(el =>
            {
                PropertyInfo pi = el.GetType().GetProperty(cmd.PropertyName);
                object cval = Converter.Convert(cmd.Value, pi.PropertyType);

                el?.Canvas.Invoke(() =>
                {
                    pi.SetValue(el, cval);
                    controller.Redraw(el);
                });
            });
        }

        // Ex: localhost:8001:flowsharp?cmd=CmdShowShape&Name=btnTest
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdShowShape cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            var el = controller.Elements.Where(e => e.Name == cmd.Name).FirstOrDefault();

            el?.Canvas.Invoke(() =>
            {
                controller.FocusOn(el);
            });
        }

        // Ex: localhost:8001/flowsharp?cmd=CmdGetShapeFiles
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdGetShapeFiles cmd)
        {
            BaseController controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            var els = controller.Elements.Where(e => e is IFileBox);
            cmd.Filenames.AddRange(els.Cast<IFileBox>().Where(el => !String.IsNullOrEmpty(el.Filename)).Select(el => el.Filename));
        }

        // FlowSharpCodeOutputWindowService required for this behavior.
        // Ex: localhost:8001/flowsharp?cmd=CmdOutputMessage&Text=foobar
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdOutputMessage cmd)
        {
            var w = proc.ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            cmd.Text.Split('\n').Where(s=>!String.IsNullOrEmpty(s.Trim())).ForEach(s => w.WriteLine(s.Trim()));
        }

        // Ex: localhost:8001/flowsharp?cmd=CmdDropShape&ShapeName=Box&X=50&Y=100
        // Ex: localhost:8001/flowsharp?cmd=CmdDropShape&ShapeName=Box&X=50&Y=100&Text=Foobar&FillColor=!FF00ff&Width=300
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdDropShape cmd)
        {
            List<Type> shapes = proc.ServiceManager.Get<IFlowSharpToolboxService>().ShapeList;
            var controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            Type t = shapes.Where(s => s.Name == cmd.ShapeName).SingleOrDefault();

            if (t != null)
            {
                GraphicElement el = (GraphicElement)Activator.CreateInstance(t, new object[] { controller.Canvas });
                el.DisplayRectangle = new Rectangle(cmd.X, cmd.Y, cmd.Width ?? el.DefaultRectangle().Width, cmd.Height ?? el.DefaultRectangle().Height);
                el.Name = cmd.Name;
                el.Text = cmd.Text;

                cmd.FillColor.IfNotNull(c=> el.FillColor = GetColor(c));
                cmd.BorderColor.IfNotNull(c => el.BorderPenColor = GetColor(c));
                cmd.TextColor.IfNotNull(c => el.TextColor = GetColor(c));

                el.UpdateProperties();
                el.UpdatePath();
                controller.Insert(el);
            }
        }

        // Ex: localhost:8001/flowsharp?cmd=CmdDropConnector&ConnectorName=DiagonalConnector&X1=50&Y1=100&X2=150&Y2=150
        public void Process(ISemanticProcessor proc, IMembrane membrane, CmdDropConnector cmd)
        {
            List<Type> shapes = proc.ServiceManager.Get<IFlowSharpToolboxService>().ShapeList;
            var controller = proc.ServiceManager.Get<IFlowSharpCanvasService>().ActiveController;
            Type t = shapes.Where(s => s.Name == cmd.ConnectorName).SingleOrDefault();

            if (t != null)
            {
                DynamicConnector el = (DynamicConnector)Activator.CreateInstance(t, new object[] { controller.Canvas });
                // el = (DynamicConnector)el.CloneDefault(controller.Canvas, new Point(cmd.X1, cmd.Y1));
                // el = (DynamicConnector)el.CloneDefault(controller.Canvas);

                el.Name = cmd.Name;
                el.StartPoint = new Point(cmd.X1, cmd.Y1);
                el.EndPoint = new Point(cmd.X2, cmd.Y2);
                int x1 = cmd.X1.Min(cmd.X2);
                int y1 = cmd.Y1.Min(cmd.Y2);
                int x2 = cmd.X1.Max(cmd.X2);
                int y2 = cmd.Y1.Max(cmd.Y2);
                el.DisplayRectangle = new Rectangle(x1, y1, x2 - x1, y2 - y1);

                el.UpdatePath();
                controller.Insert(el);
            }
        }

        protected Color GetColor(string colorString)
        {
            Color color;

            // Get the color from its name or an RGB value as hex codes #RRGGBB
            if (colorString[0] == '!')
            {
                color = ColorTranslator.FromHtml("#" + colorString.Substring(1));
            }
            else
            {
                color = Color.FromName(colorString);
            }

            return color;
        }
    }
}