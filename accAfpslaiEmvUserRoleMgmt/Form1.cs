using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using accAfpslaiEmvObjct;
using System.Windows.Forms;

namespace accAfpslaiEmvUserRoleMgmt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static string MsgHeader = "User and Role Management";
        public static MiddleServerApi msa = null;
        public static user dcsUser = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            msa = new MiddleServerApi(Properties.Settings.Default.MiddleServerUrl, Properties.Settings.Default.BranchIssue, Properties.Settings.Default.ApiKey, MiddleServerApi.afpslaiEmvSystem.urm);

            accAfpslaiEmvLogIn.LogIN li = new accAfpslaiEmvLogIn.LogIN(Properties.Settings.Default.MiddleServerUrl, Properties.Settings.Default.BranchIssue, Properties.Settings.Default.ApiKey, MsgHeader);
            li.ShowDialog();
            if (li.IsSuccess)
            {
                dcsUser = li.dcsUser;
                msa.dcsUser = li.dcsUser;
                BindUsers();
            }
            else Environment.Exit(0);
        }

        private void BindUsers()
        {
            object obj = null;           
            if (msa.GetTable(MiddleServerApi.msApi.getSystemUser, ref obj))
            {
                var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<system_user>>(obj.ToString());
                //printTypes.Insert(0, new print_type { id = 0, printType = "-Select-" });
                //cbo.DataSource = printTypes;
                //cbo.DisplayMember = "printType";
                //cbo.ValueMember = "id";
                //cbo.SelectedIndex = 0;
                grid.DataSource = users.ToList();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

        }
        private bool ValidateFields()
        {
            StringBuilder sb = new StringBuilder();
            if (txtUsername.Text == "") sb.AppendLine("Please enter Username");
            if (txtFirst.Text == "") sb.AppendLine("Please enter First name");
            if (txtLast.Text == "") sb.AppendLine("Please enter Last name");
            if (cboRole.SelectedIndex == 0) sb.AppendLine("Please select Role");
            if (cboStatus.SelectedIndex == 0) sb.AppendLine("Please select Status");

            if (sb.ToString() == "") return true;
            else
            {
                accAfpslaiEmvObjct.Utilities.ShowWarningMessage(sb.ToString());
                return false;
            }
        }

    }
}
