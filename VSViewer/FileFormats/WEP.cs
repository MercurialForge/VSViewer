using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats;

namespace VSViewer.FileFormats
{
    public class WEP : ContentBase
    {
        public List<Joint> joints = new List<Joint>();
        public List<Group> groups = new List<Group>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Polygon> polygons = new List<Polygon>();
        public List<TextureMap> textures = new List<TextureMap>();
    }
}
