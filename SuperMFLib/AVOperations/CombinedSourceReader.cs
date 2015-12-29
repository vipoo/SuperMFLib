using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MediaFoundation.Net
{
    internal class CombinedSourceReader : ISourceReader
    {
        readonly SourceReader[] readers;
        private bool reposReqeusted = false;
        private long startPosition;

        public CombinedSourceReader(IEnumerable<SourceReader> readers)
        {
            this.readers = readers.ToArray();
        }

        public long Duration
        {
            get
            {
                return readers.Sum(r => r.Duration);
            }
        }

        public IEnumerable<SourceReaderSample> Samples(int streamIndex = -2, int controlFlags = 0)
        {
            long offsetV = 0;
            long offsetA = 0;

            long nextOffsetV = 0;
            long nextOffsetA = 0;

            var duration = Duration;
            SourceReaderSample last = null;

            var readerIndex = 0;

            foreach (var reader in readers)
            {
                readerIndex++;

                if (reader.Duration + offsetV < startPosition)
                {
                    Trace.WriteLine(string.Format("File index: {0}, Duration: {1}, offset: {2}, startPosition: {3}", 
                        readerIndex, 
                        reader.Duration.FromNanoToSeconds(),
                        offsetV.FromNanoToSeconds(),
                        startPosition.FromNanoToSeconds()
                    ));

                    continue;
                }


                Trace.WriteLine(string.Format("File index: {0}, Duration: {1}", readerIndex, reader.Duration.FromNanoToSeconds()));
                Trace.WriteLine(string.Format("File index: {0}, OffsetV: {1}", readerIndex, offsetV.FromNanoToSeconds()));

                foreach (var sample in reader.Samples(streamIndex, controlFlags))
                {
                    if (sample.Flags.EndOfStream)
                    {
                        last = sample;
                        continue;
                    }

                    if(reposReqeusted && sample.SampleTime + offsetV < startPosition)
                    {
                        var newPos = startPosition - offsetV;

                        Trace.WriteLine(string.Format("File index: {0}, Repos: {1}-{2} = {3}",
                            readerIndex,
                            startPosition.FromNanoToSeconds(),
                            offsetV.FromNanoToSeconds(),
                            (newPos).FromNanoToSeconds()
                            ));

                        if (newPos > sample.Duration)
                        {
                            Trace.WriteLine(string.Format("File index: {0}, Skipping", readerIndex));
                            break;
                        }

                        sample.Reader.SetCurrentPosition(newPos);
                        reposReqeusted = false;
                        continue;
                    }

                    if( sample.Stream.NativeMediaType.IsVideo)
                    {
                        nextOffsetV = sample.Duration;
                        sample.Resequence(offsetV, duration, this);
                    }

                    if (sample.Stream.NativeMediaType.IsAudio)
                    {
                        nextOffsetA = sample.Duration;
                        sample.Resequence(offsetA, duration, this);
                    }

                    yield return sample;
                }

                offsetA += nextOffsetA;
                offsetV += nextOffsetV;
            }

            if(last != null)
                yield return last;
        }

        public void SetCurrentPosition(long position)
        {
            this.startPosition = position;
            this.reposReqeusted = true;
        }
    }
}