using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Microsoft.CSharp;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpCodeServiceInterfaces;
using FlowSharpCodeShapeInterfaces;
using FlowSharpHopeServiceInterfaces;
using FlowSharpHopeShapeInterfaces;
using FlowSharpServiceInterfaces;

namespace FlowSharpHopeService
{
    public class HigherOrderProgrammingModule : IModule
    {
        public void InitializeServices(IServiceManager serviceManager)
        {
            serviceManager.RegisterSingleton<IHigherOrderProgrammingService, HigherOrderProgrammingService>();
        }
    }

    public class HigherOrderProgrammingService : ServiceBase, IHigherOrderProgrammingService
    {
        protected ToolStripMenuItem mnuBuild = new ToolStripMenuItem() { Name = "mnuBuild", Text = "Build" };
        protected ToolStripMenuItem mnuRun = new ToolStripMenuItem() { Name = "mnuRun", Text = "Run" };
        protected ToolStripMenuItem mnuStop = new ToolStripMenuItem() { Name = "mnuStop", Text = "Stop" };
        protected Dictionary<string, string> tempToTextBoxMap = new Dictionary<string, string>();
        // protected InAppRunner runner = new InAppRunner();
        protected Runner runner = new Runner();

        public override void FinishedInitialization()
        {
            InitializeEditorsMenu();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;
            base.FinishedInitialization();
        }

        public void LoadHopeAssembly()
        {
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            string dllFilename = String.IsNullOrEmpty(menuService.Filename) ? "temp.dll" : Path.GetFileNameWithoutExtension(menuService.Filename) + ".dll";
            runner.Load(dllFilename);
        }

        public void UnloadHopeAssembly()
        {
            runner.Unload();
        }

        public void InstantiateReceptors()
        {
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            string dllFilename = String.IsNullOrEmpty(menuService.Filename) ? "temp.dll" : Path.GetFileNameWithoutExtension(menuService.Filename) + ".dll";
            Assembly assy = Assembly.ReflectionOnlyLoadFrom(dllFilename);
            var (agents, errors) = GetAgents(assy);

            if (errors.Count > 0)
            {
                outputWindow.WriteLine(String.Join("\r\n", errors));
            }
            else
            {
                IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
                BaseController canvasController = canvasService.ActiveController;
                List<IAgentReceptor> receptors = GetReceptors(canvasController);

                foreach (var el in receptors)
                {
                    Type agent = agents.SingleOrDefault(a => a.Name == el.AgentName);

                    if (agent == null)
                    {
                        outputWindow.WriteLine("Receptor " + el.Text + " references an agent that is not defined: " + el.AgentName);
                    }
                    else
                    {
                        runner.InstantiateReceptor(agent);
                    }
                }
            }
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
            runner.EnableDisableReceptor(typeName, state);
        }

        public ISemanticType InstantiateSemanticType(string typeName)
        {
            var ret = runner.InstantiateSemanticType(typeName);

            return ret;
        }

        public void Publish(ISemanticType st)
        {
            runner.Publish(st);
        }

