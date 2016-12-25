using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jiwang.view
{
    public partial class FormAddFriend : Form
    {
        FormMain form;

        public FormAddFriend(FormMain form)
        {
            InitializeComponent();
            this.form = form;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            List<string> friends = new List<string>();
            friends.Add(textBoxUserName.Text);
            string nickname = textBoxNickName.Text;
            if (form != null)
            {
                form.BeginInvoke((Action)delegate { form.addFriend(nickname, friends); });
            }
            this.Close();
        }
    }
}
