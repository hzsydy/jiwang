using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace jiwang.view
{
    public partial class FormAddGroup : Form
    {
        FormMain form;

        public FormAddGroup(FormMain form)
        {
            InitializeComponent();
            this.form = form;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            List<string> friends = new List<string>();
            foreach (object o in listBoxFriend.Items)
            {
                friends.Add((string)o);
            }
            string nickname = textBoxGroupName.Text;
            if (form != null)
            {
                form.BeginInvoke((Action)delegate { form.addFriend(nickname, friends); });
            }
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonDelFriend_Click(object sender, EventArgs e)
        {
            if (listBoxFriend.SelectedIndex != -1)
            {
                listBoxFriend.Items.RemoveAt(listBoxFriend.SelectedIndex);
            }
        }

        private void buttonAddFriend_Click(object sender, EventArgs e)
        {
            string newuser = Interaction.InputBox("请输入好友用户名", "计网大作业", "2014011493");
            listBoxFriend.Items.Add(newuser);
        }
    }
}
