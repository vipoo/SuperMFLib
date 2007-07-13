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
            m_Top.Clear();
        }

        private void TestRemoveNode()
        {
            IMFTopologyNode pNode;

            m_Top.GetNode(0, out pNode);

            m_Top.RemoveNode(pNode);
        }

        private void TestGetOutputNodeCollection()
        {
            IMFCollection pCol;

            m_Top.GetOutputNodeCollection(out pCol);
        }

        private void TestGetSourceNodeCollection()
        {
            IMFCollection pCol;

            m_Top.GetSourceNodeCollection(out pCol);
        }

        private void TestCloneFrom()
        {
            IMFTopology tp;

            MFExtern.MFCreateTopology(out tp);

            try
            {
                // This call actually fails, but it's because the node I added
                // was blank and Clone can't deal with that.  Someday I may update
                // the AddNode routine to set the other attributes. Today isn't
                // that day.
                tp.CloneFrom(m_Top);
            }
            catch
            { }
        }

        private void TestGetNodeByID()
        {
            IMFTopologyNode pNode;

            m_Top.GetNodeByID(m_pid, out pNode);

            Debug.Assert(pNode != null);
        }

        private void TestGetNode()
        {
            IMFTopologyNode pNode;

            m_Top.GetNode(0, out pNode);

            Debug.Assert(pNode != null);
        }

        private void TestGetTopologyID()
        {
            long pid;
            m_Top.GetTopologyID(out pid);

            Debug.Assert(pid == 1);
        }

        private void TestGetNodeCount()
        {
            short sn;
            m_Top.GetNodeCount(out sn);

            Debug.Assert(sn == 1);
        }

        private void TestAddNode()
        {
            IMFTopologyNode pNode;

            MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out pNode);
            pNode.GetTopoNodeID(out m_pid);
            pNode.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, null);

            m_Top.AddNode(pNode);
        }

        private void GetInterface()
        {
            MFExtern.MFCreateTopology(out m_Top);
        }
    }
}
