using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;


using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using MediaFoundation.Transform;

namespace Testv10
{
    class TestExtern : COMBase, IMFAsyncCallback, IStream
    {
        private int m_size;
        private int m_cur;
        private IntPtr m_ip;

        private AutoResetEvent m_re = new AutoResetEvent(false);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public void DoTests()
        {
            TestQM();
            TestVF();
            TestMisc();
            TestFile();
            TestBlob();
        }

        private void TestMisc()
        {
            int i;
            PropVariant p = new PropVariant();
            IMFMediaBuffer mb, mb2;
            string[] sa;
            IMFMediaTypeHandler ph;
            int hr = MFExtern.MFGetTimerPeriodicity(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 10);

            hr = MFExtern.MFCreateAlignedMemoryBuffer(1000, 8, out mb);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(mb != null);

            long l = MFExtern.MFGetSystemTime();
            Debug.Assert(l > 206563752489);

            hr = MFExtern.MFGetSupportedSchemes(p);
            MFError.ThrowExceptionForHR(hr);
            sa = p.GetStringArray();
            Debug.Assert(sa.Length > 13);

            hr = MFExtern.MFGetSupportedMimeTypes(p);
            MFError.ThrowExceptionForHR(hr);
            sa = p.GetStringArray();
            Debug.Assert(sa.Length > 7);

            hr = MFExtern.MFCreateSimpleTypeHandler(out ph);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(ph != null);

            hr = MFExtern.MFCreateSequencerSegmentOffset(1, 2, p);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(p.GetMFAttributeType() == MFAttributeType.IUnknown);

            object o;
            hr = MFExtern.MFCreateVideoRenderer(typeof(IMFGetService).GUID, out o);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(o != null);

            mb.SetCurrentLength(300);
            hr = MFExtern.MFCreateMediaBufferWrapper(mb, 32, 200, out mb2);
            Debug.Assert(mb2 != null);

            IMFSample samp;
            hr = MFExtern.MFCreateSample(out samp);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFCreateLegacyMediaBufferOnMFMediaBuffer(samp, mb, 0, out o);
            MFError.ThrowExceptionForHR(hr);
        }

        private void UsesUntested()
        {
#if false
            IMFTopoLoader tl;
            IMFAttributes pa;
            IMFMediaSink ps;

            MFExtern.MFCreateAttributes(out pa, 10);
            MFExtern.MFCreateAudioRenderer(pa, out ps);
            Debug.Assert(ps != null);

            IMFSequencerSource ss;
            MFExtern.MFCreateSequencerSource(null, out ss);
            Debug.Assert(ss != null);

            MFExtern.MFCreateTopoLoader(out tl);
            Debug.Assert(tl != null);

            IMFPMPServer pmps;
            MFExtern.MFCreatePMPServer(MFPMPSessionCreationFlags.UnprotectedProcess, out pmps);
            Debug.Assert(pmps != null);

            IMFASFContentInfo ci;
            MFExtern.MFCreateASFContentInfo(out ci);
            Debug.Assert(ci != null);

            IMFASFProfile iprof;
            MFExtern.MFCreateASFProfile(out iprof);
            Debug.Assert(iprof != null);

            IMFASFSplitter split;
            MFExtern.MFCreateASFSplitter(out split);
            Debug.Assert(split != null);

            IMFASFMultiplexer mux;
            MFExtern.MFCreateASFMultiplexer(out mux);
            Debug.Assert(mux != null);

            IMFASFIndexer index;
            MFExtern.MFCreateASFIndexer(out index);
            Debug.Assert(index != null);
#endif
        }

        private void Broken()
        {
            //IMFNetCredentialCache ncc;
            //MFExtern.MFCreateCredentialCache(out ncc);
            //Debug.Assert(ncc != null);

            //IMFRemoteDesktopPlugin plug;
            //MFExtern.MFCreateRemoteDesktopPlugin(out plug);
            //Debug.Assert(plug != null);
        }

