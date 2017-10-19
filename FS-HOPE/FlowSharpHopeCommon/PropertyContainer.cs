using System.Collections.Generic;

namespace FlowSharpHopeCommon
{
    public class PropertyContainer
    {
        public List<PropertyData> Types { get; set; }

        public PropertyContainer()
        {
            Types = new List<PropertyData>();
        }
    }
}
