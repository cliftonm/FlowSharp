/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            RunLint(canvasController);
        }

        protected void RunLint(BaseController canvasController)
        {
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            outputWindow.Clear();

            foreach (GraphicElement elClass in canvasController.Elements.Where(el => el is IPythonClass).OrderBy(el => ((IPythonClass)el).Filename))
            {
                string filename = ((IPythonClass)elClass).Filename;
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.FileName = "pylint";
                p.StartInfo.Arguments = filename;
                p.StartInfo.CreateNoWindow = true;

                outputWindow.WriteLine(filename);

                List<string> warnings = new List<string>();
                List<string> errors = new List<string>();

                p.OutputDataReceived += (sndr, args) =>
                {
                    string line = args.Data;

                    if (line != null)
                    {
                        if (line.StartsWith("W:"))
                        {
                            warnings.Add(line);
                        }

                        if (line.StartsWith("E:"))
                        {
                            errors.Add(line);
                        }
                    }
                };

                // p.ErrorDataReceived += (sndr, args) => outputWindow.WriteLine(args.Data);

                p.Start();

                // Interestingly, this has to be called after Start().
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

                warnings.ForEach(w => outputWindow.WriteLine(w));
                errors.ForEach(e => outputWindow.WriteLine(e));

                if (warnings.Count + errors.Count > 0)
                {
                    // Cosmetic - separate filenames by a whitespace if a file has warnings/errors.
                    outputWindow.WriteLine("");
                }
            }
        }

        protected bool ContainedIn<T>(BaseController canvasController, GraphicElement child)
        {
            return canvasController.Elements.Any(el => el is T && el.DisplayRectangle.Contains(child.DisplayRectangle));
        }

        protected bool CompileClassSources(BaseController canvasController)
        {
            bool ok = true;
            const string PYLINT = "#pylint: disable=C0111, C0301, C0303, W0311, W0614, W0401, W0232";

            foreach (GraphicElement elClass in canvasController.Elements.Where(el => el is IPythonClass))
            {
                List<string> imports = GetImports(canvasController, elClass);
                List<string> classSources = GetClassSources(canvasController, elClass);
                string filename = ((IPythonClass)elClass).Filename;
                string className = filename.LeftOf(".");
                StringBuilder sb = new StringBuilder();

                // If we have class sources, then we're building the full source file, replacing whatever is in the "class" shape.
                if (classSources.Count > 0)
                {
                    elClass.Json["python"] = "";
                    sb.AppendLine(PYLINT);
                }

                imports.Where(src => !String.IsNullOrEmpty(src)).ForEach(src =>
                {
                    string[] lines = src.Split('\n');
                    lines.ForEach(line => sb.AppendLine(line.TrimEnd()));
                });

                // Don't create the class definition if there's no functions defined in the class.
                if (classSources.Count > 0)
                {
                    // Formatting.
                    if (imports.Count > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine("class " + className + ":");

                    classSources.Where(src => !String.IsNullOrEmpty(src)).ForEach(src =>
                    {
                        List<string> lines = src.Split('\n').ToList();
                        // Formatting: remove all blank lines from end of each source file.
                        lines = ((IEnumerable<string>)lines).Reverse().SkipWhile(line => String.IsNullOrWhiteSpace(line)).Reverse().ToList();
                        lines.ForEach(line => sb.AppendLine("  " + line.TrimEnd()));
                        sb.AppendLine();
                    });
                }
                else
                {
                    // If there's no classes, then use whatever is in the actual class shape for code, however we need to add/replace the #pylint line with 
                    // whatever the current list of ignores are.
                    string src = elClass.Json["python"] ?? "";
                    string[] lines = src.Split('\n');

                    if (lines.Length > 0 && lines[0].StartsWith("#pylint"))
                    {
                        // Remove the existing pylint options line.
                        src = String.Join("\n", lines.Skip(1));
                    }

                    // Insert pylint options as the first line before any imports.
                    sb.Insert(0, PYLINT + "\n");

                    sb.Append(src);
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
                Where(srcEl => srcEl != elClass && (srcEl.Text ?? "").ToLower() != "imports" && srcEl.Json.ContainsKey("python") && elClass.DisplayRectangle.
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
