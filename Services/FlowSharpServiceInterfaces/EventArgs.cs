using System;

namespace FlowSharpServiceInterfaces
{
    public class FileEventArgs : EventArgs
    {
        public string Filename { get; set; }
    }
}
