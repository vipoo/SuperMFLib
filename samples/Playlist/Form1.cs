/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

//
// The sample shows steps that are required to create a sequencer source
// and add topologies. In addition, it also demonstrates the use of media session APIs
// to provide transport control such as play, pause, stop, and skip.
//
// The user creates the initial playlist of audio files by using
// a standard Open dialog box that allows multiple selection.
// The application creates topologies for the selected files and
// adds them to the sequencer source.
// The last topology is flagged to mark the end of the playlist.
// To start the sequence, the first topology is added to the media session.
// For continuous play, the next topology is queued on the media session
// when the application receives a MENewPresenationEvent.
// The user can play, pause, stop, and skip. While the session is playing,
// the user can add another segment; and delete a segment (this is not working).
// The core functionality is provided by the CPlayer class.
// As the media session The UI is shows the segment name and duration for the current segment.
// The statusbar shows the current status of the player. Segment information is stored in a list.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using MediaFoundation;

namespace Playlist
{
    public partial class Form1 : Form
    {
        #region Declarations

        private const int WM_APP = 0x8000;
        public const int WM_NOTIFY_APP = WM_APP + 1;

        private class Segment
        {
            public Segment(int id, string url)
            {
                SegID = id;
                szUrl = url;
            }

            public int SegID;
            public string szUrl;

            public override string ToString()
            {
                return szUrl;
            }
        }

        #endregion

        #region Member Variables

        CPlayer m_pPlayer;

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NOTIFY_APP:

                    if (m.WParam.ToInt32() < (int)MediaEventType.MEReservedMax)
                    {
                        Debug.WriteLine("MediaEventType: " + ((MediaEventType)m.WParam).ToString());
                    }
                    else
                    {
                        Debug.WriteLine("PlayerEvent: " + ((PlayerEvent)m.WParam).ToString());
                    }

                    switch ((MediaEventType)m.WParam)
                    {
                        case MediaEventType.MESessionStarted:
                            OnMESessionStarted((Playlist.PlayerState)m.LParam);
                            break;

                        case MediaEventType.MESessionPaused:
                            OnMESessionPaused((Playlist.PlayerState)m.LParam);
                            break;

                        case MediaEventType.MESessionStopped:
                            OnMESessionStopped((Playlist.PlayerState)m.LParam);
                            break;

                        case MediaEventType.MESessionTopologyStatus:
                            GCHandle gc = GCHandle.FromIntPtr(m.LParam);
                            OnMESessionTopologyStatus((TopologyStatusInfo)gc.Target);
                            gc.Free();

                            break;

                        case MediaEventType.MEEndOfPresentationSegment:
                            OnMEEndOfPresentationSegment((int)m.LParam);
                            break;

                        case MediaEventType.MEEndOfPresentation:
                            OnMEEndOfPresentation((int)m.LParam);
                            break;

                        case MediaEventType.MENewPresentation:
                            OnMENewPresentation((int)m.LParam);
                            break;

                        case (MediaEventType)PlayerEvent.Initialized:
                            SetPlayerStateString((PlayerState)m.LParam);
                            break;

                        case (MediaEventType)PlayerEvent.SegmentAdded:
                            OnSegmentAdded((int)m.LParam);
                            break;

                        case (MediaEventType)PlayerEvent.SegmentDeleted:
                            OnSegmentDeleted((int)m.LParam);
                            break;

                        default:
                            break;
                    }

                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #region Utility routines

