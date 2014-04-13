using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public static class HrExtension
	{
		public static void Hr(this int hr)
		{
			MFError.ThrowExceptionForHR (hr);
		}
	}

}
