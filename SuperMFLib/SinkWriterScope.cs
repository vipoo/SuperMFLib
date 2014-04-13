using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public class SinkWriterScope : IDisposable
	{
		public readonly SinkWriter sinkWriter;

		public SinkWriterScope(SinkWriter sinkWriter)
		{
			this.sinkWriter = sinkWriter;
			sinkWriter.instance.BeginWriting().Hr();
		}

		public void Dispose ()
		{
			sinkWriter.instance.Finalize_();
		}
	}
}
