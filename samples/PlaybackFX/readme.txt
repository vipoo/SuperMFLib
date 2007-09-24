/************************************************************************
PlaybackFX - An upgraded version of the BasicPlayer sample that uses a transform

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample is a c# version of the PlaybackFX sample included in the
Media Foundation SDK.  It is the same as the BasicPlayer sample, except 
that it loads the Grayscale MFT into the topology.

This sample requires the MFT_GrayScale sample to have been built and 
registered (since that's the transform we use!).

The most significant difference has to do with how error handling is performed.
See "Error handling" in docs\ReadMe.rtf for details.
