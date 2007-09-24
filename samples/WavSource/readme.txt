/************************************************************************
WavSource - A COM object that allows Media Foundation to play .wav files

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample is a c# version of the WavSource sample included in the
Media Foundation SDK.  It extends Mediafoundation to include support for 
reading .wav files.

The most significant difference has to do with how error handling is performed.
See "Error handling" in docs\ReadMe.rtf for details.

After you build this sample, you will need to register it with both COM and MF.  
In theory, you can do this by clicking the "Register as COM" box in Visual Studio.  
In practice, I find that this doesn't work on Vista due to the way they have
mucked with security.  Instead, I use this command line from a cmd window opened 
with "Run as administrator":

c:\Windows\Microsoft.NET\Framework\v2.0.50727\regasm /tlb /codebase WavSource.dll

