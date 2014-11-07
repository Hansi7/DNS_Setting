﻿using SetDNS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SetDNS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //Icon = ShareXResources.Icon;

            AddDNS(Resources.DNSChangerForm_DNSChangerForm_Manual);
            AddDNS("Google Public DNS", "8.8.8.8", "8.8.4.4"); // https://developers.google.com/speed/public-dns/
            AddDNS("OpenDNS", "208.67.222.222", "208.67.220.220"); // http://www.opendns.com/
            AddDNS("Level 3 Communications", "4.2.2.1", "4.2.2.2"); // http://www.level3.com/
            AddDNS("Norton ConnectSafe", "199.85.126.10", "199.85.127.10"); // https://dns.norton.com/
            AddDNS("Comodo Secure DNS", "8.26.56.26", "8.20.247.20"); // http://www.comodo.com/secure-dns/
            AddDNS("DNS Advantage", "156.154.70.1", "156.154.71.1"); // http://www.neustar.biz/services/dns-services/free-recursive-dns
            AddDNS("Yandex DNS", "77.88.8.2", "77.88.8.88"); // http://dns.yandex.com/

            foreach (AdapterInfo adapter in AdapterInfo.GetEnabledAdapters())
            {
                cbAdapters.Items.Add(adapter);
            }

            if (cbAdapters.Items.Count > 0)
            {
                cbAdapters.SelectedIndex = 0;
            }
        }

        private void AddDNS(string name, string primary = null, string secondary = null)
        {
            cbDNSType.Items.Add(new DNSInfo(name, primary, secondary));
        }

        private void cbAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            AdapterInfo adapter = cbAdapters.SelectedItem as AdapterInfo;

            if (adapter != null)
            {
                string[] dns = adapter.GetDNS();

                if (dns != null && dns.Length == 2)
                {
                    cbAutomatic.Checked = false;
                    txtPreferredDNS.Text = dns[0];
                    txtAlternateDNS.Text = dns[1];
                }
                else
                {
                    cbAutomatic.Checked = true;
                }

                cbDNSType.SelectedIndex = 0;
            }

            UpdateControls();
        }

        private void cbAutomatic_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void cbDNSType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbDNSType.SelectedIndex > 0)
            {
                DNSInfo dnsInfo = cbDNSType.SelectedItem as DNSInfo;

                if (dnsInfo != null)
                {
                    txtPreferredDNS.Text = dnsInfo.PrimaryDNS;
                    txtAlternateDNS.Text = dnsInfo.SecondaryDNS;
                }
            }

            UpdateControls();
        }

        private void txtPreferredDNS_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void txtAlternateDNS_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            cbDNSType.Enabled = !cbAutomatic.Checked;
            txtPreferredDNS.Enabled = !cbAutomatic.Checked && cbDNSType.SelectedIndex == 0;
            txtAlternateDNS.Enabled = !cbAutomatic.Checked && cbDNSType.SelectedIndex == 0;
        }

        private void SendPing(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
            {
                btnPingPrimary.Enabled = btnPingSecondary.Enabled = false;

                TaskEx.Run(() =>
                {
                    PingResult pingResult = PingHelper.PingHost(ip);
                    MessageBox.Show(pingResult.ToString(), "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
                },
                () =>
                {
                    btnPingPrimary.Enabled = btnPingSecondary.Enabled = true;
                });
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            AdapterInfo adapter = cbAdapters.SelectedItem as AdapterInfo;

            if (adapter != null)
            {
                uint result;

                try
                {
                    if (cbAutomatic.Checked)
                    {
                        result = adapter.SetDNSAutomatic();
                    }
                    else
                    {
                        string primaryDNS = txtPreferredDNS.Text.Trim();
                        string secondaryDNS = txtAlternateDNS.Text.Trim();

                        if (Helpers.IsValidIPAddress(primaryDNS) && Helpers.IsValidIPAddress(secondaryDNS))
                        {
                            result = adapter.SetDNS(primaryDNS, secondaryDNS);
                        }
                        else
                        {
                            throw new Exception("Not valid IP address.");
                        }
                    }

                    if (result == 0)
                    {
                        NativeMethods.DnsFlushResolverCache();
                        MessageBox.Show(Resources.DNSChangerForm_btnSave_Click_DNS_successfully_set_, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (result == 1)
                    {
                        MessageBox.Show(Resources.DNSChangerForm_btnSave_Click_DNS_successfully_set__Reboot_is_required_, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (result > 1)
                    {
                        MessageBox.Show(Resources.DNSChangerForm_btnSave_Click_Setting_DNS_failed_with_error_code_ + " " + result, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Resources.DNSChangerForm_btnSave_Click_Setting_DNS_failed_ + "\r\n" + ex, "ShareX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnPingPrimary_Click(object sender, EventArgs e)
        {
            SendPing(txtPreferredDNS.Text);
        }

        private void btnPingSecondary_Click(object sender, EventArgs e)
        {
            SendPing(txtAlternateDNS.Text);
        }

        private void lblPreferredDNS_Click(object sender, EventArgs e)
        {

        }
    }
}
