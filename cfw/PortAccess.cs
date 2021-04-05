using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;                  // MessageBox

namespace PortAccess {
    public class PortAccess {
        //inpout.dll

        [DllImport("inpout32.dll")]
        private static extern UInt32 IsInpOutDriverOpen();

        [DllImport("inpout32.dll")]
        private static extern void Out32(short PortAddress, short Data);

        [DllImport("inpout32.dll")]
        private static extern char Inp32(short PortAddress);

        [DllImport("inpout32.dll")]
        private static extern void DlPortWritePortUshort(short PortAddress, ushort Data);

        [DllImport("inpout32.dll")]
        private static extern ushort DlPortReadPortUshort(short PortAddress);

        [DllImport("inpout32.dll")]
        private static extern void DlPortWritePortUlong(int PortAddress, uint Data);

        [DllImport("inpout32.dll")]
        private static extern uint DlPortReadPortUlong(int PortAddress);

        [DllImport("inpoutx64.dll")]
        private static extern bool GetPhysLong(ref int PortAddress, ref uint Data);

        [DllImport("inpoutx64.dll")]
        private static extern bool SetPhysLong(ref int PortAddress, ref uint Data);

        //inpoutx64.dll

        [DllImport("inpoutx64.dll", EntryPoint = "IsInpOutDriverOpen")]
        private static extern UInt32 IsInpOutDriverOpen_x64();

        [DllImport("inpoutx64.dll", EntryPoint = "Out32")]
        private static extern void Out32_x64(short PortAddress, short Data);

        [DllImport("inpoutx64.dll", EntryPoint = "Inp32")]
        private static extern char Inp32_x64(short PortAddress);

        [DllImport("inpoutx64.dll", EntryPoint = "DlPortWritePortUshort")]
        private static extern void DlPortWritePortUshort_x64(short PortAddress, ushort Data);

        [DllImport("inpoutx64.dll", EntryPoint = "DlPortReadPortUshort")]
        private static extern ushort DlPortReadPortUshort_x64(short PortAddress);

        [DllImport("inpoutx64.dll", EntryPoint = "DlPortWritePortUlong")]
        private static extern void DlPortWritePortUlong_x64(int PortAddress, uint Data);

        [DllImport("inpoutx64.dll", EntryPoint = "DlPortReadPortUlong")]
        private static extern uint DlPortReadPortUlong_x64(int PortAddress);

        [DllImport("inpoutx64.dll", EntryPoint = "GetPhysLong")]
        private static extern bool GetPhysLong_x64(ref int PortAddress, ref uint Data);

        [DllImport("inpoutx64.dll", EntryPoint = "SetPhysLong")]
        private static extern bool SetPhysLong_x64(ref int PortAddress, ref uint Data);

        private readonly bool _X64;
        private readonly short _PortAddress;

        public PortAccess(short PortAddress) {
            this._X64 = false;
            this._PortAddress = PortAddress;

            try {
                uint nResult = 0;
                try {
                    nResult = IsInpOutDriverOpen();
                } catch ( BadImageFormatException ) {
                    nResult = IsInpOutDriverOpen_x64();
                    if ( nResult != 0 ) {
                        this._X64 = true;
                    }
                }

                if ( nResult == 0 ) {
                    MessageBox.Show("If you use 'PortIO' the first time on a PC, restart CFW as 'Administrator' and try again. Anytime after, you can operate 'PortIO' as normal user.", "Note");
                }
            } catch ( DllNotFoundException ) {
                ;
            }
        }

        //Public Methods
        public void Write(short Data) {
            try {
                if ( this._X64 ) {
                    Out32_x64(this._PortAddress, Data);
                } else {
                    Out32(this._PortAddress, Data);
                }
            } catch ( DllNotFoundException ) {
                ;
            }
        }
        public byte Read() {
            try {
                if ( this._X64 ) {
                    return (byte)Inp32_x64(this._PortAddress);
                } else {
                    return (byte)Inp32(this._PortAddress);
                }
            } catch ( DllNotFoundException ) {
                return 0;
            }
        }
    }
}