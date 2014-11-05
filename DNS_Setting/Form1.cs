using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DNS_Setting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_SetDNS_Click(object sender, EventArgs e)
        {
            try 
	        {
                string str = (sender as Button).Tag.ToString();
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"[0123456789]+");
                int i = 0;

                if (int.TryParse(r.Match(str).ToString(),out i))
                {
                    Control[] c1 = Controls.Find("txt_G"+ i +"_1", true);
                    Control[] c2 = Controls.Find("txt_G" + i + "_2", true);

                    DnsHelper.SetDnsServers((c1[0] as TextBox).Text, (c2[0] as TextBox).Text);
                    MessageBox.Show("设置成功");
                }
	        }
	        catch (Exception)
	        {
                MessageBox.Show("设置失败");
	        }
        }

        private void btn_defult_Click(object sender, EventArgs e)
        {
            //.Start("cmd","netsh interface ip set dns name=以太网 dhcp");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = @"cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine("netsh interface ip set dns name=以太网 dhcp"); 
            p.StandardInput.WriteLine();
            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");
            string strRst = p.StandardOutput.ReadToEnd();
            p.Close();      
        }
    }
}
