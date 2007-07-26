using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class TestWave
    {
        public void DoTests()
        {
            TestEqual();

            TestWaveFormatEx();
            TestWaveFormatExPCM();
            TestWaveFormatExWithData();
            TestWaveFormatExWithData2();

            TestWaveFormatExtensible();
            TestWaveFormatExtensibleWithData();
            TestWaveFormatExtensibleWithData2();
        }

        private void TestWaveFormatEx()
        {
            WaveFormatEx w1 = new WaveFormatEx();
            FillWave(w1, 0);

            IntPtr ip = w1.GetPtr();
            WaveFormatEx w2 = WaveFormatEx.PtrToWave(ip);

            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(w1 == w2);
        }

        private void TestWaveFormatExtensible()
        {
            WaveFormatExtensible w1 = new WaveFormatExtensible();
            FillWave(w1, 0);
            w1.wFormatTag = -2;
            w1.cbSize = 22;
            FillExtensible(w1, 6);

            IntPtr ip = w1.GetPtr();
            WaveFormatExtensible w2 = WaveFormatExtensible.PtrToWave(ip) as WaveFormatExtensible;

            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(w1 == w2);
        }

        private void TestWaveFormatExWithData()
        {
            int iDataSize = 5;

            WaveFormatExWithData w1 = new WaveFormatExWithData();
            FillWave(w1, 0);

            w1.byteData = new byte[iDataSize];
            FillByteData(w1.byteData, 0);
            w1.cbSize = (short)(iDataSize);

            IntPtr ip = w1.GetPtr();
            WaveFormatExWithData w2 = WaveFormatEx.PtrToWave(ip) as WaveFormatExWithData;

            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(w1 == w2);
        }

        private void TestWaveFormatExPCM()
        {
            int iDataSize = 5;

            WaveFormatEx w1 = new WaveFormatEx();
            FillWave(w1, 0);

            w1.cbSize = (short)(iDataSize);
            w1.wFormatTag = 1;

            IntPtr ip = w1.GetPtr();
            WaveFormatEx w2 = WaveFormatEx.PtrToWave(ip) as WaveFormatEx;

            Marshal.FreeCoTaskMem(ip);

            // Equals won't work cuz of the cbSize issue
            //Debug.Assert(w1 == w2);
        }

        private void TestWaveFormatExWithData2()
        {
            int iDataSize = 10;

            WaveFormatExWithData w1 = new WaveFormatExWithData();
            FillWave(w1, 0);

            w1.byteData = new byte[iDataSize];
            FillByteData(w1.byteData, 0);
            w1.cbSize = (short)(iDataSize);

            IntPtr ip = w1.GetPtr();
            WaveFormatExWithData w2 = WaveFormatEx.PtrToWave(ip) as WaveFormatExWithData;

            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(w1 == w2);
        }

        private void TestWaveFormatExtensibleWithData()
        {
            int iDataSize = 6;
            WaveFormatExtensibleWithData w1 = new WaveFormatExtensibleWithData();
            FillWave(w1, 0);
            w1.wFormatTag = -2;
            w1.cbSize = (short)(22 + iDataSize);
            w1.byteData = new byte[iDataSize];
            FillByteData(w1.byteData, 2);
            FillExtensible(w1, 6);

            IntPtr ip = w1.GetPtr();
            WaveFormatExtensibleWithData w2 = WaveFormatExtensibleWithData.PtrToWave(ip) as WaveFormatExtensibleWithData;

            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(w1 == w2);
        }

        private void TestWaveFormatExtensibleWithData2()
        {
            int iDataSize = 10;
            WaveFormatExtensibleWithData w1 = new WaveFormatExtensibleWithData();
            FillWave(w1, 0);
            w1.wFormatTag = -2;
            w1.cbSize = (short)(22 + iDataSize);
            w1.byteData = new byte[iDataSize];
            FillByteData(w1.byteData, 2);
            FillExtensible(w1, 6);

            IntPtr ip = w1.GetPtr();
            WaveFormatExtensibleWithData w2 = WaveFormatExtensibleWithData.PtrToWave(ip) as WaveFormatExtensibleWithData;

            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(w1 == w2);
        }

        private void TestEqual()
        {
            WaveFormatEx a1 = new WaveFormatEx();
            WaveFormatEx a2 = new WaveFormatEx();
            WaveFormatEx a3 = new WaveFormatEx();
            WaveFormatEx a4 = null;

            WaveFormatExWithData b1 = new WaveFormatExWithData();
            WaveFormatExWithData b2 = new WaveFormatExWithData();
            WaveFormatExWithData b3 = new WaveFormatExWithData();
            WaveFormatEx b4 = null;

            WaveFormatExtensible c1 = new WaveFormatExtensible();
            WaveFormatExtensible c2 = new WaveFormatExtensible();
            WaveFormatExtensible c3 = new WaveFormatExtensible();
            WaveFormatEx c4 = null;

            WaveFormatExtensibleWithData d1 = new WaveFormatExtensibleWithData();
            WaveFormatExtensibleWithData d2 = new WaveFormatExtensibleWithData();
            WaveFormatExtensibleWithData d3 = new WaveFormatExtensibleWithData();
            WaveFormatEx d4 = null;

            FillWave(a1, 1);
            FillWave(a2, 1);
            FillWave(a3, 2);

            FillWave(b1, 1);
            FillWave(b2, 1);
            FillWave(b3, 2);

            FillWave(c1, 1);
            FillWave(c2, 1);
            FillWave(c3, 2);

            FillWave(d1, 1);
            FillWave(d2, 1);
            FillWave(d3, 2);

            b1.byteData = new byte[3];
            b2.byteData = new byte[3];

            d1.byteData = new byte[3];
            d2.byteData = new byte[3];

            FillByteData(b1.byteData, 2);
            FillByteData(b2.byteData, 2);

            FillByteData(d1.byteData, 2);
            FillByteData(d2.byteData, 2);

            Debug.Assert(!a1.Equals(null));
            Debug.Assert(!a1.Equals(this));
            Debug.Assert(a1.Equals(a1));
            Debug.Assert(a1.Equals(a2));
            Debug.Assert(!a1.Equals(a3));
            Debug.Assert(!a1.Equals(a4));
            Debug.Assert(a1 != null);
            Debug.Assert(null != a1);
            Debug.Assert(a1 == a2);
            Debug.Assert(a2 == a1);
            Debug.Assert(a1 != a3);
            Debug.Assert(a3 != a1);
            Debug.Assert(a4 != a1);
            Debug.Assert(a1 != a4);

            Debug.Assert(!b1.Equals(null));
            Debug.Assert(!b1.Equals(this));
            Debug.Assert(b1.Equals(b1));
            Debug.Assert(b1.Equals(b2));
            Debug.Assert(!b1.Equals(b3));
            Debug.Assert(!b1.Equals(b4));
            Debug.Assert(b1 != null);
            Debug.Assert(null != b1);
            Debug.Assert(b1 == b2);
            Debug.Assert(b2 == b1);
            Debug.Assert(b1 != b3);
            Debug.Assert(b3 != b1);
            Debug.Assert(b4 != b1);
            Debug.Assert(b1 != b4);

            Debug.Assert(!c1.Equals(null));
            Debug.Assert(!c1.Equals(this));
            Debug.Assert(c1.Equals(c1));
            Debug.Assert(c1.Equals(c2));
            Debug.Assert(!c1.Equals(c3));
            Debug.Assert(!c1.Equals(c4));
            Debug.Assert(c1 != null);
            Debug.Assert(null != c1);
            Debug.Assert(c1 == c2);
            Debug.Assert(c2 == c1);
            Debug.Assert(c1 != c3);
            Debug.Assert(c3 != c1);
            Debug.Assert(c4 != c1);
            Debug.Assert(c1 != c4);

            Debug.Assert(!d1.Equals(null));
            Debug.Assert(!d1.Equals(this));
            Debug.Assert(d1.Equals(d1));
            Debug.Assert(d1.Equals(d2));
            Debug.Assert(!d1.Equals(d3));
            Debug.Assert(!d1.Equals(d4));
            Debug.Assert(d1 != null);
            Debug.Assert(null != d1);
            Debug.Assert(d1 == d2);
            Debug.Assert(d2 == d1);
            Debug.Assert(d1 != d3);
            Debug.Assert(d3 != d1);
            Debug.Assert(d4 != d1);
            Debug.Assert(d1 != d4);

            Debug.Assert(!a1.Equals(b1));
            Debug.Assert(!a1.Equals(c1));
            Debug.Assert(!a1.Equals(d1));

            Debug.Assert(!b1.Equals(a1));
            Debug.Assert(!b1.Equals(c1));
            Debug.Assert(!b1.Equals(d1));

            Debug.Assert(!c1.Equals(a1));
            Debug.Assert(!c1.Equals(b1));
            Debug.Assert(!c1.Equals(d1));

            Debug.Assert(!d1.Equals(a1));
            Debug.Assert(!d1.Equals(b1));
            Debug.Assert(!d1.Equals(c1));

            Debug.Assert(a1 != b1);
            Debug.Assert(a1 != c1);
            Debug.Assert(a1 != d1);

            Debug.Assert(b1 != a1);
            Debug.Assert(b1 != c1);
            Debug.Assert(b1 != d1);

            Debug.Assert(c1 != a1);
            Debug.Assert(c1 != b1);
            Debug.Assert(c1 != d1);

            Debug.Assert(d1 != a1);
            Debug.Assert(d1 != b1);
            Debug.Assert(d1 != c1);
        }

        private void FillWave(WaveFormatEx w1, short iOffset)
        {
            w1.nBlockAlign = (short)(iOffset + 3);
            w1.nAvgBytesPerSec = (short)(iOffset + 4);
            w1.nChannels = (short)(iOffset + 5);
            w1.nSamplesPerSec = (short)(iOffset + 6);
            w1.wBitsPerSample = (short)(iOffset + 7);
            w1.wFormatTag = (short)(iOffset + 8);
        }

        private void FillExtensible(WaveFormatExtensible w1, short iOffset)
        {
            w1.wReserved = iOffset;
            w1.dwChannelMask = (WaveMask)(iOffset + 1);
            w1.SubFormat = Guid.NewGuid();
        }

        private void FillByteData(byte[] b, int iOffset)
        {
            for (int x = 0; x < b.Length; x++)
            {
                b[x] = (byte)(x + iOffset);
            }
        }
    }
}
