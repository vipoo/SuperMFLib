The EVRPresenter is the largest and most complex of the samples.  Don't attempt to use
this code unless you are a *serious* c# & MF programmer.  As MS says on their web site
<http://msdn.microsoft.com/en-us/library/bb530107(VS.85).aspx>

-----------
Before writing a custom presenter, you should be familiar with the following technologies:

    * The enhanced video renderer.
    * Direct3D graphics. You don't need to understand 3-D graphics to write a presenter, 
      but you must know how to create a Direct3D device and manage Direct3D surfaces. If 
      you aren't familiar with Direct3D, read the sections "Direct3D Devices" and 
      "Direct3D Resources" in the DirectX Graphics SDK documentation.
    * DirectShow filter graphs or the Media Foundation pipeline, depending on which 
      technology your application will use to render video.
    * Media Foundation Transforms. The EVR mixer is a Media Foundation transform, and the 
      presenter calls methods directly on the mixer.
    * Implementing COM objects. The presenter is an in-process, free-threaded COM object.
-----------

If that list intimidates you, writing a presenter is not for you.

YOU HAVE BEEN WARNED.


Before you start:
-----------------

You need to build and register the Hack project.  This allows the EVRPresenter to 
work around problems in Vista and W7.  See the readme in that project.

Second, you will need the v2.1 DirectShowNet library (or later) from 
http://DirectShowNet.SourceForge.Net

Finally, as with any code that is labeled "sample," you should be clear about what level of 
quality you are expecting.  While there are no bugs or gotchas in this code TO MY 
KNOWLEDGE, that doesn't mean there aren't any.  You should review the code yourself, 
and test it for your specific purposes.

Understanding how it works:
---------------------------

There are a bunch of things you should understand before diving into this code.  Start
with these pages on MSDN:

EVR Docs: http://msdn.microsoft.com/en-us/library/ms694916(VS.85).aspx
How to write a presenter: http://msdn.microsoft.com/en-us/library/bb530107(VS.85).aspx
How to write a mixer: http://msdn.microsoft.com/en-us/library/ms701624(VS.85).aspx

After you have read those docs, you will understand what a presenter is, how they
work, and why you might want one.

Be aware that a c# EVR Presenter *MUST* be called on an MTA thread.  Otherwise
it tends to hang.

This presenter was written and tested on Vista SP1.

You will find that this code is pretty much a direct translation of the c++ 
EVRPresenter.  It has a little bit of c# flavor, but mostly it looks like c++
code that has been translated to c#.

It is a COM object, so it must be registered with REGASM.  In Visual Studio, this is done for
you, otherwise use regasm from 

   %windir%\Microsoft.NET\Framework\v2.0.50727\RegAsm.exe EVRPresenter.dll
   
   64 bit:
   %windir%\Microsoft.NET\Framework64\v2.0.50727\RegAsm.exe EVRPresenter.dll


You may also need to run RegAsm /TLB against MediaFoundation.dll and/or DirectShowLib before 
you can register EVRPresenter.dll.


Making it go:
-------------

See the EVRPlayer sample.