        //////////////////////////////////////////////////////////////////////////
        //  Name: NotifyError
        //  Description: Show a message box with an error message.
        //
        //  sErrorMessage: NULL-terminated string containing the error message.
        //  hrErr: HRESULT from the error.
        /////////////////////////////////////////////////////////////////////////
        private void NotifyError(string szErrorMessage, int hrErr)
        {
            string szMessage;

            szMessage = string.Format("{0} (HRESULT = 0x{1:x})", szErrorMessage, hrErr);

            MessageBox.Show(this, szMessage, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //////////////////////////////////////////////////////////////////////////
        //  Name: FormatTimeString
        //  Description: Converts MFTIME format to hh:mm:ss format.
        //
        //  time: MFTIME type 100 nano second unit.
        //  szTimeString: NULL-terminated string containing the time string.
        ///////////////////////////////////////////////////////////////////////////
        private string FormatTimeString(long time)
        {
            //Convert nanoseconds to seconds
            TimeSpan ts = new TimeSpan(time);

            return ts.ToString().Substring(0, 8);
        }

        //////////////////////////////////////////////////////////////////////////
        //  Name: SetPlayerStateString
        //  Description: Returns the player.
        //
        //  time: MFTIME type 100 nano second unit.
        //  szTimeString: NULL-terminated string containing the time string.
        ///////////////////////////////////////////////////////////////////////////
        private void SetPlayerStateString(PlayerState dwState)
        {
            string szPlayerState;

            switch (dwState)
            {
                case PlayerState.PlayerCreated:
                    szPlayerState = "Player Created.";
                    break;

                case PlayerState.Initialized:
                    szPlayerState = "Player initialized, empty playlist.";
                    break;

                case PlayerState.Paused:
                    szPlayerState = "MESessionPaused: Paused.";
                    break;

                case PlayerState.Playing:
                    szPlayerState = "MESessionStarted: Playing.";
                    break;

                case PlayerState.Stopped:
                    szPlayerState = "MESessionStopped: Stopped.";
                    break;

                default:
                    szPlayerState = "Unknown PlayerEvent type";
                    break;
            }

            AddEvent(szPlayerState);
        }

        private void SkipToSegment(int iIndex)
        {
            int hr;
            int SegmentID;

            SegmentID = ((Segment)(lbPlaylist.Items[iIndex])).SegID;

            hr = m_pPlayer.Skip(SegmentID);
            if (hr < 0)
            {
                NotifyError("Could not skip playback.", hr);
            }
        }

        private void AddEvent(string s)
        {
            lbEventNotification.Items.Add(s);
            lbEventNotification.SelectedIndex = lbEventNotification.Items.Count - 1;
        }

        #endregion

        #region MediaFoundation Event Handlers

        private void OnMESessionStarted(PlayerState dwState)
        {
            bnPlay.Text = "Pause";
            bnStop.Enabled = true;

            SetPlayerStateString(dwState);
            timer1.Enabled = true;
        }

        private void OnMESessionPaused(PlayerState dwState)
        {
            bnPlay.Text = "Continue";

            SetPlayerStateString(dwState);
            timer1.Enabled = false;
        }

        private void OnMESessionStopped(PlayerState dwState)
        {
            bnPlay.Text = "Play";
            bnStop.Enabled = false;

            SetPlayerStateString(dwState);

            timer1.Enabled = false;
        }

        private void OnMESessionTopologyStatus(TopologyStatusInfo ptopo)
        {
            string szSegmentInfoString;
            string szMessage;

            int hr;

            long hnsSegmentDuration;

            int iIndex = 0, iSegID = 0;

            hr = m_pPlayer.GetSegmentInfo(ptopo.iSegmentId, out hnsSegmentDuration, out szSegmentInfoString);

            tbDuration.Text = FormatTimeString(hnsSegmentDuration);

            //find the corresponding iIndex in the listbox
            for (iIndex = 0; iIndex < lbPlaylist.Items.Count; iIndex++)
            {
                iSegID = ((Segment)(lbPlaylist.Items[iIndex])).SegID;

                if (iSegID == ptopo.iSegmentId)
                {
                    break;
                }
            }

            lbPlaylist.SelectedIndex = iIndex;

            switch (ptopo.iTopologyStatusType)
            {
                case MFTopoStatus.Ready:
                    szMessage = string.Format("MESessionTopologyStatus: Segment {0} ready to start.", ptopo.iSegmentId);

                    break;
                case MFTopoStatus.StartedSource:
                    szMessage = string.Format("MESessionTopologyStatus: Reading data for segment {0}.", ptopo.iSegmentId);

                    break;
                case MFTopoStatus.SinkSwitched:
                    szMessage = string.Format("MESessionTopologyStatus: Switching to segment {0}.", ptopo.iSegmentId);
                    break;

                case MFTopoStatus.Ended:
                    szMessage = string.Format("MESessionTopologyStatus: Playback ended for segment {0}.", ptopo.iSegmentId);

                    break;

                default:
                    szMessage = string.Format("Topo status message {0}.", ptopo.iTopologyStatusType);
                    break;
            }

            AddEvent(szMessage);
        }

        private void OnMENewPresentation(int SegmentID)
        {
            string szMessage = string.Format("MENewPresentation: Prerolled segment {0}", SegmentID);
            AddEvent(szMessage);
        }

        private void OnMEEndOfPresentation(int value)
        {
            bnPlay.Text = "Play";
            bnStop.Enabled = false;
            string szMessage;

            if (value == 1)
            {
                szMessage = "MEEndOfPresentation: Segment canceled by Sequencer Source. End of Playlist.";
            }
            else
            {
                szMessage = "MEEndOfPresentation: End of Playlist.";
            }

            AddEvent(szMessage);

            timer1.Enabled = false;
        }

        private void OnMEEndOfPresentationSegment(int value)
        {
            string szMessage;

            if (value == 1)
            {
                szMessage = "MEEndOfPresentationSegment: Segment canceled by Sequencer Source.";
            }
            else
            {
                szMessage = "MEEndOfPresentationSegment: Segment ended normally.";
            }

            AddEvent(szMessage);
        }

        //////////////////////////////////////////////////////////////////////////
        //  Name: OnSegmentAdded
        //  Description: Updates the list box with the specified segment name
        //
        /////////////////////////////////////////////////////////////////////////

        private void OnSegmentAdded(int SegmentID)
        {
            string szSegmentURL;

            long hnsSegmentDuration;
            int hr;

            hr = m_pPlayer.GetSegmentInfo(SegmentID, out hnsSegmentDuration, out szSegmentURL);

            if (hr >= 0)
            {
                Segment seg = new Segment(SegmentID, szSegmentURL);

                lbPlaylist.Items.Add(seg);

                lbPlaylist.SelectedIndex = lbPlaylist.Items.Count - 1;

                tbDuration.Text = FormatTimeString(hnsSegmentDuration);

                if (lbPlaylist.Items.Count == 1) // todo - this can't be right
                {
                    bnPlay.Enabled = true;
                    removeFromPlaylistToolStripMenuItem.Enabled = true;
                }

                string szMessage = string.Format("Added: {0}", SegmentID);
                AddEvent(szMessage);

            }
            else
            {
                NotifyError("Could not add segment", hr);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        //  Name: OnSegmentDeleted
        //  Description: Deletes the specified segment from the listbox
        //
        /////////////////////////////////////////////////////////////////////////

        private void OnSegmentDeleted(int SegmentID)
        {
            int SegID = 0, iIndex = 0;

            //find the corresponding iIndex in the listbox
            while (SegID != SegmentID)
            {
                SegID = ((Segment)(lbPlaylist.Items[iIndex])).SegID;
                iIndex++;
            }

            lbPlaylist.Items.RemoveAt(iIndex - 1);

            if (iIndex - 1 <= lbPlaylist.Items.Count - 1)
            {
                lbPlaylist.SelectedIndex = iIndex - 1;
            }
            else if (lbPlaylist.Items.Count > 0)
            {
                lbPlaylist.SelectedIndex = iIndex - 2;
            }

            string szMessage = string.Format("Deleted: {0}", SegmentID);
            AddEvent(szMessage);

            bnPlay.Enabled = removeFromPlaylistToolStripMenuItem.Enabled = lbPlaylist.Items.Count > 0;
        }

        #endregion

        #region Form Events

        private void Form1_Load(object sender, EventArgs e)
        {
            //Initialize CPlayer object

            m_pPlayer = new CPlayer(this.Handle);

            m_pPlayer.Initialize();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            int hr = 0;

            if (m_pPlayer.GetState() == PlayerState.Playing)
            {
                hr = m_pPlayer.Pause();
                if (hr < 0)
                {
                    NotifyError("Could not pause playback.", hr);
                }
            }
            else
            {
                hr = m_pPlayer.Play();
                if (hr < 0)
                {
                    NotifyError("Could not start playback.", hr);
                }
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            int hr = 0;

            hr = m_pPlayer.Stop();
            if (hr < 0)
            {
                NotifyError("Could not start playback.", hr);
            }
        }

        private void addToPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the File Open dialog.
            DialogResult dr = openFileDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                foreach (string szUrl in openFileDialog1.FileNames)
                {
                    int hr = m_pPlayer.AddToPlaylist(szUrl);

                    if (hr < 0)
                    {
                        NotifyError("Could not add to the playlist.", hr);
                        break;
                    }
                }
            }
        }

        private void removeFromPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int hr = 0;
            int SegmentID = 0;
            int iIndex = 0;

            iIndex = lbPlaylist.SelectedIndex;

            if (iIndex != -1)
            {
                SegmentID = ((Segment)(lbPlaylist.Items[iIndex])).SegID;

                hr = m_pPlayer.DeleteSegment(SegmentID);
                if (hr < 0)
                {
                    NotifyError("Could not delete segment.", hr);
                }
            }
            else
            {
                NotifyError("Segment not selected.", -1);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            long hnsCurrentTime;
            long hnsPresentationTime;

            m_pPlayer.GetCurrentSegmentTime(out hnsCurrentTime);
            m_pPlayer.GetPresentationTime(out hnsPresentationTime);

            tbCurrentTime.Text = FormatTimeString(hnsCurrentTime);
            tbPresentationTime.Text = FormatTimeString(hnsPresentationTime);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lbPlaylist_DoubleClick(object sender, EventArgs e)
        {
            int iIndex = lbPlaylist.SelectedIndex;
            SkipToSegment(iIndex);
        }

        #endregion

    }
}