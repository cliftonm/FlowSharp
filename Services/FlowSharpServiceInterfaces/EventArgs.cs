using System;

using FlowSharpLib;

namespace FlowSharpServiceInterfaces
{
    public class FileEventArgs : EventArgs
    {
        public string Filename { get; set; }
    }

    public class NewCanvasEventArgs : EventArgs
    {
        public BaseController Controller { get; set; }
    }
}
