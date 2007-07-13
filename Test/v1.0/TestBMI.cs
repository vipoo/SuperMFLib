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
            w1.biCompression = 3;

            IntPtr ip = w1.GetPtr();
            BitmapInfoHeaderWithData w2 = BitmapInfoHeader.PtrToBMI(ip) as BitmapInfoHeaderWithData;

            Marshal.FreeCoTaskMem(ip);
        }

        private void TestBitmapInfoHeaderWithData2()
        {
            BitmapInfoHeaderWithData w1 = new BitmapInfoHeaderWithData();
            FillBMI(w1, 0);
            w1.bmiColors = new int[] { 1, 2, 3, 4, 5, 6 };
            w1.biCompression = 0;
            w1.biClrUsed = 6;

            IntPtr ip = w1.GetPtr();
            BitmapInfoHeaderWithData w2 = BitmapInfoHeader.PtrToBMI(ip) as BitmapInfoHeaderWithData;

            Marshal.FreeCoTaskMem(ip);
        }

        private void TestBitmapInfoHeaderWithData3()
        {
            BitmapInfoHeaderWithData w1 = new BitmapInfoHeaderWithData();
            FillBMI(w1, 0);
            w1.bmiColors = new int[256];
            w1.biCompression = 0;
            w1.biClrUsed = 0;
            w1.biBitCount = 8;

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
            w1.biSize = Marshal.SizeOf(typeof(BitmapInfoHeader));
            w1.biClrUsed = 0;
            w1.biBitCount = 32;
            w1.biCompression = 0;

            w1.biClrImportant = 1 + iOffset;
            w1.biHeight = 2 + iOffset;
            w1.biPlanes = (short)(3 + iOffset);
            w1.biSizeImage = 4 + iOffset;
            w1.biWidth = 5 + iOffset;
            w1.biXPelsPerMeter = 6 + iOffset;
            w1.biYPelsPerMeter = 7 + iOffset;
        }
    }
}
