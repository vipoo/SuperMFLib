/****************************************************************************
While the underlying libraries are covered by LGPL, this sample is released
as public domain.  It is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
*****************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using MediaFoundation.Misc;

namespace MF_ProtectedPlayback
{
    public partial class Form1 : Form
    {
        const int WM_PAINT = 0x000F;
        const int WM_SIZE =                        0x0005;
        const int WM_ERASEBKGND =                  0x0014;
        const int WM_CHAR =                        0x0102;
        const int WM_SETCURSOR =                   0x0020;
        const int WM_APP = 0x8000;
        const int WM_APP_NOTIFY = WM_APP + 1;   // wparam = state
        const int WM_APP_ERROR = WM_APP + 2;    // wparam = HRESULT

        private CPlayer g_pPlayer;
        private bool g_bRepaintClient = true;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PAINT:
                    OnPaint(m.HWnd);
                    base.WndProc(ref m);
                    break;

                case WM_SIZE:
                    g_pPlayer.ResizeVideo((short)(m.LParam.ToInt32() & 65535), (short)(m.LParam.ToInt32() >> 16));
                    break;

                case WM_CHAR:
                    OnKeyPress(m.WParam.ToInt32());
                    break;

                case WM_SETCURSOR:
                    m.Result = new IntPtr(1);
                    break;

                case WM_APP_NOTIFY:
                    UpdateUI(m.HWnd, (CPlayer.PlayerState)m.WParam);
                    break;

                case WM_APP_ERROR:
                    NotifyError(m.HWnd, "An error occurred.", (int)m.WParam);
                    UpdateUI(m.HWnd, CPlayer.PlayerState.Ready);
                    break;

                case ContentProtectionManager.WM_APP_CONTENT_ENABLER:
                    OnContentEnablerMessage();
                    break;

                case ContentProtectionManager.WM_APP_BROWSER_DONE:
                    OnWebBrowserClosed();
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public Form1()
        {
            InitializeComponent();

            g_pPlayer = new CPlayer(this.Handle, this.Handle);
        }

        void OnPaint(IntPtr hwnd)
        {
            if (!g_bRepaintClient)
            {
                // Video is playing. Ask the player to repaint.
                g_pPlayer.Repaint();
            }
        }

        void OnKeyPress(int key)
        {
            switch (key)
            {
                // Space key toggles between running and paused
                case 0x20:
                    if (g_pPlayer.GetState() == CPlayer.PlayerState.Started)
                    {
                        g_pPlayer.Pause();
                    }
                    else if (g_pPlayer.GetState() == CPlayer.PlayerState.Paused)
                    {
                        g_pPlayer.Play();
                    }
                    break;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  Name: OnContentEnablerMessage
        //  Description: Handles requests from the content protection manager.
        /////////////////////////////////////////////////////////////////////////
        void OnContentEnablerMessage()
        {
            ContentProtectionManager pManager;

            int hr = g_pPlayer.GetContentProtectionManager(out pManager);

            if (hr >= 0)
            {
                ContentProtectionManager.Enabler state = pManager.GetState();
                int hrStatus = pManager.GetStatus();   // Status of the last action.

                // EnablerState is a defined for this application; it is not a standard
                // Media Foundation enum. It specifies what action the
                // ContentProtectionManager helper object is requesting.

                try
                {
                    switch (state)
                    {
                        case ContentProtectionManager.Enabler.Ready:
                            // Start the enable action.
                            pManager.DoEnable(ContentProtectionManager.EnablerFlags.SilentOrNonSilent);
                            break;

                        case ContentProtectionManager.Enabler.SilentInProgress:
                            // We are currently in the middle of silent enable.

                            // If the status code is NS_E_DRM_LICENSE_NOTACQUIRED,
                            // we need to try non-silent enable.
                            if (hrStatus == ContentProtectionManager.NS_E_DRM_LICENSE_NOTACQUIRED)
                            {
                                Debug.WriteLine("Silent enabler failed, attempting non-silent.");
                                pManager.DoEnable(ContentProtectionManager.EnablerFlags.ForceNonSilent); // Try non-silent this time;
                            }
                            else
                            {
                                // Complete the operation. If it succeeded, the content will play.
                                // If it failed, the pipeline will queue an event with an error code.
                                pManager.CompleteEnable();
                            }
                            break;

                        case ContentProtectionManager.Enabler.NonSilentInProgress:
                            // We are currently in the middle of non-silent enable.
                            // Either we succeeded or an error occurred. Either way, complete
                            // the operation.
                            pManager.CompleteEnable();
                            break;

                        case ContentProtectionManager.Enabler.Complete:
                            // Nothing to do.
                            break;

                        default:
                            Debug.WriteLine(string.Format("Unknown EnablerState value! ({0})", state));
                            break;
                    }
                }
                catch
                {
                    // If a previous call to DoEnable() failed, complete the operation
                    // so that the pipeline will get the correct failure code.
                    pManager.CompleteEnable();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  Name: OnWebBrowserClosed
        //  Description: Called when the user closes the browser window.
        /////////////////////////////////////////////////////////////////////////
        void OnWebBrowserClosed()
        {
            ContentProtectionManager pManager;

            int hr = g_pPlayer.GetContentProtectionManager(out pManager);

            if (hr >= 0)
            {
                ContentProtectionManager.Enabler state = pManager.GetState();

                if (state != ContentProtectionManager.Enabler.Complete)
                {
                    Debug.WriteLine("User closed the browser window before we got the licence. Cancel.");
                    pManager.CancelEnable();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  Name: UpdateUI
        //  Description: Enables or disables controls, based on the player state.
        void UpdateUI(IntPtr hwnd, CPlayer.PlayerState state)
        {
            bool bWaiting = false;
            bool bPlayback = false;

            Debug.Assert(g_pPlayer != null);

            switch (state)
            {
                case CPlayer.PlayerState.OpenPending:
                    bWaiting = true;
                    break;

                case CPlayer.PlayerState.Started:
                    bPlayback = true;
                    break;

                case CPlayer.PlayerState.Paused:
                    bPlayback = true;
                    break;

                case CPlayer.PlayerState.PausePending:
                    bWaiting = true;
                    bPlayback = true;
                    break;

                case CPlayer.PlayerState.StartPending:
                    bWaiting = true;
                    bPlayback = true;
                    break;
            }

            bool uEnable = !bWaiting;

            openToolStripMenuItem.Enabled = uEnable;
            openToolStripMenuItem.Enabled = uEnable;
            openUrlToolStripMenuItem.Enabled = uEnable;

            if (bWaiting)
            {
                Cursor.Current = Cursors.WaitCursor;
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }

            if (bPlayback && g_pPlayer.HasVideo())
            {
                g_bRepaintClient = false;
            }
            else
            {
                g_bRepaintClient = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int hr = 0;

            openFileDialog1.Filter = "Windows Media|*.wmv;*.wma;*.asf|Wave|*.wav|MP3|*.mp3|All files|*.*";

            // File dialog windows must be on STA threads.  ByteStream handlers are happier if
            // they are opened on MTA.  So, the application stays MTA and we call OpenFileDialog
            // on its own thread.
            Invoker I = new Invoker(openFileDialog1);

            // Show the File Open dialog.
            if (I.Invoke() == DialogResult.OK)
            {
                // Open the file with the playback object.
                //openFileDialog1.FileName = "C:\\sourceforge\\mflib\\Test\\Media\\Welcome.wavx";
                hr = g_pPlayer.OpenURL(openFileDialog1.FileName);

                if (hr >= 0)
                {
                    UpdateUI(this.Handle, CPlayer.PlayerState.OpenPending);
                }
                else
                {
                    NotifyError(this.Handle, "Could not open the file.", hr);
                    UpdateUI(this.Handle, CPlayer.PlayerState.Ready);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor.Current = Cursors.Default;

            if (g_pPlayer != null)
            {
                g_pPlayer.Shutdown();
                g_pPlayer = null;
            }
        }

        void NotifyError(IntPtr hwnd, string sErrorMessage, int hrErr)
        {
            string s = string.Format("{0} (HRESULT = 0x{1:x} {2})", sErrorMessage, hrErr, MFError.GetErrorText(hrErr));

            MessageBox.Show(this, s, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void openUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int hr;

            fmURL f = new fmURL();

            if (f.ShowDialog(this) == DialogResult.OK)
            {
                // Open the file with the playback object.
                hr = g_pPlayer.OpenURL(f.tbURL.Text);

                if (hr >= 0)
                {
                    UpdateUI(this.Handle, CPlayer.PlayerState.OpenPending);
                }
                else
                {
                    NotifyError(this.Handle, "Could not open this URL.", hr);
                    UpdateUI(this.Handle, CPlayer.PlayerState.Ready);
                }
            }
        }
    }

    /// <summary>
    /// Opens a specified FileOpenDialog box on an STA thread
    /// </summary>
    public class Invoker
    {
        private OpenFileDialog m_Dialog;
        private DialogResult m_InvokeResult;
        private Thread m_InvokeThread;

        // Constructor is passed the dialog to use
        public Invoker(OpenFileDialog Dialog)
        {
            m_InvokeResult = DialogResult.None;
            m_Dialog = Dialog;

            // No reason to waste a thread if we aren't MTA
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
            {
                m_InvokeThread = new Thread(new ThreadStart(InvokeMethod));
                m_InvokeThread.SetApartmentState(ApartmentState.STA);
            }
            else
            {
                m_InvokeThread = null;
            }
        }

        // Start the thread and get the result
        public DialogResult Invoke()
        {
            if (m_InvokeThread != null)
            {
                m_InvokeThread.Start();
                m_InvokeThread.Join();
            }
            else
            {
                m_InvokeResult = m_Dialog.ShowDialog();
            }

            return m_InvokeResult;
        }

        // The thread entry point
        private void InvokeMethod()
        {
            m_InvokeResult = m_Dialog.ShowDialog();
        }
    }

}