using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;

using FlowSharpHopeCommon;
using FlowSharpHopeServiceInterfaces;

using HopeShapes.PropertyGridHelpers;

namespace HopeShapes
{
    public partial class PublishSemanticType : Form
    {
        protected PropertyContainer pc;
        protected IHigherOrderProgrammingService hope;
        protected string typeName;

        public PublishSemanticType(string typeName, PropertyContainer pc, object st, IHigherOrderProgrammingService hope)
        {
            if (st is ExpandoObject)
            {
                TypeDescriptor.AddProvider(new ExpandoObjectTypeDescriptionProvider(), st);
            }

            InitializeComponent();
            Text = typeName;
            this.pc = pc;
            this.hope = hope;
            this.typeName = typeName;
            pgSemanticType.SelectedObject = st;
            FormClosing += OnFormClosing;
			TopMost = true;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (ckUnload.Checked)
            {
                hope.UnloadHopeAssembly();
            }
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            string json = CreateJson(pc, pgSemanticType.SelectedObject);
            // hope.Publish(typeName, pgSemanticType.SelectedObject);
            hope.Publish(typeName, json);
        }

        private string CreateJson(PropertyContainer pc, object obj)
        {
            StringBuilder sb = new StringBuilder("{");
            SerializeProperties(sb, pc, obj);
            sb.Append("}");

            return sb.ToString();
        }

        // TODO: As per comment in SemanticTypeShapes.cs, this does not account for properties with the same name.
        private void SerializeProperties(StringBuilder sb, PropertyContainer pc, object obj)
        {
            string comma = SerializeValueTypes(sb, pc.Types, obj);
            SerializeObjectTypes(sb, pc.Types, obj, comma);
        }

        private string SerializeValueTypes(StringBuilder sb, List<PropertyData> propertyData, object obj)
        {
            string comma = "";

            propertyData.Where(t => t.ChildType == null).ForEach(ct =>
            {
                string val = ((CustomClass)obj)[ct.Name]?.Value?.ToString();

                if (val != null)
                {
                    sb.Append(comma);
                    sb.Append(ct.Name.Quote() + ":" + val.Quote());
                    comma = ", ";
                }
            });

            return comma;
        }

        private void SerializeObjectTypes(StringBuilder sb, List<PropertyData> propertyData, object obj, string comma)
        {
            propertyData.Where(t => t.ChildType != null).ForEach(ct =>
            {
                sb.Append(comma);
                sb.Append(ct.Name.Quote() + ":{");
                string comma2 = SerializeValueTypes(sb, ct.ChildType.Types, obj);
                SerializeObjectTypes(sb, ct.ChildType.Types, obj, comma2);
                sb.Append("}");
                comma = ", ";
            });
        }
    }

    // All this comes from https://stackoverflow.com/questions/16567283/exposing-properties-of-an-expandoobject
    public class ExpandoObjectTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly ExpandoObject _expando;

        public ExpandoObjectTypeDescriptor(ExpandoObject expando)
        {
            _expando = expando;
        }

        // Just use the default behavior from TypeDescriptor for most of these
        // This might need some tweaking to work correctly for ExpandoObjects though...

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _expando;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        // This is where the GetProperties() calls are
        // Ignore the Attribute for now, if it's needed support will have to be implemented
        // Should be enough for simple usage...

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            // This just casts the ExpandoObject to an IDictionary<string, object> to get the keys
            return new PropertyDescriptorCollection(
                ((IDictionary<string, object>)_expando).Keys
                .Select(x => new ExpandoPropertyDescriptor(((IDictionary<string, object>)_expando), x))
                .ToArray());
        }

        // A nested PropertyDescriptor class that can get and set properties of the
        // ExpandoObject dynamically at run time
        private class ExpandoPropertyDescriptor : PropertyDescriptor
        {
            private readonly IDictionary<string, object> _expando;
            private readonly string _name;

            public ExpandoPropertyDescriptor(IDictionary<string, object> expando, string name)
                : base(name, null)
            {
                _expando = expando;
                _name = name;
            }

            public override Type PropertyType
            {
                get { return _expando[_name]?.GetType() ?? typeof(string); }
            }

            public override void SetValue(object component, object value)
            {
                _expando[_name] = value;
            }

            public override object GetValue(object component)
            {
                return _expando[_name];
            }

            public override bool IsReadOnly
            {
                get
                {
                    // You might be able to implement some better logic here
                    return false;
                }
            }

            public override Type ComponentType
            {
                get { return null; }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override string Category
            {
                get { return string.Empty; }
            }

            public override string Description
            {
                get { return string.Empty; }
            }
        }
    }
    public class ExpandoObjectTypeDescriptionProvider : TypeDescriptionProvider
    {
        private static readonly TypeDescriptionProvider m_Default = TypeDescriptor.GetProvider(typeof(ExpandoObject));

        public ExpandoObjectTypeDescriptionProvider()
            : base(m_Default)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var defaultDescriptor = base.GetTypeDescriptor(objectType, instance);

            return instance == null ? defaultDescriptor :
                new ExpandoObjectTypeDescriptor((ExpandoObject)instance);
        }
    }
}
