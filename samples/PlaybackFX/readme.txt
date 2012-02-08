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

There has been a major change to how error handling is done since this sample
was first released.  Now all COM methods return an int (HRESULT) that must
explicitly be check to make sure the method worked as expected.  Commonly
this would be done as:

   int hr;

   hr = iSomething.DoSomething();
   MFError.ThrowExceptionForHR(hr); // Turn hr into exception if it was an error
