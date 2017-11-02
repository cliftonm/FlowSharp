using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Microsoft.CSharp;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpCodeServiceInterfaces;
using FlowSharpCodeShapeInterfaces;
using FlowSharpHopeCommon;
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
        public bool RunnerLoaded { get { return runner.Loaded; } }

        protected ToolStripMenuItem mnuBuild = new ToolStripMenuItem() { Name = "mnuBuild", Text = "Build" };
        protected ToolStripMenuItem mnuRun = new ToolStripMenuItem() { Name = "mnuRun", Text = "Run" };
        protected ToolStripMenuItem mnuStop = new ToolStripMenuItem() { Name = "mnuStop", Text = "Stop" };
        protected ToolStripMenuItem mnuShowAnimation = new ToolStripMenuItem() { Name = "mnuShowAnimation", Text = "Show Animation" };
        protected ToolStripMenuItem mnuShowActivation = new ToolStripMenuItem() { Name = "mnuShowActivation", Text = "Show Activation" };
        protected ToolStripMenuItem mnuShowRouting = new ToolStripMenuItem() { Name = "mnuShowRouting", Text = "Show Routing" };
        protected Dictionary<string, string> tempToTextBoxMap = new Dictionary<string, string>();
        protected IRunner runner;
        protected Animator animator;

        public override void FinishedInitialization()
        {
            // runner = new AppDomainRunner();
            runner = new StandAloneRunner(ServiceManager);
            // runner = new InAppRunner();
            animator = new Animator(ServiceManager);
            runner.Processing += animator.Animate;

			InitializeEditorsMenu();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;
            base.FinishedInitialization();
        }

        public void LoadHopeAssembly()
        {
			IFlowSharpMenuService menuService = ServiceManager.Get<IFlowSharpMenuService>();
			string filename = GetExeOrDllFilename(menuService.Filename);
            runner.Load(filename);
        }

        public void UnloadHopeAssembly()
        {
            Assert.SilentTry(() =>
            {
                runner.Unload();
                animator.RemoveCarriers();
            });
        }

        //public List<ReceptorDescription> DescribeReceptors()
        //{
        //}

        public void InstantiateReceptors()
        {
            List<IAgentReceptor> receptors = GetReceptors();
            receptors.Where(r=>r.Enabled).ForEach(r => runner.InstantiateReceptor(r.AgentName));
        }

        protected List<IAgentReceptor> GetReceptors()
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            BaseController canvasController = canvasService.ActiveController;
            List<IAgentReceptor> receptors = GetReceptors(canvasController);

            return receptors;
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
            runner.EnableDisableReceptor(typeName, state);
        }

        public PropertyContainer DescribeSemanticType(string typeName)
        {
            var ret = runner.DescribeSemanticType(typeName);

            return ret;
        }

        public void Publish(string typeName, object st)
        {
            runner.Publish(typeName, st);
        }

        public void Publish(string typeName, string json)
        {
            runner.Publish(typeName, json);
        }

        protected void InitializeEditorsMenu()
        {
            ToolStripMenuItem hopeToolStripMenuItem = new ToolStripMenuItem() { Name = "mnuHope", Text = "&Hope" };
            hopeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] 
            {
                mnuBuild,
                mnuRun,
                mnuStop,
                new ToolStripSeparator(),
                mnuShowAnimation,
                mnuShowActivation,
                new ToolStripSeparator(),
                mnuShowRouting
            });
            ServiceManager.Get<IFlowSharpMenuService>().AddMenu(hopeToolStripMenuItem);
            mnuBuild.Click += OnHopeBuild;
            mnuRun.Click += OnHopeRun;
            mnuStop.Click += OnHopeStop;
            mnuShowRouting.Click += OnShowRouting;
            mnuShowAnimation.Click += (_, __) =>
            {
                mnuShowAnimation.Checked ^= true;
                animator.ShowAnimation = mnuShowAnimation.Checked;
            };

            mnuShowActivation.Click += (_, __) =>
            {
                mnuShowActivation.Checked ^= true;
                animator.ShowActivation = mnuShowActivation.Checked;
            };

            mnuShowAnimation.Checked = true;
            mnuShowActivation.Checked = true;
            animator.ShowAnimation = true;
            animator.ShowActivation = true;
        }

        protected void OnShowRouting(object sender, EventArgs e)
        {
            mnuShowRouting.Checked ^= true;
            mnuShowRouting.Checked.IfElse(ShowRouting, RemoveRouting);
        }

        protected void ShowRouting()
        {
            LoadIfNotLoaded();
            List<IAgentReceptor> receptors = GetReceptors();
            List<ReceptorDescription> descr = new List<ReceptorDescription>();

            receptors.Where(r => r.Enabled).ForEach(r =>
            {
                descr.AddRange(runner.DescribeReceptor(r.AgentName));
            });

            CreateConnectors(descr);
        }

        protected void RemoveRouting()
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            BaseController canvasController = canvasService.ActiveController;
            var receptorConnections = canvasController.Elements.Where(el => el.Name == "_RCPTRCONN_").ToList();

            receptorConnections.ForEach(rc => canvasController.DeleteElement(rc));
        }

        protected void CreateConnectors(List<ReceptorDescription> descr)
        {
            IFlowSharpCanvasService canvasService = ServiceManager.Get<IFlowSharpCanvasService>();
            BaseController canvasController = canvasService.ActiveController;
            Canvas canvas = canvasController.Canvas;

            descr.ForEach(d =>
            {
				// TODO: Deal with namespace handling better than this RightOof kludge.
				// TODO: Converting to lowercase is a bit of a kludge as well.
                GraphicElement elSrc = canvasController.Elements.SingleOrDefault(el => (el is IAgentReceptor) && el.Text.RemoveWhitespace().ToLower() == d.ReceptorTypeName.RightOf(".").ToLower());

				if (elSrc != null)
				{
					d.Publishes.ForEach(p =>
					{
					// Get all receivers that receive the type being published.
					var receivers = descr.Where(r => r.ReceivingSemanticType == p);

						receivers.ForEach(r =>
						{
							// TODO: Deal with namespace handling better than this RightOof kludge.
							// TODO: Converting to lowercase is a bit of a kludge as well.
							GraphicElement elDest = canvasController.Elements.SingleOrDefault(el => (el is IAgentReceptor) && el.Text.RemoveWhitespace().ToLower() == r.ReceptorTypeName.RightOf(".").ToLower());

							if (elDest != null)
							{
								DiagonalConnector dc = new DiagonalConnector(canvas, elSrc.DisplayRectangle.Center(), elDest.DisplayRectangle.Center());
								dc.Name = "_RCPTRCONN_";
								dc.EndCap = AvailableLineCap.Arrow;
								dc.BorderPenColor = Color.Red;
								dc.UpdateProperties();
								canvasController.Insert(dc);
							}
						});
					});
				}
            });
        }

		protected void LoadIfNotLoaded()
        {
            if (!runner.Loaded)
            {
                LoadHopeAssembly();
            }
        }

        protected void OnHopeBuild(object sender, EventArgs e)
        {
            runner.Unload();
            Compile();
        }

        protected void OnHopeRun(object sender, EventArgs e)
        {
			LoadHopeAssembly();
			InstantiateReceptors();
		}

		protected void OnHopeStop(object sender, EventArgs e)
		{
			UnloadHopeAssembly();
		}

		private Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dll = args.Name.LeftOf(',') + ".dll";
            Assembly assy = Assembly.ReflectionOnlyLoadFrom(dll);
            return assy;
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

			bool isStandAlone = runner is StandAloneRunner;
			string filename = GetExeOrDllFilename(menuService.Filename);
			CompilerResults results = Compile(filename, sourceFilenames, refs, isStandAlone);			
            DeleteTempFiles(sourceFilenames);

            if (!results.Errors.HasErrors)
            {
                outputWindow.WriteLine("No Errors");
            }
        }

		protected string GetExeOrDllFilename(string fn)
		{
			// TODO: We should really check if the any of the C# shape code-behind contains App.Main
			bool isStandAlone = runner is StandAloneRunner;
            string ext = isStandAlone ? ".exe" : ".dll";
			string filename = String.IsNullOrEmpty(fn) ? "temp" + ext : Path.GetFileNameWithoutExtension(fn) + ext;

			return filename;
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
            string filename = Path.GetFileNameWithoutExtension(shapeText.RemoveWhitespace()) + ".cs";
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
			parameters.CompilerOptions = "/t:winexe";

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Data.Linq.dll");
			parameters.ReferencedAssemblies.Add("System.Design.dll");
			parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add("System.Net.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
			parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");

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
                        sb.AppendLine(String.Format("Error ({0} - {1}): {2}", tempToTextBoxMap[Path.GetFileNameWithoutExtension(error.FileName.RemoveWhitespace()) + ".cs"], error.Line, error.ErrorText));
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
