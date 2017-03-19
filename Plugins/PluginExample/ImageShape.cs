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

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
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

        public override void Draw(Graphics gr, bool showSelection = true)
        {
            if (image == null)
            {
                gr.FillRectangle(FillBrush, ZoomRectangle);
            }
            else
            {
                gr.DrawImage(image, ZoomRectangle, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            gr.DrawRectangle(BorderPen, ZoomRectangle);
            base.Draw(gr, showSelection);
        }

        protected void TryLoadImage()
        {
            try
            {
                string fn = filename;

                if (!File.Exists(filename))
                {
                    // Try relative path...
                    fn = Path.Combine(Path.GetDirectoryName(canvas.Controller.Filename), filename);
                }

                if (File.Exists(fn))
                {
                    image?.Dispose();
                    image = null;


                    image = Image.FromFile(fn);
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
