using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using MediaFoundation.Transform;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
	public class _MFCollection : COMDisposable<IMFCollection>, IEnumerable<MediaType>
	{
		public _MFCollection (IMFCollection instance) :base(instance) { }

		public IEnumerator<MediaType> GetEnumerator ()
		{
			int count;
			instance.GetElementCount (out count).Hr ();
			for (var i = 0; i < count; i++)
			{
				object result;
				instance.GetElement (i, out result);
				yield return new MediaType ((IMFMediaType)result);
			}
		}

		global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	}
}
