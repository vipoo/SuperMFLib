/************************************************************************
EVRPresenter - Custom presenter using EVR

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample currently doesn't run!

To test it, I'm using the newest c++ MFPlayer sample.  I have modified it to use the GUID from the c# EVRPresenter sample.  In the EVR project, I set MFPlayer as the program to debug.

In addition to the sample code from cvs, you'll also need the changes I've made to the library (get the raw files from cvs, not the stuff in "downloads.").  Minor changes, but helpful.

You may need to run RegAsm against MediaFoundation.dll (although there's nothing in there that needs registering) before you can register EVRPresenter.dll.

If you make improvements to this code (ideally getting it to run), please let me know so that we can share them with future users of the library.

snarfle@users.sourceforge.net