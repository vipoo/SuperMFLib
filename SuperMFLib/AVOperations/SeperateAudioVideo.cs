using System;

namespace MediaFoundation.Net
{
    public partial class AVOperations
    {
        public static ProcessSample SeperateAudioVideo(ProcessSample audioStreams, ProcessSample videoStreams)
        {
            return sample =>
            {
                if (sample.Stream.NativeMediaType.IsVideo)
                    return videoStreams(sample);

                if (sample.Stream.NativeMediaType.IsAudio)
                    return audioStreams(sample);

                throw new Exception("Unknown stream type");
            };
        }

    }
}
