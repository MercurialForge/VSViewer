using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VSViewer.FileFormats.Sections
{
    class Palette
    {
        public byte size;
        public List<Color> colors = new List<Color>();
    }
}
