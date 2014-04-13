using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public class ReadWriteClassFactory : COMDisposable<IMFReadWriteClassFactory>
    {
		public ReadWriteClassFactory() : base( (IMFReadWriteClassFactory)new MFReadWriteClassFactory())  {  }

        public SourceReader CreateSourceReaderFromURL(string url, Attributes attributes)
        {
            object tmp;

            var hr = instance.CreateInstanceFromURL(CLSID.CLSID_MFSourceReader, url, attributes.instance, typeof(IMFSourceReader).GUID, out tmp);
            MFError.ThrowExceptionForHR(hr);
            return new SourceReader((IMFSourceReader)tmp);
        }

        public int CreateInstanceFromObject(Guid clsid, object punkObject, IMFAttributes pAttributes, Guid riid, out object ppvObject)
        {
            throw new NotImplementedException();
        }

        public SinkWriter CreateSinkWriterFromURL(string url, Attributes attributes)
        {
            object tmp;

            var hr = instance.CreateInstanceFromURL(CLSID.CLSID_MFSinkWriter, url, attributes.instance, typeof(IMFSinkWriter).GUID, out tmp);
            MFError.ThrowExceptionForHR(hr);
            return new SinkWriter((IMFSinkWriter)tmp);
        }
    }    
}
