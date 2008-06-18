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

using MediaFoundation.Misc;

namespace EVRPresenter
{
    #region Externs

    public class D3DExtern
    {
        public const int D3D_SDK_VERSION = 32;

        [DllImport("D3D9.DLL", PreserveSig = false)]
        public extern static void Direct3DCreate9Ex(int SDKVersion, out IDirect3D9Ex a);

        [DllImport("dxva2.DLL", PreserveSig = false)]
        public extern static void DXVA2CreateDirect3DDeviceManager9(
            out int pResetToken,
            out IDirect3DDeviceManager9 ppDXVAManager
        );
    }

    #endregion

    #region Definitions

    public enum D3DCREATE
    {
        FPU_PRESERVE = 0x00000002,
        MULTITHREADED = 0x00000004,

        PUREDEVICE = 0x00000010,
        SOFTWARE_VERTEXPROCESSING = 0x00000020,
        HARDWARE_VERTEXPROCESSING = 0x00000040,
        MIXED_VERTEXPROCESSING = 0x00000080,

        DISABLE_DRIVER_MANAGEMENT = 0x00000100,
        ADAPTERGROUP_DEVICE = 0x00000200,
        DISABLE_DRIVER_MANAGEMENT_EX = 0x00000400,

        // This flag causes the D3D runtime not to alter the focus 
        // window in any way. Use with caution- the burden of supporting
        // focus management events (alt-tab, etc.) falls on the 
        // application, and appropriate responses (switching display
        // mode, etc.) should be coded.
        NOWINDOWCHANGES = 0x00000800,

        // Disable multithreading for software vertex processing
        DISABLE_PSGP_THREADING = 0x00002000,
        // This flag enables present statistics on device.
        ENABLE_PRESENTSTATS = 0x00004000,
        // This flag disables printscreen support in the runtime for this device
        DISABLE_PRINTSCREEN = 0x00008000,

        SCREENSAVER = 0x10000000
    }

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

    public enum D3DPRESENT_INTERVAL
    {
        DEFAULT = 0x00000000,
        ONE = 0x00000001,
        TWO = 0x00000002,
        THREE = 0x00000004,
        FOUR = 0x00000008,
        IMMEDIATE = unchecked((int)0x80000000)
    }

    public enum MONITOR_DEFAULTTO
    {
        NULL = 0x00000000,
        PRIMARY = 0x00000001,
        NEAREST = 0x00000002
    }

    public enum D3DDISPLAYROTATION
    {
    }

    public enum D3DCOMPOSERECTSOP
    {
    }

    public enum D3DQUERYTYPE
    {
    }

    public enum D3DSAMPLERSTATETYPE
    {
    }

    public enum D3DTEXTURESTAGESTATETYPE
    {
    }

    public enum D3DSTATEBLOCKTYPE
    {
    }

    public enum D3DRENDERSTATETYPE
    {
    }

    public enum D3DPRIMITIVETYPE
    {
    }

    public enum D3DTRANSFORMSTATETYPE
    {
    }

    public enum D3DTEXTUREFILTERTYPE
    {
    }

    public enum D3DPOOL
    {
    }

    public enum D3DBACKBUFFER_TYPE
    {
        D3DBACKBUFFER_TYPE_MONO = 0,
        D3DBACKBUFFER_TYPE_LEFT = 1,
        D3DBACKBUFFER_TYPE_RIGHT = 2,

        D3DBACKBUFFER_TYPE_FORCE_DWORD = 0x7fffffff
    }

    [Flags]
    public enum D3DPRESENTFLAG
    {
        None = 0,
        LOCKABLE_BACKBUFFER = 0x00000001,
        DISCARD_DEPTHSTENCIL = 0x00000002,
        DEVICECLIP = 0x00000004,
        VIDEO = 0x00000010,
        NOAUTOROTATE = 0x00000020,
        UNPRUNEDMODE = 0x00000040
    }

    public enum D3DRESOURCETYPE
    {
        D3DRTYPE_SURFACE = 1,
        D3DRTYPE_VOLUME = 2,
        D3DRTYPE_TEXTURE = 3,
        D3DRTYPE_VOLUMETEXTURE = 4,
        D3DRTYPE_CUBETEXTURE = 5,
        D3DRTYPE_VERTEXBUFFER = 6,
        D3DRTYPE_INDEXBUFFER = 7,           //if this changes, change _D3DDEVINFO_RESOURCEMANAGER definition


        D3DRTYPE_FORCE_DWORD = 0x7fffffff
    }

    public enum D3DError
    {
        D3D_OK = 0,

        D3DERR_WRONGTEXTUREFORMAT = unchecked((int)0x88760818),
        D3DERR_UNSUPPORTEDCOLOROPERATION,
        D3DERR_UNSUPPORTEDCOLORARG,
        D3DERR_UNSUPPORTEDALPHAOPERATION,
        D3DERR_UNSUPPORTEDALPHAARG,
        D3DERR_TOOMANYOPERATIONS,
        D3DERR_CONFLICTINGTEXTUREFILTER,
        D3DERR_UNSUPPORTEDFACTORVALUE,
        junk1,
        D3DERR_CONFLICTINGRENDERSTATE,
        D3DERR_UNSUPPORTEDTEXTUREFILTER,
        junk2,
        junk3,
        junk4,
        D3DERR_CONFLICTINGTEXTUREPALETTE,
        D3DERR_DRIVERINTERNALERROR,

        D3DERR_NOTFOUND = unchecked((int)0x88760866),
        D3DERR_MOREDATA,
        D3DERR_DEVICELOST,
        D3DERR_DEVICENOTRESET,
        D3DERR_NOTAVAILABLE,
        D3DERR_INVALIDDEVICE,
        D3DERR_INVALIDCALL,
        D3DERR_DRIVERINVALIDCALL,
        junk5,
        D3DOK_NOAUTOGEN,
        D3DERR_DEVICEREMOVED,
        junk6,
        junk7,
        junk8,
        D3DERR_DEVICEHUNG,
        S_NOT_RESIDENT = 0x8760875,
        S_RESIDENT_IN_SHARED_MEMORY,
        S_PRESENT_MODE_CHANGED,
        S_PRESENT_OCCLUDED,
    }

