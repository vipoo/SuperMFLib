using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
	public struct SourceStream
	{
		public readonly SourceReader sourceReader;
		public readonly int index;

		public SourceStream(SourceReader sourceReader, int index)
		{
			this.sourceReader = sourceReader;
			this.index = index;
		}
			
		public bool IsSelected
		{
			get
			{
				bool result;
				sourceReader.instance.GetStreamSelection (index, out result).Hr ();
				return result;
			}
		}

		public MediaType CurrentMediaType
		{
			get
			{
				IMFMediaType ppMediaType;

				sourceReader.instance.GetCurrentMediaType (index, out ppMediaType).Hr ();

				return new MediaType(ppMediaType);
			}
			set
			{
				sourceReader.instance.SetCurrentMediaType (index, IntPtr.Zero, value.instance).Hr ();
			}
		}

		public MediaType NativeMediaType
		{
			get
			{
				IMFMediaType ppMediaType;

				sourceReader.instance.GetNativeMediaType (index, 0, out ppMediaType).Hr ();

				return new MediaType(ppMediaType);
			}
		}

		public ulong Duration
		{
			get
			{
				PropVariant result = new PropVariant ();
				sourceReader.instance.GetPresentationAttribute (index, MFAttributesClsid.MF_PD_DURATION, result).Hr ();
				return result.GetULong ();
			}
		}
	}


}
