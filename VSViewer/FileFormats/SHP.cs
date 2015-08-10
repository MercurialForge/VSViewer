using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats
{
    public class SHP : ContentBase
    {
        public List<Joint> joints = new List<Joint>();
        public List<Group> groups = new List<Group>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Polygon> polygons = new List<Polygon>();
        //AKAO
        //MAGIC EFFECT
        public List<TextureMap> textures = new List<TextureMap>();
    }
}
