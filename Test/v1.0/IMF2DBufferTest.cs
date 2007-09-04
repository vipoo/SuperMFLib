// Add this to MFT_GrayScale

        private void IMF2DBufferTest(IMF2DBuffer pOut2D)
        {
            int i;
            bool b;
            IntPtr ip;

            pOut2D.GetScanline0AndPitch(out ip, out i);

            pOut2D.GetContiguousLength(out i);
            Debug.Assert(i > 0, "GetContiguousLength");

            pOut2D.IsContiguousFormat(out b);

            ip = Marshal.AllocCoTaskMem(i);
            pOut2D.ContiguousCopyTo(ip, i);

            pOut2D.ContiguousCopyFrom(ip, i);
        }

