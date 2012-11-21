using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class IPropertyStoreTest
    {
        IPropertyStore m_ps;

        public void DoTests()
        {
            GetInterface();

            TestSetValue();

            TestGetCount();
            TestGetAt();
            TestCommit();
        }

        void TestGetCount()
        {
            int i;
            int hr = m_ps.GetCount(out i);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == 1);
        }

        void TestGetAt()
        {
            PropVariant var = new PropVariant();
            PropertyKey key = new PropertyKey();
            int hr = m_ps.GetAt(0, key);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(key.pID == 3);

            hr = m_ps.GetValue(key, var);
            MFError.ThrowExceptionForHR(hr);
            Debug.Assert(var.GetString() == "asdf");
        }

        void TestSetValue()
        {
            PropVariant var = new PropVariant("asdf");
            PropertyKey key = new PropertyKey();

            key.fmtid = Guid.NewGuid();
            key.pID = 3;

            int hr = m_ps.SetValue(key, var);
            MFError.ThrowExceptionForHR(hr);
        }

        void TestCommit()
        {
            try
            {
                int hr = m_ps.Commit();
                MFError.ThrowExceptionForHR(hr);
            }
            catch { } // not implemented
        }

        private void GetInterface()
        {
            int hr = MFExtern.CreatePropertyStore(out m_ps);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
