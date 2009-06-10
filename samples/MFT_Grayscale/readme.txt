/************************************************************************
MFT_Grayscale - A COM object that performs transforms on video

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample is a c# version of the MFT_Grayscale sample included in the
Media Foundation SDK.  It changes video to grayscale.  It serves as a useful 
guide showing how to work with video under Media Foundation.

The most significant difference between this code and the original c++ has to do 
with how error handling is performed.  See "Error handling" in docs\ReadMe.rtf 
for details.

After you build this sample, you will need to use this line (or one like it) to 
register the MFT:

	%windir%\Microsoft.NET\Framework\v2.0.50727\regasm /tlb /codebase MFT_Grayscale.dll

To invoke this MFT, use the PlaybackFx sample.
