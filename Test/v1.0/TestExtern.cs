using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using MediaFoundation.Transform;

using MediaFoundation;
using MediaFoundation.EVR;
using MediaFoundation.Misc;
using System.Runtime.InteropServices.ComTypes;

namespace Testv10
{
    class TestExtern : IMFAsyncCallback, IStream
    {
        private int m_size;
        private int m_cur;
        private IntPtr m_ip;

        private AutoResetEvent m_re = new AutoResetEvent(false);

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public void DoTests()
        {
            //TestQM();
            TestVF();
            TestMFT();
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
            MFExtern.MFGetTimerPeriodicity(out i);

            Debug.Assert(i == 10);

            MFExtern.MFCreateAlignedMemoryBuffer(1000, 8, out mb);
            Debug.Assert(mb != null);

            long l = MFExtern.MFGetSystemTime();
            Debug.Assert(l > 1243466238863);

            MFExtern.MFGetSupportedSchemes(p);
            sa = p.GetStringArray();
            Debug.Assert(sa.Length > 13);

            MFExtern.MFGetSupportedMimeTypes(p);
            sa = p.GetStringArray();
            Debug.Assert(sa.Length > 7);

            MFExtern.MFCreateSimpleTypeHandler(out ph);
            Debug.Assert(ph != null);

            MFExtern.MFCreateSequencerSegmentOffset(1, 2, p);
            Debug.Assert(p.GetMFAttributeType() == MFAttributeType.IUnknown);

            object o;
            MFExtern.MFCreateVideoRenderer(typeof(IMFGetService).GUID, out o);
            Debug.Assert(o != null);

            mb.SetCurrentLength(300);
            MFExtern.MFCreateMediaBufferWrapper(mb, 32, 200, out mb2);
            Debug.Assert(mb2 != null);

            IMFSample samp;
            MFExtern.MFCreateSample(out samp);
            MFExtern.MFCreateLegacyMediaBufferOnMFMediaBuffer(samp, mb, 0, out o);
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
            IMFVideoMediaType vmt;
            MFMediaEqual me;
            int i;
            MFVideoFormat vf;
            IMFMediaType mt, mt2, mt3;
            FourCC cc4 = new FourCC("YUY2");

            MFExtern.MFCreateMediaType(out mt);
            mt.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, cc4.ToMediaSubtype());
            mt.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            MFExtern.MFCreateMFVideoFormatFromMFMediaType(mt, out vf, out i);
            int cc = MFExtern.MFGetUncompressedVideoFormat(vf);

            MFExtern.MFCreateMediaType(out mt2);
            MFExtern.MFInitMediaTypeFromMFVideoFormat(mt2, vf, i);

            int iRet = mt.IsEqual(mt2, out me);
            Debug.Assert(iRet == 0);

            AMMediaType amt = new AMMediaType();
            MFExtern.MFInitAMMediaTypeFromMFMediaType(mt, Guid.Empty, amt);

            AMMediaType amt2;
            Guid VideoInfo2 = new Guid(0xf72a76A0, 0xeb0a, 0x11d0, 0xac, 0xe4, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);
            MFExtern.MFCreateAMMediaTypeFromMFMediaType(mt, VideoInfo2, out amt2);

            MFExtern.MFCreateMediaType(out mt3);
            MFExtern.MFInitMediaTypeFromAMMediaType(mt3, amt2);

            iRet = mt.IsEqual(mt3, out me);
            Debug.Assert(iRet == 0);

            MFExtern.MFCreateVideoMediaType(vf, out vmt);

            VideoInfoHeader vih = new VideoInfoHeader();
            Marshal.PtrToStructure(amt.formatPtr, vih);

            IMFMediaType mt4;
            MFExtern.MFCreateMediaType(out mt4);
            MFExtern.MFInitMediaTypeFromVideoInfoHeader(mt4, vih, amt.formatSize, Guid.Empty);

            iRet = mt.IsEqual(mt4, out me);
            Debug.Assert(iRet == 0);

            VideoInfoHeader2 vih2 = new VideoInfoHeader2();
            Marshal.PtrToStructure(amt2.formatPtr, vih2);

            IMFMediaType mt5;
            MFExtern.MFCreateMediaType(out mt5);
            MFExtern.MFInitMediaTypeFromVideoInfoHeader2(mt5, vih2, amt2.formatSize, Guid.Empty);

