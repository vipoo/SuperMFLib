using System;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using MediaFoundation.Transform;
using System.Collections.Generic;

namespace MediaFoundation.Net
{
	public abstract class COMDisposable<T> : COMBase, IDisposable
	{
		public readonly T instance;

		public COMDisposable(T instance)
		{
			this.instance = instance;
		}

		public void Dispose ()
		{
			SafeRelease(instance);

			GC.SuppressFinalize(this);
		}

		~COMDisposable()
		{
			Dispose ();
		}
	}
	
}
