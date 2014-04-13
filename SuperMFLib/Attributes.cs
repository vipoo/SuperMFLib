// This file is part of SuperMFLib.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/SuperMFLib
//
// SuperMFLib is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SuperMFLib is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with SuperMFLib.  If not, see <http://www.gnu.org/licenses/>.

using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFoundation.Net
{
	public class Attributes : COMDisposable<IMFAttributes>
    {
		public Attributes(int initialSize = 1) : base(NewInstance(initialSize))  {  }

		private static IMFAttributes NewInstance(int initialSize)
		{
			IMFAttributes instance;
			MFExtern.MFCreateAttributes(out instance, initialSize).Hr();
			return instance;
		}

        public bool ReadWriterEnableHardwareTransforms
        {
            get
            {
                int result;
                MFError.ThrowExceptionForHR(instance.GetUINT32(MFAttributesClsid.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, out result));
                return result != 0;
            }
            set
            {
                MFError.ThrowExceptionForHR(instance.SetUINT32(MFAttributesClsid.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, value ? 1 : 0));

            }
        }

        public bool SourceReaderEnableVideoProcessing
        {
            get
            {
                int result;
                MFError.ThrowExceptionForHR(instance.GetUINT32(MFAttributesClsid.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING, out result));
                return result != 0;
            }
            set
            {
                MFError.ThrowExceptionForHR(instance.SetUINT32(MFAttributesClsid.MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING, value ? 1 : 0));
            }
        }
    }
}