        private void TestVF()
        {
            int hr;
            IMFVideoMediaType vmt;
            MFMediaEqual me;
            int i;
            MFVideoFormat vf;
            IMFMediaType mt, mt2, mt3;
            FourCC cc4 = new FourCC("YUY2");

            hr = MFExtern.MFCreateMediaType(out mt);
            MFError.ThrowExceptionForHR(hr);

            hr = mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            MFError.ThrowExceptionForHR(hr);
            hr = mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateMFVideoFormatFromMFMediaType(mt, out vf, out i);
            MFError.ThrowExceptionForHR(hr);
            int cc = MFExtern.MFGetUncompressedVideoFormat(vf);

            hr = MFExtern.MFCreateMediaType(out mt2);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFInitMediaTypeFromMFVideoFormat(mt2, vf, i);
            MFError.ThrowExceptionForHR(hr);

            int iRet = mt.IsEqual(mt2, out me);
            Debug.Assert(iRet == 0);

            AMMediaType amt = new AMMediaType();
            hr = MFExtern.MFInitAMMediaTypeFromMFMediaType(mt, Guid.Empty, amt);
            MFError.ThrowExceptionForHR(hr);

            AMMediaType amt2;
            hr = MFExtern.MFCreateAMMediaTypeFromMFMediaType(mt, MFRepresentation.VideoInfo2, out amt2);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFCreateMediaType(out mt3);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFInitMediaTypeFromAMMediaType(mt3, amt2);
            MFError.ThrowExceptionForHR(hr);

            iRet = mt.IsEqual(mt3, out me);
            Debug.Assert(iRet == 0);

            hr = MFExtern.MFCreateVideoMediaType(vf, out vmt);

            VideoInfoHeader vih = new VideoInfoHeader();
            Marshal.PtrToStructure(amt.formatPtr, vih);

            IMFMediaType mt4;
            hr = MFExtern.MFCreateMediaType(out mt4);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFInitMediaTypeFromVideoInfoHeader(mt4, vih, amt.formatSize, Guid.Empty);
            MFError.ThrowExceptionForHR(hr);

            iRet = mt.IsEqual(mt4, out me);
            Debug.Assert(iRet == 1 && me == (MediaFoundation.MFMediaEqual.MajorTypes | MediaFoundation.MFMediaEqual.FormatUserData));

            VideoInfoHeader2 vih2 = new VideoInfoHeader2();
            Marshal.PtrToStructure(amt2.formatPtr, vih2);

            IMFMediaType mt5;
            hr = MFExtern.MFCreateMediaType(out mt5);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFInitMediaTypeFromVideoInfoHeader2(mt5, vih2, amt2.formatSize, Guid.Empty);
            MFError.ThrowExceptionForHR(hr);

            iRet = mt.IsEqual(mt5, out me);
            Debug.Assert(iRet == 1 && me == (MediaFoundation.MFMediaEqual.MajorTypes | MediaFoundation.MFMediaEqual.FormatUserData));

            IntPtr ip;
            hr = vmt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            MFError.ThrowExceptionForHR(hr);
            hr = vmt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            MFError.ThrowExceptionForHR(hr);

            vf = vmt.GetVideoFormat();

            // This statement seems to cause crashes in 64bit code.  The crash comes on leaving DoTests.
            // Since this doesn't occur in 32 bit, and since this is a deprecated method, I don't think
            // I care.
            vmt.GetVideoRepresentation(MFRepresentation.VideoInfo, out ip, 10);

            AMMediaType amt5 = new AMMediaType();
            Marshal.PtrToStructure(ip, amt5);
            Debug.Assert(amt5.formatType == MFRepresentation.VideoInfo);
            hr = vmt.FreeRepresentation(MFRepresentation.VideoInfo, ip);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestQM()
        {
#if false
            IMFQualityManager pq;
            IMFPresentationClock pc;

            MFExtern.MFCreatePresentationClock(out pc);

            MFExtern.MFCreateStandardQualityManager(out pq);
            Debug.Assert(pq != null);

            pq.NotifyPresentationClock(pc);
            pq.NotifyProcessInput(null, 0, null);
#endif
        }

        private void TestFile()
        {
            IMFByteStream bs;
            object pCookie;

            int hr = MFExtern.MFCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.NoBuffering, "test.out", out bs);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(bs != null);

            hr = MFExtern.MFCreateTempFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, out bs);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(bs != null);

