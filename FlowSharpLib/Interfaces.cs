/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

namespace FlowSharpLib
{
    public interface IPropertyObject
    {
        // void Update(GraphicElement el, string label);
        void Update(string label);
    }

    // TODO: Probably not the best place for this interface, but I wanted it removed from FlowSharpCodeShapeInterfaces
    public interface IFileBox
    {
        string Filename { get; }
    }
}
