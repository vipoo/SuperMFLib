---------------------------------------------------------------------
MFCaptureAlt Sample
================================


While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
---------------------------------------------------------------------

MFCaptureAlt is a combination of MFCaptureD3D and MFCaptureToFile.  It previews the output from the
capture device, and writes it to disk.

It also shows how to modify the capture buffer to add a watermark.

The SlimDX runtime (http://slimdx.org/) must be installed for this sample to work.  Note that the SlimDX 
assemblies are platform specific (x86 vs x64), so you may need to modify your references to match your
target platform.