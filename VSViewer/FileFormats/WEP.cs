using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats.Sections;

namespace VSViewer.FileFormats
{
    class WEP
    {
        public List<Joint> joints = new List<Joint>();
        public List<Group> groups = new List<Group>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Polygon> polygons = new List<Polygon>();
        //public Texture[] textures; // this may end up being the string to the output texture for the item.
    }
}
