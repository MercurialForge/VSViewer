using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    class SkinWeight
    {
        public int weight;
        public int vertex;
        public int boneID;

        public SkinWeight (int w, int v, int bID)
        {
            weight = w;
            vertex = v;
            boneID = bID;
        }
    }
}
