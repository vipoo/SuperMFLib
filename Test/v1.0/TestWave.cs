using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Utils;
using MediaFoundation.Misc;

namespace Testv10
{
    class TestWave
    {
        public void DoTests()
        {
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

            Debug.Assert(w1.IsEqual(w2));
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

            Debug.Assert(w1.IsEqual(w2));
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

            Debug.Assert(w1.IsEqual(w2));
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

            // IsEqual won't work cuz of the cbSize issue
            //Debug.Assert(w1.IsEqual(w2));
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

            Debug.Assert(w1.IsEqual(w2));
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

            Debug.Assert(w1.IsEqual(w2));
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

            Debug.Assert(w1.IsEqual(w2));
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
