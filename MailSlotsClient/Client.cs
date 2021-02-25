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
using System.Threading;

namespace MailSlots
{
    public partial class frmMain : Form
    {
        private Int32 HandleMailSlot, ReturnMailSlot;   // дескриптор мэйлслота
        private string mailslot;
        public string Mailslot { get { return mailslot; } set { mailslot = value; } }
        private string login;
        public string Login { get { return login; } set { login = value; } }
        private Thread t;                       // поток для обслуживания мэйлслота
        private bool _continue = true;          // флаг, указывающий продолжается ли работа с мэйлслотом

        // конструктор формы
        public frmMain(Int32 mailslot, string login)
        {
            InitializeComponent();
            this.HandleMailSlot = mailslot; this.Login = login;
            this.Text = "Клиент: " + Login;   // выводим имя текущей машины в заголовок формы
            SendMessage(Login);
            ReturnMailSlot = DIS.Import.CreateMailslot("\\\\.\\mailslot\\" + Login, 
                0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);
            Thread t = new Thread(ReceiveMessage);
            t.Start();
        }

        private void ReceiveMessage()
        {
            while (_continue)
            {
                uint realBytesReaded = 0;
                byte[] buff = new byte[1024];
                int MailslotSize = 0;       // максимальный размер сообщения
                int lpNextSize = 0;         // размер следующего сообщения
                int MessageCount = 0;       // количество сообщений в мэйлслоте
                DIS.Import.FlushFileBuffers(ReturnMailSlot);      // "принудительная" запись данных, расположенные в буфере операционной системы, в файл мэйлслота
                DIS.Import.GetMailslotInfo(ReturnMailSlot, MailslotSize, ref lpNextSize, 
                    ref MessageCount, 0);
                DIS.Import.ReadFile(ReturnMailSlot, buff, 1024, ref realBytesReaded, 0);      // считываем последовательность байтов из мэйлслота в буфер buff
                string msg = Encoding.Unicode.GetString(buff).Trim('\0');                 // выполняем преобразование байтов в последовательность символов
                if (msg != "")
                {
                    rtbMessages.Invoke((MethodInvoker)delegate { rtbMessages.Text += "\n >> " + msg; });
                }
            }
        }

        // присоединение к мэйлслоту
        //private void btnConnect_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // открываем мэйлслот, имя которого указано в поле tbMailSlot
        //        HandleMailSlot = DIS.Import.CreateFile(Mailslot, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
        //        if (HandleMailSlot != -1)
        //        {
        //            //btnConnect.Enabled = false;
        //            btnSend.Enabled = true;
        //        }
        //        else
        //            MessageBox.Show("Не удалось подключиться к мейлслоту");
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Не удалось подключиться к мейлслоту");
        //    }
        //}

        // отправка сообщения

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage(Login + " >> " + tbMessage.Text);
            tbMessage.Text = "";
        }

        private void SendMessage(string text)
        {
            uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
            byte[] buff = Encoding.Unicode.GetBytes(text);    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт

            DIS.Import.WriteFile(HandleMailSlot, buff, 
                Convert.ToUInt32(buff.Length), ref BytesWritten, 0);     // выполняем запись последовательности байт в мэйлслот
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SendMessage(Login + "_logout");
            DIS.Import.CloseHandle(HandleMailSlot);     // закрываем дескриптор мэйлслота
        }
    }
}