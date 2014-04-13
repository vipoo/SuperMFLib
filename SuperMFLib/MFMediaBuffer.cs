using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaFoundation.Net
{
    public class MFMediaBuffer : COMDisposable<IMFMediaBuffer>
    {
        public MFMediaBuffer(IMFMediaBuffer instance) : base(instance) { }

        public LockedMediaBuffer Lock()
        {
            LockedMediaBuffer result = new LockedMediaBuffer { mediaBuffer = this };

            instance.Lock(out result.buffer, out result.maxLength, out result.currentLength).Hr();

            return result;
        }

        public struct LockedMediaBuffer : IDisposable
        {
            internal MFMediaBuffer mediaBuffer;
            internal IntPtr buffer;
            internal int maxLength;
            internal int currentLength;

            public void Dispose()
            {
                mediaBuffer.instance.Unlock().Hr();
            }

            public IntPtr Buffer { get { return buffer; } }
        }

    }
}
