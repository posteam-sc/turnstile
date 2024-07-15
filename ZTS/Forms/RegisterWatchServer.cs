using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZTS.DataAccess;

namespace ZTS.Forms
{
    public partial class RegisterWatchServer : Form
    {
        GateEntities db = new GateEntities();
        public RegisterWatchServer()
        {
            InitializeComponent();
        }

        private void RegisterWatchServer_Load(object sender, EventArgs e)
        {
            clear();
        }

        private void btnregister_Click(object sender, EventArgs e)
        {
            if (true)
            {
                Saver();
                clear();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            clear();
        }
        #region Functions
        private void LoadGrid()
        {
            dgvList.AutoGenerateColumns = false;
            var ts = db.TurnStileServers.ToList();
            dgvList.DataSource = ts;
        }
        private void Saver()
        {
            if (!string.IsNullOrEmpty(txtIp.Tag.ToString()))//edit
            {
                int Id = (int)txtIp.Tag;


                string Ip = txtIp.Text.Trim();
                string description = txtDescription.Text.Trim();
                int port = int.Parse(txtPort.Text.Trim());


                TurnStileServer ts = db.TurnStileServers.Find(Id);


                ts.ServerIP = Ip;
                ts.Description = description;
                ts.Port = port;

                db.Entry(ts).State = EntityState.Modified;
                int rowEffected = db.SaveChanges();
                if (rowEffected > 0)
                {

                }
            }
            else //entry
            {

                string Ip = txtIp.Text.Trim();
                string description = txtDescription.Text.Trim();
                int port = int.Parse(txtPort.Text.Trim());


                TurnStileServer ts = new TurnStileServer();

                ts.ServerIP = Ip;
                ts.Description = description;
                ts.Port = port;


                db.TurnStileServers.Add(ts);
                int rowEffected = db.SaveChanges();
                if (rowEffected > 0)
                {

                }
            }

        }
        void clear()
        {
            txtIp.Text = txtPort.Text=txtDescription.Text = "";

            txtIp.Tag = "";
            btnregister.Text = "Register";
            LoadGrid();
        }
        #endregion

        private void dgvList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow currentRow = dgvList.Rows[e.RowIndex];
                if (e.ColumnIndex == 4) //edit
                {


                    txtIp.Tag = currentRow.Cells["id"].Value;// Edit Mode Signal
                    var serverrunning = db.TurnStileServers.Find(Convert.ToInt16(currentRow.Cells["id"].Value));
                    if (serverrunning.Turnstiles.Where(a=>a.onoff==true).FirstOrDefault()!=null)
                    {
                        MessageBox.Show(this, "Cannot edit this server because it's in use. Please Turn Off turnstile first!!");
                        return;
                    }
                    btnregister.Text = "Update";

                    txtIp.Text = currentRow.Cells["IP"].Value.ToString();
                    txtDescription.Text = currentRow.Cells["Description"].Value.ToString();
                    txtPort.Text = currentRow.Cells["Port"].Value.ToString();
                  
                }
                else if (e.ColumnIndex == 5)//delete
                {
                    if (MessageBox.Show("Are you Sure want to delete?", "TCS", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        int id = (int)currentRow.Cells["id"].Value;
                        TurnStileServer tsTodelete = db.TurnStileServers.Find(id);
                        if (tsTodelete.Turnstiles!=null)
                        {
                            MessageBox.Show(this, "Cannot delete this server because it's in use.");
                            return;
                        }
                        db.TurnStileServers.Remove(tsTodelete);
                        int roweffected = db.SaveChanges();
                        if (roweffected > 0)
                        {
                            clear();
                        }
                    }
                }
            }
        }
    }
}
