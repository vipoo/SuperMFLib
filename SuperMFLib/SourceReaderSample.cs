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
