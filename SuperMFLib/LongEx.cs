using System;
using MediaFoundation.Misc;
using System.Drawing;

namespace MediaFoundation.Net
{
	public static class LongEx
	{
		public static int LowPart(this long value)
		{
			return (int)(value & uint.MaxValue);
		}

		public static int HighPart(this long value)
		{
			return (int)(value >> 32);
		}

		public static long FromInts(int highPart, int lowPart)
		{
			return ((long)highPart << 32) | (uint)lowPart;
		}
	}

}
