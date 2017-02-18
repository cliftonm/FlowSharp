using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.CSharp;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceManagement;

using FlowSharpCodeDrakonShapes;
using FlowSharpCodeServiceInterfaces;
using FlowSharpCodeShapeInterfaces;
using FlowSharpServiceInterfaces;
using FlowSharpLib;

namespace FlowSharpCodeCompilerService
{
    public class FlowSharpCodeCompilerModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCodeCompilerService, FlowSharpCodeCompilerService>();
        }
    }

    public class FlowSharpCodeCompilerService : ServiceBase, IFlowSharpCodeCompilerService
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        protected Dictionary<string, string> tempToTextBoxMap = new Dictionary<string, string>();
        protected string exeFilename;
        protected CompilerResults results;
        protected Process runningProcess;

        public override void FinishedInitialization()
        {
            base.FinishedInitialization();
            InitializeBuildMenu();
        }

        public void Run()
        {
            TerminateRunningProcess();
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();

            // Ever compiled?
            // TODO: Compile when code has changed!
            if (results == null || results.Errors.HasErrors)
            {
                Compile();
            }

            // If no errors:
            if (!results.Errors.HasErrors)
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.FileName = exeFilename;
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
                runningProcess = p;

                /*
                await Task.Run(() =>
                {
                    bool found = false;

                    while (!found)
                    {
                        System.Threading.Thread.Sleep(100);
                        Application.DoEvents();         // Necessary for Exited event to fire.

                        if (runningProcess != null)
                        {
                            Assert.SilentTry(() => found = Process.GetProcesses().Any(pr => pr.ProcessName == runningProcess.ProcessName));
                        }
                    }
                });

                Assert.SilentTry(() =>
                {
                    if (runningProcess != null)
                    {
                        IntPtr hWnd = runningProcess.MainWindowHandle;
                        ShowWindow(hWnd, SW_HIDE);
                    }
                });
                */
            }
        }

        public void Stop()
        {
            TerminateRunningProcess();
        }

        public void Compile()
        {
            TerminateRunningProcess();

            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            outputWindow.Clear();

            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            IFlowSharpCodeService codeService = ServiceManager.Get<IFlowSharpCodeService>();
            BaseController canvasController = canvasService.ActiveController;
            tempToTextBoxMap.Clear();

            List<GraphicElement> compiledAssemblies = new List<GraphicElement>();
            bool ok = CompileAssemblies(canvasController, compiledAssemblies);

            if (!ok)
            {
                DeleteTempFiles();
                return;
            }

            List<string> refs = new List<string>();
            List<string> sources = new List<string>();

            // Add specific assembly references on the drawing.
            List<IAssemblyReferenceBox> references = GetReferences(canvasController);
            refs.AddRange(references.Select(r => r.Filename));

            List<GraphicElement> rootSourceShapes = GetSources(canvasController);
            rootSourceShapes.ForEach(root => GetReferencedAssemblies(root).Where(refassy => refassy is IAssemblyBox).ForEach(refassy => refs.Add(((IAssemblyBox)refassy).Filename)));

            // Get code for workflow boxes first, as this code will then be included in the rootSourceShape code listing.
            IEnumerable<GraphicElement> workflowShapes = canvasController.Elements.Where(el => el is IWorkflowBox);
            workflowShapes.ForEach(wf =>
            {
                string code = GetWorkflowCode(codeService, canvasController, wf);
                wf.Json["Code"] = code;
                // CreateCodeFile(wf, sources, code);
            });

            // TODO: Better Linq!
            rootSourceShapes.Where(root => !String.IsNullOrEmpty(GetCode(root))).ForEach(root =>
            {
                CreateCodeFile(root, sources, GetCode(root));
            });

            exeFilename = String.IsNullOrEmpty(menuService.Filename) ? "temp.exe" : Path.GetFileNameWithoutExtension(menuService.Filename) + ".exe";
            Compile(exeFilename, sources, refs, true);
            DeleteTempFiles();

            if (!results.Errors.HasErrors)
            {
                outputWindow.WriteLine("No Errors");
            }
        }

        protected void InitializeBuildMenu()
        {
            ToolStripMenuItem buildToolStripMenuItem = new ToolStripMenuItem();
            ToolStripMenuItem mnuCompile = new ToolStripMenuItem();
            ToolStripMenuItem mnuRun = new ToolStripMenuItem();
            ToolStripMenuItem mnuStop = new ToolStripMenuItem();

            mnuCompile.Name = "mnuCompile";
            mnuCompile.ShortcutKeys = Keys.Alt | Keys.C;
            mnuCompile.Size = new System.Drawing.Size(165, 24);
            mnuCompile.Text = "&Compile";

            mnuRun.Name = "mnuRun";
            mnuRun.ShortcutKeys = Keys.Alt | Keys.R;
            mnuRun.Size = new System.Drawing.Size(165, 24);
            mnuRun.Text = "&Run";

            mnuStop.Name = "mnuStop";
            mnuStop.ShortcutKeys = Keys.Alt | Keys.S;
            // mnuStop.ShortcutKeys = Keys.Alt | Keys.R;
            mnuStop.Size = new System.Drawing.Size(165, 24);
            mnuStop.Text = "&Stop";

            buildToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuCompile, mnuRun, mnuStop });
            buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            buildToolStripMenuItem.Size = new System.Drawing.Size(37, 21);
            buildToolStripMenuItem.Text = "C#";

            mnuCompile.Click += OnCompile;
            mnuRun.Click += OnRun;
            mnuStop.Click += OnStop;

            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(buildToolStripMenuItem);
        }

        protected void OnCompile(object sender, EventArgs e)
        {
            Compile();
        }

        protected void OnRun(object sender, EventArgs e)
        {
            Run();
        }

        protected void OnStop(object sender, EventArgs e)
        {
            Stop();
        }

        protected void TerminateRunningProcess()
        {
            if (runningProcess != null)
            {
                Assert.SilentTry(() => runningProcess.Kill());
                runningProcess = null;
            }
        }

        protected void CreateCodeFile(GraphicElement root, List<string> sources, string code)
        {
            string filename = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".cs";
            tempToTextBoxMap[filename] = root.Text;
            File.WriteAllText(filename, GetCode(root));
            sources.Add(filename);
        }

        public string GetWorkflowCode(IFlowSharpCodeService codeService, BaseController canvasController, GraphicElement wf)
        {
            StringBuilder sb = new StringBuilder();
            string packetName = Clifton.Core.ExtensionMethods.ExtensionMethods.LeftOf(wf.Text, "Workflow");
            GraphicElement elDefiningPacket = FindPacket(canvasController, packetName);

            if (elDefiningPacket == null)
            {
                ServiceManager.Get<IFlowSharpCodeOutputWindowService>().WriteLine("Workflow packet '" + packetName + "' must be defined.");
            }
            else
            {
                bool packetHasParameterlessConstructor = HasParameterlessConstructor(GetCode(elDefiningPacket), packetName);

                // TODO: Hardcoded for now for POC.
                sb.AppendLine("// This code has been auto-generated by FlowSharpCode");
                sb.AppendLine("// Do not modify this code -- your changes will be overwritten!");
                sb.AppendLine("namespace App");
                sb.AppendLine("{");
                sb.AppendLine("\tpublic partial class " + wf.Text);
                sb.AppendLine("\t{");

                if (packetHasParameterlessConstructor)
                {
                    sb.AppendLine("\t\tpublic static void Execute()");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine("\t\t\tExecute(new " + packetName + "());");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();
                }

                sb.AppendLine("\t\tpublic static void Execute(" + packetName + " packet)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\t" + wf.Text + " workflow = new " + wf.Text + "();");

                // Fill in the workflow steps.
                GraphicElement el = codeService.FindStartOfWorkflow(canvasController, wf);
                GenerateCodeForWorkflow(codeService, sb, el, 3);

                // We're all done.
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        protected GraphicElement FindPacket(BaseController canvasController, string packetName)
        {
            GraphicElement elPacket = canvasController.Elements.SingleOrDefault(el => el.Text == packetName);

            return elPacket;
        }

        /// <summary>
        /// Returns true if the code block contains a parameterless constructor of the form "public [packetname]()" or no constructor at all.
        /// </summary>
        protected bool HasParameterlessConstructor(string code, string packetName)
        {
            string signature = "public " + packetName + "(";

            bool ret = !code.Contains(signature) ||
                code.AllIndexesOf(signature).Any(idx => code.Substring(idx).RightOf('(')[0] == ')');

            return ret;
        }

        protected void GenerateCodeForWorkflow(IFlowSharpCodeService codeService, StringBuilder sb, GraphicElement el, int indent)
        {
            string strIndent = new String('\t', indent);

            while (el != null)
            {
                if ( (el is IIfBox) )
                {

                    // True clause
                    var elTrue = codeService.GetTruePathFirstShape((IIfBox)el);
                    // False clause
                    var elFalse = codeService.GetFalsePathFirstShape((IIfBox)el);

                    if (elTrue != null)
                    {
                        sb.AppendLine(strIndent + "bool " + el.Text.ToLower() + " = workflow." + el.Text + "(packet);");
                        sb.AppendLine();
                        sb.AppendLine(strIndent + "if (" + el.Text.ToLower() + ")");
                        sb.AppendLine(strIndent + "{");
                        GenerateCodeForWorkflow(codeService, sb, elTrue, indent + 1);
                        sb.AppendLine(strIndent + "}");

                        if (elFalse != null)
                        {
                            sb.AppendLine(strIndent + "else");
                            sb.AppendLine(strIndent + "{");
                            GenerateCodeForWorkflow(codeService, sb, elFalse, indent + 1);
                            sb.AppendLine(strIndent + "}");
                        }
                    }
                    else if (elFalse != null)
                    {
                        sb.AppendLine(strIndent + "bool " + el.Text.ToLower() + " = workflow." + el.Text + "(packet);");
                        sb.AppendLine();
                        sb.AppendLine(strIndent + "if (!" + el.Text.ToLower() + ")");
                        sb.AppendLine(strIndent + "{");
                        GenerateCodeForWorkflow(codeService, sb, elFalse, indent + 1);
                        sb.AppendLine(strIndent + "}");
                    }

                    // TODO: How to join back up with workflows that rejoin from if-then-else?
                    break;

                }
                else
                {
                    sb.AppendLine(strIndent + "workflow." + el.Text + "(packet);");
                }

                el = codeService.NextElementInWorkflow(el);
            }
        }

        protected void DeleteTempFiles()
        {
            tempToTextBoxMap.ForEach(kvp => File.Delete(kvp.Key));
        }

        private void MnuRun_Click(object sender, EventArgs e)
        {
            // Ever compiled?
            if (results == null || results.Errors.HasErrors)
            {
                Compile();
            }

            // If no errors:
            if (!results.Errors.HasErrors)
            {
                //ProcessStartInfo psi = new ProcessStartInfo(exeFilename);
                //psi.UseShellExecute = true;     // must be true if we want to keep a console window open.
                Process p = Process.Start(exeFilename);
                //p.WaitForExit();
                //p.Close();
                //Type program = compiledAssembly.GetType("WebServerDemo.Program");
                //MethodInfo main = program.GetMethod("Main");
                //main.Invoke(null, null);
            }
        }

        protected bool CompileAssemblies(BaseController canvasController, List<GraphicElement> compiledAssemblies)
        {
            bool ok = true;

            foreach (GraphicElement elAssy in canvasController.Elements.Where(el => el is IAssemblyBox))
            {
                CompileAssembly(canvasController, elAssy, compiledAssemblies);
            }

            return ok;
        }

        protected string CompileAssembly(BaseController canvasController, GraphicElement elAssy, List<GraphicElement> compiledAssemblies)
        {
            string assyFilename = ((IAssemblyBox)elAssy).Filename;

            if (!compiledAssemblies.Contains(elAssy))
            {
                // Add now, so we don't accidentally recurse infinitely.
                compiledAssemblies.Add(elAssy);

                List<GraphicElement> referencedAssemblies = GetReferencedAssemblies(elAssy);
                List<string> refs = new List<string>();

                // Recurse into referenced assemblies that need compiling first.
                foreach (GraphicElement el in referencedAssemblies)
                {
                    string refAssy = CompileAssembly(canvasController, el, compiledAssemblies);
                    refs.Add(refAssy);
                }

                List<string> sources = GetSources(canvasController, elAssy);
                Compile(assyFilename, sources, refs);
            }

            return assyFilename;
        }

        protected List<GraphicElement> GetReferencedAssemblies(GraphicElement elAssy)
        {
            List<GraphicElement> refs = new List<GraphicElement>();

            // TODO: Qualify EndConnectedShape as being IAssemblyBox
            elAssy.Connections.Where(c => (c.ToElement is Connector) && ((Connector)c.ToElement).EndCap == AvailableLineCap.Arrow).ForEach(c =>
            {
                // Connector endpoint will reference ourselves, so exclude.
                if (((Connector)c.ToElement).EndConnectedShape != elAssy)
                {
                    GraphicElement toAssy = ((Connector)c.ToElement).EndConnectedShape;
                    refs.Add(toAssy);
                }
            });

            // TODO: Qualify EndConnectedShape as being IAssemblyBox
            elAssy.Connections.Where(c => (c.ToElement is Connector) && ((Connector)c.ToElement).StartCap == AvailableLineCap.Arrow).ForEach(c =>
            {
                // Connector endpoint will reference ourselves, so exclude.
                if (((Connector)c.ToElement).StartConnectedShape != elAssy)
                {
                    GraphicElement toAssy = ((Connector)c.ToElement).StartConnectedShape;
                    refs.Add(toAssy);
                }
            });

            return refs;
        }

        protected bool Compile(string assyFilename, List<string> sources, List<string> refs, bool generateExecutable = false)
        {
            bool ok = false;

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.IncludeDebugInformation = true;
            parameters.GenerateInMemory = false;
            parameters.GenerateExecutable = generateExecutable;

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Data.Linq.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add("System.Net.dll");
            parameters.ReferencedAssemblies.Add("System.Net.Http.dll");
            // parameters.ReferencedAssemblies.Add("System.Speech.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            // parameters.ReferencedAssemblies.Add("Clifton.Core.dll");
            // parameters.ReferencedAssemblies.Add("websocket-sharp.dll");
            parameters.ReferencedAssemblies.AddRange(refs.ToArray());
            parameters.OutputAssembly = assyFilename;

            if (generateExecutable)
            {
                parameters.MainClass = "App.Program";
            }

            // results = provider.CompileAssemblyFromSource(parameters, sources.ToArray());

            results = provider.CompileAssemblyFromFile(parameters, sources.ToArray());
            ok = !results.Errors.HasErrors;

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    try
                    {
                        sb.AppendLine(String.Format("Error ({0} - {1}): {2}", tempToTextBoxMap[Path.GetFileNameWithoutExtension(error.FileName) + ".cs"], error.Line, error.ErrorText));
                    }
                    catch
                    {
                        sb.AppendLine(error.ErrorText);     // other errors, like "process in use", do not have an associated filename, so general catch-all here.
                    }
                }

                // MessageBox.Show(sb.ToString(), assyFilename, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ServiceManager.Get<IFlowSharpCodeOutputWindowService>().WriteLine(sb.ToString());
            }

            return ok;
        }

        protected List<IAssemblyReferenceBox> GetReferences(BaseController canvasController)
        {
            return canvasController.Elements.Where(el => el is IAssemblyReferenceBox).Cast<IAssemblyReferenceBox>().ToList();
        }

        /// <summary>
        /// Returns only top level sources - those not contained within AssemblyBox shapes.
        /// </summary>
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

        protected bool ContainedIn<T>(BaseController canvasController, GraphicElement child)
        {
            return canvasController.Elements.Any(el => el is T && el.DisplayRectangle.Contains(child.DisplayRectangle));
        }

        /// <summary>
        /// Returns sources contained in an element (ie., AssemblyBox shape).
        /// </summary>
        protected List<string> GetSources(BaseController canvasController, GraphicElement elAssy)
        {
            List<string> sourceList = new List<string>();

            foreach (GraphicElement srcEl in canvasController.Elements.Where(srcEl => elAssy.DisplayRectangle.Contains(srcEl.DisplayRectangle)))
            {
                string filename = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".cs";
                tempToTextBoxMap[filename] = srcEl.Text;
                File.WriteAllText(filename, GetCode(srcEl));
                sourceList.Add(filename);
            }

            return sourceList;
        }

        protected string GetCode(GraphicElement el)
        {
            string code;
            el.Json.TryGetValue("Code", out code);

            return code ?? "";
        }
    }
}
