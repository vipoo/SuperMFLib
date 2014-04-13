using System;
using MediaFoundation.Misc;
using System.Drawing;

namespace MediaFoundation.Net
{
	public struct FrameRate
	{
		public readonly int Numerator;
		public readonly int Denominator;

		public FrameRate(int numerator, int denominator)
		{
			this.Numerator = numerator;
			this.Denominator = denominator;
		}
	}
}
