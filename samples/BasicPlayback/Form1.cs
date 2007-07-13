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

namespace MF_BasicPlayback
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

            openFileDialog1.Filter = "Windows Media|*.wmv;*.wma;*.asf;*.wav|MP3|*.mp3|All files|*.*";

            // File dialog windows must be on STA threads.  ByteStream handlers are happier if
            // they are opened on MTA.  So, the application stays MTA.
            Invoker I = new Invoker(openFileDialog1);

            // Show the File Open dialog.
            if (I.Invoke() == DialogResult.OK)
            {
                // Open the file with the playback object.
                //openFileDialog1.FileName = "C:\\sourceforge\\mflib\\Test\\Media\\Welxcome.waxv";
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
            string s = string.Format("{0} (HRESULT = {1:x} {2})", sErrorMessage, hrErr, MFError.GetErrorText(hrErr));

            MessageBox.Show(this, s, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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