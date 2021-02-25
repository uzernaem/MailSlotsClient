using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MailSlots
{
    public partial class Login : Form
    {
        private Int32 HandleMailSlot;
        Random rnd = new Random();

        public Login()
        {
            InitializeComponent();
            tbLogin.Text += rnd.Next();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                HandleMailSlot = DIS.Import.CreateFile("\\\\*\\mailslot\\ServerMailslot", 
                    DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, 
                    DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
                if (HandleMailSlot != -1)
                {
                    frmMain chat = new frmMain(HandleMailSlot, tbLogin.Text);
                    this.Hide();
                    chat.ShowDialog();
                    
                    this.Close();
                }
                else
                    MessageBox.Show("Не удалось подключиться к мейлслоту");
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к мейлслоту");
            }

        }
    }
}