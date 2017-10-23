using System.Collections.Generic;

namespace FlowSharpHopeCommon
{
    public class ReceptorDescription
    {
        /// <summary>
        /// The type name of the receptor class that receives the ReceptorSemanticType and optionally publishes other types.
        /// </summary>
        public string ReceptorTypeName { get; set; }

        /// <summary>
        /// The semantic type that the receptor Process method receives.
        /// </summary>
        public string ReceivingSemanticType { get; set; }

        /// <summary>
        /// The types that the receptor Process method publishes.
        /// </summary>
        public List<string> Publishes { get; set; }

        public ReceptorDescription()
        {
            Publishes = new List<string>();
        }
    }
}
