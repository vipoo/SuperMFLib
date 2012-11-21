---------------------------------------------------------------------
MFCaptureD3D Sample
================================


While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

Written by:
Gerardo Hernandez
BrightApp.com 

Modified by snarfle
---------------------------------------------------------------------

MFCaptureD3D is an example to demonstrates how to preview video from a capture device, using 
Direct3D to draw the video frames.


This sample uses the following Mediafoundation Interfaces :

	IMFSourceReader
	IMFSourceReaderAsync (Managed code version of IMFSourceReader)
	IMFAttributes
	IMFMediaType
	IMFSample
	IMFMediaBuffer
	IMFActivate
	IMF2DBuffer
	IMFSourceReaderCallback
	IMFMediaSource

The SlimDX runtime (http://slimdx.org/) must be installed for this sample to work.  Note that the SlimDX 
assemblies are platform specific (x86 vs x64), so you may need to modify your references to match your
target platform.