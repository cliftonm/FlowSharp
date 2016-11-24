using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.ModuleManagement;
using Clifton.Core.ServiceManagement;

namespace FlowSharp
{
    static partial class Program
    {
        public static ServiceManager ServiceManager;

        static void Bootstrap()
        {
            ServiceManager = new ServiceManager();
            ServiceManager.RegisterSingleton<IServiceModuleManager, ServiceModuleManager>();

            try
            {
                IModuleManager moduleMgr = (IModuleManager)ServiceManager.Get<IServiceModuleManager>();
                List<AssemblyFileName> modules = GetModuleList(XmlFileName.Create("modules.xml"));
                moduleMgr.RegisterModules(modules);
                ServiceManager.FinishedInitialization();
            }
            catch(ReflectionTypeLoadException lex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (Exception ex in lex.LoaderExceptions)
                {
                    sb.AppendLine(ex.Message);
                }

                MessageBox.Show(sb.ToString(), "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Return the list of assembly names specified in the XML file so that
        /// we know what assemblies are considered modules as part of the application.
        /// </summary>
        static private List<AssemblyFileName> GetModuleList(XmlFileName filename)
        {
            Assert.That(File.Exists(filename.Value), "Module definition file " + filename.Value + " does not exist.");
            XDocument xdoc = XDocument.Load(filename.Value);

            return GetModuleList(xdoc);
        }

        /// <summary>
        /// Returns the list of modules specified in the XML document so we know what
        /// modules to instantiate.
        /// </summary>
        static private List<AssemblyFileName> GetModuleList(XDocument xdoc)
        {
            List<AssemblyFileName> assemblies = new List<AssemblyFileName>();
            (from module in xdoc.Element("Modules").Elements("Module")
             select module.Attribute("AssemblyName").Value).ForEach(s => assemblies.Add(AssemblyFileName.Create(s)));

            return assemblies;
        }
    }
}
