This project is required by the EVRPresenter project, but may be used by other code as well.

To build it, you'll want the latest version of the Windows SDK.  As of this writing, that's at http://www.microsoft.com/download/en/details.aspx?id=8279

As with any code that is labeled "sample," you should be clear about what level of quality you are expecting.
While there are no bugs or gotchas in this code TO MY KNOWLEDGE, that doesn't mean there aren't any.  You should
review the code yourself, and test it for your specific purposes.

This project is a c++ COM object.  It is intended to allow c# to use c++ objects that
have broken implementations of QueryInterface.

******************************************************************************
* NOTE: You do not have to build this project in order to use it.  Pre-built *
*       versions of the x86 and x64 DLLs are included                        *
******************************************************************************

To use this COM object, add HackClasses.cs to your project.  Then create an instance of it:

   IHack ih = new Hack() as IHack;
   
Next, pass an IntPtr of the interface that has a broken QI to IHack.Set().  You can see this used
in Presenter.cs, but it looks something like this:

    // The IntPtr of the broken object
    // The IID of the method the object should support (but doesn't)
    // A bool indicating whether the COMObject should add a reference, or take over the existing pointer.
    h1.Set(p1Lookup, typeof(IMFTopologyServiceLookup).GUID, true);

If you want to study how this works, there are only 2 routines that are worth reading and they are both 
in Hack.cpp.  Check out Hack::Set and Hack::QueryInterface.

Be aware, this COM object masks an IUknown interface that violates COM rules by violating a *different*
set of COM rules (ie identity).  However, it appears that c# is willing to live with the violations this 
object introduces.

The real fix would be for the people who created the rules for COM/IUnknown to *follow* those rules in
their MF code.  However, since their failure to do so essentially only affects .Net developers, don't expect 
to see MS in a hurry to repair these types of problems.

As with all COM objects, this object must be registered with COM before it will work.  If you
build the project with Visual Studio, it will register it for you.  Otherwise, you can run

    regsvr32 Hack-32.dll
    
or

    regsvr32 Hack-64.dll

