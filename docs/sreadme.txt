Media Foundation Lib Samples 2012-02-14

http://mflib.SourceForge.net

These samples should not be considered commercial quality applications.  They are just 
intended to illustrate how to use some particular feature, or group of features in 
Media Foundation.  Feel free to polish them into applications of your own, or extract 
sections to use in your own code.  Each sample has (at least one) readme file.  
If you are looking for info regarding a sample, these are always a good place to start.

Also, while the Media Foundation library is licensed under LGPL, these samples are 
public domain.  Use them as you like.  Every one of these samples needs the MediaFoundation
library which is not included in this package.  Get the latest version of the library from 
the SourceForge website.

These samples have been updated to work with the v2.0 library.

The people who wrote these samples usually hang out in 
http://sourceforge.net/forum/forum.php?forum_id=711229.  If you have questions, 
comments, or just want to say thanks to the people who have taken the time to
create these, feel free to stop by.

Also, if you have samples you think would be useful (or would like to write some), 
that forum would be the place to get started.

=====================================================================================

This is the current list of samples along with a short description.  See the
readme.txt in the individual samples for more details.


Samples\BasicPlayer
--------------------
A c# implementation of the BasicPlayer sample that ships with the Vista PSDK.  It allows
you to use MF to play various media files.


Samples\WavSource
--------------------
A c# implementation of the WavSource sample that ships with the Vista PSDK.  It extends
Mediafoundation to include support for reading .wav files.


MFT_Grayscale
--------------------
A c# implementation of the MFT_Grayscale sample that ships with the Vista PSDK.  It allows
you to modify data as it passes down the topology.


PlaybackFX
--------------------
A c# implementation of the PlaybackFX sample that ships with the Vista PSDK.  It is the
same as the BasicPlayer sample, except that it loads the Grayscale MFT into the topology.


Playlist
--------------------
A c# implementation of the PlaybackFX sample that ships with the Vista PSDK.  It plays a
collection of media files, one after the other.


ProtectedPlayback
--------------------
A c# implementation of the ProtectedPlayback sample that ships with the Vista PSDK.  It is the
same as the BasicPlayer sample, except that it allows for playing protected content.


Splitter
--------------------
A c# implementation of the code on http://msdn2.microsoft.com/en-us/library/bb530124.aspx.  It
shows how to parse/process data from WM files.


WavSink
--------------------
A c# implementation of the WavSink sample that ships with the Vista PSDK.  Is the opposite
of the WavSource sample: It writes audio output to a .wav file.


EVRPresenter
--------------------
A c# implementation of the EVRPresenter sample that ships with the Vista PSDK.  This code replaces
the default renderer of the Enhanced Video Renderer (EVR).  To have any hope of understanding what
this code does and how it works, read the readmes included with the project.


Hack
--------------------
This c++ project allows c# code to work around flaws in c++ COM objects that don't correctly implement 
the IUnknown interface.  It is used by the EVRPresenter project, but is a general-purpose COM object
that can be used to work around similar problems in other COM objects.

