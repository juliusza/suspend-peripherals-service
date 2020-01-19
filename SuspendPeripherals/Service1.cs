using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO.Ports;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading;

namespace SuspendPeripherals
{
    public partial class Service1 : ServiceBase
    {
        private SerialPort port = null;
        private System.Timers.Timer timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected void Initialize()
        {
            if (port != null)
            {
                port.Close();
            }
            port = new SerialPort("COM3", 2400, Parity.None, 8, StopBits.One);
            port.Open();
            port.ReadLine();
        }

        public void RelayOff()
        {
            timer.Stop();

            if (port == null || !port.IsOpen)
            {
                Initialize();
            }
            port.Write("l");
            port.Write("k");
            port.Write("m");
            port.Write("j");
            port.Close();
        }

        public void RelayOn()
        {
            if (port == null || !port.IsOpen)
            {
                Initialize();
            }
            port.Write("b");
            port.Write("c");
            port.Write("d");
            port.Write("e");

            timer.Start();
        }

        protected override void OnStart(string[] args)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 10000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);

            RelayOn();

            new Thread(RunMessagePump).Start();
        }

        protected override void OnStop()
        {
            RelayOff();
        }

        protected void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (port == null || !port.IsOpen)
            {
                RelayOn();
            }
        }

        void RunMessagePump()
        {
            Application.Run(new HiddenForm(this));
        }
    }

    public partial class HiddenForm : Form
    {
        private readonly Service1 parent;

        public HiddenForm(Service1 parentService)
        {
            InitializeComponent();

            parent = parentService;
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                parent.RelayOff();
            }
            else if (e.Mode == PowerModes.Resume)
            {
                parent.RelayOn();
            }
        }
    }

    partial class HiddenForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HiddenForm";
            this.Text = "HiddenForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.HiddenForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HiddenForm_FormClosing);
            this.ResumeLayout(false);

        }
    }
}
