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

After you build this sample, you will need to register it with both COM and MF.  
In theory, you can do this by clicking the "Register as COM" box in Visual Studio.  
In practice, I find that this doesn't work on Vista due to the way they have
mucked with security.  Instead, I use this command line from a cmd window opened 
with "Run as administrator":

c:\Windows\Microsoft.NET\Framework\v2.0.50727\regasm /tlb /codebase MFT_Grayscale.dll

Also worth noting is that of the 3 different video types the original c++ supports,
I have only translated 1.  This is solely due to the fact that I haven't yet found
a video file that contains the other 2 types.  If you want to enable those routines,
you'll also need to turn on "unsafe" code for the project.

