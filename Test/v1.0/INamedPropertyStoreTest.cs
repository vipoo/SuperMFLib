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

            int hr = m_pStore.GetNameAt(0, out s);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(s == "asdf");
        }

        private void TestSetValue()
        {
            PropVariant p = new PropVariant();

            int hr = m_pStore.SetNamedValue("asdf", new PropVariant("testme"));
            MFError.ThrowExceptionForHR(hr);
            hr = m_pStore.GetNamedValue("asdf", p);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(p.GetString() == "testme");
        }

        private void TestCount(int i)
        {
            int iCnt;

            int hr = m_pStore.GetNameCount(out iCnt);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(i == iCnt);
        }

        private void GetInterface()
        {
            int hr = MFExtern.CreateNamedPropertyStore(out m_pStore);
            MFError.ThrowExceptionForHR(hr);
        }
    }
}
