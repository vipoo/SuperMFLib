using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
	public struct SourceReaderSample
	{
		readonly SourceStream sourceStream;
		readonly SourceReaderSampleFlags flags;
		readonly long timestamp;
		readonly int percentageCompleted;
		readonly int count;
		readonly Sample sample;

		public SourceReaderSample (SourceStream sourceStream, SourceReaderSampleFlags flags, long timestamp, int percentageCompleted, Sample sample, int count)
		{
			this.sourceStream = sourceStream;
			this.flags = flags;
			this.timestamp = timestamp;
			this.percentageCompleted = percentageCompleted;
			this.count = count;
			this.sample = sample;
		}

		public SourceStream Stream
		{
			get { return this.sourceStream; }
		}

		public SourceReaderSampleFlags Flags
		{
			get { return flags; }
		}

		public long Timestamp
		{
			get	{ return timestamp;	}
		}

		public int PercentageCompleted
		{
			get { return percentageCompleted; }
		}

		public int Count
		{
			get { return count;	}
		}

		public Sample Sample
		{
			get { return this.sample; }
		}
	}
}