    public enum D3DDEVTYPE
    {
        D3DDEVTYPE_HAL = 1,
        D3DDEVTYPE_REF = 2,
        D3DDEVTYPE_SW = 3,

        D3DDEVTYPE_NULLREF = 4,

        D3DDEVTYPE_FORCE_DWORD = 0x7fffffff
    }

    public enum DeviceState
    {
        DeviceOK,
        DeviceReset,    // The device was reset OR re-created.
        DeviceRemoved,  // The device was removed.
    }

    public enum D3DMULTISAMPLE_TYPE
    {
        D3DMULTISAMPLE_NONE = 0,
        D3DMULTISAMPLE_NONMASKABLE = 1,
        D3DMULTISAMPLE_2_SAMPLES = 2,
        D3DMULTISAMPLE_3_SAMPLES = 3,
        D3DMULTISAMPLE_4_SAMPLES = 4,
        D3DMULTISAMPLE_5_SAMPLES = 5,
        D3DMULTISAMPLE_6_SAMPLES = 6,
        D3DMULTISAMPLE_7_SAMPLES = 7,
        D3DMULTISAMPLE_8_SAMPLES = 8,
        D3DMULTISAMPLE_9_SAMPLES = 9,
        D3DMULTISAMPLE_10_SAMPLES = 10,
        D3DMULTISAMPLE_11_SAMPLES = 11,
        D3DMULTISAMPLE_12_SAMPLES = 12,
        D3DMULTISAMPLE_13_SAMPLES = 13,
        D3DMULTISAMPLE_14_SAMPLES = 14,
        D3DMULTISAMPLE_15_SAMPLES = 15,
        D3DMULTISAMPLE_16_SAMPLES = 16,

        D3DMULTISAMPLE_FORCE_DWORD = 0x7fffffff
    }

    public enum D3DSWAPEFFECT
    {
        D3DSWAPEFFECT_DISCARD = 1,
        D3DSWAPEFFECT_FLIP = 2,
        D3DSWAPEFFECT_COPY = 3,

        D3DSWAPEFFECT_FORCE_DWORD = 0x7fffffff
    }

    public enum D3DFORMAT
    {
        D3DFMT_UNKNOWN = 0,

        D3DFMT_R8G8B8 = 20,
        D3DFMT_A8R8G8B8 = 21,
        D3DFMT_X8R8G8B8 = 22,
        D3DFMT_R5G6B5 = 23,
        D3DFMT_X1R5G5B5 = 24,
        D3DFMT_A1R5G5B5 = 25,
        D3DFMT_A4R4G4B4 = 26,
        D3DFMT_R3G3B2 = 27,
        D3DFMT_A8 = 28,
        D3DFMT_A8R3G3B2 = 29,
        D3DFMT_X4R4G4B4 = 30,
        D3DFMT_A2B10G10R10 = 31,
        D3DFMT_A8B8G8R8 = 32,
        D3DFMT_X8B8G8R8 = 33,
        D3DFMT_G16R16 = 34,
        D3DFMT_A2R10G10B10 = 35,
        D3DFMT_A16B16G16R16 = 36,

        D3DFMT_A8P8 = 40,
        D3DFMT_P8 = 41,

        D3DFMT_L8 = 50,
        D3DFMT_A8L8 = 51,
        D3DFMT_A4L4 = 52,

        D3DFMT_V8U8 = 60,
        D3DFMT_L6V5U5 = 61,
        D3DFMT_X8L8V8U8 = 62,
        D3DFMT_Q8W8V8U8 = 63,
        D3DFMT_V16U16 = 64,
        D3DFMT_A2W10V10U10 = 67,

        D3DFMT_UYVY = 1498831189, //new FourCC('U', 'Y', 'V', 'Y').ToInt32(),
        D3DFMT_R8G8_B8G8 = 1195525970, //MAKEFOURCC('R', 'G', 'B', 'G'),
        D3DFMT_YUY2 = 844715353, //MAKEFOURCC('Y', 'U', 'Y', '2'),
        D3DFMT_G8R8_G8B8 = 1111970375, //MAKEFOURCC('G', 'R', 'G', 'B'),
        D3DFMT_DXT1 = 827611204, //MAKEFOURCC('D', 'X', 'T', '1'),
        D3DFMT_DXT2 = 844388420, //MAKEFOURCC('D', 'X', 'T', '2'),
        D3DFMT_DXT3 = 861165636, //MAKEFOURCC('D', 'X', 'T', '3'),
        D3DFMT_DXT4 = 877942852, //MAKEFOURCC('D', 'X', 'T', '4'),
        D3DFMT_DXT5 = 894720068, //MAKEFOURCC('D', 'X', 'T', '5'),

        D3DFMT_D16_LOCKABLE = 70,
        D3DFMT_D32 = 71,
        D3DFMT_D15S1 = 73,
        D3DFMT_D24S8 = 75,
        D3DFMT_D24X8 = 77,
        D3DFMT_D24X4S4 = 79,
        D3DFMT_D16 = 80,

        D3DFMT_D32F_LOCKABLE = 82,
        D3DFMT_D24FS8 = 83,

        /* Z-Stencil formats valid for CPU access */
        D3DFMT_D32_LOCKABLE = 84,
        D3DFMT_S8_LOCKABLE = 85,



        D3DFMT_L16 = 81,

        D3DFMT_VERTEXDATA = 100,
        D3DFMT_INDEX16 = 101,
        D3DFMT_INDEX32 = 102,

