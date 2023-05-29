using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNCSync.RulerControl
{
    public class RulerStepProperties
    {
        public double PixelSize { get; set; }
        public double Value { get; set; }

        public void Deconstruct(out double pixelSize, out double value)
        {
            pixelSize = PixelSize;
            value = Value;
        }
    }
}
