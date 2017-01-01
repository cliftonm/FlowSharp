using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
    public class FlowSharpCodeCompilerModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IFlowSharpCodeCompilerService, FlowSharpCodeCompilerService>();
        }
    }

    public class FlowSharpCodeCompilerService : ServiceBase, IFlowSharpCodeCompilerService
    {
        protected Dictionary<string, string> tempToTextBoxMap = new Dictionary<string, string>();
        protected string exeFilename;
        protected CompilerResults results;

        public void Run()
        {
            // Ever compiled?
            if (results == null || results.Errors.HasErrors)
            {
                Compile();
            }

            // If no errors:
            if (!results.Errors.HasErrors)
            {
                Process p = Process.Start(exeFilename);
            }
        }

        public void Compile()
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
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
            List<GraphicElement> rootSourceShapes = GetSources(canvasController);
            rootSourceShapes.ForEach(root => GetReferencedAssemblies(root).Where(refassy => refassy is IAssemblyBox).ForEach(refassy => refs.Add(((IAssemblyBox)refassy).Filename)));

            // Get code for workflow boxes first, as this code will then be included in the rootSourceShape code listing.
            IEnumerable<GraphicElement> workflowShapes = canvasController.Elements.Where(el => el is IWorkflowBox);
            workflowShapes.ForEach(wf =>
            {
                string code = GetWorkflowCode(canvasController, wf);
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
        }

        protected void CreateCodeFile(GraphicElement root, List<string> sources, string code)
        {
            string filename = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".cs";
            tempToTextBoxMap[filename] = root.Text;
            File.WriteAllText(filename, GetCode(root));
            sources.Add(filename);
        }

        public string GetWorkflowCode(BaseController canvasController, GraphicElement wf)
        {
            StringBuilder sb = new StringBuilder();
            string packetName = Clifton.Core.ExtensionMethods.ExtensionMethods.LeftOf(wf.Text, "Workflow");
            GraphicElement elDefiningPacket = FindPacket(canvasController, packetName);
            bool packetHasParameterlessConstructor = HasParameterlessConstructor(GetCode(elDefiningPacket), packetName);

            // TODO: Hardcoded for now for POC.
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
            GraphicElement el = FindStartOfWorkflow(canvasController, wf);
            GenerateCodeForWorkflow(sb, el, 3);

            // We're all done.
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }

        protected GraphicElement FindPacket(BaseController canvasController, string packetName)
        {
            GraphicElement elPacket = canvasController.Elements.Single(el => el.Text == packetName);

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

        protected void GenerateCodeForWorkflow(StringBuilder sb, GraphicElement el, int indent)
        {
            string strIndent = new String('\t', indent);

            while (el != null)
            {
                if (el is Diamond)
                {
                    sb.AppendLine(strIndent + "bool " + el.Text.ToLower() + " = workflow." + el.Text + "(packet);");
                    sb.AppendLine();
                    sb.AppendLine(strIndent + "if (" + el.Text.ToLower() + ")");
                    sb.AppendLine(strIndent + "{");

                    // True clause
                    var elTrue = GetTruePathFirstShape(el);
                    GenerateCodeForWorkflow(sb, elTrue, indent + 1);

                    sb.AppendLine(strIndent + "}");
                    sb.AppendLine(strIndent + "else");
                    sb.AppendLine(strIndent + "{");

                    // False clause
                    var elFalse = GetFalsePathFirstShape(el);
                    GenerateCodeForWorkflow(sb, elFalse, indent + 1);

                    sb.AppendLine(strIndent + "}");

                    // TODO: How to join back up with workflows that rejoin from if-then-else?
                    break;

                }
                else
                {
                    sb.AppendLine(strIndent + "workflow." + el.Text + "(packet);");
                }

                el = NextElementInWorkflow(el);
            }
        }

        // True path is always the bottom of the diamond.
        protected GraphicElement GetTruePathFirstShape(GraphicElement el)
        {
            GraphicElement trueStart = ((Connector)el.Connections.First(c => c.ElementConnectionPoint.Type == GripType.BottomMiddle).ToElement).EndConnectedShape;

            return trueStart;
        }

        // False path is always the left or right point of the diamond.
        public GraphicElement GetFalsePathFirstShape(GraphicElement el)
        {
            GraphicElement falseStart = ((Connector)el.Connections.First(c => c.ElementConnectionPoint.Type == GripType.LeftMiddle || c.ElementConnectionPoint.Type == GripType.RightMiddle).ToElement).EndConnectedShape;

            return falseStart;
        }

        protected GraphicElement FindStartOfWorkflow(BaseController canvasController, GraphicElement wf)
        {
            GraphicElement start = null;

            foreach (GraphicElement srcEl in canvasController.Elements.Where(srcEl => wf.DisplayRectangle.Contains(srcEl.DisplayRectangle)))
            {
                if (!srcEl.IsConnector && srcEl != wf)
                {
                    // Special case for a 1 step workflow.  Untested.
                    // Exclude any text annotations.
                    if (srcEl.Connections.Count == 0)
                    {
                        if (!(srcEl is TextShape))
                        {
                            start = srcEl;
                            break;
                        }
                    }
                    else
                    {
                        // start begins with a box or diamond shape, having only Start connection types.
                        if (srcEl.Connections.All(c => c.ToConnectionPoint.Type == GripType.Start))
                        {
                            start = srcEl;
                            break;
                        }
                    }
                }
            }

            return start;
        }

        /// <summary>
        /// Find the next shape connected to el.
        /// </summary>
        /// <param name="el"></param>
        /// <returns>The next connected shape or null if no connection exists.</returns>
        protected GraphicElement NextElementInWorkflow(GraphicElement el)
        {
            GraphicElement ret = null;

            // The starting shape has one connection where the StartConnectedShape should be the el
            // and the EndConnectedShape is the next shape in the workflow.

            // A middle workflow element has two connections, again where the StartConnectedShape should be the el
            // and the EndConnectedShape is the next shape in the workflow.

            // The final workflow step has one connector, where the EndConnectedShape is the el.

            // 12/20/16, because of a current bug with connectors, where shapes incorrectly retain
            // connections to other shapes that they aren't actually connected to, we try to 
            // compensate for this.

            foreach (Connection connection in el.Connections)
            {
                GraphicElement gr = connection.ToElement;       // a shape's Connections should always be a Connector

                if (gr is Connector)
                {
                    Connector connector = (Connector)gr;

                    if (connector.StartConnectedShape == el)
                    {
                        ret = connector.EndConnectedShape;
                        break;
                    }
                }
                else
                {
                    Trace.WriteLine("*** EXPECTED CONNECTOR FOR " + el.GetType().Name + " ID=" + el.Id.ToString() + " Text=" + el.Text + " ***");
                }
            }

            return ret;
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
            parameters.ReferencedAssemblies.Add("System.Speech.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("Clifton.Core.dll");
            parameters.ReferencedAssemblies.Add("websocket-sharp.dll");
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

                MessageBox.Show(sb.ToString(), assyFilename, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ok;
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
