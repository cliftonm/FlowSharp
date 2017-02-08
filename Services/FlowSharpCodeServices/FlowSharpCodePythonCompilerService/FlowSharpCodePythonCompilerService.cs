using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.CSharp;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceManagement;

using FlowSharpCodeServiceInterfaces;
using FlowSharpCodeShapeInterfaces;
using FlowSharpServiceInterfaces;
using FlowSharpLib;

namespace FlowSharpCodeCompilerService
{
    public class FlowSharpCodePythonCompilerModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCodePythonCompilerService, FlowSharpCodePythonCompilerService>();
        }
    }

    public class FlowSharpCodePythonCompilerService : ServiceBase, IFlowSharpCodePythonCompilerService
    {
        protected Dictionary<string, string> tempToTextBoxMap = new Dictionary<string, string>();

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            InitializeBuildMenu();
        }

        public void Compile()
        {
        }

        protected void InitializeBuildMenu()
        {
            ToolStripMenuItem buildToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem mnuCompile = new ToolStripMenuItem();

            mnuCompile.Name = "mnuCompile";
            // mnuCompile.ShortcutKeys = Keys.Alt | Keys.C;
            mnuCompile.Size = new System.Drawing.Size(165, 24);
            mnuCompile.Text = "&Compile";

            buildToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuCompile });
            buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            buildToolStripMenuItem.Size = new System.Drawing.Size(37, 21);
            buildToolStripMenuItem.Text = "Python";

            mnuCompile.Click += OnCompile;

            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(buildToolStripMenuItem);
        }

        protected void OnCompile(object sender, EventArgs e)
        {
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            outputWindow.Clear();
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            BaseController canvasController = canvasService.ActiveController;
            // List<GraphicElement> rootSourceShapes = GetSources(canvasController);
            CompileClassSources(canvasController);
        }

        /*
        protected List<GraphicElement> GetSources(BaseController canvasController)
        {
            List<GraphicElement> sourceList = new List<GraphicElement>();

            foreach (GraphicElement srcEl in canvasController.Elements.Where(
                srcEl => !ContainedIn<IAssemblyBox>(canvasController, srcEl) &&
                !(srcEl is IFileBox)))
            {
                sourceList.Add(srcEl);
            }

            return sourceList;
        }
        */

        protected bool ContainedIn<T>(BaseController canvasController, GraphicElement child)
        {
            return canvasController.Elements.Any(el => el is T && el.DisplayRectangle.Contains(child.DisplayRectangle));
        }

        protected bool CompileClassSources(BaseController canvasController)
        {
            bool ok = true;

            foreach (GraphicElement elClass in canvasController.Elements.Where(el => el is IPythonClass))
            {
                List<string> imports = GetImports(canvasController, elClass);
                List<string> classSources = GetClassSources(canvasController, elClass);
                string filename = ((IPythonClass)elClass).Filename;
                string className = filename.LeftOf(".");
                StringBuilder sb = new StringBuilder();

                imports.Where(src => !String.IsNullOrEmpty(src)).ForEach(src =>
                {
                    string[] lines = src.Split('\n');
                    lines.ForEach(line => sb.AppendLine(line.TrimEnd()));
                });

                sb.AppendLine();

                // Don't create the class definition if there's no functions defined in the class.
                if (classSources.Count > 0)
                {
                    sb.AppendLine("class " + className + ":");

                    classSources.Where(src => !String.IsNullOrEmpty(src)).ForEach(src =>
                    {
                        List<string> lines = src.Split('\n').ToList();
                        // Remove all blank lines from end of each source file.
                        lines = ((IEnumerable<string>)lines).Reverse().SkipWhile(line => String.IsNullOrWhiteSpace(line)).Reverse().ToList();
                        lines.ForEach(line => sb.AppendLine("  " + line.TrimEnd()));
                        sb.AppendLine();
                    });
                }
                else
                {
                    // If there's no classes, then use whatever is in the actual class shape for code.
                    sb.Append(elClass.Json["python"] ?? "");
                }

                File.WriteAllText(filename, sb.ToString());
                elClass.Json["python"] = sb.ToString();
            }

            return ok;
        }

        /// <summary>
        /// Returns sources contained in an element (ie., AssemblyBox shape).
        /// </summary>
        protected List<string> GetImports(BaseController canvasController, GraphicElement elClass)
        {
            return canvasController.Elements.
                Where(srcEl => srcEl != elClass && (srcEl.Text ?? "").ToLower() == "imports" && elClass.DisplayRectangle.
                Contains(srcEl.DisplayRectangle)).
                OrderBy(srcEl => srcEl.DisplayRectangle.Y).
                Select(srcEl => srcEl.Json["python"] ?? "").
                ToList();
        }

        /// <summary>
        /// Returns sources contained in an element (ie., AssemblyBox shape).
        /// </summary>
        protected List<string> GetClassSources(BaseController canvasController, GraphicElement elClass)
        {
            return canvasController.Elements.
                Where(srcEl => srcEl != elClass && (srcEl.Text ?? "").ToLower() != "imports" && elClass.DisplayRectangle.
                Contains(srcEl.DisplayRectangle)).
                OrderBy(srcEl=>srcEl.DisplayRectangle.Y).
                Select(srcEl => srcEl.Json["python"] ?? "").
                ToList();
        }

        protected string GetCode(GraphicElement el)
        {
            string code;
            el.Json.TryGetValue("python", out code);

            return code ?? "";
        }
    }
}
