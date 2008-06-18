/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released 
as public domain.  It is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

namespace EVRPresenter
{
    class Winmm
    {
        [DllImport("Winmm.dll", PreserveSig = false)]
        public extern static int timeBeginPeriod(int x);

        [DllImport("Winmm.dll", PreserveSig = false)]
        public extern static int timeEndPeriod(int uPeriod);

    }
}
