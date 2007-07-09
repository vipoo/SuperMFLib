using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using MediaFoundation;
using MediaFoundation.Utils;
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
            m_col.GetElementCount(out i);
            Debug.Assert(i == iCnt);
        }

        void TestGetElement()
        {
            object o;
            m_col.GetElement(0, out o);

            Debug.Assert(o == this);
        }

        void TestAddElement()
        {
            m_col.AddElement(this);
        }

        void TestRemoveElement()
        {
            object o;
            m_col.RemoveElement(0, out o);
            TestGetElementCount(1);
        }

        void TestInsertElementAt()
        {
            m_col.InsertElementAt(0, this);
            TestGetElementCount(2);
        }

        void TestRemoveAllElements()
        {
            m_col.RemoveAllElements();
            TestGetElementCount(0);
        }

        private void GetInterface()
        {
            MFPlatDll.MFCreateCollection(out m_col);
        }
    }
}
