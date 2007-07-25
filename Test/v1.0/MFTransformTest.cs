using System;
using System.Diagnostics;
using System.Text;
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
            Guid g1 = new Guid("{ffa0d1f1-da7c-49cc-91ea-484dcf94a70a}");
            string s;
            int icnt, ocnt;
            IntPtr ip = IntPtr.Zero;
            MFTRegisterTypeInfo[] it = null; // new MFTRegisterTypeInfo[5];
            MFTRegisterTypeInfo[] ot = null; // new MFTRegisterTypeInfo[5];

            MFTRegisterTypeInfo[] it2 = null; // new MFTRegisterTypeInfo[5];
            MFTRegisterTypeInfo[] ot2 = null; // new MFTRegisterTypeInfo[5];

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

            MFExtern.MFTGetInfo(g1, out s, it, out icnt, ot, out ocnt, ip);

            MFExtern.MFTUnregister(g1);

            it = new MFTRegisterTypeInfo[2];
            ot = new MFTRegisterTypeInfo[3];

            it[0].guidMajorType = MFMediaType.Video;
            it[1].guidSubtype = new Guid("00000000-1111-2222-3333-444444444444");
            it[1].guidMajorType = MFMediaType.Video;

            ot[0].guidMajorType = MFMediaType.Video;
            ot[1].guidMajorType = MFMediaType.Video;
            ot[2].guidMajorType = MFMediaType.Video;

            MFExtern.MFTRegister(
                g1,
                MFTransformCategory.MFT_CATEGORY_VIDEO_EFFECT,
                "fdsa",
                0,
                it.Length,
                it,
                ot.Length,
                ot,
                null);

            MFExtern.MFTGetInfo(g1, out s, it2, out icnt, ot2, out ocnt, IntPtr.Zero);

            MFExtern.MFTUnregister(g1);
        }

    }
}
