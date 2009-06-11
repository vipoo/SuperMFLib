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

After you build this sample, you will need to use this line (or one like it) to 
register the handler:

	%windir%\Microsoft.NET\Framework\v2.0.50727\regasm /tlb /codebase WavSource.dll

To invoke this handler, use the BasicPlayback sample to play a .wav file.

Update!

MS has added a WavByteStreamHandler for wav files.  Attempting to register this provider will
*APPEAR* to work, but will actually fail.  To actually see this code run, change sWavFileExtension 
in WavByteStreamHandler.cs to some other extension (.xyz).  Then rename a .wav file to .xyz.