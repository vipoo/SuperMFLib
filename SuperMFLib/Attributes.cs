using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public class Attributes : COMDisposable<IMFAttributes>
    {
		public Attributes(int initialSize = 1) : base(NewInstance(initialSize))  {  }

		private static IMFAttributes NewInstance(int initialSize)
		{
			IMFAttributes instance;
			MFExtern.MFCreateAttributes(out instance, initialSize).Hr();
			return instance;
		}

        public bool ReadWriterEnableHardwareTransforms
        {
            get
            {
                int result;
                MFError.ThrowExceptionForHR(instance.GetUINT32(MFAttributesClsid.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, out result));
                return result != 0;
            }
            set
            {
                MFError.ThrowExceptionForHR(instance.SetUINT32(MFAttributesClsid.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, value ? 1 : 0));

            }
        }

        public bool SourceReaderEnableVideoProcessing
        {
            get
            {
                int result;
                MFError.ThrowExceptionForHR(instance.GetUINT32(MFAttributesClsid.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING, out result));
                return result != 0;
            }
            set
            {
                MFError.ThrowExceptionForHR(instance.SetUINT32(MFAttributesClsid.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING, value ? 1 : 0));
            }
        }
    }
}
