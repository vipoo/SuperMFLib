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

There has been a major change to how error handling is done since this sample
was first released.  Now all COM methods return an int (HRESULT) that must
explicitly be check to make sure the method worked as expected.  Commonly
this would be done as:

   int hr;

   hr = iSomething.DoSomething();
   MFError.ThrowExceptionForHR(hr); // Turn hr into exception if it was an error

After you build this sample, you will need to use this line (or one like it) to 
register the MFT:

	%windir%\Microsoft.NET\Framework\v2.0.50727\regasm /tlb /codebase MFT_Grayscale.dll

To invoke this MFT, use the PlaybackFx sample.
