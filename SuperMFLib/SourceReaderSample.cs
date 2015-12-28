// This file is part of SuperMFLib.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/SuperMFLib
//
// SuperMFLib is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SuperMFLib is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with SuperMFLib.  If not, see <http://www.gnu.org/licenses/>.

using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
	public class SourceReaderSample
	{
		public readonly SourceStream Stream;
		public readonly SourceReaderSampleFlags Flags;
        /// <summary>
        /// The original timestamp for this sample, as per its source
        /// </summary>
		public readonly long Timestamp;
		public readonly long Duration;
		public readonly int Count;
		public readonly Sample Sample;
        public readonly SourceReader Reader;
        internal long SegmentDuration;
        internal long SegmentTimeStamp;

        public SourceReaderSample(SourceReader reader, SourceStream stream, SourceReaderSampleFlags flags, long timestamp, long duration, Sample sample, int count)
		{
            Reader = reader;
			Stream = stream;
			Flags = flags;
			Timestamp = timestamp;
            Duration = duration;
			Count = count;
			Sample = sample;

            SegmentDuration = duration;
            SegmentTimeStamp = timestamp;
		}

        /// <summary>
        /// The time stamp embedded in the sample
        /// </summary>
        public long SampleTime
        {
            get
            {
                return GetSample().SampleTime;
            }
            set
            {
                GetSample().SampleTime = value;
            }
        }

        /// <summary>
        /// Set the embedded timestamp within this sample
        /// </summary>
        /// <param name="newTime"></param>
        public void SetSampleTime(long newTime)
        {
            GetSample().SetSampleTime(newTime);
        }

        /// <summary>
        /// The time stamp embedded in the sample
        /// </summary>
        public long GetSampleTime()
        {
            return GetSample().GetSampleTime();
        }

        Sample GetSample()
        {
            if (this.Sample == null)
                throw new Exception(string.Format("This sample does not contain any sample data {0}", Flags));

            return this.Sample;
        }
    }
}
