using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;

namespace Testv10
{
    class IMFTopologyNodeTest
    {
        IMFTopologyNode m_tn;

        public void DoTests()
        {
            GetInterface();

            TestSetObject();
            TestGetNodeType();
            TestSetTopoNodeID();
            TestSetOutputPrefType();
            TestSetInputPrefType();
            TestGetInputCount();
            TestGetOutputCount();
            TestCloneFrom();
            TestConnectOutput();
            TestGetOutput();
            TestGetInput();
            TestDisconnectOutput();
        }

        void TestSetObject()
        {
            object o;
            m_tn.SetObject(this);

            m_tn.GetObject(out o);

            Debug.Assert(this == o);
        }

        void TestGetNodeType()
        {
            MFTopologyType pType;

            m_tn.GetNodeType(out pType);

            Debug.Assert(pType == MFTopologyType.TransformNode);
        }

        void TestSetTopoNodeID()
        {
            long l;

            m_tn.SetTopoNodeID(123456789);
            m_tn.GetTopoNodeID(out l);

            Debug.Assert(l == 123456789);
        }

        void TestGetInputCount()
        {
            int i;

            m_tn.GetInputCount(out i);
            Debug.Assert(i == 1);
        }

        void TestGetOutputCount()
        {
            int i;

            m_tn.GetOutputCount(out i);
            Debug.Assert(i == 1);
        }

        void TestConnectOutput()
        {
            IMFTopologyNode tn;

            MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out tn);

            m_tn.ConnectOutput(0, tn, 0);
        }

        void TestDisconnectOutput()
        {
            m_tn.DisconnectOutput(0);
        }

        void TestGetInput()
        {
            int pIndex;
            IMFTopologyNode pNode;

            IMFTopologyNode tn;

            MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out tn);

            tn.ConnectOutput(0, m_tn, 0);

            m_tn.GetInput(0, out pNode, out pIndex);

            Debug.Assert(pNode == tn);
        }

        void TestGetOutput()
        {
            int pIndex;
            IMFTopologyNode pNode;

            m_tn.GetOutput(0, out pNode, out pIndex);
        }

        void TestSetOutputPrefType()
        {
            IMFMediaType pType, pType2;

            MFExtern.MFCreateMediaType(out pType);

            m_tn.SetOutputPrefType(0, pType);
            m_tn.GetOutputPrefType(0, out pType2);

            Debug.Assert(pType == pType2);
        }

        void TestSetInputPrefType()
        {
            IMFMediaType pType, pType2;

            MFExtern.MFCreateMediaType(out pType);

            // Returns E_NOTIMPL since this is a source node
            try
            {
                m_tn.SetInputPrefType(0, pType);
                m_tn.GetInputPrefType(0, out pType2);
                Debug.Assert(pType == pType2);
            }
            catch { }
        }

        void TestCloneFrom()
        {
            IMFTopologyNode tn;

            MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out tn);

            tn.CloneFrom(m_tn);
        }

        private void GetInterface()
        {
            MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out m_tn);
        }
    }
}
