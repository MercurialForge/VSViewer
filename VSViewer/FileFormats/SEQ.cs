using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats.Sections;

namespace VSViewer.FileFormats
{
    public class SEQ : AssetBase
    {
        public List<Animation> animations = new List<Animation>();
    }
}
