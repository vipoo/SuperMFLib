using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public struct SinkStream 
	{
		readonly SinkWriter sinkWriter;
		readonly int index;

		public SinkStream (SinkWriter sinkWriter, int streamIndex) 
		{
			this.sinkWriter = sinkWriter;
			this.index = streamIndex;
		}

		public MediaType InputMediaType
		{
			set
			{
				sinkWriter.instance.SetInputMediaType (index, value.instance, null).Hr ();
			}
		}

        public void WriteSample(Sample sample)
        {
            sinkWriter.instance.WriteSample(index, sample.instance);
        }

        public void SendStreamTick(long timestamp)
        {
            sinkWriter.instance.SendStreamTick(index, timestamp);
        }
    }
}