        D3DFMT_Q16W16V16U16 = 110,

        D3DFMT_MULTI2_ARGB8 = 827606349, //MAKEFOURCC('M','E','T','1'),

        // Floating point surface formats

        // s10e5 formats (16-bits per channel)
        D3DFMT_R16F = 111,
        D3DFMT_G16R16F = 112,
        D3DFMT_A16B16G16R16F = 113,

        // IEEE s23e8 formats (32-bits per channel)
        D3DFMT_R32F = 114,
        D3DFMT_G32R32F = 115,
        D3DFMT_A32B32G32R32F = 116,

        D3DFMT_CxV8U8 = 117,

        // Monochrome 1 bit per pixel format
        D3DFMT_A1 = 118,


        // Binary format indicating that the data has no inherent type
        D3DFMT_BINARYBUFFER = 199,


        D3DFMT_FORCE_DWORD = 0x7fffffff
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DLOCKED_RECT
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DSURFACE_DESC
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DDISPLAYMODEFILTER
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DRECTPATCH_INFO
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DTRIPATCH_INFO
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DVERTEXELEMENT9
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PALETTEENTRY
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DCLIPSTATUS9
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DLIGHT9
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DMATERIAL9
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DVIEWPORT9
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DMATRIX
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DRECT
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DGAMMARAMP
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DRASTER_STATUS
    {
        public bool InVBlank;
        public int ScanLine;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DVSHADERCAPS2_0
    {
        public int Caps;
        public int DynamicFlowControlDepth;
        public int NumTemps;
        public int StaticFlowControlDepth;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DPSHADERCAPS2_0
    {
        public int Caps;
        public int DynamicFlowControlDepth;
        public int NumTemps;
        public int StaticFlowControlDepth;
        public int NumInstructionSlots;
    }

    [StructLayout(LayoutKind.Sequential)]
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

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DDEVICE_CREATION_PARAMETERS
    {
        public int AdapterOrdinal;
        public D3DDEVTYPE DeviceType;
        public IntPtr hFocusWindow;
        public int BehaviorFlags;
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("D3DPRESENT_PARAMETERS")]
    public struct D3DPRESENT_PARAMETERS
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct D3DADAPTER_IDENTIFIER9
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public char Driver;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public char Description;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public char DeviceName;         /* Device name for GDI (ex. \\.\DISPLAY1) */

        public long DriverVersion;          /* Defined for 32 bit components */

        public int VendorId;
        public int DeviceId;
        public int SubSysId;
        public int Revision;

        public Guid DeviceIdentifier;

        public int WHQLLevel;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DDISPLAYMODE
    {
        public int Width;
        public int Height;
        public int RefreshRate;
        public D3DFORMAT Format;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class D3DDISPLAYMODEEX
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public class RGNDATA
    {
    }

    #endregion

    #region Interfaces

    public interface IDirect3DQuery9
    {
    }

    public interface IDirect3DResource9
    {
    }

    public interface IDirect3DPixelShader9
    {
    }

    public interface IDirect3DVertexShader9
    {
    }

    public interface IDirect3DVertexDeclaration9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("02177241-69FC-400C-8FF1-93A44DF6861D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3D9Ex : IDirect3D9
    {
        #region IDirect3D9

        new void RegisterSoftwareDevice(IntPtr pInitializeFunction);
        new int GetAdapterCount();
        new void GetAdapterIdentifier(int Adapter, int Flags, out D3DADAPTER_IDENTIFIER9 pIdentifier);
        new int GetAdapterModeCount(int Adapter, D3DFORMAT Format);
        new void EnumAdapterModes(int Adapter, D3DFORMAT Format, int Mode, out D3DDISPLAYMODE pMode);
        new void GetAdapterDisplayMode(int Adapter, out D3DDISPLAYMODE pMode);
        new void CheckDeviceType(int Adapter, D3DDEVTYPE DevType, D3DFORMAT AdapterFormat, D3DFORMAT BackBufferFormat, [MarshalAs(UnmanagedType.Bool)] bool bWindowed);
        new void CheckDeviceFormat(int Adapter, D3DDEVTYPE DeviceType, D3DFORMAT AdapterFormat, int Usage, D3DRESOURCETYPE RType, D3DFORMAT CheckFormat);
        new void CheckDeviceMultiSampleType(int Adapter, D3DDEVTYPE DeviceType, D3DFORMAT SurfaceFormat, [MarshalAs(UnmanagedType.Bool)] bool Windowed, D3DMULTISAMPLE_TYPE MultiSampleType, out int pQualityLevels);
        new void CheckDepthStencilMatch(int Adapter, D3DDEVTYPE DeviceType, D3DFORMAT AdapterFormat, D3DFORMAT RenderTargetFormat, D3DFORMAT DepthStencilFormat);
        new void CheckDeviceFormatConversion(int Adapter, D3DDEVTYPE DeviceType, D3DFORMAT SourceFormat, D3DFORMAT TargetFormat);
        new void GetDeviceCaps(int Adapter, D3DDEVTYPE DeviceType, out D3DCAPS9 pCaps);
        new IntPtr GetAdapterMonitor(int Adapter);
        new void CreateDevice(int Adapter, D3DDEVTYPE DeviceType, IntPtr hFocusWindow, int BehaviorFlags, D3DPRESENT_PARAMETERS pPresentationParameters, out IDirect3DDevice9 ppReturnedDeviceInterface);

        #endregion

        int GetAdapterModeCountEx(int Adapter, out D3DDISPLAYMODEFILTER pFilter);
        void EnumAdapterModesEx(int Adapter, D3DDISPLAYMODEFILTER pFilter, int Mode, out D3DDISPLAYMODEEX pMode);
        void GetAdapterDisplayModeEx(int Adapter, out D3DDISPLAYMODEEX pMode, out D3DDISPLAYROTATION pRotation);
        void CreateDeviceEx(int Adapter, D3DDEVTYPE DeviceType, IntPtr hFocusWindow, D3DCREATE BehaviorFlags, ref D3DPRESENT_PARAMETERS pPresentationParameters, D3DDISPLAYMODEEX pFullscreenDisplayMode, out IDirect3DDevice9Ex ppReturnedDeviceInterface);
        void GetAdapterLUID(int Adapter, LUID pLUID);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("85C31227-3DE5-4f00-9B3A-F11AC38C18B5"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DTexture9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2518526C-E789-4111-A7B9-47EF328D13E6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DVolumeTexture9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FFF32F81-D953-473a-9223-93D652ABA93F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DCubeTexture9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B64BB1B5-FD70-4df6-BF91-19D0A12455E3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DVertexBuffer9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7C9DD65E-D3F7-4529-ACEE-785830ACDE35"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DIndexBuffer9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("580CA87E-1D3C-4d54-991D-B7D3E3C298CE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DBaseTexture9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B07C4FE5-310D-4ba8-A23C-4F0F206F218B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DStateBlock9
    {
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("D0223B96-BF7A-43fd-92BD-A43B0D82B9EB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DDevice9
    {
        void TestCooperativeLevel();
        int GetAvailableTextureMem();
        void EvictManagedResources();
        void GetDirect3D(out IDirect3D9 ppD3D9);
        void GetDeviceCaps(out D3DCAPS9 pCaps);
        void GetDisplayMode(int iSwapChain, out D3DDISPLAYMODE pMode);
        void GetCreationParameters(out D3DDEVICE_CREATION_PARAMETERS pParameters);
        void SetCursorProperties(int XHotSpot, int YHotSpot, IDirect3DSurface9 pCursorBitmap);
        void SetCursorPosition(int X, int Y, int Flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);
        void CreateAdditionalSwapChain(D3DPRESENT_PARAMETERS pPresentationParameters, out IDirect3DSwapChain9 pSwapChain);
        void GetSwapChain(int iSwapChain, out IDirect3DSwapChain9 pSwapChain);
        int GetNumberOfSwapChains();
        void Reset(D3DPRESENT_PARAMETERS pPresentationParameters);
        void Present(RECT pSourceRect, RECT pDestRect, IntPtr hDestWindowOverride, RGNDATA pDirtyRegion);
        void GetBackBuffer(int iSwapChain, int iBackBuffer, D3DBACKBUFFER_TYPE Type, out IDirect3DSurface9 ppBackBuffer);
        void GetRasterStatus(int iSwapChain, out D3DRASTER_STATUS pRasterStatus);
        void SetDialogBoxMode([MarshalAs(UnmanagedType.Bool)] bool bEnableDialogs);
        void SetGammaRamp(int iSwapChain, int Flags, D3DGAMMARAMP pRamp);
        void GetGammaRamp(int iSwapChain, out D3DGAMMARAMP pRamp);
        void CreateTexture(int Width, int Height, int Levels, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DTexture9 ppTexture, out IntPtr pSharedHandle);
        void CreateVolumeTexture(int Width, int Height, int Depth, int Levels, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DVolumeTexture9 ppVolumeTexture, out IntPtr pSharedHandle);
        void CreateCubeTexture(int EdgeLength, int Levels, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DCubeTexture9 ppCubeTexture, out IntPtr pSharedHandle);
        void CreateVertexBuffer(int Length, int Usage, int FVF, D3DPOOL Pool, out IDirect3DVertexBuffer9 ppVertexBuffer, out IntPtr pSharedHandle);
        void CreateIndexBuffer(int Length, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DIndexBuffer9 ppIndexBuffer, out IntPtr pSharedHandle);
        void CreateRenderTarget(int Width, int Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, int MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Lockable, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle);
        void CreateDepthStencilSurface(int Width, int Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, int MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Discard, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle);
        void UpdateSurface(IDirect3DSurface9 pSourceSurface, RECT pSourceRect, IDirect3DSurface9 pDestinationSurface, POINT pDestPoint);
        void UpdateTexture(IDirect3DBaseTexture9 pSourceTexture, IDirect3DBaseTexture9 pDestinationTexture);
        void GetRenderTargetData(IDirect3DSurface9 pRenderTarget, IDirect3DSurface9 pDestSurface);
        void GetFrontBufferData(int iSwapChain, IDirect3DSurface9 pDestSurface);
        void StretchRect(IDirect3DSurface9 pSourceSurface, RECT pSourceRect, IDirect3DSurface9 pDestSurface, RECT pDestRect, D3DTEXTUREFILTERTYPE Filter);
        void ColorFill(IDirect3DSurface9 pSurface, RECT pRect, int color);
        void CreateOffscreenPlainSurface(int Width, int Height, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle);
        void SetRenderTarget(int RenderTargetIndex, IDirect3DSurface9 pRenderTarget);
        void GetRenderTarget(int RenderTargetIndex, out IDirect3DSurface9 ppRenderTarget);
        void SetDepthStencilSurface(IDirect3DSurface9 pNewZStencil);
        void GetDepthStencilSurface(out IDirect3DSurface9 ppZStencilSurface);
        void BeginScene();
        void EndScene();
        void Clear(int Count, D3DRECT pRects, int Flags, int Color, float Z, int Stencil);
        void SetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX pMatrix);
        void GetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX pMatrix);
        void MultiplyTransform(D3DTRANSFORMSTATETYPE a, D3DMATRIX b);
        void SetViewport(D3DVIEWPORT9 pViewport);
        void GetViewport(D3DVIEWPORT9 pViewport);
        void SetMaterial(D3DMATERIAL9 pMaterial);
        void GetMaterial(D3DMATERIAL9 pMaterial);
        void SetLight(int Index, D3DLIGHT9 a);
        void GetLight(int Index, D3DLIGHT9 a);
        void LightEnable(int Index, [MarshalAs(UnmanagedType.Bool)] bool Enable);
        void GetLightEnable(int Index, [MarshalAs(UnmanagedType.Bool)] out bool pEnable);
        void SetClipPlane(int Index, float pPlane);
        void GetClipPlane(int Index, out float pPlane);
        void SetRenderState(D3DRENDERSTATETYPE State, int Value);
        void GetRenderState(D3DRENDERSTATETYPE State, out int pValue);
        void CreateStateBlock(D3DSTATEBLOCKTYPE Type, out IDirect3DStateBlock9 ppSB);
        void BeginStateBlock();
        void EndStateBlock(out IDirect3DStateBlock9 ppSB);
        void SetClipStatus(D3DCLIPSTATUS9 pClipStatus);
        void GetClipStatus(out D3DCLIPSTATUS9 pClipStatus);
        void GetTexture(int Stage, out IDirect3DBaseTexture9 ppTexture);
        void SetTexture(int Stage, IDirect3DBaseTexture9 pTexture);
        void GetTextureStageState(int Stage, D3DTEXTURESTAGESTATETYPE Type, out int pValue);
        void SetTextureStageState(int Stage, D3DTEXTURESTAGESTATETYPE Type, int Value);
        void GetSamplerState(int Sampler, D3DSAMPLERSTATETYPE Type, out int pValue);
        void SetSamplerState(int Sampler, D3DSAMPLERSTATETYPE Type, int Value);
        void ValidateDevice(ref int pNumPasses);
        void SetPaletteEntries(int PaletteNumber, PALETTEENTRY pEntries);
        void GetPaletteEntries(int PaletteNumber, PALETTEENTRY pEntries);
        void SetCurrentTexturePalette(int PaletteNumber);
        void GetCurrentTexturePalette(out int PaletteNumber);
        void SetScissorRect(RECT pRect);
        void GetScissorRect(out RECT pRect);
        void SetSoftwareVertexProcessing([MarshalAs(UnmanagedType.Bool)] bool bSoftware);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetSoftwareVertexProcessing();
        void SetNPatchMode(float nSegments);
        float GetNPatchMode();
        void DrawPrimitive(D3DPRIMITIVETYPE PrimitiveType, int StartVertex, int PrimitiveCount);
        void DrawIndexedPrimitive(D3DPRIMITIVETYPE a, int BaseVertexIndex, int MinVertexIndex, int NumVertices, int startIndex, int primCount);
        void DrawPrimitiveUP(D3DPRIMITIVETYPE PrimitiveType, int PrimitiveCount, IntPtr pVertexStreamZeroData, int VertexStreamZeroStride);
        void DrawIndexedPrimitiveUP(D3DPRIMITIVETYPE PrimitiveType, int MinVertexIndex, int NumVertices, int PrimitiveCount, IntPtr pIndexData, D3DFORMAT IndexDataFormat, IntPtr pVertexStreamZeroData, int VertexStreamZeroStride);
        void ProcessVertices(int SrcStartIndex, int DestIndex, int VertexCount, IDirect3DVertexBuffer9 pDestBuffer, IDirect3DVertexDeclaration9 pVertexDecl, int Flags);
        void CreateVertexDeclaration(D3DVERTEXELEMENT9 pVertexElements, out IDirect3DVertexDeclaration9 ppDecl);
        void SetVertexDeclaration(IDirect3DVertexDeclaration9 pDecl);
        void GetVertexDeclaration(out IDirect3DVertexDeclaration9 ppDecl);
        void SetFVF(int FVF);
        void GetFVF(int pFVF);
        void CreateVertexShader(int pFunction, out IDirect3DVertexShader9 ppShader);
        void SetVertexShader(IDirect3DVertexShader9 pShader);
        void GetVertexShader(out IDirect3DVertexShader9 ppShader);
        void SetVertexShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        void GetVertexShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        void SetVertexShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        void GetVertexShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        void SetVertexShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        void GetVertexShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        void SetStreamSource(int StreamNumber, IDirect3DVertexBuffer9 pStreamData, int OffsetInBytes, int Stride);
        void GetStreamSource(int StreamNumber, out IDirect3DVertexBuffer9 ppStreamData, int pOffsetInBytes, int pStride);
        void SetStreamSourceFreq(int StreamNumber, int Setting);
        void GetStreamSourceFreq(int StreamNumber, int pSetting);
        void SetIndices(IDirect3DIndexBuffer9 pIndexData);
        void GetIndices(out IDirect3DIndexBuffer9 ppIndexData);
        void CreatePixelShader(int pFunction, out IDirect3DPixelShader9 ppShader);
        void SetPixelShader(IDirect3DPixelShader9 pShader);
        void GetPixelShader(out IDirect3DPixelShader9 ppShader);
        void SetPixelShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        void GetPixelShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        void SetPixelShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        void GetPixelShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        void SetPixelShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        void GetPixelShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        void DrawRectPatch(int Handle, float pNumSegs, D3DRECTPATCH_INFO pRectPatchInfo);
        void DrawTriPatch(int Handle, float pNumSegs, D3DTRIPATCH_INFO pTriPatchInfo);
        void DeletePatch(int Handle);
        void CreateQuery(D3DQUERYTYPE Type, out IDirect3DQuery9 ppQuery);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("B18B10CE-2649-405a-870F-95F777D4313A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DDevice9Ex : IDirect3DDevice9
    {
        #region IDirect3DDevice9

        new void TestCooperativeLevel();
        new int GetAvailableTextureMem();
        new void EvictManagedResources();
        new void GetDirect3D(out IDirect3D9 ppD3D9);
        new void GetDeviceCaps(out D3DCAPS9 pCaps);
        new void GetDisplayMode(int iSwapChain, out D3DDISPLAYMODE pMode);
        new void GetCreationParameters(out D3DDEVICE_CREATION_PARAMETERS pParameters);
        new void SetCursorProperties(int XHotSpot, int YHotSpot, IDirect3DSurface9 pCursorBitmap);
        new void SetCursorPosition(int X, int Y, int Flags);
        [return: MarshalAs(UnmanagedType.Bool)]
        new bool ShowCursor([MarshalAs(UnmanagedType.Bool)] bool bShow);
        new void CreateAdditionalSwapChain(D3DPRESENT_PARAMETERS pPresentationParameters, out IDirect3DSwapChain9 pSwapChain);
        new void GetSwapChain(int iSwapChain, out IDirect3DSwapChain9 pSwapChain);
        new int GetNumberOfSwapChains();
        new void Reset(D3DPRESENT_PARAMETERS pPresentationParameters);
        new void Present(RECT pSourceRect, RECT pDestRect, IntPtr hDestWindowOverride, RGNDATA pDirtyRegion);
        new void GetBackBuffer(int iSwapChain, int iBackBuffer, D3DBACKBUFFER_TYPE Type, out IDirect3DSurface9 ppBackBuffer);
        new void GetRasterStatus(int iSwapChain, out D3DRASTER_STATUS pRasterStatus);
        new void SetDialogBoxMode([MarshalAs(UnmanagedType.Bool)] bool bEnableDialogs);
        new void SetGammaRamp(int iSwapChain, int Flags, D3DGAMMARAMP pRamp);
        new void GetGammaRamp(int iSwapChain, out D3DGAMMARAMP pRamp);
        new void CreateTexture(int Width, int Height, int Levels, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DTexture9 ppTexture, out IntPtr pSharedHandle);
        new void CreateVolumeTexture(int Width, int Height, int Depth, int Levels, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DVolumeTexture9 ppVolumeTexture, out IntPtr pSharedHandle);
        new void CreateCubeTexture(int EdgeLength, int Levels, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DCubeTexture9 ppCubeTexture, out IntPtr pSharedHandle);
        new void CreateVertexBuffer(int Length, int Usage, int FVF, D3DPOOL Pool, out IDirect3DVertexBuffer9 ppVertexBuffer, out IntPtr pSharedHandle);
        new void CreateIndexBuffer(int Length, int Usage, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DIndexBuffer9 ppIndexBuffer, out IntPtr pSharedHandle);
        new void CreateRenderTarget(int Width, int Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, int MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Lockable, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle);
        new void CreateDepthStencilSurface(int Width, int Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, int MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Discard, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle);
        new void UpdateSurface(IDirect3DSurface9 pSourceSurface, RECT pSourceRect, IDirect3DSurface9 pDestinationSurface, POINT pDestPoint);
        new void UpdateTexture(IDirect3DBaseTexture9 pSourceTexture, IDirect3DBaseTexture9 pDestinationTexture);
        new void GetRenderTargetData(IDirect3DSurface9 pRenderTarget, IDirect3DSurface9 pDestSurface);
        new void GetFrontBufferData(int iSwapChain, IDirect3DSurface9 pDestSurface);
        new void StretchRect(IDirect3DSurface9 pSourceSurface, RECT pSourceRect, IDirect3DSurface9 pDestSurface, RECT pDestRect, D3DTEXTUREFILTERTYPE Filter);
        new void ColorFill(IDirect3DSurface9 pSurface, RECT pRect, int color);
        new void CreateOffscreenPlainSurface(int Width, int Height, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle);
        new void SetRenderTarget(int RenderTargetIndex, IDirect3DSurface9 pRenderTarget);
        new void GetRenderTarget(int RenderTargetIndex, out IDirect3DSurface9 ppRenderTarget);
        new void SetDepthStencilSurface(IDirect3DSurface9 pNewZStencil);
        new void GetDepthStencilSurface(out IDirect3DSurface9 ppZStencilSurface);
        new void BeginScene();
        new void EndScene();
        new void Clear(int Count, D3DRECT pRects, int Flags, int Color, float Z, int Stencil);
        new void SetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX pMatrix);
        new void GetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX pMatrix);
        new void MultiplyTransform(D3DTRANSFORMSTATETYPE a, D3DMATRIX b);
        new void SetViewport(D3DVIEWPORT9 pViewport);
        new void GetViewport(D3DVIEWPORT9 pViewport);
        new void SetMaterial(D3DMATERIAL9 pMaterial);
        new void GetMaterial(D3DMATERIAL9 pMaterial);
        new void SetLight(int Index, D3DLIGHT9 a);
        new void GetLight(int Index, D3DLIGHT9 a);
        new void LightEnable(int Index, [MarshalAs(UnmanagedType.Bool)] bool Enable);
        new void GetLightEnable(int Index, [MarshalAs(UnmanagedType.Bool)] out bool pEnable);
        new void SetClipPlane(int Index, float pPlane);
        new void GetClipPlane(int Index, out float pPlane);
        new void SetRenderState(D3DRENDERSTATETYPE State, int Value);
        new void GetRenderState(D3DRENDERSTATETYPE State, out int pValue);
        new void CreateStateBlock(D3DSTATEBLOCKTYPE Type, out IDirect3DStateBlock9 ppSB);
        new void BeginStateBlock();
        new void EndStateBlock(out IDirect3DStateBlock9 ppSB);
        new void SetClipStatus(D3DCLIPSTATUS9 pClipStatus);
        new void GetClipStatus(out D3DCLIPSTATUS9 pClipStatus);
        new void GetTexture(int Stage, out IDirect3DBaseTexture9 ppTexture);
        new void SetTexture(int Stage, IDirect3DBaseTexture9 pTexture);
        new void GetTextureStageState(int Stage, D3DTEXTURESTAGESTATETYPE Type, out int pValue);
        new void SetTextureStageState(int Stage, D3DTEXTURESTAGESTATETYPE Type, int Value);
        new void GetSamplerState(int Sampler, D3DSAMPLERSTATETYPE Type, out int pValue);
        new void SetSamplerState(int Sampler, D3DSAMPLERSTATETYPE Type, int Value);
        new void ValidateDevice(ref int pNumPasses);
        new void SetPaletteEntries(int PaletteNumber, PALETTEENTRY pEntries);
        new void GetPaletteEntries(int PaletteNumber, PALETTEENTRY pEntries);
        new void SetCurrentTexturePalette(int PaletteNumber);
        new void GetCurrentTexturePalette(out int PaletteNumber);
        new void SetScissorRect(RECT pRect);
        new void GetScissorRect(out RECT pRect);
        new void SetSoftwareVertexProcessing([MarshalAs(UnmanagedType.Bool)] bool bSoftware);
        [return: MarshalAs(UnmanagedType.Bool)]
        new bool GetSoftwareVertexProcessing();
        new void SetNPatchMode(float nSegments);
        new float GetNPatchMode();
        new void DrawPrimitive(D3DPRIMITIVETYPE PrimitiveType, int StartVertex, int PrimitiveCount);
        new void DrawIndexedPrimitive(D3DPRIMITIVETYPE a, int BaseVertexIndex, int MinVertexIndex, int NumVertices, int startIndex, int primCount);
        new void DrawPrimitiveUP(D3DPRIMITIVETYPE PrimitiveType, int PrimitiveCount, IntPtr pVertexStreamZeroData, int VertexStreamZeroStride);
        new void DrawIndexedPrimitiveUP(D3DPRIMITIVETYPE PrimitiveType, int MinVertexIndex, int NumVertices, int PrimitiveCount, IntPtr pIndexData, D3DFORMAT IndexDataFormat, IntPtr pVertexStreamZeroData, int VertexStreamZeroStride);
        new void ProcessVertices(int SrcStartIndex, int DestIndex, int VertexCount, IDirect3DVertexBuffer9 pDestBuffer, IDirect3DVertexDeclaration9 pVertexDecl, int Flags);
        new void CreateVertexDeclaration(D3DVERTEXELEMENT9 pVertexElements, out IDirect3DVertexDeclaration9 ppDecl);
        new void SetVertexDeclaration(IDirect3DVertexDeclaration9 pDecl);
        new void GetVertexDeclaration(out IDirect3DVertexDeclaration9 ppDecl);
        new void SetFVF(int FVF);
        new void GetFVF(int pFVF);
        new void CreateVertexShader(int pFunction, out IDirect3DVertexShader9 ppShader);
        new void SetVertexShader(IDirect3DVertexShader9 pShader);
        new void GetVertexShader(out IDirect3DVertexShader9 ppShader);
        new void SetVertexShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        new void GetVertexShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        new void SetVertexShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        new void GetVertexShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        new void SetVertexShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        new void GetVertexShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        new void SetStreamSource(int StreamNumber, IDirect3DVertexBuffer9 pStreamData, int OffsetInBytes, int Stride);
        new void GetStreamSource(int StreamNumber, out IDirect3DVertexBuffer9 ppStreamData, int pOffsetInBytes, int pStride);
        new void SetStreamSourceFreq(int StreamNumber, int Setting);
        new void GetStreamSourceFreq(int StreamNumber, int pSetting);
        new void SetIndices(IDirect3DIndexBuffer9 pIndexData);
        new void GetIndices(out IDirect3DIndexBuffer9 ppIndexData);
        new void CreatePixelShader(int pFunction, out IDirect3DPixelShader9 ppShader);
        new void SetPixelShader(IDirect3DPixelShader9 pShader);
        new void GetPixelShader(out IDirect3DPixelShader9 ppShader);
        new void SetPixelShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        new void GetPixelShaderConstantF(int StartRegister, float pConstantData, int Vector4fCount);
        new void SetPixelShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        new void GetPixelShaderConstantI(int StartRegister, int pConstantData, int Vector4iCount);
        new void SetPixelShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        new void GetPixelShaderConstantB(int StartRegister, [MarshalAs(UnmanagedType.Bool)] bool pConstantData, int BoolCount);
        new void DrawRectPatch(int Handle, float pNumSegs, D3DRECTPATCH_INFO pRectPatchInfo);
        new void DrawTriPatch(int Handle, float pNumSegs, D3DTRIPATCH_INFO pTriPatchInfo);
        new void DeletePatch(int Handle);
        new void CreateQuery(D3DQUERYTYPE Type, out IDirect3DQuery9 ppQuery);

        #endregion

        void SetConvolutionMonoKernel(int width, int height, float rows, float columns);
        void ComposeRects(IDirect3DSurface9 pSrc, IDirect3DSurface9 pDst, IDirect3DVertexBuffer9 pSrcRectDescs, int NumRects, IDirect3DVertexBuffer9 pDstRectDescs, D3DCOMPOSERECTSOP Operation, int Xoffset, int Yoffset);
        void PresentEx(RECT pSourceRect, RECT pDestRect, IntPtr hDestWindowOverride, RGNDATA pDirtyRegion, int dwFlags);
        void GetGPUThreadPriority(int pPriority);
        void SetGPUThreadPriority(int Priority);
        void WaitForVBlank(int iSwapChain);
        void CheckResourceResidency(out IDirect3DResource9 pResourceArray, int NumResources);
        void SetMaximumFrameLatency(int MaxLatency);
        void GetMaximumFrameLatency(int pMaxLatency);
        [PreserveSig]
        int CheckDeviceState(IntPtr hDestinationWindow);
        void CreateRenderTargetEx(int Width, int Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, int MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Lockable, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle, int Usage);
        void CreateOffscreenPlainSurfaceEx(int Width, int Height, D3DFORMAT Format, D3DPOOL Pool, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle, int Usage);
        void CreateDepthStencilSurfaceEx(int Width, int Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, int MultisampleQuality, [MarshalAs(UnmanagedType.Bool)] bool Discard, out IDirect3DSurface9 ppSurface, out IntPtr pSharedHandle, int Usage);
        void ResetEx(D3DPRESENT_PARAMETERS pPresentationParameters, D3DDISPLAYMODEEX pFullscreenDisplayMode);
        void GetDisplayModeEx(int iSwapChain, D3DDISPLAYMODEEX pMode, D3DDISPLAYROTATION pRotation);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("a0cade0f-06d5-4cf4-a1c7-f3cdd725aa75"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DDeviceManager9
    {
        void ResetDevice(
            [In]  IDirect3DDevice9 pDevice,
            [In]  int resetToken);

        void OpenDeviceHandle(
            out IntPtr phDevice);

        void CloseDeviceHandle(
            [In]  IntPtr hDevice);

        void TestDevice(
            [In]  IntPtr hDevice);

        void LockDevice(
            [In]  IntPtr hDevice,
            out IDirect3DDevice9 ppDevice,
            [In, MarshalAs(UnmanagedType.Bool)]  bool fBlock);

        void UnlockDevice(
            [In]  IntPtr hDevice,
            [In, MarshalAs(UnmanagedType.Bool)]  bool fSaveState);

        void GetVideoService(
            [In]  IntPtr hDevice,
            [In]  Guid riid,
            out object ppService);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0CFBAF3A-9FF6-429a-99B3-A2796AF8B89B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DSurface9
    {
        void GetDevice(out IDirect3DDevice9 ppDevice);
        void SetPrivateData(Guid refguid, IntPtr pData, int SizeOfData, int Flags);
        void GetPrivateData(Guid refguid, IntPtr pData, out int pSizeOfData);
        void FreePrivateData(Guid refguid);
        [PreserveSig]
        int SetPriority(int PriorityNew);
        [PreserveSig]
        int GetPriority();
        void PreLoad();
        [PreserveSig]
        D3DRESOURCETYPE GetType();
        void GetContainer(Guid riid, out object ppContainer);
        void GetDesc(out D3DSURFACE_DESC pDesc);
        void LockRect(D3DLOCKED_RECT pLockedRect, RECT pRect, int Flags);
        void UnlockRect();
        void GetDC(out IntPtr phdc);
        void ReleaseDC(IntPtr hdc);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("81BDCBCA-64D4-426d-AE8D-AD0147F4275C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3D9
    {
        void RegisterSoftwareDevice(
            IntPtr pInitializeFunction
            );

        [PreserveSig]
        int GetAdapterCount();

        void GetAdapterIdentifier(
            int Adapter,
            int Flags,
            out D3DADAPTER_IDENTIFIER9 pIdentifier
            );

        [PreserveSig]
        int GetAdapterModeCount(
            int Adapter,
            D3DFORMAT Format
            );

        void EnumAdapterModes(
            int Adapter,
            D3DFORMAT Format,
            int Mode,
            out D3DDISPLAYMODE pMode
            );

        void GetAdapterDisplayMode(
            int Adapter,
            out D3DDISPLAYMODE pMode
            );

        void CheckDeviceType(
            int Adapter,
            D3DDEVTYPE DevType,
            D3DFORMAT AdapterFormat,
            D3DFORMAT BackBufferFormat,
            [MarshalAs(UnmanagedType.Bool)] bool bWindowed
            );

        void CheckDeviceFormat(
            int Adapter,
            D3DDEVTYPE DeviceType,
            D3DFORMAT AdapterFormat,
            int Usage,
            D3DRESOURCETYPE RType,
            D3DFORMAT CheckFormat
            );

        void CheckDeviceMultiSampleType(
            int Adapter,
            D3DDEVTYPE DeviceType,
            D3DFORMAT SurfaceFormat,
            [MarshalAs(UnmanagedType.Bool)] bool bWindowed,
            D3DMULTISAMPLE_TYPE MultiSampleType,
            out int pQualityLevels
            );

        void CheckDepthStencilMatch(
            int Adapter,
            D3DDEVTYPE DeviceType,
            D3DFORMAT AdapterFormat,
            D3DFORMAT RenderTargetFormat,
            D3DFORMAT DepthStencilFormat
            );

        void CheckDeviceFormatConversion(
            int Adapter,
            D3DDEVTYPE DeviceType,
            D3DFORMAT SourceFormat,
            D3DFORMAT TargetFormat
            );

        void GetDeviceCaps(
            int Adapter,
            D3DDEVTYPE DeviceType,
            out D3DCAPS9 pCaps
            );

        [PreserveSig]
        IntPtr GetAdapterMonitor(
            int Adapter
            );

        void CreateDevice(
            int Adapter,
            D3DDEVTYPE DeviceType, IntPtr hFocusWindow,
            int BehaviorFlags,
            D3DPRESENT_PARAMETERS pPresentationParameters,
            out IDirect3DDevice9 ppReturnedDeviceInterface
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("794950F2-ADFC-458a-905E-10A10B0B503B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirect3DSwapChain9
    {
        void Present(RECT pSourceRect, RECT pDestRect, IntPtr hDestWindowOverride, RGNDATA pDirtyRegion, int dwFlags);
        void GetFrontBufferData(IDirect3DSurface9 pDestSurface);
        void GetBackBuffer(int iBackBuffer, D3DBACKBUFFER_TYPE Type, out IDirect3DSurface9 ppBackBuffer);
        void GetRasterStatus(out D3DRASTER_STATUS pRasterStatus);
        void GetDisplayMode(out D3DDISPLAYMODE pMode);
        void GetDevice(out IDirect3DDevice9 ppDevice);
        void GetPresentParameters(out D3DPRESENT_PARAMETERS pPresentationParameters);
    }

    #endregion

}
