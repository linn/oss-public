﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
using Linn;

namespace KinskyWeb
{
    public partial class MainForm : Form
    {

        private const string kStop = "&Stop";
        private const string kStart = "&Start";

        public MainForm()
        {
            InitializeComponent();
            notifyIcon.BalloonTipTitle = "Not subscribed";
            notifyIcon.BalloonTipText = "No infomation";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            Icon = Icon.FromHandle(Resources.KinskyLogo.GetHicon());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = KinskyStack.GetDefault().Helper.Product;
            notifyIcon.Text = KinskyStack.GetDefault().Helper.Product;
            this.Hide();
            KinskyStack.GetDefault().Helper.Stack.EventStatusChanged += ConnectionStateChanged;
            StartKinsky();
        }

        private void ConnectionStateChanged(object sender, EventArgsStackStatus args)
        {
            this.startStopToolStripMenuItem.Text = args.Status.State == EStackState.eOk ? kStop : kStart;
            this.startToolStripMenuItem2.Text = args.Status.State == EStackState.eOk ? kStop : kStart;
        }

        private void StartKinsky()
        {
            try
            {
                KinskyStack.GetDefault().Open();
            }
            catch (KinskyWeb.Kinsky.KinskyStack.ApplicationAlreadyRunningException)
            {
                MessageBox.Show("Another instance of KinskyWeb is running.\nPlease shut this down first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // problem starting kinsky - probably port already in use
                UserLog.WriteLine("Exception caught when starting services:");
                UserLog.WriteLine(ex.Message + " " + ex.StackTrace);
                notifyIcon.ShowBalloonTip(5000, "Error", "KinskyWeb failed to start.\nPlease check user logs for details.", ToolTipIcon.Error);
            }
        }

        private void StopKinsky()
        {
            try
            {
                KinskyStack.GetDefault().Close();
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("Exception caught when stopping services:");
                UserLog.WriteLine(ex.Message + " " + ex.StackTrace);
                notifyIcon.ShowBalloonTip(5000, "Error", "KinskyWeb failed to stop.\nPlease check user logs for details.", ToolTipIcon.Error);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                KinskyStack.GetDefault().Stop();
            }
            catch (Exception ex)
            {
                UserLog.WriteLine("Exception caught when shutting down:");
                UserLog.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //KinskyStack.GetDefault().AppKinskyWeb.UserOptionsDialog.ShowDialog();
        }

        private void startStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartStop();
        }

        private void startToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            StartStop(); 
        }

        private void StartStop()
        {
            if (KinskyStack.GetDefault().Helper.Stack.Status.State == EStackState.eOk)
            {
                StopKinsky();
            }
            else
            {
                StartKinsky();
            }
        }

        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //KinskyStack.GetDefault().AppKinskyWeb.UserOptionsDialog.ShowDialog();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormAboutBox().ShowDialog();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(Resources.Watermark, new Point(0, 24));
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new FormAboutBox().ShowDialog();
        }
    }
}