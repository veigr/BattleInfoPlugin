using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    internal class DVTARGETDEVICE
    {
        public ushort tdSize;
        public uint tdDeviceNameOffset;
        public ushort tdDriverNameOffset;
        public ushort tdExtDevmodeOffset;
        public ushort tdPortNameOffset;
        public byte tdData;
    }
}
