using System;

namespace MediaFoundation.Net
{
    public partial class AVOperations
    {
        public static ProcessSample SaveTo(SinkStream sinkStream)
        {
            return sample =>
            {
                if (sample.Flags.StreamTick)
                    throw new NotImplementedException();
                //sinkStream.SendStreamTick(sample.Timestamp - offset);

                if (sample.Sample == null)
                    return true;

                if (sample.Count == 0)
                    sample.Sample.Discontinuity = true;

                System.Diagnostics.Trace.WriteLine(string.Format("Saving:   {0} -> {1}", sample.Timestamp.FromNanoToSeconds(), sample.SampleTime.FromNanoToSeconds()));


                sinkStream.WriteSample(sample.Sample);

                return true;
            };
        }
        
    }
}
