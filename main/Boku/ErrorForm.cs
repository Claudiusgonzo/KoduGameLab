﻿using System;

#if NETFX_CORE
#else
    using System.Drawing;
    using System.Windows.Forms;
#endif

namespace Boku
{
    // (TODO (scoy) BROKEN
#if !NETFX_CORE
    public partial class ErrorForm : Form
    {
        public ErrorForm()
        {
            InitializeComponent();
        }

        private void ErrorForm_Load(object sender, EventArgs e)
        {
            Cursor.Show();

            buttonSendAndClose.Enabled = Program2.SiteOptions.NetworkEnabled;
            textBoxAddInfo.Enabled = Program2.SiteOptions.NetworkEnabled;
            textBoxLiveId.Enabled = Program2.SiteOptions.NetworkEnabled;
        }
    }
#endif
}
