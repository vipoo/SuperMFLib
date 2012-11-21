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
    class IMFCollectionTest
    {
        IMFCollection m_col;

        public void DoTests()
        {
            GetInterface();

            TestAddElement();
            TestGetElementCount(1);
            TestGetElement();
            TestInsertElementAt();
            TestRemoveElement();
            TestRemoveAllElements();
        }

        void TestGetElementCount(int iCnt)
        {
            int i;
            int hr = m_col.GetElementCount(out i);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(i == iCnt);
        }

        void TestGetElement()
        {
            object o;
            int hr = m_col.GetElement(0, out o);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(o == this);
        }

        void TestAddElement()
        {
            int hr = m_col.AddElement(this);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestRemoveElement()
        {
            object o;
            int hr = m_col.RemoveElement(0, out o);
            MFError.ThrowExceptionForHR(hr);
            TestGetElementCount(1);
        }

        void TestInsertElementAt()
        {
            int hr = m_col.InsertElementAt(0, this);
            MFError.ThrowExceptionForHR(hr);
            TestGetElementCount(2);
        }

        void TestRemoveAllElements()
        {
            int hr = m_col.RemoveAllElements();
            MFError.ThrowExceptionForHR(hr);
            TestGetElementCount(0);
        }

        private void GetInterface()
        {
            int hr = MFExtern.MFCreateCollection(out m_col);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