        protected void InitializeEditorsMenu()
        {
            ToolStripMenuItem hopeToolStripMenuItem = new ToolStripMenuItem() { Name = "mnuHope", Text = "&Hope" };
            hopeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuBuild, mnuRun, mnuStop });
            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(hopeToolStripMenuItem);
            mnuBuild.Click += OnHopeBuild;
            mnuRun.Click += OnHopeRun;
            mnuStop.Click += OnHopeStop;
        }

        protected void OnHopeBuild(object sender, EventArgs e)
        {
            runner.Unload();
            Compile();
        }

        protected void OnHopeRun(object sender, EventArgs e)
        {
            runner.Unload();
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            string dllFilename = String.IsNullOrEmpty(menuService.Filename) ? "temp.dll" : Path.GetFileNameWithoutExtension(menuService.Filename) + ".dll";
            Assembly assy = Assembly.ReflectionOnlyLoadFrom(dllFilename);
            var (agents, errors) = GetAgents(assy);

            if (errors.Count > 0)
            {
                outputWindow.WriteLine(String.Join("\r\n", errors));
            }
            else
            {
                // Temporary, for testing getting things working.
                runner.Load(dllFilename);
                InstantiateReceptors();

                // Testing
                dynamic st = runner.InstantiateSemanticType("ST_Text");
                st.Text = "Hello World!";
                runner.Publish(st);
            }
        }

        private Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dll = args.Name.LeftOf(',') + ".dll";
            Assembly assy = Assembly.ReflectionOnlyLoadFrom(dll);
            return assy;
        }

        protected void OnHopeStop(object sender, EventArgs e)
        {
            runner.Unload();
        }

        protected (List<Type> agents, List<string> errors) GetAgents(Assembly assy)
        {
            List<Type> agents = new List<Type>();
            List<string> errors = new List<string>();

            try
            {
                agents = assy.GetTypes().Where(t => t.IsClass && t.IsPublic && t.GetInterfaces().Any(i=>i.Name==nameof(IReceptor))).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loadEx in ex.LoaderExceptions)
                {
                    errors.AddIfUnique(loadEx.Message);
                }
            }
            catch (Exception ex)
            {
                errors.AddIfUnique(ex.Message);
            }

            return (agents, errors);
        }

        protected List<IAgentReceptor> GetReceptors(BaseController canvasController)
        {
            List<IAgentReceptor> receptors = new List<IAgentReceptor>();
            receptors.AddRange(canvasController.Elements.Where(srcEl => srcEl is IAgentReceptor).Cast<IAgentReceptor>().Where(agent=>agent.Enabled));

            return receptors;
        }

        protected void Compile()
        {
            tempToTextBoxMap.Clear();
            var outputWindow = ServiceManager.Get<IFlowSharpCodeOutputWindowService>();
            outputWindow.Clear();

            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
            IFlowSharpCodeService codeService = ServiceManager.Get<IFlowSharpCodeService>();
            BaseController canvasController = canvasService.ActiveController;

            List<string> refs = GetCanvasReferences(canvasController);
            List<GraphicElement> shapeSources = GetCanvasSources(canvasController);
            List<string> sourceFilenames = GetSourceFiles(shapeSources);

            string dllFilename = String.IsNullOrEmpty(menuService.Filename) ? "temp.dll" : Path.GetFileNameWithoutExtension(menuService.Filename) + ".dll";
            CompilerResults results = Compile(dllFilename, sourceFilenames, refs);
            DeleteTempFiles(sourceFilenames);

            if (!results.Errors.HasErrors)
            {
                outputWindow.WriteLine("No Errors");
            }
        }

        protected void DeleteTempFiles(List<string> files)
        {
            // files.ForEach(fn => File.Delete(fn));
        }

        protected List<string> GetCanvasReferences(BaseController canvasController)
        {
            List<string> refs = new List<string>();
            List<IAssemblyReferenceBox> references = GetReferences(canvasController);
            refs.AddRange(references.Select(r => r.Filename));

            return refs;
        }

        /// <summary>
        /// Returns only top level sources - those not contained within AssemblyBox shapes.
        /// </summary>
        protected List<GraphicElement> GetCanvasSources(BaseController canvasController)
        {
            List<GraphicElement> sourceList = new List<GraphicElement>();

            foreach (GraphicElement srcEl in canvasController.Elements.Where(
                srcEl => !ContainedIn<IAssemblyBox>(canvasController, srcEl) /* && !(srcEl is IFileBox) */ ))
            {
                sourceList.Add(srcEl);
            }

            return sourceList;
        }

        protected bool ContainedIn<T>(BaseController canvasController, GraphicElement child)
        {
            return canvasController.Elements.Any(el => el is T && el.DisplayRectangle.Contains(child.DisplayRectangle));
        }

        protected List<string> GetSourceFiles(List<GraphicElement> shapeSources)
        {
            List<string> files = new List<string>();
            shapeSources.ForEach(shape =>
                {
                    // Get all other shapes that are not part of CSharpClass shapes:
                    // TODO: Better Linq!
                    string code = GetCode(shape);
                    if (!String.IsNullOrEmpty(code))
                    {
                        string filename = CreateCodeFile(code, shape.Text);
                        files.Add(filename);
                    }
                });

            return files;
        }

        protected string GetCode(GraphicElement el)
        {
            string code;
            el.Json.TryGetValue("Code", out code);

            return code ?? "";
        }

        protected List<IAssemblyReferenceBox> GetReferences(BaseController canvasController)
        {
            return canvasController.Elements.Where(el => el is IAssemblyReferenceBox).Cast<IAssemblyReferenceBox>().ToList();
        }

        protected string CreateCodeFile(string code, string shapeText)
        {
            // string filename = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".cs";
            string filename = Path.GetFileNameWithoutExtension(shapeText.Replace("\r", "").Replace("\n", "")) + ".cs";
            File.WriteAllText(filename, code);
            tempToTextBoxMap[filename] = shapeText;

            return filename;
        }

        protected CompilerResults Compile(string assyFilename, List<string> sources, List<string> refs, bool generateExecutable = false)
        {
            // https://stackoverflow.com/questions/31639602/using-c-sharp-6-features-with-codedomprovider-rosyln
            // The built-in CodeDOM provider doesn't support C# 6. Use this one instead:
            // https://www.nuget.org/packages/Microsoft.CodeDom.Providers.DotNetCompilerPlatform/
            // var options = new Dictionary<string, string>() { { "CompilerVersion", "v7.0" } };
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
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            parameters.ReferencedAssemblies.Add("System.Speech.dll");

            //parameters.ReferencedAssemblies.Add("HopeRunner.dll");
            //parameters.ReferencedAssemblies.Add("HopeRunnerAppDomainInterface.dll");

            // parameters.ReferencedAssemblies.Add("System.Xml.dll");
            // parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            // parameters.ReferencedAssemblies.Add("Clifton.Core.dll");
            // parameters.ReferencedAssemblies.Add("websocket-sharp.dll");
            parameters.ReferencedAssemblies.AddRange(refs.ToArray());
            parameters.OutputAssembly = assyFilename;

            if (generateExecutable)
            {
                parameters.MainClass = "App.Program";
            }

            // results = provider.CompileAssemblyFromSource(parameters, sources.ToArray());

            CompilerResults results = provider.CompileAssemblyFromFile(parameters, sources.ToArray());

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

            return results;
        }
    }
}
