using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VSViewer.FileFormats
{
    /// <summary>
    /// A CLUT (Color Look Up Table)
    /// The first palette seems to always be the original from the artists and the subsequent palettes appear
    /// to have been generated procedurally at build time. This is evidenced in the artifacts in some palettes > 0.
    /// </summary>
    public class Palette
    {
        public List<byte[]> colors = new List<byte[]>();

        public int GetColorCount() { return colors.Count; }
    }
}
