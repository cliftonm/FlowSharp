using System;
using System.ComponentModel;

namespace HopeShapes.PropertyGridHelpers
{
    // From: https://www.codeproject.com/Articles/9280/Add-Remove-Items-to-from-PropertyGrid-at-Runtime
    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        CustomProperty m_Property;

        public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs) : base(myProperty.Name, attrs)
        {
            m_Property = myProperty;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return null;
            }
        }

        public override object GetValue(object component)
        {
            return m_Property.Value;
        }

        public override string Description
        {
            get
            {
                return m_Property.Description;
            }
        }

        public override string Category
        {
            get
            {
                return m_Property.Category;
            }
        }

        public override string DisplayName
        {
            get
            {
                return m_Property.Name;
            }

        }

        public override bool IsReadOnly
        {
            get
            {
                return m_Property.ReadOnly;
            }
        }

        public override void ResetValue(object component)
        {
            //Have to implement
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            m_Property.Value = value;
        }

        public override Type PropertyType
        {
            get { return m_Property.PropertyType; }
        }
    }
}
