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

            txtFirst.KeyPress += textBox1_KeyPress;
            txtMiddle.KeyPress += textBox1_KeyPress;
            txtLast.KeyPress += textBox1_KeyPress;
            txtSuffix.KeyPress += textBox1_KeyPress;
        }
        
        public static MiddleServerApi msa = null;
        public static user dcsUser = null;
        public state formState = Form1.state.init;
        private static int userId = 0;

        public enum state
        {
            init = 0,
            edit
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            grid.AutoGenerateColumns = false;
            msa = new MiddleServerApi(Properties.Settings.Default.MiddleServerUrl, Properties.Settings.Default.ApiKey, Properties.Settings.Default.BranchIssue, MiddleServerApi.afpslaiEmvSystem.urm);
            linkLabel1.Text = Properties.Settings.Default.MiddleServerUrl;

            //accAfpslaiEmvLogIn.LogIN li = new accAfpslaiEmvLogIn.LogIN(Properties.Settings.Default.MiddleServerUrl, Properties.Settings.Default.ApiKey, Properties.Settings.Default.BranchIssue, MsgHeader);
            accAfpslaiEmvLogIn.LogIN li = new accAfpslaiEmvLogIn.LogIN(msa);
            li.ShowDialog();
            if (li.IsSuccess)
            {
                //dcsUser = li.dcsUser;
                //msa.dcsUser = li.dcsUser;
                if (msa.dcsUser.roleId == 2)
                {
                    BindGrid();
                    PopulateRoles();
                    cboStatus.SelectedIndex = 1;
                }
                else
                {
                    Utilities.ShowWarningMessage("User have no access to this application");
                    Environment.Exit(0);
                }

            }
            else Environment.Exit(0);
        }

        private List<user> users = null;

        private void BindGrid()
        {
            object obj = null;
            if (msa.GetTable(MiddleServerApi.msApi.getSystemUser, ref obj))
            {
                users = null;
                users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<user>>(obj.ToString());
                grid.DataSource = users.Where(o => o.is_deleted == false && o.status != "Not active").ToList();
            }
        }

        private void BindGrid2()
        {
            if (!chkDeleted.Checked && !chkStatus.Checked) grid.DataSource = users.Where(o => o.is_deleted == false && o.status != "Not active").ToList();
            else if (chkDeleted.Checked && !chkStatus.Checked)
            {
                var revised = users.Where(o => o.status != "Not active");
                grid.DataSource = revised.ToList();
            }
            else if (!chkDeleted.Checked && chkStatus.Checked)
            {
                var revised = users.Where(o => o.is_deleted == false);
                grid.DataSource = revised.ToList();

            }
            else if (chkDeleted.Checked && chkStatus.Checked)
            {
                var revised = users;
                grid.DataSource = revised.ToList();
            }
        }

        private void PopulateRoles()
        {
            object obj = null;
            if (msa.GetTable(MiddleServerApi.msApi.getRole, ref obj))
            {
                var roles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<system_role>>(obj.ToString());
                roles.Insert(0, new system_role { id = 0, role = "-Select-" });
                cboRole.DataSource = roles;
                cboRole.DisplayMember = "role";
                cboRole.ValueMember = "id";
                cboRole.SelectedIndex = 0;
            }
        }

        private void ResetForm()
        {
            userId = 0;
            txtUsername.Clear();
            txtFirst.Clear();
            txtMiddle.Clear();
            txtLast.Clear();
            txtSuffix.Clear();
            cboRole.SelectedIndex = 0;
            cboStatus.SelectedIndex = 1;
            btnAdd.Text = "ADD";
            btnEdit.Text = "EDIT";
            btnDelete.Visible = true;
            btnResetPass.Visible = true;
            formState = state.init;
        }

        private bool ValidateFields()
        {
            StringBuilder sb = new StringBuilder();
            if (txtUsername.Text == "") sb.AppendLine("Please enter Username");
            if (txtFirst.Text == "") sb.AppendLine("Please enter First name");
            if (txtLast.Text == "") sb.AppendLine("Please enter Last name");
            if (cboRole.SelectedIndex == 0) sb.AppendLine("Please select Role");
            if (cboStatus.SelectedIndex == 0) sb.AppendLine("Please select Status");

            if (txtUsername.Text.Length < 3) sb.AppendLine("Please enter valid Username");
            
            //System.Text.RegularExpressions.Regex r1 = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9]*$");
            //System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex(@"^[\.a-zA-Z0-9,!? ]*$");
            //if (r1.IsMatch(txtFirst.Text)) sb.AppendLine("Alphanumeric is not allowed in First name");
            //if (r2.IsMatch(txtMiddle.Text)) sb.AppendLine("Alphanumeric is not allowed in Middle name");
            //if (r2.IsMatch(txtLast.Text)) sb.AppendLine("Alphanumeric is not allowed in Last name");
            //if (r2.IsMatch(txtSuffix.Text)) sb.AppendLine("Alphanumeric is not allowed in Suffix");

            if (sb.ToString() == "") return true;
            else
            {
                accAfpslaiEmvObjct.Utilities.ShowWarningMessage(sb.ToString());
                return false;
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                system_user user = new system_user();
                user.id = userId;
                user.user_name = txtUsername.Text;
                user.first_name = txtFirst.Text;
                user.middle_name = txtMiddle.Text;
                user.last_name = txtLast.Text;
                user.suffix = txtSuffix.Text;
                user.role_id = (int)cboRole.SelectedValue;
                user.status = cboStatus.Text.Trim();
                if (msa.addEditUser(user))
                {
                    BindGrid();
                    ResetForm();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(grid.Rows[grid.CurrentRow.Index].Cells[0].Value.ToString().Trim());
            string userName = grid.Rows[grid.CurrentRow.Index].Cells[1].Value.ToString().Trim();
            if (MessageBox.Show("Are you sure you want to delete '" + userName + "'?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                system_user user = new system_user();
                user.id = id;
                if (msa.addDeleteGenericTable(user, false)) BindGrid();
            }
        }       

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (formState == state.init)
            {
                formState = state.edit;
                userId = Convert.ToInt32(grid.Rows[grid.CurrentRow.Index].Cells[0].Value.ToString().Trim());
                txtUsername.Text = grid.Rows[grid.CurrentRow.Index].Cells[1].Value.ToString().Trim();
                txtFirst.Text = grid.Rows[grid.CurrentRow.Index].Cells[2].Value.ToString().Trim();
                if (grid.Rows[grid.CurrentRow.Index].Cells[3].Value != null) txtMiddle.Text = grid.Rows[grid.CurrentRow.Index].Cells[3].Value.ToString().Trim();
                txtLast.Text = grid.Rows[grid.CurrentRow.Index].Cells[4].Value.ToString().Trim();
                if (grid.Rows[grid.CurrentRow.Index].Cells[5].Value != null) txtSuffix.Text = grid.Rows[grid.CurrentRow.Index].Cells[5].Value.ToString().Trim();
                cboRole.SelectedIndex = cboRole.FindString(grid.Rows[grid.CurrentRow.Index].Cells[7].Value.ToString().Trim());
                cboStatus.SelectedIndex = cboStatus.FindString(grid.Rows[grid.CurrentRow.Index].Cells[8].Value.ToString().Trim());

                btnAdd.Text = "SAVE";
                btnEdit.Text = "CANCEL";
                btnDelete.Visible = false;
                btnResetPass.Visible = false;
            }
            else
            {
                ResetForm();
            }
        }

        private void btnResetPass_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(grid.Rows[grid.CurrentRow.Index].Cells[0].Value.ToString().Trim());
            string userName = grid.Rows[grid.CurrentRow.Index].Cells[1].Value.ToString().Trim();
            if (MessageBox.Show("Are you sure you want to reset password of '" + userName + "'?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                system_user user = new system_user();
                user.id = id;
                if (msa.resetUserPassword(user)) BindGrid();
            }
        }

        private void chkStatus_CheckedChanged(object sender, EventArgs e)
        {
            BindGrid2();
        }

        private void chkDeleted_CheckedChanged(object sender, EventArgs e)
        {
            BindGrid2();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(char.IsLetter(e.KeyChar) || e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Space || e.KeyChar == (char)'-');
        }
    }
}
