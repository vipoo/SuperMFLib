using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
	public struct SourceReaderSampleFlags
	{
		readonly int flags;

		public SourceReaderSampleFlags (int flags)
		{
			this.flags = flags;
		}

		public bool CurrentMediaTypeChanged
		{
			get { return (flags & (int)MF_SOURCE_READER_FLAG.CurrentMediaTypeChanged) != 0; }
		}


        public bool StreamTick
        {
            get { return (flags & (int)MF_SOURCE_READER_FLAG.StreamTick) != 0; }
        }
    }	
}