            iRet = mt.IsEqual(mt5, out me);
            Debug.Assert(iRet == 0);

            IntPtr ip;
            vmt.GetVideoRepresentation(MFRepresentation.VideoInfo, out ip, 10);
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

        private void TestMFT()
        {
#if false
            int i;
            Guid[] ga = new Guid[8];
            MFTRegisterTypeInfo rin = new MFTRegisterTypeInfo();
            MFTRegisterTypeInfo rout = new MFTRegisterTypeInfo();

            IntPtr ip = IntPtr.Zero;
            MFExtern.MFTEnum(MFTransformCategory.MFT_CATEGORY_VIDEO_DECODER, 0, null, null, null, out ip, out i);
            ga = ParseGuidArray(ip, i);

            int itypescnt, otypescnt;
            string s;
            MFTRegisterTypeInfo[] itypes, otypes;
            itypes = null; // new MFTRegisterTypeInfo[10];
            otypes = null; // new MFTRegisterTypeInfo[11];

            for (int x = 0; x < ga.Length; x++)
            {
                itypescnt = 33;
                otypescnt = 34;
                MFExtern.MFTGetInfo(ga[x], out s, itypes, out itypescnt, otypes, out otypescnt, IntPtr.Zero);
            }
#endif
        }

        private Guid[] ParseGuidArray(IntPtr ip, int iSize)
        {
            const int iGuidSize = 16;
            Guid[] ga;

            try
            {
                byte[] ba = new byte[iGuidSize];
                ga = new Guid[iSize];

                for (int x = 0; x < iSize; x++)
                {
                    Marshal.Copy(new IntPtr(ip.ToInt64() + (x * iGuidSize)), ba, 0, ba.Length);
                    ga[x] = new Guid(ba);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            return ga;
        }

        private void TestFile()
        {
            IMFByteStream bs;
            object pCookie;

            MFExtern.MFCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.NoBuffering, "test.out", out bs);
            Debug.Assert(bs != null);

            MFExtern.MFCreateTempFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, out bs);
            Debug.Assert(bs != null);

            MFExtern.MFBeginCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, "test3.out", this, null, out pCookie);
            m_re.WaitOne();

            MFExtern.MFBeginCreateFile(MFFileAccessMode.Write, MFFileOpenMode.DeleteIfExist, MFFileFlags.None, "http://192.168.1.152", this, null, out pCookie);
            MFExtern.MFCancelCreateFile(pCookie);
        }

        private void TestBlob()
        {
            IMFAttributes pa, pa2;
            int i;
            bool b;
            Guid g1 = Guid.NewGuid();
            Guid g2 = Guid.NewGuid();

            MFExtern.MFCreateAttributes(out pa, 10);
            MFExtern.MFCreateAttributes(out pa2, 10);
            pa.SetGUID(g1, g2);
            pa.SetUINT32(g2, 1234321);

            MFExtern.MFGetAttributesAsBlobSize(pa, out i);
            IntPtr ip = Marshal.AllocCoTaskMem(i);
            try
            {
                MFExtern.MFGetAttributesAsBlob(pa, ip, i);
                MFExtern.MFInitAttributesFromBlob(pa2, ip, i);
                pa2.Compare(pa, MFAttributesMatchType.AllItems, out b);
                Debug.Assert(b);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ip);
            }

            m_ip = Marshal.AllocCoTaskMem(1000);
            m_size = 0;
            pa.SetUINT64(Guid.NewGuid(), 77666776);
            MFExtern.MFSerializeAttributesToStream(pa, MFAttributeSerializeOptions.None, this);

            m_cur = 0;
            MFExtern.MFDeserializeAttributesFromStream(pa2, MFAttributeSerializeOptions.None, this);
            pa.Compare(pa2, MFAttributesMatchType.AllItems, out b);
            Debug.Assert(b);
        }

        #region IMFAsyncCallback Members

        public void  GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
 	        throw new Exception("The method or operation is not implemented.");
        }

        public void  Invoke(IMFAsyncResult pAsyncResult)
        {
            IMFByteStream bs;
 	        MFExtern.MFEndCreateFile(pAsyncResult, out bs);
            m_re.Set();
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
