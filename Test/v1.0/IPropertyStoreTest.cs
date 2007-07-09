using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using MediaFoundation;
using MediaFoundation.Utils;
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
            m_ps.GetCount(out i);

            Debug.Assert(i == 1);
        }

        void TestGetAt()
        {
            PropVariant var = new PropVariant();
            PropertyKey key = new PropertyKey();
            m_ps.GetAt(0, key);

            Debug.Assert(key.pID == 3);

            m_ps.GetValue(key, var);
            Debug.Assert(var.GetString() == "asdf");
        }

        void TestSetValue()
        {
            PropVariant var = new PropVariant("asdf");
            PropertyKey key = new PropertyKey();

            key.fmtid = Guid.NewGuid();
            key.pID = 3;

            m_ps.SetValue(key, var);
        }

        void TestCommit()
        {
            try
            {
                m_ps.Commit();
            }
            catch { } // not implemented
        }

        private void GetInterface()
        {
            MFPlatDll.CreatePropertyStore(out m_ps);
        }
    }
}
