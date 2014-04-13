using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public class SinkWriter : COMDisposable<IMFSinkWriter>
    {
		public SinkWriter(IMFSinkWriter instance) : base(instance)  {  }

		public SinkStream AddStream (MediaType mediaType)
		{
			int streamIndex;
			instance.AddStream(mediaType.instance, out streamIndex).Hr();
			return new SinkStream(this, streamIndex);
		}

		public SinkWriterScope BeginWriting ()
		{
			return new SinkWriterScope(this);
		}
    }
}
