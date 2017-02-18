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

    public class PythonCodeGeneratorService : ICodeGeneratorService
    {
        public StringBuilder CodeResult { get; protected set; }

        protected int indent = 0;

        public PythonCodeGeneratorService()
        {
            CodeResult = new StringBuilder();
        }

        public void BeginIf(string code)
        {
            CodeResult.Append(new string(' ', indent));
            CodeResult.AppendLine("if " + code + ":");
            indent += 4;
        }

        public void Else()
        {
            indent = 0.MaxDelta(indent - 4);
            CodeResult.Append(new string(' ', indent));
            CodeResult.AppendLine("else:");
            indent += 4;
        }

        public void EndIf()
        {
            indent = 0.MaxDelta(indent - 4);
        }

        public void BeginFor(string code)
        {
            CodeResult.Append(new string(' ', indent));
            CodeResult.AppendLine("for " + code + ":");
            indent += 4;
        }

        public void EndFor()
        {
            indent = 0.MaxDelta(indent - 4);
        }

        public void Statement(string code)
        {
            CodeResult.Append(new string(' ', indent));
            CodeResult.AppendLine(code);
        }
    }

    public class FlowSharpCodePythonCompilerService : ServiceBase, IFlowSharpCodePythonCompilerService
    {
        protected Dictionary<string, string> tempToTextBoxMap = new Dictionary<string, string>();
        private const string PYLINT = "#pylint: disable=C0111, C0301, C0303, W0311, W0614, W0401, W0232";

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
            ToolStripMenuItem mnuRun = new ToolStripMenuItem();

            mnuCompile.Name = "mnuPythonCompile";
            // mnuCompile.ShortcutKeys = Keys.Alt | Keys.C;
            mnuCompile.Text = "&Compile";

            mnuRun.Name = "mnuPythonRun";
            // mnuCompile.ShortcutKeys = Keys.Alt | Keys.C;
            mnuRun.Text = "&Run";

            buildToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuCompile, mnuRun });
            buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            buildToolStripMenuItem.Size = new System.Drawing.Size(37, 21);
            buildToolStripMenuItem.Text = "Python";

            mnuCompile.Click += OnCompile;
            mnuRun.Click += OnRun;

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

        protected void OnRun(object sender, EventArgs e)
        {
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            outputWindow.Clear();
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            BaseController canvasController = canvasService.ActiveController;

            // One and only one Python class element must be selected.
            if (canvasController.SelectedElements.Count == 1)
            {
                var el = canvasController.SelectedElements[0];

                if (el is IPythonClass)
                {
                    // TODO: Unify with FlowSharpCodeCompilerService.Run
                    string filename = ((IPythonClass)el).Filename;
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.FileName = "python";
                    p.StartInfo.Arguments = filename;
                    p.StartInfo.CreateNoWindow = true;      // TODO: useful for console apps, not so good for WinForm apps?

                    p.OutputDataReceived += (sndr, args) => outputWindow.WriteLine(args.Data);
                    p.ErrorDataReceived += (sndr, args) => outputWindow.WriteLine(args.Data);

                    // This unfortunately doesn't work!
                    // TODO: p.EnableRaisingEvents = true should enable this.
                    // p.Exited += (object sender, EventArgs e) => runningProcess = null;

                    p.Start();

                    // Interestingly, this has to be called after Start().
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }
                else
                {
                    outputWindow.WriteLine("Please select a Python class file to run.");
                }
            }
            else
            {
                outputWindow.WriteLine("Please select a single Python class file to run.");
            }
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

            foreach (GraphicElement elClass in canvasController.Elements.Where(el => el is IPythonClass))
            {
                List<string> imports = GetImports(canvasController, elClass);
                List<string> classSources = GetClassSources(canvasController, elClass);
                string filename = ((IPythonClass)elClass).Filename;

                if (String.IsNullOrEmpty(filename))
                {
                    filename = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".py";
                }

                string className = filename.LeftOf(".");
                bool hasDrakonShapes = HasDrakonShapes(canvasController, elClass);

                if ((classSources.Count > 0 || imports.Count > 0) && !(hasDrakonShapes))
                {
                    ok = BuildClassFromCodeBoxes(elClass, className, filename, classSources, imports);
                }
                else if (hasDrakonShapes)
                {
                    // Embedded code, not represented as a workflow, which is a special case.  However, the processing is very much the same.
                    DrakonWorkflowCodeGenerator(canvasController, elClass, className, filename);
                }
                else
                {
                    BuildModuleFromModuleSource(elClass, className, filename);
                }
            }

            return ok;
        }

        protected void DrakonWorkflowCodeGenerator(BaseController canvasController, GraphicElement elClass, string className, string filename)
        {
            IFlowSharpCodeService codeService = ServiceManager.Get<IFlowSharpCodeService>();
            GraphicElement el = codeService.FindStartOfWorkflow(canvasController, elClass);
            DrakonCodeGenerator dcg = new DrakonCodeGenerator();
            ParseDrakonWorkflow(dcg, codeService, canvasController, el);
            var codeGenSvc = new PythonCodeGeneratorService();
            dcg.GenerateCode(codeGenSvc);
            codeGenSvc.CodeResult.Insert(0, PYLINT + "\n");
            File.WriteAllText(filename, codeGenSvc.CodeResult.ToString());
            elClass.Json["python"] = codeGenSvc.CodeResult.ToString();
        }

        /// <summary>
        /// Return the structure of the Drakon code from the workflow.
        /// </summary>
        /// <param name="codeService"></param>
        /// <param name="canvasController"></param>
        /// <param name="el">The start element</param>
        /// <returns></returns>
        protected void ParseDrakonWorkflow(DrakonCodeGenerator dcg, IFlowSharpCodeService codeService, BaseController canvasController, GraphicElement el, bool inCondition = false)
        {
            while (el != null)
            {
                // If we're in a conditional and we encounter a shape with multiple "merge" connections, then we assume (I think rightly so)
                // that this is the end of the conditional branch, and that code should continue at this point outside of the "if-else" statement.
                if (inCondition)
                {
                    var connections = el.Connections.Where(c => c.ElementConnectionPoint.Type == GripType.TopMiddle);

                    if (connections.Count() > 1)
                    {
                        return;
                    }
                }

                // Yuck.
                if (el is IBeginForLoopBox)
                {
                    var drakonLoop = new DrakonBeginLoop() { Code = ParseCode(el) };
                    dcg.AddInstruction(drakonLoop);
                    // Get next shape and add instructions to "loop"
                }
                else if (el is IEndForLoopBox)
                {
                    dcg.AddInstruction(new DrakonEndLoop());
                }
                else if (el is IIfBox)
                {
                    var drakonIf = new DrakonIf() { Code = ParseCode(el) };
                    dcg.AddInstruction(drakonIf);

                    var elTrue = codeService.GetTruePathFirstShape((IIfBox)el);
                    var elFalse = codeService.GetFalsePathFirstShape((IIfBox)el);

                    if (elTrue != null)
                    {
                        ParseDrakonWorkflow(drakonIf.TrueInstructions, codeService, canvasController, elTrue, true);
                    }

                    if (elFalse != null)
                    {
                        ParseDrakonWorkflow(drakonIf.FalseInstructions, codeService, canvasController, elFalse, true);
                    }

                    // dcg.AddInstruction(new DrakonEndIf());
                }
                else if (el is IOutputBox)
                {
                    dcg.AddInstruction(new DrakonOutput() { Code = ParseCode(el) });
                }
                else
                {
                    string code;
                    if (el.Json.TryGetValue("python", out code))
                    {
                        dcg.AddInstruction(new DrakonStatement() { Code = code });
                    }
                }

                el = codeService.NextElementInWorkflow(el);
            }
        }

        protected string ParseCode(GraphicElement el)
        {
            // Replace crlf with space and if element has 'python" code in Json, use that instead.
            string ret = el.Text;

            ret = ret.Replace("\r", "").Replace("\n", " ");

            return ret;
        }

        protected bool BuildClassFromCodeBoxes(GraphicElement elClass, string className, string filename, List<string> classSources, List<string> imports)
        {
            StringBuilder sb = new StringBuilder();
            bool ok = true;

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

            return ok;
        }

        protected bool BuildModuleFromModuleSource(GraphicElement elClass, string className, string filename)
        {
            bool ok = true;
            StringBuilder sb = new StringBuilder();

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

            File.WriteAllText(filename, sb.ToString());
            elClass.Json["python"] = sb.ToString();

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
                Select(srcEl => srcEl.Json["python"] ?? "").Where(src => !String.IsNullOrEmpty(src)).
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

        protected bool HasDrakonShapes(BaseController canvasController, GraphicElement elClass)
        {
            return canvasController.Elements.Any(srcEl => srcEl != elClass && 
                elClass.DisplayRectangle.Contains(srcEl.DisplayRectangle) &&
                srcEl is IDrakonShape);
        }

        protected string GetCode(GraphicElement el)
        {
            string code;
            el.Json.TryGetValue("python", out code);

            return code ?? "";
        }
    }
}
