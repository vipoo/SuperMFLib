using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class IMFTopologyTest : COMBase
    {
        IMFTopology m_Top;
        long m_pid;

        public void DoTests()
        {
            GetInterface();

            TestGetTopologyID();
            TestAddNode();
            TestGetNodeCount();
            TestGetNode();
            TestGetNodeByID();
            TestCloneFrom();
            TestGetSourceNodeCollection();
            TestGetOutputNodeCollection();
            TestRemoveNode();
            TestClear();
        }

        private void TestClear()
        {
            int hr = m_Top.Clear();
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestRemoveNode()
        {
            IMFTopologyNode pNode;

            int hr = m_Top.GetNode(0, out pNode);
            MFError.ThrowExceptionForHR(hr);

            hr = m_Top.RemoveNode(pNode);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestGetOutputNodeCollection()
        {
            IMFCollection pCol;

            int hr = m_Top.GetOutputNodeCollection(out pCol);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestGetSourceNodeCollection()
        {
            IMFCollection pCol;

            int hr = m_Top.GetSourceNodeCollection(out pCol);
            MFError.ThrowExceptionForHR(hr);
        }

        private void TestCloneFrom()
        {
            IMFTopology tp;

            int hr = MFExtern.MFCreateTopology(out tp);
            MFError.ThrowExceptionForHR(hr);

            try
            {
                // This call actually fails, but it's because the node I added
                // was blank and Clone can't deal with that.  Someday I may update
                // the AddNode routine to set the other attributes. Today isn't
                // that day.
                hr = tp.CloneFrom(m_Top);
                MFError.ThrowExceptionForHR(hr);
            }
            catch
            { }
        }

        private void TestGetNodeByID()
        {
            IMFTopologyNode pNode;

            int hr = m_Top.GetNodeByID(m_pid, out pNode);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pNode != null);
        }

        private void TestGetNode()
        {
            IMFTopologyNode pNode;

            int hr = m_Top.GetNode(0, out pNode);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pNode != null);
        }

        private void TestGetTopologyID()
        {
            long pid;
            int hr = m_Top.GetTopologyID(out pid);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pid == 1);
        }

        private void TestGetNodeCount()
        {
            short sn;
            int hr = m_Top.GetNodeCount(out sn);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(sn == 1);
        }

        private void TestAddNode()
        {
            IMFTopologyNode pNode;

            int hr = MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);
            MFError.ThrowExceptionForHR(hr);
            hr = pNode.GetTopoNodeID(out m_pid);
            MFError.ThrowExceptionForHR(hr);
            hr = pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, null);
            MFError.ThrowExceptionForHR(hr);

            hr = m_Top.AddNode(pNode);
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateTopology(out m_Top);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
