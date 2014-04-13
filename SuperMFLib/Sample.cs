using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
    public class Sample : COMDisposable<IMFSample>
	{
		public Sample(IMFSample instance) :base(instance) { }

		public bool Discontinuity
		{
			get 
			{
				int result;
				instance.GetUINT32(MFAttributesClsid.MFSampleExtension_Discontinuity, out result).Hr();
				return result != 0;
			}
			set
            {
                instance.SetUINT32 (MFAttributesClsid.MFSampleExtension_Discontinuity, value ? 1 : 0).Hr ();
            }
		}

        public MFMediaBuffer ConvertToContiguousBuffer()
        {
            IMFMediaBuffer mediaBuffer;
            instance.ConvertToContiguousBuffer(out mediaBuffer);

            return new MFMediaBuffer(mediaBuffer);
        }
    }
}
