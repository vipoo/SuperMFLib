// Add this code to Splitter:
#if false

        void CreateIndexer(IMFASFContentInfo pContentInfo, out IMFASFIndexer ai)
        {
            MFAsfIndexerFlags f;
            long l;
            int i;
            IMFMediaBuffer mb;

            MFExtern.MFCreateASFIndexer(out ai);
            MFExtern.MFCreateMemoryBuffer(1000, out mb);

            ai.Initialize(pContentInfo);
            ai.GetIndexPosition(pContentInfo, out l);
            ai.GetFlags(out f);
            ai.SetFlags(f);
            ai.GetIndexByteStreamCount(out i);
            ai.GetCompletedIndex(mb, 0);
            ai.GetIndexWriteSpace(out l);
        }

            IMFByteStream[] aia = new IMFByteStream[1];
            aia[0] = pStream;

            ai.SetIndexByteStreams(aia, 1);

            ASFIndexIdentifier ii = new ASFIndexIdentifier();
            ii.guidIndexType = Guid.Empty;
            ii.wStreamNumber = 2;
            bool b;
            int i1 = 100;
            IntPtr ip = Marshal.AllocCoTaskMem(i1);
            ai.GetIndexStatus(ii, out b, ip, ref i1);
            long l, l2;
            PropVariant pv = new PropVariant(50000000L);
            ai.GetSeekPositionForValue(pv, ii, out l, IntPtr.Zero, out i1);

#endif

// http://msdn2.microsoft.com/en-us/library/bb530117.aspx

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;

namespace Testv10
{
    class IMFASFIndexerTest
    {
        private IMFASFIndexer m_index;

        public void DoTests()
        {
            GetInterface();

            TestInit();
            TestFlags();
            Stuff();
        }

        private void TestFlags()
        {
            MFAsfIndexerFlags f;

            m_index.SetFlags(MFAsfIndexerFlags.ReadForReversePlayback);
            m_index.GetFlags(out f);

            Debug.Assert(f == MFAsfIndexerFlags.ReadForReversePlayback);
        }

        private void Stuff()
        {
            int i;
            long l;
            IMFSample sample;

            MFExtern.MFCreateSample(out sample);
            m_index.GetIndexWriteSpace(out l);

            m_index.GetIndexByteStreamCount(out i);
            Debug.Assert(i == 1);


            //m_index.GenerateIndexEntries(sample);
            ASFIndexIdentifier ii = new ASFIndexIdentifier();
            ii.wStreamNumber = 0;
            bool b;
            int id = 10000;
            IntPtr ip = Marshal.AllocCoTaskMem(id);
            m_index.GetIndexStatus(ii, out b, out ip, ref id);
        }

#if false
        void GetIndexPosition(
            [In] IMFASFContentInfo pIContentInfo,
            out long pcbIndexOffset);
        
        void SetIndexByteStreams(
            [In] IMFByteStream[] ppIByteStreams,
            [In] int cByteStreams);
        
        void GetIndexStatus(
            [In] ASFIndexIdentifier pIndexIdentifier,
            out bool pfIsIndexed,
            out byte pbIndexDescriptor,
            out int pcbIndexDescriptor);
        
        void SetIndexStatus(
            [In] IntPtr pbIndexDescriptor,
            [In] int cbIndexDescriptor,
            [In] bool fGenerateIndex);
        
        void GetSeekPositionForValue(
            [In] PropVariant pvarValue,
            [In] ASFIndexIdentifier pIndexIdentifier,
            out long pcbOffsetWithinData,
            out long phnsApproxTime,
            out int pdwPayloadNumberOfStreamWithinPacket);
        
        void GenerateIndexEntries(
            [In] IMFSample pIASFPacketSample);
        
        void CommitIndex(
            [In] IMFASFContentInfo pIContentInfo);
        
        void GetIndexWriteSpace(
            out long pcbIndexWriteSpace);
        
        void GetCompletedIndex(
            [In] IMFMediaBuffer pIIndexBuffer,
            [In] long cbOffsetWithinIndex);
#endif

        private void TestInit()
        {
            IMFMediaBuffer mb;
            IMFByteStream bs;
            MFObjectType pObjectType;
            object pSource;
            IMFSourceResolver sr;
            IntPtr b;
            int i;

            MFExtern.MFCreateSourceResolver(out sr);

            sr.CreateObjectFromURL(
                @"file://c:/sourceforge/mflib/test/media/AspectRatio4x3.wmv",
                MFResolution.ByteStream,
                null,
                out pObjectType,
                out pSource);

            bs = pSource as IMFByteStream;

            int iReq = 3200;
            int iRead;

            MFExtern.MFCreateMemoryBuffer(iReq + 100, out mb);
            mb.Lock(out b, out iRead, out i);
            bs.Read(b, iReq, out iRead);

            mb.Unlock();
            mb.SetCurrentLength(iReq);

            IMFASFContentInfo ci;

            MFExtern.MFCreateASFContentInfo(out ci);
            ci.ParseHeader(mb, 0);

            m_index.Initialize(ci);
            m_index.CommitIndex(ci);
        }

        private void GetInterface()
        {
            MFExtern.MFCreateASFIndexer(out m_index);
        }
    }
}
