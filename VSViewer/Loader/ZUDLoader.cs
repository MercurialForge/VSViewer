using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSViewer.FileFormats;

namespace VSViewer.Loader
{
    static class ZUDLoader
    {
        static public ZUD FromStream(EndianBinaryReader reader)
        {
            /*=====================================================================
                ZUD HEADER
            =====================================================================*/
            byte IDCharacter = reader.ReadByte();
            byte IDWeapon = reader.ReadByte();
            byte IDWeaponCategory = reader.ReadByte();
            byte IDWeaponMaterial = reader.ReadByte();
            byte IDShield = reader.ReadByte();
            byte IDShieldMaterial = reader.ReadByte();
            byte unknown = reader.ReadByte();
            reader.SkipByte();
            UInt32 ptrCharacterSHP = reader.ReadUInt32();
            UInt32 lenCharacterSHP = reader.ReadUInt32();
            UInt32 ptrWeaponWEP = reader.ReadUInt32();
            UInt32 lenWeaponWEP= reader.ReadUInt32();
            UInt32 ptrShieldWEP = reader.ReadUInt32();
            UInt32 lenShieldWEP = reader.ReadUInt32();
            UInt32 ptrCommonSEQ = reader.ReadUInt32();
            UInt32 lenCommonSEQ = reader.ReadUInt32();
            UInt32 ptrBattleSEQ = reader.ReadUInt32();
            UInt32 lenBattleSEQ = reader.ReadUInt32();

            /*=====================================================================
                STREAM READER
            =====================================================================*/
            ZUD zud = new ZUD();

            zud.Character = SHPLoader.FromStream(reader);

            if (lenWeaponWEP != 0)
            {
                zud.HasWeapon = true;
                reader.BaseStream.Seek(ptrWeaponWEP, System.IO.SeekOrigin.Begin);
                zud.Weapon = WEPLoader.FromStream(reader);
            }

            if (lenShieldWEP != 0)
            {
                zud.HasShield = true;
                reader.BaseStream.Seek(ptrShieldWEP, System.IO.SeekOrigin.Begin);
                zud.Shield = WEPLoader.FromStream(reader);
            }

            if (lenCommonSEQ != 0)
            {
                zud.HasCommon = true;
                reader.BaseStream.Seek(ptrCommonSEQ, System.IO.SeekOrigin.Begin);
                zud.Common = SEQLoader.FromStream(reader, zud.Character);
            }

            if (lenBattleSEQ != 0)
            {
                zud.HasBattle = true;
                reader.BaseStream.Seek(ptrBattleSEQ, System.IO.SeekOrigin.Begin);
                zud.Battle = SEQLoader.FromStream(reader, zud.Character);
            }

            return zud;
        }
    }
}
