using System;
using MediaFoundation.Misc;
using System.Drawing;

namespace MediaFoundation.Net
{
	public struct AspectRatio
	{
		public readonly int XAspect;
		public readonly int YAspect;

		public AspectRatio(int xAspect, int yAspect)
		{
			XAspect = xAspect;
			YAspect = yAspect;
		}
	}
}
