using System;

namespace HopeShapes.PropertyGridHelpers
{
    // From: https://www.codeproject.com/Articles/9280/Add-Remove-Items-to-from-PropertyGrid-at-Runtime
    public class CustomProperty
    {
        public string Root { get; set; }
        public string Name { get; set; }
        public bool ReadOnly { get; set; }
        public bool Visible { get; set; }
        public object Value { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public Type PropertyType { get; set; }

        public CustomProperty(string root, string name, object value, string description, string category, Type propertyType, bool readOnly = false, bool visible = true)
        {
            Root = root;
            Name = name;
            Value = value;
            ReadOnly = readOnly;
            Visible = visible;
            Description = description;
            Category = category;
            PropertyType = propertyType;
        }
    }
}
