using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;

namespace Testv10
{
    class IMFAttributesTest
    {
        IMFAttributes m_attr;

        public void DoTests()
        {
            GetInterface();

            TestGetUINT32();
            TestGetUINT64();
            TestGetDouble();
            TestGetGuid();
            TestGetString();
            TestGetAllocatedString();
            TestGetUnknown();
            TestGetAllocatedBlob();
            TestGetBlob();
            TestGetItem();

            TestCopyAllItems();

            TestGetItemByIndex();

            TestLockStore();
        }

        private void TestGetItem()
        {
            PropVariant o2;
            byte[] b = { 1, 2, 3 };
            byte[] b2;

            o2 = TestGetItem2(new PropVariant("asdf"));
            Debug.Assert(o2.GetString() == "asdf");

            o2 = TestGetItem2(new PropVariant(13));
            Debug.Assert(o2.GetInt() == 13);

            o2 = TestGetItem2(new PropVariant(5.5));
            Debug.Assert(o2.GetDouble() == 5.5);

            o2 = TestGetItem2(new PropVariant(long.MaxValue));
            Debug.Assert(o2.GetLong() == long.MaxValue);

            o2 = TestGetItem2(new PropVariant(typeof(IMFAttributes).GUID));
            Debug.Assert(o2.GetGuid() == typeof(IMFAttributes).GUID);

            o2 = TestGetItem2(new PropVariant(b));
            b2 = o2.GetBlob();
            Debug.Assert(b[0] == b2[0] && b[1] == b2[1] && b[2] == b2[2] && b.Length == b2.Length);

            o2 = TestGetItem2(new PropVariant(this));
            Debug.Assert(o2.GetIUnknown() == this);
        }

        private PropVariant TestGetItem2(PropVariant o)
        {
            MFAttributeType pType;
            PropVariant o2 = new PropVariant();

            Guid g = Guid.NewGuid();
            m_attr.SetItem(g, o);

            m_attr.GetItem(g, o2);
            m_attr.GetItemType(g, out pType);
            Debug.Assert(o2.GetMFAttributeType() == o.GetMFAttributeType() && o2.GetMFAttributeType() == pType);

            return o2;
        }

        private void TestGetUINT32()
        {
            int i;
            Guid g = Guid.NewGuid();
            m_attr.SetUINT32(g, 3);

            m_attr.GetUINT32(g, out i);

            Debug.Assert(i == 3);
        }

        private void TestGetUINT64()
        {
            long l;
            Guid g = Guid.NewGuid();
            m_attr.SetUINT64(g, 4);

            m_attr.GetUINT64(g, out l);

            Debug.Assert(l == 4);
        }

        private void TestGetDouble()
        {
            double d;
            Guid g = Guid.NewGuid();
            m_attr.SetDouble(g, 5.5);

            m_attr.GetDouble(g, out d);

            Debug.Assert(d == 5.5);
        }

        private void TestGetGuid()
        {
            Guid gv1, gv2;
            Guid g = Guid.NewGuid();
            gv1 = Guid.NewGuid();
            m_attr.SetGUID(g, gv1);

            m_attr.GetGUID(g, out gv2);

            Debug.Assert(gv1 == gv2);
        }

        private void TestGetString()
        {
            int c;
            int c2;
            Guid g = Guid.NewGuid();
            StringBuilder s;

            m_attr.SetString(g, "hey");

            m_attr.GetStringLength(g, out c);
            Debug.Assert(c == 3);

            s = new StringBuilder(c + 1);
            m_attr.GetString(g, s, c + 1, out c2);

            Debug.Assert(s.ToString() == "hey");
        }

        private void TestGetAllocatedString()
        {
            int c;
            Guid g = Guid.NewGuid();
            string s;

            m_attr.SetString(g, "hey there");

            m_attr.GetAllocatedString(g, out s, out c);

            Debug.Assert(c == 9 && s == "hey there");
        }

        private void TestGetBlob()
        {
            int c, c2;
            Guid g = Guid.NewGuid();
            byte[] b = new byte[33];
            byte[] b2;

            for (int x = 13; x < b.Length + 13; x++)
            {
                b[x - 13] = (byte)x;
            }

            m_attr.SetBlob(g, b, b.Length);
            m_attr.GetBlobSize(g, out c);

            b2 = new byte[c];

            m_attr.GetBlob(g, b2, c, out c2);

            Debug.Assert(b[0] == b2[0] && b[13] == b2[13] && c2 == b.Length);
        }

        private void TestGetAllocatedBlob()
        {
            int c, c2;
            Guid g = Guid.NewGuid();
            byte[] b = new byte[33];
            byte[] b2;

            for (int x = 13; x < b.Length + 13; x++)
            {
                b[x - 13] = (byte)x;
            }

            m_attr.SetBlob(g, b, b.Length);
            m_attr.GetBlobSize(g, out c);

            b2 = new byte[c];

            IntPtr ip = IntPtr.Zero;

            m_attr.GetAllocatedBlob(g, out ip, out c2);
            Marshal.Copy(ip, b2, 0, c2);
            Marshal.FreeCoTaskMem(ip);

            Debug.Assert(b[0] == b2[0] && b[13] == b2[13] && c2 == b.Length);
        }

        private void TestGetUnknown()
        {
            Guid g = Guid.NewGuid();
            object o;
            Guid IID = new Guid("00000000-0000-0000-C000-000000000046");

            m_attr.SetUnknown(g, m_attr);

            m_attr.GetUnknown(g, IID, out o);

            Debug.Assert(o == m_attr);
        }

        private void TestLockStore()
        {
            m_attr.LockStore();
            m_attr.UnlockStore();
        }

        private void TestGetItemByIndex()
        {
            Guid g;
            PropVariant o = new PropVariant();
            int iCnt1, iCnt2;
            bool bRes;

            m_attr.GetCount(out iCnt1);

            m_attr.GetItemByIndex(0, out g, o);
            m_attr.CompareItem(g, o, out bRes);
            Debug.Assert(bRes);

            m_attr.DeleteItem(g);
            m_attr.GetCount(out iCnt2);

            Debug.Assert(iCnt2 == iCnt1 - 1);
       }

        private void TestCopyAllItems()
        {
            IMFAttributes attr;
            int iCnt1, iCnt2;
            bool bRes;

            m_attr.GetCount(out iCnt1);

            MFExtern.MFCreateAttributes(out attr, 20);

            m_attr.CopyAllItems(attr);
            attr.GetCount(out iCnt2);

            Debug.Assert(iCnt1 == iCnt2 && iCnt1 > 0);

            m_attr.Compare(attr, MFAttributesMatchType.AllItems, out bRes);
            Debug.Assert(bRes);

            attr.DeleteAllItems();

            m_attr.Compare(attr, MFAttributesMatchType.AllItems, out bRes);
            Debug.Assert(!bRes);
        }

        private void GetInterface()
        {
            MFExtern.MFCreateAttributes(out m_attr, 20);
        }
    }
}
