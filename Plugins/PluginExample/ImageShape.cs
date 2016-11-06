/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.IO;

using FlowSharpLib;

namespace PluginExample
{
    public class ImageShape : GraphicElement
    {
        protected Image image;
        protected string filename;

        public string Filename
        {
            get { return filename; }
            set
            {
                if (filename != value)
                {
                    filename = value;
                    TryLoadImage();
                }
            }
        }

        public ImageShape(Canvas canvas) : base(canvas)
		{
            image = Resource1.DefaultImage;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                image?.Dispose();
            }

            base.Dispose(disposing);
        }

        public override ElementProperties CreateProperties()
        {
            return new ImageShapeProperties(this);
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            ImageShape img = (ImageShape)base.CloneDefault(canvas);
            img.Filename = Filename;

            if (string.IsNullOrEmpty(Filename))
            {
                img.image = null;
            }

            return img;
        }

        public override void Serialize(ElementPropertyBag epb, List<GraphicElement> elementsBeingSerialized)
        {
            // TODO: Use JSON dictionary instead.
            epb.ExtraData = Filename;
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            // TODO: Use JSON dictionary instead.
            Filename = epb.ExtraData;
            base.Deserialize(epb);
        }

        public override void Draw(Graphics gr)
        {
            if (image == null)
            {
                gr.FillRectangle(FillBrush, DisplayRectangle);
            }
            else
            {
                gr.DrawImage(image, DisplayRectangle, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            gr.DrawRectangle(BorderPen, DisplayRectangle);
            base.Draw(gr);
        }

        protected void TryLoadImage()
        {
            try
            {
                if (File.Exists(filename))
                {
                    image?.Dispose();
                    image = null;
                    image = Image.FromFile(filename);
                }
                else
                {
                    image?.Dispose();
                    image = null;
                }
            }
            catch { }       // TODO: Silent catch is not good.
        }
    }
}
