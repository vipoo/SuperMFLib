/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Security;

using MediaFoundation.Misc;

namespace D3D
{
    #region Externs

    public static class D3DExtern
    {
        public const int D3D_SDK_VERSION = 32;

        // Debug value (32 | 0x80000000)
        //public const int D3D_SDK_VERSION = unchecked((int)0x80000020);

        [DllImport("D3D9.DLL", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
        public extern static void Direct3DCreate9Ex(
            int SDKVersion,
            out IDirect3D9Ex pD3D
            );

        [DllImport("dxva2.DLL", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
        public extern static void DXVA2CreateDirect3DDeviceManager9(
            out int pResetToken,
            out IDirect3DDeviceManager9 ppDXVAManager
            );
    }

    #endregion

    #region Definitions

    [UnmanagedName("D3DBACKBUFFER_TYPE")]
    public enum D3DBACKBUFFER_TYPE
    {
        Mono = 0,
        Left = 1,
        Right = 2,

        ForceDWORD = 0x7fffffff
    }

    [UnmanagedName("D3DERR_* defines")]
    public enum D3DError
    {
        Ok = 0,

        WrongTextureFormat = unchecked((int)0x88760818),
        UnsupportedColorOperation,
        UnsupportedColorArg,
        UnsupportedAlphaOperation,
        UnsupportedAlphaArg,
        TooManyOperations,
        ConflictingTextureFilter,
        UnsupportedFactorValue,
        junk1,
        ConflictingRenderState,
        UnsupportedTextureFilter,
        junk2,
        junk3,
        junk4,
        ConflictingTexturePalette,
        DriverInternalError,

        NotFound = unchecked((int)0x88760866),
        MoreData,
        DeviceLost,
        DeviceNotReset,
        NotAvailable,
        InvalidDevice,
        InvalidCall,
        DriverInvalidCall,
        junk5,
        NoAutoGen,
        DeviceRemoved,
        junk6,
        junk7,
        junk8,
        DeviceHung,
        S_NotResident = 0x8760875,
        S_ResidentInSharedMemory,
        S_PresentModeChanged,
        S_PresentOccluded
    }

    [UnmanagedName("D3DFORMAT")]
    public enum D3DFORMAT
    {
        Unknown = 0,

        R8G8B8 = 20,
        A8R8G8B8 = 21,
        X8R8G8B8 = 22,
        R5G6B5 = 23,
        X1R5G5B5 = 24,
        A1R5G5B5 = 25,
        A4R4G4B4 = 26,
        R3G3B2 = 27,
        A8 = 28,
        A8R3G3B2 = 29,
        X4R4G4B4 = 30,
        A2B10G10R10 = 31,
        A8B8G8R8 = 32,
        X8B8G8R8 = 33,
        G16R16 = 34,
        A2R10G10B10 = 35,
        A16B16G16R16 = 36,

        A8P8 = 40,
        P8 = 41,

        L8 = 50,
        A8L8 = 51,
        A4L4 = 52,

        V8U8 = 60,
        L6V5U5 = 61,
        X8L8V8U8 = 62,
        Q8W8V8U8 = 63,
        V16U16 = 64,
        A2W10V10U10 = 67,

        UYVY = 1498831189, // MAKEFOURCC('U', 'Y', 'V', 'Y'),
        R8G8_B8G8 = 1195525970, // MAKEFOURCC('R', 'G', 'B', 'G'),
        YUY2 = 844715353, // MAKEFOURCC('Y', 'U', 'Y', '2'),
        G8R8_G8B8 = 1111970375, // MAKEFOURCC('G', 'R', 'G', 'B'),
        DXT1 = 827611204, // MAKEFOURCC('D', 'X', 'T', '1'),
        DXT2 = 844388420, // MAKEFOURCC('D', 'X', 'T', '2'),
        DXT3 = 861165636, // MAKEFOURCC('D', 'X', 'T', '3'),
        DXT4 = 877942852, // MAKEFOURCC('D', 'X', 'T', '4'),
        DXT5 = 894720068, // MAKEFOURCC('D', 'X', 'T', '5'),

        D16_LOCKABLE = 70,
        D32 = 71,
        D15S1 = 73,
        D24S8 = 75,
        D24X8 = 77,
        D24X4S4 = 79,
        D16 = 80,

        D32F_LOCKABLE = 82,
        D24FS8 = 83,

        /* Z-Stencil formats valid for CPU access */
        D32_LOCKABLE = 84,
        S8_LOCKABLE = 85,

        L16 = 81,

        VERTEXDATA = 100,
        INDEX16 = 101,
        INDEX32 = 102,

        Q16W16V16U16 = 110,

        MULTI2_ARGB8 = 827606349, // MAKEFOURCC('M','E','T','1'),

        // Floating point surface formats

        // s10e5 formats (16-bits per channel)
        R16F = 111,
        G16R16F = 112,
        A16B16G16R16F = 113,

        // IEEE s23e8 formats (32-bits per channel)
        R32F = 114,
        G32R32F = 115,
        A32B32G32R32F = 116,

        CxV8U8 = 117,

        // Monochrome 1 bit per pixel format
        A1 = 118,

        // Binary format indicating that the data has no inherent type
        BINARYBUFFER = 199,

        FORCE_DWORD = 0x7fffffff
    }

    [UnmanagedName("D3DMULTISAMPLE_TYPE")]
    public enum D3DMULTISAMPLE_TYPE
    {
        None = 0,
        NonMaskable = 1,
        Samples2 = 2,
        Samples3 = 3,
        Samples4 = 4,
        Samples5 = 5,
        Samples6 = 6,
        Samples7 = 7,
        Samples8 = 8,
        Samples9 = 9,
        Samples10 = 10,
        Samples11 = 11,
        Samples12 = 12,
        Samples13 = 13,
        Samples14 = 14,
        Samples15 = 15,
        Samples16 = 16,

        ForceDWORD = 0x7fffffff
    }

    [UnmanagedName("D3DSWAPEFFECT")]
    public enum D3DSWAPEFFECT
    {
        None = 0,
        Discard = 1,
        Flip = 2,
        Copy = 3,

        ForceDWORD = 0x7fffffff
    }

    [UnmanagedName("D3DSCANLINEORDERING")]
    public enum D3DSCANLINEORDERING
    {
        Unknown = 0,
        Progressive = 1,
        Interlaced = 2
    }

    [UnmanagedName("D3DDEVTYPE")]
    public enum D3DDEVTYPE
    {
        HAL = 1,
        REF = 2,
        SW = 3,

        NULLREF = 4,

        FORCE_DWORD = 0x7fffffff
    }

    [Flags,
    UnmanagedName("D3DPRESENTFLAG_* defines")]
    public enum D3DPRESENTFLAG
    {
        None = 0,
        LockableBackbuffer = 0x00000001,
        DiscardDepthStencil = 0x00000002,
        DeviceClip = 0x00000004,
        Video = 0x00000010,
        NoAutoRotate = 0x00000020,
        UnPrunedMode = 0x00000040
    }

    [Flags,
    UnmanagedName("D3DPRESENT_INTERVAL* defines")]
    public enum D3DPRESENT_INTERVAL
    {
        Default = 0x00000000,
        One = 0x00000001,
        Two = 0x00000002,
        Three = 0x00000004,
        Four = 0x00000008,
        Immediate = unchecked((int)0x80000000)
    }

    [Flags,
    UnmanagedName("D3DDEVCAPS* defines")]
    public enum D3DDEVCAPS
    {
        EXECUTESYSTEMMEMORY = 0x00000010, /* Device can use execute buffers from system memory */
        EXECUTEVIDEOMEMORY = 0x00000020, /* Device can use execute buffers from video memory */
        TLVERTEXSYSTEMMEMORY = 0x00000040, /* Device can use TL buffers from system memory */
        TLVERTEXVIDEOMEMORY = 0x00000080, /* Device can use TL buffers from video memory */
        TEXTURESYSTEMMEMORY = 0x00000100, /* Device can texture from system memory */
        TEXTUREVIDEOMEMORY = 0x00000200, /* Device can texture from device memory */
        DRAWPRIMTLVERTEX = 0x00000400, /* Device can draw TLVERTEX primitives */
        CANRENDERAFTERFLIP = 0x00000800, /* Device can render without waiting for flip to complete */
        TEXTURENONLOCALVIDMEM = 0x00001000, /* Device can texture from nonlocal video memory */
        DRAWPRIMITIVES2 = 0x00002000, /* Device can support DrawPrimitives2 */
        SEPARATETEXTUREMEMORIES = 0x00004000, /* Device is texturing from separate memory pools */
        DRAWPRIMITIVES2EX = 0x00008000, /* Device can support Extended DrawPrimitives2 i.e. DX7 compliant driver*/
        HWTRANSFORMANDLIGHT = 0x00010000, /* Device can support transformation and lighting in hardware and DRAWPRIMITIVES2EX must be also */
        CANBLTSYSTONONLOCAL = 0x00020000, /* Device supports a Tex Blt from system memory to non-local vidmem */
        HWRASTERIZATION = 0x00080000, /* Device has HW acceleration for rasterization */
        PUREDEVICE = 0x00100000, /* Device supports D3DCREATE_PUREDEVICE */
        QUINTICRTPATCHES = 0x00200000, /* Device supports quintic Beziers and BSplines */
        RTPATCHES = 0x00400000, /* Device supports Rect and Tri patches */
        RTPATCHHANDLEZERO = 0x00800000, /* Indicates that RT Patches may be drawn efficiently using handle 0 */
        NPATCHES = 0x01000000 /* Device supports N-Patches */
    }

    [Flags,
    UnmanagedName("D3DCREATE_* defines")]
    public enum D3DCREATE
    {
        FPU_PRESERVE = 0x00000002,
        MultiThreaded = 0x00000004,

        PureDevice = 0x00000010,
        SoftwareVertexProcessing = 0x00000020,
        HardwareVertexProcessing = 0x00000040,
        MixedVertexProcessing = 0x00000080,

        DisableDriverManagement = 0x00000100,
        AdapterGroupDevice = 0x00000200,
        DisableDriverManagementEx = 0x00000400,

        // This flag causes the D3D runtime not to alter the focus
        // window in any way. Use with caution- the burden of supporting
        // focus management events (alt-tab, etc.) falls on the
        // application, and appropriate responses (switching display
        // mode, etc.) should be coded.
        NoWindowChanges = 0x00000800,

        // Disable multithreading for software vertex processing
        DisablePSGPThreading = 0x00002000,
        // This flag enables present statistics on device.
        EnablePresentStats = 0x00004000,
        // This flag disables printscreen support in the runtime for this device
        DisablePrintScreen = 0x00008000,

        ScreenSaver = 0x10000000
    }

    [UnmanagedName("D3DVSHADERCAPS2_0"),
    StructLayout(LayoutKind.Sequential)]
    public class D3DVSHADERCAPS2_0
    {
        public int Caps;
        public int DynamicFlowControlDepth;
        public int NumTemps;
        public int StaticFlowControlDepth;
    }

    [UnmanagedName("D3DPSHADERCAPS2_0"),
    StructLayout(LayoutKind.Sequential)]
    public class D3DPSHADERCAPS2_0
    {
        public int Caps;
        public int DynamicFlowControlDepth;
        public int NumTemps;
        public int StaticFlowControlDepth;
        public int NumInstructionSlots;
    }

    [UnmanagedName("D3DDEVICE_CREATION_PARAMETERS"),
    StructLayout(LayoutKind.Sequential)]
    public struct D3DDEVICE_CREATION_PARAMETERS
    {
        public int AdapterOrdinal;
        public D3DDEVTYPE DeviceType;
        public IntPtr hFocusWindow;
        public D3DCREATE BehaviorFlags;
    }

    [UnmanagedName("D3DDISPLAYMODE"),
    StructLayout(LayoutKind.Sequential)]
    public struct D3DDISPLAYMODE
    {
        public int Width;
        public int Height;
        public int RefreshRate;
        public D3DFORMAT Format;
    }

    [UnmanagedName("D3DCAPS9"),
    StructLayout(LayoutKind.Sequential)]
    public struct D3DCAPS9
    {
        /* Device Info */
        public D3DDEVTYPE DeviceType;
        public int AdapterOrdinal;

        /* Caps from DX7 Draw */
        public int Caps;
        public int Caps2;
        public int Caps3;
        public int PresentationIntervals;

        /* Cursor Caps */
        public int CursorCaps;

        /* 3D Device Caps */
        public D3DDEVCAPS DevCaps;

        public int PrimitiveMiscCaps;
        public int RasterCaps;
        public int ZCmpCaps;
        public int SrcBlendCaps;
        public int DestBlendCaps;
        public int AlphaCmpCaps;
        public int ShadeCaps;
        public int TextureCaps;
        public int TextureFilterCaps;          // D3DPTFILTERCAPS for IDirect3DTexture9's
        public int CubeTextureFilterCaps;      // D3DPTFILTERCAPS for IDirect3DCubeTexture9's
        public int VolumeTextureFilterCaps;    // D3DPTFILTERCAPS for IDirect3DVolumeTexture9's
        public int TextureAddressCaps;         // D3DPTADDRESSCAPS for IDirect3DTexture9's
        public int VolumeTextureAddressCaps;   // D3DPTADDRESSCAPS for IDirect3DVolumeTexture9's

        public int LineCaps;                   // D3DLINECAPS

        public int MaxTextureWidth, MaxTextureHeight;
        public int MaxVolumeExtent;

        public int MaxTextureRepeat;
        public int MaxTextureAspectRatio;
        public int MaxAnisotropy;
        public float MaxVertexW;

        public float GuardBandLeft;
        public float GuardBandTop;
        public float GuardBandRight;
        public float GuardBandBottom;

        public float ExtentsAdjust;
        public int StencilCaps;

        public int FVFCaps;
        public int TextureOpCaps;
        public int MaxTextureBlendStages;
        public int MaxSimultaneousTextures;

        public int VertexProcessingCaps;
        public int MaxActiveLights;
        public int MaxUserClipPlanes;
        public int MaxVertexBlendMatrices;
        public int MaxVertexBlendMatrixIndex;

        public float MaxPointSize;

        public int MaxPrimitiveCount;          // max number of primitives per DrawPrimitive call
        public int MaxVertexIndex;
        public int MaxStreams;
        public int MaxStreamStride;            // max stride for SetStreamSource

        public int VertexShaderVersion;
        public int MaxVertexShaderConst;       // number of vertex shader constant registers

        public int PixelShaderVersion;
        public float PixelShader1xMaxValue;      // max value storable in registers of ps.1.x shaders

        // Here are the DX9 specific ones
        public int DevCaps2;

        public float MaxNpatchTessellationLevel;
        public int Reserved5;

        public int MasterAdapterOrdinal;       // ordinal of master adaptor for adapter group
        public int AdapterOrdinalInGroup;      // ordinal inside the adapter group
        public int NumberOfAdaptersInGroup;    // number of adapters in this adapter group (only if master)
        public int DeclTypes;                  // Data types, supported in vertex declarations
        public int NumSimultaneousRTs;         // Will be at least 1
        public int StretchRectFilterCaps;      // Filter caps supported by StretchRect
        public D3DVSHADERCAPS2_0 VS20Caps;
        public D3DPSHADERCAPS2_0 PS20Caps;
        public int VertexTextureFilterCaps;    // D3DPTFILTERCAPS for IDirect3DTexture9's for texture, used in vertex shaders
        public int MaxVShaderInstructionsExecuted; // maximum number of vertex shader instructions that can be executed
        public int MaxPShaderInstructionsExecuted; // maximum number of pixel shader instructions that can be executed
        public int MaxVertexShader30InstructionSlots;
        public int MaxPixelShader30InstructionSlots;
    }

    [UnmanagedName("D3DDISPLAYMODEEX"),
    StructLayout(LayoutKind.Sequential)]
    public class D3DDISPLAYMODEEX
    {
        int Size;
        int Width;
        int Height;
        int RefreshRate;
        D3DFORMAT Format;
        D3DSCANLINEORDERING ScanLineOrdering;
    }

    [UnmanagedName("RGNDATAHEADER"),
    StructLayout(LayoutKind.Sequential)]
    public class RGNDATAHEADER
    {
        public int dwSize;
        public int iType;
        public int nCount;
        public int nRgnSize;
        public Rectangle rcBound;
    }

    [UnmanagedName("RGNDATA"),
    StructLayout(LayoutKind.Sequential)]
    public class RGNDATA
    {
        public RGNDATAHEADER rdh;
        public Rectangle[] Buffer;
    }

    [UnmanagedName("D3DPRESENT_PARAMETERS"),
    StructLayout(LayoutKind.Sequential)]
    public class D3DPRESENT_PARAMETERS
    {
        public int BackBufferWidth;
        public int BackBufferHeight;
        public D3DFORMAT BackBufferFormat;
        public int BackBufferCount;

        public D3DMULTISAMPLE_TYPE MultiSampleType;
        public int MultiSampleQuality;

        public D3DSWAPEFFECT SwapEffect;
        public IntPtr hDeviceWindow;
        public bool Windowed;
        public bool EnableAutoDepthStencil;
        public D3DFORMAT AutoDepthStencilFormat;
        public D3DPRESENTFLAG Flags;

        /* FullScreen_RefreshRateInHz must be zero for Windowed mode */
        public int FullScreen_RefreshRateInHz;
        public D3DPRESENT_INTERVAL PresentationInterval;

        public void Empty()
        {
            BackBufferWidth = 0;
            BackBufferHeight = 0;
            BackBufferFormat = 0;
            BackBufferCount = 0;

            MultiSampleType = 0;
            MultiSampleQuality = 0;

            SwapEffect = 0;
            hDeviceWindow = IntPtr.Zero;
            Windowed = false;
            EnableAutoDepthStencil = false;
            AutoDepthStencilFormat = 0;
            Flags = 0;

            /* FullScreen_RefreshRateInHz must be zero for Windowed mode */
            FullScreen_RefreshRateInHz = 0;
            PresentationInterval = 0;
        }
    }

    #endregion

    #region Interfaces

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("81BDCBCA-64D4-426d-AE8D-AD0147F4275C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3D9
    {
        void Junk01();
        [PreserveSig]
        int GetAdapterCount();
        void Junk03();
        void Junk04();
        void Junk05();
        void GetAdapterDisplayMode(int Adapter, out D3DDISPLAYMODE pMode);
        void CheckDeviceType(int Adapter, D3DDEVTYPE DevType, D3DFORMAT AdapterFormat, D3DFORMAT BackBufferFormat, [MarshalAs(UnmanagedType.Bool)] bool bWindowed);
        void Junk08();
        void Junk09();
        void Junk10();
        void Junk11();
        void GetDeviceCaps(int Adapter, D3DDEVTYPE DeviceType, out D3DCAPS9 pCaps);
        [PreserveSig]
        IntPtr GetAdapterMonitor(int Adapter);
        void Junk14();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("02177241-69FC-400C-8FF1-93A44DF6861D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3D9Ex : IDirect3D9
    {
        #region IDirect3D9

        new void Junk01();
        [PreserveSig]
        new int GetAdapterCount();
        new void Junk03();
        new void Junk04();
        new void Junk05();
        new void GetAdapterDisplayMode(int Adapter, out D3DDISPLAYMODE pMode);
        new void CheckDeviceType(int Adapter, D3DDEVTYPE DevType, D3DFORMAT AdapterFormat, D3DFORMAT BackBufferFormat, [MarshalAs(UnmanagedType.Bool)] bool bWindowed);
        new void Junk08();
        new void Junk09();
        new void Junk10();
        new void Junk11();
        new void GetDeviceCaps(int Adapter, D3DDEVTYPE DeviceType, out D3DCAPS9 pCaps);
        [PreserveSig]
        new IntPtr GetAdapterMonitor(int Adapter);
        new void Junk14();

        #endregion

        void Junk1();
        void Junk2();
        void Junk3();
        void CreateDeviceEx(
            int Adapter,
            D3DDEVTYPE DeviceType,
            IntPtr hFocusWindow,
            D3DCREATE BehaviorFlags,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)]D3DPRESENT_PARAMETERS pPresentationParameters,
            D3DDISPLAYMODEEX pFullscreenDisplayMode,
            out IDirect3DDevice9Ex ppReturnedDeviceInterface);
        void Junk5();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D0223B96-BF7A-43fd-92BD-A43B0D82B9EB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DDevice9
    {
        void Junk001();
        void Junk002();
        void Junk003();
        void Junk004();
        void Junk005();
        void Junk006();
        void GetCreationParameters(out D3DDEVICE_CREATION_PARAMETERS pParameters);
        void Junk008();
        void Junk009();
        void Junk010();
        void CreateAdditionalSwapChain([In, MarshalAs(UnmanagedType.LPStruct)]D3DPRESENT_PARAMETERS pPresentationParameters, out IDirect3DSwapChain9 pSwapChain);
        void Junk012();
        void Junk013();
        void Junk014();
        void Junk015();
        void Junk016();
        void Junk017();
        void Junk018();
        void Junk019();
        void Junk020();
        void Junk021();
        void Junk022();
        void Junk023();
        void Junk024();
        void Junk025();
        void Junk026();
        void Junk027();
        void Junk028();
        void Junk029();
        void Junk030();
        void Junk031();
        void Junk032();
        void Junk033();
        void Junk034();
        void Junk035();
        void Junk036();
        void Junk037();
        void Junk038();
        void Junk039();
        void Junk040();
        void Junk041();
        void Junk042();
        void Junk043();
        void Junk044();
        void Junk045();
        void Junk046();
        void Junk047();
        void Junk048();
        void Junk049();
        void Junk050();
        void Junk051();
        void Junk052();
        void Junk053();
        void Junk054();
        void Junk055();
        void Junk056();
        void Junk057();
        void Junk058();
        void Junk059();
        void Junk060();
        void Junk061();
        void Junk062();
        void Junk063();
        void Junk064();
        void Junk065();
        void Junk066();
        void Junk067();
        void Junk068();
        void Junk069();
        void Junk070();
        void Junk071();
        void Junk072();
        void Junk073();
        void Junk074();
        void Junk075();
        void Junk076();
        void Junk077();
        void Junk078();
        void Junk079();
        void Junk080();
        void Junk081();
        void Junk082();
        void Junk083();
        void Junk084();
        void Junk085();
        void Junk086();
        void Junk087();
        void Junk088();
        void Junk089();
        void Junk090();
        void Junk091();
        void Junk092();
        void Junk093();
        void Junk094();
        void Junk095();
        void Junk096();
        void Junk097();
        void Junk098();
        void Junk099();
        void Junk100();
        void Junk101();
        void Junk102();
        void Junk103();
        void Junk104();
        void Junk105();
        void Junk106();
        void Junk107();
        void Junk108();
        void Junk109();
        void Junk110();
        void Junk111();
        void Junk112();
        void Junk113();
        void Junk114();
        void Junk115();
        void Junk116();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B18B10CE-2649-405a-870F-95F777D4313A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DDevice9Ex : IDirect3DDevice9
    {
        #region IDirect3DDevice9

        new void Junk001();
        new void Junk002();
        new void Junk003();
        new void Junk004();
        new void Junk005();
        new void Junk006();
        new void GetCreationParameters(out D3DDEVICE_CREATION_PARAMETERS pParameters);
        new void Junk008();
        new void Junk009();
        new void Junk010();
        new void CreateAdditionalSwapChain([In, MarshalAs(UnmanagedType.LPStruct)]D3DPRESENT_PARAMETERS pPresentationParameters, out IDirect3DSwapChain9 pSwapChain);
        new void Junk012();
        new void Junk013();
        new void Junk014();
        new void Junk015();
        new void Junk016();
        new void Junk017();
        new void Junk018();
        new void Junk019();
        new void Junk020();
        new void Junk021();
        new void Junk022();
        new void Junk023();
        new void Junk024();
        new void Junk025();
        new void Junk026();
        new void Junk027();
        new void Junk028();
        new void Junk029();
        new void Junk030();
        new void Junk031();
        new void Junk032();
        new void Junk033();
        new void Junk034();
        new void Junk035();
        new void Junk036();
        new void Junk037();
        new void Junk038();
        new void Junk039();
        new void Junk040();
        new void Junk041();
        new void Junk042();
        new void Junk043();
        new void Junk044();
        new void Junk045();
        new void Junk046();
        new void Junk047();
        new void Junk048();
        new void Junk049();
        new void Junk050();
        new void Junk051();
        new void Junk052();
        new void Junk053();
        new void Junk054();
        new void Junk055();
        new void Junk056();
        new void Junk057();
        new void Junk058();
        new void Junk059();
        new void Junk060();
        new void Junk061();
        new void Junk062();
        new void Junk063();
        new void Junk064();
        new void Junk065();
        new void Junk066();
        new void Junk067();
        new void Junk068();
        new void Junk069();
        new void Junk070();
        new void Junk071();
        new void Junk072();
        new void Junk073();
        new void Junk074();
        new void Junk075();
        new void Junk076();
        new void Junk077();
        new void Junk078();
        new void Junk079();
        new void Junk080();
        new void Junk081();
        new void Junk082();
        new void Junk083();
        new void Junk084();
        new void Junk085();
        new void Junk086();
        new void Junk087();
        new void Junk088();
        new void Junk089();
        new void Junk090();
        new void Junk091();
        new void Junk092();
        new void Junk093();
        new void Junk094();
        new void Junk095();
        new void Junk096();
        new void Junk097();
        new void Junk098();
        new void Junk099();
        new void Junk100();
        new void Junk101();
        new void Junk102();
        new void Junk103();
        new void Junk104();
        new void Junk105();
        new void Junk106();
        new void Junk107();
        new void Junk108();
        new void Junk109();
        new void Junk110();
        new void Junk111();
        new void Junk112();
        new void Junk113();
        new void Junk114();
        new void Junk115();
        new void Junk116();

        #endregion

        void Junk1();
        void Junk2();
        void Junk3();
        void Junk4();
        void Junk5();
        void Junk6();
        void Junk7();
        void Junk8();
        void Junk9();
        [PreserveSig]
        int CheckDeviceState(IntPtr hDestinationWindow);
        void Junk11();
        void Junk12();
        void Junk13();
        void Junk14();
        void Junk15();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("a0cade0f-06d5-4cf4-a1c7-f3cdd725aa75"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DDeviceManager9
    {
        void ResetDevice(
            [In]  IDirect3DDevice9 pDevice,
            [In]  int resetToken);

        void Junk2();
        void Junk3();
        void Junk4();
        void Junk5();
        void Junk6();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0CFBAF3A-9FF6-429a-99B3-A2796AF8B89B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DSurface9
    {
        void Junk1();
        void Junk2();
        void Junk3();
        void Junk4();
        void Junk5();
        void Junk6();
        void Junk7();
        void Junk8();
        void GetContainer(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppContainer);
        void Junk10();
        void Junk11();
        void Junk12();
        void Junk13();
        void Junk14();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("794950F2-ADFC-458a-905E-10A10B0B503B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DSwapChain9
    {
        void Present(
            [In, MarshalAs(UnmanagedType.LPStruct)] MFRect pSourceRect,
            [In, MarshalAs(UnmanagedType.LPStruct)] MFRect pDestRect,
            IntPtr hDestWindowOverride,
            [In, MarshalAs(UnmanagedType.LPStruct)] RGNDATA pDirtyRegion,
            int dwFlags
            );

        void Junk2();

        void GetBackBuffer(
            int iBackBuffer,
            D3DBACKBUFFER_TYPE Type,
            out IDirect3DSurface9 ppBackBuffer
            );

        void Junk4();
        void Junk5();
        void Junk6();
        void Junk7();
    }

    #endregion

}
