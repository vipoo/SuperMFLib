﻿using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System.Collections.Generic;
using System.Linq;

namespace MediaFoundation.Net
{
	public enum MFSourceReader
	{
		MediaSource = -1
	}

	public class SourceReader : COMDisposable<IMFSourceReader>
	{
		public SourceReader(IMFSourceReader instance) : base(instance) { }

		public IEnumerable<SourceReaderSample> Samples(int streamIndex = (int)MF_SOURCE_READER.AnyStream, int controlFlags = 0)
		{
			var countOfSelectedStreams = Streams.Where( s => s.IsSelected ).Count();
			var countOfClosedStreams = 0;

			var sampleCounts = new int[countOfSelectedStreams];
			for(var i = 0; i < countOfSelectedStreams; i++)
				sampleCounts[i] = 0;

			var duration = MediaSource.Duration;

			while(countOfClosedStreams < countOfSelectedStreams  )
			{
				int actualStreamIndex;
				int flags;
				long timestamp;
				IMFSample sample;

				this.instance.ReadSample(streamIndex, controlFlags, out actualStreamIndex, out flags, out timestamp, out sample).Hr();

				yield return new SourceReaderSample(
					new SourceStream(this, actualStreamIndex),
					new SourceReaderSampleFlags(flags), 
					timestamp,
					(int)(Math.Max(timestamp, 0) * 100L / (long)duration),
					sample == null ? null : new Sample(sample),
					sampleCounts[actualStreamIndex]++
				);

                System.GC.Collect(10, GCCollectionMode.Forced);

				if( (flags & (int)MF_SOURCE_READER_FLAG.EndOfStream) != 0 )
					countOfClosedStreams++;
			}
		}

		public IEnumerable<SourceStream> Streams
		{
			get
			{
				int i = 0;
				bool ignored;
				while (true)
				{
					var hr = instance.GetStreamSelection (i, out ignored);
					if (hr == MFError.MF_E_INVALIDSTREAMNUMBER)
						yield break;
				
					yield return new SourceStream(this, i++);
				}
			}
		}			
			
		public SourceStream MediaSource
		{
			get
			{
				return new SourceStream (this, (int)MFSourceReader.MediaSource);
			}
		}


		public object GetPresentationAttribute(MFSourceReader dwStreamIndex, Guid guidAttribute)
		{
			PropVariant result = new PropVariant ();
			instance.GetPresentationAttribute ((int)dwStreamIndex, guidAttribute,  result);
			switch (result.GetVariantType ()) {

			case ConstPropVariant.VariantType.Double:
				return result.GetDouble ();

			case ConstPropVariant.VariantType.UInt32:
				return result.GetUInt ();

			case ConstPropVariant.VariantType.UInt64:
				return result.GetULong ();
			}

			return null;
		}
	}
}