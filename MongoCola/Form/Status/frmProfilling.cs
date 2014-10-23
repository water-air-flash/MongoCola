﻿using System;
using System.Windows.Forms;
using MongoCola.Module;
using MongoDB.Driver;

namespace MongoCola
{
    public partial class frmProfilling : Form
    {
        /// <summary>
        /// </summary>
        public frmProfilling()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmProfilling_Load(object sender, EventArgs e)
        {
            if (!SystemManager.IsUseDefaultLanguage)
            {
                cmdCancel.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Cancel);
                cmdOK.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_OK);
                lblProfilingLevel.Text =
                    SystemManager.MStringResource.GetText(StringResource.TextType.Main_Menu_Operation_ProfillingLevel);
            }

            cmbProfillingLv.Items.Add("0-No Logging");
            cmbProfillingLv.Items.Add("1-Log Slow Operations");
            cmbProfillingLv.Items.Add("2-Log All Operations");

            cmbProfillingLv.SelectedIndex = (int) SystemManager.GetCurrentDataBase().GetProfilingLevel().Level;
            switch (cmbProfillingLv.SelectedIndex)
            {
                case 1:
                    NumTime.Enabled = true;
                    NumTime.Value = (decimal) SystemManager.GetCurrentDataBase().GetProfilingLevel().Slow.TotalSeconds*
                                    1000;
                    break;
                default:
                    NumTime.Enabled = false;
                    break;
            }
        }

        /// <summary>
        ///     OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdOK_Click(object sender, EventArgs e)
        {
            ProfilingLevel mProfillingLv;
            switch (cmbProfillingLv.SelectedIndex)
            {
                case 0:
                    mProfillingLv = ProfilingLevel.None;
                    break;
                case 1:
                    mProfillingLv = ProfilingLevel.Slow;
                    break;
                case 2:
                    mProfillingLv = ProfilingLevel.All;
                    break;
                default:
                    mProfillingLv = ProfilingLevel.None;
                    break;
            }
            if (mProfillingLv == ProfilingLevel.Slow)
            {
                SystemManager.GetCurrentDataBase()
                    .SetProfilingLevel(mProfillingLv, new TimeSpan(0, 0, 0, 0, (int) NumTime.Value));
            }
            else
            {
                SystemManager.GetCurrentDataBase().SetProfilingLevel(mProfillingLv);
            }
            Close();
        }

        /// <summary>
        ///     Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     调整级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbProfillingLv_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbProfillingLv.SelectedIndex)
            {
                case 1:
                    NumTime.Enabled = true;
                    break;
                default:
                    NumTime.Enabled = false;
                    break;
            }
        }
    }
}