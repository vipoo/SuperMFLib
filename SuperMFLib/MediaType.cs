using System;
using MediaFoundation.Misc;
using System.Drawing;

namespace MediaFoundation.Net
{
	public class MediaType : COMDisposable<IMFMediaType>, IDisposable
    {
		public MediaType(IMFMediaType instance) : base(instance) 	{ }

		public MediaType () : base(MediaType.NewInstance())			{ }

		public MediaType (MediaType type) : base(CopyFrom(type))	{ }
			
		private static IMFMediaType NewInstance()
		{
			IMFMediaType result;
			MFExtern.MFCreateMediaType(out result).Hr();
			return result;
		}

		private static IMFMediaType CopyFrom(MediaType type)
		{
			IMFMediaType result;
			MFExtern.MFCreateMediaType(out result).Hr();
			type.instance.CopyAllItems(result);

			return result;
		}

		public Guid MajorType
		{
			get { return GetGuid (MFAttributesClsid.MF_MT_MAJOR_TYPE);	}
			set	{ SetGuid (MFAttributesClsid.MF_MT_MAJOR_TYPE, value); }
		}

		public Guid SubType
		{
			get	{ return GetGuid (MFAttributesClsid.MF_MT_SUBTYPE); }
			set	{ SetGuid (MFAttributesClsid.MF_MT_SUBTYPE, value); }
		}

		public bool IsAudio
		{
			get	{ return MajorType == MFMediaType.Audio; }
		}

		public bool IsVideo
		{
			get	{ return MajorType == MFMediaType.Video; }
		}

		public int AudioNumberOfChannels
		{
			get { return GetInt (MFAttributesClsid.MF_MT_AUDIO_NUM_CHANNELS); }
		}

		public int AudioSamplesPerSecond
		{
			get	{ return GetInt (MFAttributesClsid.MF_MT_AUDIO_SAMPLES_PER_SECOND);	}
		}

		public int BitRate
		{
			get { return GetInt (MFAttributesClsid.MF_MT_AVG_BITRATE); }
			set { SetInt (MFAttributesClsid.MF_MT_AVG_BITRATE, value); }
		}

		public MFVideoInterlaceMode InterlaceMode
		{
			get
			{
				return (MFVideoInterlaceMode)GetInt (MFAttributesClsid.MF_MT_INTERLACE_MODE);
			}
			set
			{
				SetInt (MFAttributesClsid.MF_MT_INTERLACE_MODE, (int)value);
			}
		}

		public Size FrameSize
		{
			get
			{
				var result = GetLong(MFAttributesClsid.MF_MT_FRAME_SIZE);

				return new Size (result.HighPart(), result.LowPart());
			}
			set
			{
				var size = LongEx.FromInts (value.Width, value.Height);

				SetLong (MFAttributesClsid.MF_MT_FRAME_SIZE, size);
			}
		}

		public FrameRate FrameRate
		{
			get
			{
				var result = GetLong(MFAttributesClsid.MF_MT_FRAME_RATE);

				return new FrameRate (result.HighPart(), result.LowPart());
			}
			set
			{
				var size = LongEx.FromInts (value.Numerator, value.Denominator);

				SetLong (MFAttributesClsid.MF_MT_FRAME_RATE, size);
			}
		}

		public AspectRatio AspectRatio
		{
			get
			{
				var result = GetLong(MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO);

				return new AspectRatio (result.HighPart(), result.LowPart());
			}
			set
			{
				var size = LongEx.FromInts (value.XAspect, value.YAspect);

				SetLong (MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, size);
			}
		}


		public Guid GetGuid(Guid guid)
		{
			Guid result;
			instance.GetGUID (guid, out result).Hr();
			return result;
		}

		public void SetGuid(Guid guid, Guid value)
		{
			instance.SetGUID (guid, value).Hr();
		}

		public int GetInt(Guid guid)
		{
			int result;
			instance.GetUINT32 (guid, out result).Hr ();
			return result;
		}

		public void SetInt(Guid guid, int value)
		{
			instance.SetUINT32 (guid, value).Hr ();;
		}

		public long GetLong(Guid guid)
		{
			long result;
			instance.GetUINT64 (guid, out result).Hr ();
			return result;
		}

		public void SetLong(Guid guid, long value)
		{
			instance.SetUINT64 (guid, value).Hr();
		}
    }
}
