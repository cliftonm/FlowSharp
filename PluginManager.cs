using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using FlowSharpLib;

namespace FlowSharp
{
    public class PluginManager
    {
        protected List<Type> pluginShapes = new List<Type>();
        protected List<Assembly> pluginAssemblies = new List<Assembly>();
        protected List<string> pluginFiles = new List<string>();

        public PluginManager()
        {
            Persist.AssemblyResolver = AssemblyResolver;
            Persist.TypeResolver = TypeResolver;
        }

        public void InitializePlugins()
        {
            if (File.Exists(FlowSharpUI.PLUGIN_FILE_LIST))
            {
                string[] plugins = File.ReadAllLines(FlowSharpUI.PLUGIN_FILE_LIST);

                foreach (string plugin in plugins.Where(p => !String.IsNullOrWhiteSpace(p)))
                {
                    RegisterPlugin(plugin);
                }
            }
        }

        public void UpdatePlugins()
        {
            if (File.Exists(FlowSharpUI.PLUGIN_FILE_LIST))
            {
                string[] plugins = File.ReadAllLines(FlowSharpUI.PLUGIN_FILE_LIST);

                foreach (string plugin in plugins.Where(p => !String.IsNullOrWhiteSpace(p) && !pluginFiles.Contains(p)))
                {
                    RegisterPlugin(plugin);
                }
            }
        }

        public List<Type> GetShapeTypes()
        {
            return pluginShapes;
        }

        protected void RegisterPlugin(string plugin)
        {
            try
            {
                Assembly assy = Assembly.LoadFrom(plugin);
                pluginAssemblies.Add(assy);

                assy.GetTypes().ForEach(t =>
                {
                    if (t.IsSubclassOf(typeof(GraphicElement)))
                    {
                        pluginShapes.Add(t);
                    }
                });

                pluginFiles.Add(plugin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(plugin + "\r\n" + ex.Message, "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected Assembly AssemblyResolver(AssemblyName assyName)
        {
            Assembly ret = null;

            foreach (Assembly assy in pluginAssemblies)
            {
                if (assy.FullName == assyName.FullName)
                {
                    ret = assy;
                    break;
                }
            }

            if (ret == null)
            {
                ret = Assembly.Load(assyName);
            }

            return ret;
        }

        protected Type TypeResolver(Assembly assy, string typeName, bool ignoreCase)
        {
            return assy.GetType(typeName);
        }
    }
}
