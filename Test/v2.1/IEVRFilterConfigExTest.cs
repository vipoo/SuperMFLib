using System;
using System.Text;

using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.EVR;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Testv21
{
    class IEVRFilterConfigExTest
    {
        IEVRFilterConfigEx _filterConfig;

        public void DoTests()
        {
            GetInterface();   
         
            TestGetConfigPrefs();
            TestSetConfigPrefs();

            CleanUp();
        }


        private void GetInterface()
        {
            _filterConfig = new EnhancedVideoRenderer() as IEVRFilterConfigEx;
        }


        private void TestSetConfigPrefs()
        {
            int hr;
            EVRFilterConfigPrefs prefGet;
            EVRFilterConfigPrefs prefSet;

            prefSet = EVRFilterConfigPrefs.EnableQoS;
            prefGet = EVRFilterConfigPrefs.Mask;

            hr = _filterConfig.GetConfigPrefs(out prefGet);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(prefGet == EVRFilterConfigPrefs.None);

            hr = _filterConfig.SetConfigPrefs(prefSet);
            MFError.ThrowExceptionForHR(hr);

            hr = _filterConfig.GetConfigPrefs(out prefGet);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(prefGet == EVRFilterConfigPrefs.EnableQoS);
        }


        private void TestGetConfigPrefs()
        {
            int hr;
            EVRFilterConfigPrefs preferences;
            preferences = EVRFilterConfigPrefs.Mask;

            hr = _filterConfig.GetConfigPrefs(out preferences);
            MFError.ThrowExceptionForHR(hr);

            Debug.Assert(preferences == EVRFilterConfigPrefs.None);
        }


        private void CleanUp()
        {
            Marshal.ReleaseComObject(_filterConfig);
            _filterConfig = null;
        }
    }
}
