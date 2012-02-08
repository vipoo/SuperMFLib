/************************************************************************
Splitter - Parsing an ASF Stream

While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  

**************************************************************************/

This sample is a c# version of the code from the MF docs on MSDN:

http://msdn2.microsoft.com/en-us/library/bb530124.aspx

It shows how to parse/process data from WM files.  Test with 
c:\Program Files\Microsoft SDKs\Windows\v7.0\Samples\Multimedia\WMP_11\media\smooth.wmv

There has been a major change to how error handling is done since this sample
was first released.  Now all COM methods return an int (HRESULT) that must
explicitly be check to make sure the method worked as expected.  Commonly
this would be done as:

   int hr;

   hr = iSomething.DoSomething();
   MFError.ThrowExceptionForHR(hr); // Turn hr into exception if it was an error
