using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using MediaFoundation.Transform;

namespace MediaFoundation.Net
{
	public class MFSystem : IDisposable
    {
		public static _MFCollection TranscodeGetAudioOutputAvailableTypes (Guid subType, MFT_EnumFlag flags)
		{
			IMFCollection availableTypes;
			MFExtern.MFTranscodeGetAudioOutputAvailableTypes (subType, (int)flags, null, out availableTypes).Hr();

			return new _MFCollection (availableTypes);
		}

        public static MFSystem Start(MFStartup flags = MFStartup.Full)
        {
			MFExtern.MFStartup (0x10070, flags).Hr ();
            return new MFSystem();
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
			MFExtern.MFShutdown ().Hr ();
			GC.SuppressFinalize(this);
        }

		~MFSystem()
		{
			Dispose();
		}
        private MFSystem()
        { }
    }
}
