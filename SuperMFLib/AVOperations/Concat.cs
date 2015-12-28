using System;

namespace MediaFoundation.Net
{
    public partial class AVOperations
    {
        public static Action<ProcessSample> FromSource(SourceReader shortSourceReader)
        {
            return next =>
            {
                foreach (var s in shortSourceReader.Samples())
                    next(s);
            };
        }

        public static void StartConcat(SourceReader reader, ProcessSample transforms, Action<long, long> next)
        {
            Concat(reader, transforms, next)(0, 0);
        }

        public static Action<long, long> Concat(SourceReader reader, ProcessSample transforms, Action<long, long> next)
        {
            return (offsetA, offsetV) =>
            {
                var newOffsetA = offsetA;
                var newOffsetV = offsetV;

                reader.SetCurrentPosition(0);
                var stream = FromSource(reader);

                stream(s =>
                {
                    if (s.Flags.EndOfStream)
                        return false;

                    if (s.Stream.CurrentMediaType.IsVideo)
                        s.SampleTime += offsetV;

                    if (s.Stream.CurrentMediaType.IsAudio)
                        s.SampleTime += offsetA;

                    var r = transforms(s);

                    if (s.Stream.CurrentMediaType.IsVideo)
                        newOffsetV = s.SampleTime;

                    if (s.Stream.CurrentMediaType.IsAudio)
                        newOffsetA = s.SampleTime;


                    return r;
                });

                next(newOffsetA, newOffsetV);
            };
        }

        public static Action<long, long> Concat(SourceReader reader, ProcessSample transforms)
        {
            return (offsetA, offsetV) =>
            {
                reader.SetCurrentPosition(0);

                var stream = FromSource(reader);
                bool firstV = false;

                stream(s =>
                {
                    if (s.Flags.EndOfStream)
                        return transforms(s);

                    if (!firstV && s.Stream.CurrentMediaType.IsVideo)
                    {
                        firstV = true;
                        return true;
                    }

                    if (s.Stream.CurrentMediaType.IsVideo)
                        s.SampleTime += offsetV;

                    if (s.Stream.CurrentMediaType.IsAudio)
                        s.SampleTime += offsetA;

                    return transforms(s);
                });
            };
        }
    }
}
