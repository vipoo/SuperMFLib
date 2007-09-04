// test MFTRegister, MFTUnregister, MFTGetInfo, MFTEnum
using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Transform;

namespace Testv10
{
    class MFTransformTest
    {
        public void DoTests()
        {
            TestReg();
            TestEnum();
        }

        private void TestReg()
        {
            // HKEY_CLASSES_ROOT\MediaFoundation\Transforms\ffa0d1f1-da7c-49cc-91ea-484dcf94a70a

            Guid g1 = new Guid("{ffa0d1f1-da7c-49cc-91ea-484dcf94a70a}");
            string s;
            MFInt icnt = new MFInt(0);
            MFInt ocnt = new MFInt(0);
            IntPtr ip = IntPtr.Zero;
            ArrayList it = new ArrayList();
            ArrayList ot = new ArrayList();

            MFExtern.MFTRegister(
                g1,
                MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT,
                "asdf",
                0,
                0,
                null,
                0,
                null,
                null);


            MFExtern.MFTGetInfo(g1, out s, it, icnt, ot, ocnt, ip);

            MFExtern.MFTUnregister(g1);

            Debug.Assert(s == "asdf");

            MFTRegisterTypeInfo [] it1 = new MFTRegisterTypeInfo[2];
            MFTRegisterTypeInfo [] ot1 = new MFTRegisterTypeInfo[3];

            it1[0] = new MFTRegisterTypeInfo();
            it1[1] = new MFTRegisterTypeInfo();
            ot1[0] = new MFTRegisterTypeInfo();
            ot1[1] = new MFTRegisterTypeInfo();
            ot1[2] = new MFTRegisterTypeInfo();

            it1[0].guidMajorType = MFMediaType.Video;
            it1[1].guidSubtype = new Guid("00000000-1111-2222-3333-444444444444");
            it1[1].guidMajorType = MFMediaType.Video;

            ot1[0].guidMajorType = MFMediaType.Video;
            ot1[1].guidMajorType = MFMediaType.Video;
            ot1[2].guidMajorType = MFMediaType.Video;

            MFExtern.MFTRegister(
                g1,
                MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT,
                "fdsa",
                0,
                it1.Length,
                it1,
                ot1.Length,
                ot1,
                null);

            ArrayList it2 = new ArrayList();
            ArrayList ot2 = new ArrayList();
            MFExtern.MFTGetInfo(g1, out s, it2, icnt, ot2, ocnt, IntPtr.Zero);

            Debug.Assert(s == "fdsa");
            Debug.Assert(icnt == it1.Length && ocnt == ot1.Length);
            Debug.Assert(it2.Count == icnt && 
                it1[0].guidMajorType == ((MFTRegisterTypeInfo)it2[0]).guidMajorType &&
                it1[0].guidSubtype == ((MFTRegisterTypeInfo)it2[0]).guidSubtype &&
                it1[1].guidMajorType == ((MFTRegisterTypeInfo)it2[1]).guidMajorType &&
                it1[1].guidSubtype == ((MFTRegisterTypeInfo)it2[1]).guidSubtype
                );

            Debug.Assert(ot2.Count == ocnt &&
                ot1[0].guidMajorType == ((MFTRegisterTypeInfo)ot2[0]).guidMajorType &&
                ot1[0].guidSubtype == ((MFTRegisterTypeInfo)ot2[0]).guidSubtype &&
                ot1[1].guidMajorType == ((MFTRegisterTypeInfo)ot2[1]).guidMajorType &&
                ot1[1].guidSubtype == ((MFTRegisterTypeInfo)ot2[1]).guidSubtype &&
                ot1[2].guidMajorType == ((MFTRegisterTypeInfo)ot2[2]).guidMajorType &&
                ot1[2].guidSubtype == ((MFTRegisterTypeInfo)ot2[2]).guidSubtype
                );

            MFExtern.MFTUnregister(g1);
        }

        private void TestEnum()
        {
            ArrayList a1 = new ArrayList();
            MFInt i1 = new MFInt(0);
            MFInt i2 = new MFInt(0);
            MFInt i3 = new MFInt(0);
            MFInt i4 = new MFInt(0);
            MFTRegisterTypeInfo rin = new MFTRegisterTypeInfo();
            MFTRegisterTypeInfo rout = new MFTRegisterTypeInfo();

            rin.guidMajorType = MFMediaType.Video;
            rin.guidSubtype = new FourCC("AYUV").ToMediaSubtype();

            rout.guidMajorType = MFMediaType.Video;
            rout.guidSubtype = new FourCC("NV12").ToMediaSubtype();

            MFExtern.MFTEnum(Guid.Empty, 0, null, null, null, a1, i1);
            MFExtern.MFTEnum(MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT, 0, null, null, null, a1, i2);
            MFExtern.MFTEnum(MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT, 0, rin, null, null, a1, i3);
            MFExtern.MFTEnum(MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT, 0, rin, rout, null, a1, i4);

            Debug.Assert(i1 > 0 && i1 > i2 && i2 >= i3 && i3 >= i4 && i4 > 0);

            for (int y = 0; y < i4; y++)
            {
                MFInt itypescnt = new MFInt(0);
                MFInt otypescnt = new MFInt(0);
                ArrayList a = new ArrayList();
                ArrayList b = new ArrayList();
                Guid mft = (Guid)a1[y];
                string s;

                MFExtern.MFTGetInfo(
                    mft,
                    out s,
                    a,
                    itypescnt,
                    b,
                    otypescnt,
                    IntPtr.Zero);

                MFExtern.MFTGetInfo(
                    mft,
                    out s,
                    null,
                    null,
                    null,
                    null,
                    IntPtr.Zero);

                for (int x = 0; x < itypescnt; x++)
                {
                    MFTRegisterTypeInfo rti = a[x] as MFTRegisterTypeInfo;

                    if (FourCC.IsA4ccSubtype(rti.guidMajorType))
                    {
                        Debug.Write(new FourCC(rti.guidMajorType).ToString());
                        Debug.Write(" ");
                    }
                    Debug.WriteLine(rti.guidMajorType);
                    if (FourCC.IsA4ccSubtype(rti.guidSubtype))
                    {
                        Debug.Write(new FourCC(rti.guidSubtype).ToString());
                        Debug.Write(" ");
                    }
                    Debug.WriteLine(rti.guidSubtype);
                }

                Debug.WriteLine("----------------------");

                for (int x = 0; x < otypescnt; x++)
                {
                    MFTRegisterTypeInfo rti = b[x] as MFTRegisterTypeInfo;

                    if (FourCC.IsA4ccSubtype(rti.guidMajorType))
                    {
                        Debug.Write(new FourCC(rti.guidMajorType).ToString());
                        Debug.Write(" ");
                    }
                    Debug.WriteLine(rti.guidMajorType);
                    if (FourCC.IsA4ccSubtype(rti.guidSubtype))
                    {
                        Debug.Write(new FourCC(rti.guidSubtype).ToString());
                        Debug.Write(" ");
                    }
                    Debug.WriteLine(rti.guidSubtype);
                }

                Debug.WriteLine("===============================");
            }
        }

    }
}
