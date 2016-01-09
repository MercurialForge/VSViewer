using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.FileFormats
{
    public class ZUD
    {
        public SHP Character { get; set; }

        public bool HasWeapon { get; set; }
        public WEP Weapon { get; set; }

        public bool HasShield { get; set; }
        public WEP Shield { get; set; }

        public bool HasCommon { get; set; }
        public SEQ Common { get; set; }

        public bool HasBattle { get; set; } 
        public SEQ Battle { get; set; }
    }
}
