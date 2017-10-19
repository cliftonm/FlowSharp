using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Newtonsoft.Json;

using Clifton.Core.Semantics;

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


    public class ST_Address : ISemanticType
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public ST_City City { get; set; }
        public ST_State State { get; set; }
        public ST_Zip Zip { get; set; }

        public ST_Address()
        {
            City = new ST_City();
            State = new ST_State();
            Zip = new ST_Zip();
        }
    }

    public class ST_Zip : ISemanticType
    {
        public ST_Zip5 Zip5 { get; set; }
        public ST_Zip4 Zip4 { get; set; }

        public ST_Zip()
        {
            Zip5 = new ST_Zip5();
            Zip4 = new ST_Zip4();
        }
    }

    public class ST_Zip4 : ISemanticType
    {
        public string Zip4 { get; set; }
    }

    public class ST_Zip5 : ISemanticType
    {
        public string Zip5 { get; set; }
    }

    public class ST_City : ISemanticType
    {
        public string City { get; set; }
    }

    public class ST_State : ISemanticType
    {
        public string State { get; set; }
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
            Type t = typeof(ST_Address);
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
