using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Newtonsoft.Json;

namespace CodeTester
{
    public class Foo
    {
        [Category("A")]
        [Description("A Text")]
        public string Text { get; set; }

        [Category("A")]
        public DateTime Date { get; set; }
        [Category("A")]
        public Bar Bar { get; set; }
    }

    public class Bar
    {
        [Category("B")]
        public int I { get; set; }

        public int J { get; set; }
    }

    public class PropertyContainer
    {
        public List<PropertyData> Types { get; set; }

        public PropertyContainer()
        {
            Types = new List<PropertyData>();
        }
    }

    public class PropertyData
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public PropertyContainer ChildType { get; set; }

        public PropertyData()
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Type t = typeof(Foo);
            PropertyInfo[] pis = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyContainer pc = new PropertyContainer();
            BuildTypes(pc, pis);
            string json = JsonConvert.SerializeObject(pc);
        }

        static void BuildTypes(PropertyContainer pc, PropertyInfo[] pis)
        {
            foreach (PropertyInfo pi in pis)
            {
                PropertyData pd = new PropertyData() { Name = pi.Name, TypeName = pi.PropertyType.FullName };
                pd.Category = pi.GetCustomAttribute<CategoryAttribute>()?.Category;
                pd.Description = pi.GetCustomAttribute<DescriptionAttribute>()?.Description;
                pc.Types.Add(pd);

                if ((!pi.PropertyType.IsValueType) && (pd.TypeName != "System.String"))
                {
                    PropertyInfo[] pisChild = pi.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    pd.ChildType = new PropertyContainer();
                    BuildTypes(pd.ChildType, pisChild);
                }
            }
        }
    }
}
