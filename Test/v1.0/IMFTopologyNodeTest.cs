using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;

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
            int hr = m_tn.SetObject(this);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tn.GetObject(out o);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(this == o);
        }

        void TestGetNodeType()
        {
            MFTopologyType pType;

            int hr = m_tn.GetNodeType(out pType);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pType == MFTopologyType.TransformNode);
        }

        void TestSetTopoNodeID()
        {
            long l;

            int hr = m_tn.SetTopoNodeID(123456789);
            MFError.ThrowExceptionForHR(hr);
            hr = m_tn.GetTopoNodeID(out l);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(l == 123456789);
        }

        void TestGetInputCount()
        {
            int i;

            int hr = m_tn.GetInputCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 1);
        }

        void TestGetOutputCount()
        {
            int i;

            int hr = m_tn.GetOutputCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == 1);
        }

        void TestConnectOutput()
        {
            IMFTopologyNode tn;

            int hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out tn);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tn.ConnectOutput(0, tn, 0);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestDisconnectOutput()
        {
            int hr = m_tn.DisconnectOutput(0);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestGetInput()
        {
            int pIndex;
            IMFTopologyNode pNode;

            IMFTopologyNode tn;

            int hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out tn);
            MFError.ThrowExceptionForHR(hr);

            hr = tn.ConnectOutput(0, m_tn, 0);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tn.GetInput(0, out pNode, out pIndex);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pNode == tn);
        }

        void TestGetOutput()
        {
            int pIndex;
            IMFTopologyNode pNode;

            int hr = m_tn.GetOutput(0, out pNode, out pIndex);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestSetOutputPrefType()
        {
            IMFMediaType pType, pType2;

            int hr = MFExtern.MFCreateMediaType(out pType);
            MFError.ThrowExceptionForHR(hr);

            hr = m_tn.SetOutputPrefType(0, pType);
            MFError.ThrowExceptionForHR(hr);
            hr = m_tn.GetOutputPrefType(0, out pType2);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(pType == pType2);
        }

        void TestSetInputPrefType()
        {
            IMFMediaType pType, pType2;

            int hr = MFExtern.MFCreateMediaType(out pType);
            MFError.ThrowExceptionForHR(hr);

            // Returns E_NOTIMPL since this is a source node
            try
            {
                hr = m_tn.SetInputPrefType(0, pType);
                MFError.ThrowExceptionForHR(hr);
                hr = m_tn.GetInputPrefType(0, out pType2);
                MFError.ThrowExceptionForHR(hr);
                Debug.Assert(pType == pType2);
            }
            catch { }
        }

        void TestCloneFrom()
        {
            IMFTopologyNode tn;

            int hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out tn);
            MFError.ThrowExceptionForHR(hr);

            hr = tn.CloneFrom(m_tn);
            MFError.ThrowExceptionForHR(hr);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateTopologyNode(MFTopologyType.TransformNode, out m_tn);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
