using System;
using System.Diagnostics;
using System.Text;

using MediaFoundation.Misc;

namespace Testv10
{
    class TestPropVariant
    {
        public void DoTests()
        {
            byte [] b = new byte[] {1, 2, 3};
            double d = 2.3;
            Guid g = Guid.NewGuid();
            string s = "hello there";
            int i = 123;
            long l = 456;
            string[] sa = new string[] { "a", "b", "c" };
            string[] sa2 = new string[] { "a", "b", "c" };

            PropVariant p1 = new PropVariant();
            PropVariant p2 = new PropVariant(b);
            PropVariant p3 = new PropVariant(d);
            PropVariant p4 = new PropVariant(g);
            PropVariant p5 = new PropVariant(s);
            PropVariant p6 = new PropVariant(i);
            PropVariant p7 = new PropVariant(l);
            PropVariant p8 = new PropVariant(sa);
            PropVariant p9 = new PropVariant(this as object);

            Debug.Assert(p1.ToString() == "<Empty>");
            Debug.Assert(p2.ToString() == "01,02,03");
            Debug.Assert(p3.ToString() == d.ToString());
            Debug.Assert(p4.ToString() == g.ToString());
            Debug.Assert(p5.ToString() == s);
            Debug.Assert(p6.ToString() == i.ToString());
            Debug.Assert(p7.ToString() == l.ToString());
            Debug.Assert(p8.ToString() == "\"a\",\"b\",\"c\"");
            Debug.Assert(p9.ToString() == "Testv10.TestPropVariant");

            Type t1 = p1.GetRuntimeType();
            Type t2 = p2.GetRuntimeType();
            Type t3 = p3.GetRuntimeType();
            Type t4 = p4.GetRuntimeType();
            Type t5 = p5.GetRuntimeType();
            Type t6 = p6.GetRuntimeType();
            Type t7 = p7.GetRuntimeType();
            Type t8 = p8.GetRuntimeType();
            Type t9 = p9.GetRuntimeType();

            int i1 = p1.GetHashCode();
            int i2 = p2.GetHashCode();
            int i3 = p3.GetHashCode();
            int i4 = p4.GetHashCode();
            int i5 = p5.GetHashCode();
            int i6 = p6.GetHashCode();
            int i7 = p7.GetHashCode();
            int i8 = p8.GetHashCode();
            int i9 = p9.GetHashCode();

            bool bret = p2.Equals(new PropVariant(b));
            bool bret2 = p8.Equals(new PropVariant(sa2));
        }
    }
}
