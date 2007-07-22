using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class INamedPropertyStoreTest
    {
        INamedPropertyStore m_pStore;

        public void DoTests()
        {
            GetInterface();

            TestCount(0);
            TestSetValue();
            TestCount(1);

            TestGetNameAt();

        }

        private void TestGetNameAt()
        {
            string s;

            m_pStore.GetNameAt(0, out s);

            Debug.Assert(s == "asdf");
        }

        private void TestSetValue()
        {
            PropVariant p = new PropVariant();

            m_pStore.SetNamedValue("asdf", new PropVariant("testme"));
            m_pStore.GetNamedValue("asdf", p);

            Debug.Assert(p.GetString() == "testme");
        }

        private void TestCount(int i)
        {
            int iCnt;

            m_pStore.GetNameCount(out iCnt);

            Debug.Assert(i == iCnt);
        }

        private void GetInterface()
        {
            MFExtern.CreateNamedPropertyStore(out m_pStore);
        }
    }
}
