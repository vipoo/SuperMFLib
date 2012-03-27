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
            string EVRRenderer = "FA10746C-9B63-4B6C-BC49-FC300EA5F256";
            Type comtype = Type.GetTypeFromCLSID(new Guid(EVRRenderer));

            _filterConfig = Activator.CreateInstance(comtype) as IEVRFilterConfigEx;             
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
