/************************************************************************
BasicPlayer - A basic player of audio and video using Media Foundation

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample is a c# version of the BasicPlayer sample included in the
Media Foundation SDK.  It allows you to use MF to play various media files.

Note that in order to play wave files, you need to compile and register the 
WavSource sample.

There has been a major change to how error handling is done since this sample
was first released.  Now all COM methods return an int (HRESULT) that must
explicitly be check to make sure the method worked as expected.  Commonly
this would be done as:

   int hr;

   hr = iSomething.DoSomething();
   MFError.ThrowExceptionForHR(hr); // Turn hr into exception if it was an error
