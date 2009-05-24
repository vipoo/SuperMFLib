using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;

namespace Testv10
{
    class TestBMI
    {
        public void DoTests()
        {
            TestBitmapInfoHeader();
            TestBitmapInfoHeaderWithData();
            TestBitmapInfoHeaderWithData2();
            TestBitmapInfoHeaderWithData3();
        }

        private void TestBitmapInfoHeader()
        {
            BitmapInfoHeader w1 = new BitmapInfoHeader();
            FillBMI(w1, 0);

            IntPtr ip = w1.GetPtr();
            BitmapInfoHeader w2 = BitmapInfoHeader.PtrToBMI(ip);

            Marshal.FreeCoTaskMem(ip);
        }

        private void TestBitmapInfoHeaderWithData()
        {
            BitmapInfoHeaderWithData w1 = new BitmapInfoHeaderWithData();
            FillBMI(w1, 0);
            w1.bmiColors = new int[] { 1, 2, 3 };
            w1.Compression = 3;

            IntPtr ip = w1.GetPtr();
            BitmapInfoHeaderWithData w2 = BitmapInfoHeader.PtrToBMI(ip) as BitmapInfoHeaderWithData;

            Marshal.FreeCoTaskMem(ip);
        }

        private void TestBitmapInfoHeaderWithData2()
        {
            BitmapInfoHeaderWithData w1 = new BitmapInfoHeaderWithData();
            FillBMI(w1, 0);
            w1.bmiColors = new int[] { 1, 2, 3, 4, 5, 6 };
            w1.Compression = 0;
            w1.ClrUsed = 6;

            IntPtr ip = w1.GetPtr();
            BitmapInfoHeaderWithData w2 = BitmapInfoHeader.PtrToBMI(ip) as BitmapInfoHeaderWithData;

            Marshal.FreeCoTaskMem(ip);
        }

        private void TestBitmapInfoHeaderWithData3()
        {
            BitmapInfoHeaderWithData w1 = new BitmapInfoHeaderWithData();
            FillBMI(w1, 0);
            w1.bmiColors = new int[256];
            w1.Compression = 0;
            w1.ClrUsed = 0;
            w1.BitCount = 8;

            for (int x = 0; x < 256; x++)
            {
                w1.bmiColors[x] = x + 7;
            }

            IntPtr ip = w1.GetPtr();
            BitmapInfoHeaderWithData w2 = BitmapInfoHeader.PtrToBMI(ip) as BitmapInfoHeaderWithData;

            Marshal.FreeCoTaskMem(ip);
        }

        private void FillBMI(BitmapInfoHeader w1, int iOffset)
        {
            w1.Size = Marshal.SizeOf(typeof(BitmapInfoHeader));
            w1.ClrUsed = 0;
            w1.BitCount = 32;
            w1.Compression = 0;

            w1.ClrImportant = 1 + iOffset;
            w1.Height = 2 + iOffset;
            w1.Planes = (short)(3 + iOffset);
            w1.ImageSize = 4 + iOffset;
            w1.Width = 5 + iOffset;
            w1.XPelsPerMeter = 6 + iOffset;
            w1.YPelsPerMeter = 7 + iOffset;
        }
    }
}