            hr = MFExtern.MFBeginCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, "test3.out", this, null, out pCookie);
            MFError.ThrowExceptionForHR(hr);
            m_re.WaitOne();

            hr = MFExtern.MFBeginCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, "http://192.168.1.152", this, null, out pCookie);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFCancelCreateFile(pCookie);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestBlob()
        {
            IMFAttributes pa, pa2;
            int i;
            bool b;
            Guid g1 = Guid.NewGuid();
            Guid g2 = Guid.NewGuid();

            int hr = MFExtern.MFCreateAttributes(out pa, 10);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFCreateAttributes(out pa2, 10);
            MFError.ThrowExceptionForHR(hr);
            hr = pa.SetGUID(g1, g2);
            MFError.ThrowExceptionForHR(hr);
            hr = pa.SetUINT32(g2, 1234321);
            MFError.ThrowExceptionForHR(hr);

            hr = MFExtern.MFGetAttributesAsBlobSize(pa, out i);
            MFError.ThrowExceptionForHR(hr);
            IntPtr ip = Marshal.AllocCoTaskMem(i);
            try
            {
                hr = MFExtern.MFGetAttributesAsBlob(pa, ip, i);
                MFError.ThrowExceptionForHR(hr);
                hr = MFExtern.MFInitAttributesFromBlob(pa2, ip, i);
                MFError.ThrowExceptionForHR(hr);
                hr = pa2.Compare(pa, MFAttributesMatchType.AllItems, out b);
                MFError.ThrowExceptionForHR(hr);
                Debug.Assert(b);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            m_ip = Marshal.AllocCoTaskMem(1000);
            m_size = 0;
            hr = pa.SetUINT64(Guid.NewGuid(), 77666776);
            MFError.ThrowExceptionForHR(hr);
            hr = MFExtern.MFSerializeAttributesToStream(pa, MFAttributeSerializeOptions.None, this);
            MFError.ThrowExceptionForHR(hr);

            m_cur = 0;
            hr = MFExtern.MFDeserializeAttributesFromStream(pa2, MFAttributeSerializeOptions.None, this);
            MFError.ThrowExceptionForHR(hr);
            hr = pa.Compare(pa2, MFAttributesMatchType.AllItems, out b);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(b);
        }

        #region IMFAsyncCallback Members

        public int  GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = 0;
            pdwQueue = 0;
            return E_NotImplemented;
        }

        public int  Invoke(IMFAsyncResult pAsyncResult)
        {
            IMFByteStream bs;
 	        int hr = MFExtern.MFEndCreateFile(pAsyncResult, out bs);
            MFError.ThrowExceptionForHR(hr);
            m_re.Set();

            return 0;
        }

        #endregion

        #region IStream Members

        public void Clone(out IStream ppstm)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Commit(int grfCommitFlags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            Marshal.Copy(new IntPtr(m_ip.ToInt64() + m_cur), pv, 0, cb);

            if (pcbRead != IntPtr.Zero)
            {
                Marshal.WriteInt32(pcbRead, cb);
            }
            m_cur += cb;
        }

        public void Revert()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetSize(long libNewSize)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            Marshal.Copy(pv, 0, new IntPtr(m_ip.ToInt64() + m_size), cb);
            m_size += cb;

            if (pcbWritten != IntPtr.Zero)
            {
                Marshal.WriteInt32(pcbWritten, cb);
            }
        }

        #endregion
    }
}
