using System;
using System.Collections;
using System.ComponentModel;

namespace HopeShapes.PropertyGridHelpers
{
    // From: https://www.codeproject.com/Articles/9280/Add-Remove-Items-to-from-PropertyGrid-at-Runtime
    public class CustomClass : CollectionBase, ICustomTypeDescriptor
    {
        public void Add(CustomProperty Value)
        {
            List.Add(Value);
        }

        /// <summary>
        /// Remove item from List
        /// </summary>
        /// <param name="Name"></param>
        public void Remove(string Name)
        {
            foreach (CustomProperty prop in base.List)
            {
                if (prop.Name == Name)
                {
                    base.List.Remove(prop);
                    return;
                }
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        public CustomProperty this[int index]
        {
            get
            {
                return (CustomProperty)base.List[index];
            }
            set
            {
                base.List[index] = (CustomProperty)value;
            }
        }


        /// <summary>
        /// Get Class Name
        /// </summary>
        /// <returns>String</returns>
        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        /// <summary>
        /// GetAttributes
        /// </summary>
        /// <returns>AttributeCollection</returns>
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        /// <summary>
        /// GetComponentName
        /// </summary>
        /// <returns>String</returns>
        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        /// <summary>
        /// GetConverter
        /// </summary>
        /// <returns>TypeConverter</returns>
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        /// <summary>
        /// GetDefaultEvent
        /// </summary>
        /// <returns>EventDescriptor</returns>
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        /// <summary>
        /// GetDefaultProperty
        /// </summary>
        /// <returns>PropertyDescriptor</returns>
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        /// <summary>
        /// GetEditor
        /// </summary>
        /// <param name="editorBaseType">editorBaseType</param>
        /// <returns>object</returns>
        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
            for (int i = 0; i < this.Count; i++)
            {

                CustomProperty prop = (CustomProperty)this[i];
                newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
            }

            return new PropertyDescriptorCollection(newProps);
        }

        public PropertyDescriptorCollection GetProperties()
        {

            return TypeDescriptor.GetProperties(this, true);

        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
