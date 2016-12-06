namespace jiwang
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonLogOut = new System.Windows.Forms.Button();
            this.buttonLogIn = new System.Windows.Forms.Button();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonDelFriend = new System.Windows.Forms.Button();
            this.buttonAddFriend = new System.Windows.Forms.Button();
            this.listBoxFriend = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonSend = new System.Windows.Forms.Button();
            this.textBoxMsgSend = new System.Windows.Forms.TextBox();
            this.textBoxMsgReceive = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonLogOut);
            this.groupBox1.Controls.Add(this.buttonLogIn);
            this.groupBox1.Controls.Add(this.textBoxPassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxUsername);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(147, 101);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "用户登录";
            // 
            // buttonLogOut
            // 
            this.buttonLogOut.Location = new System.Drawing.Point(83, 70);
            this.buttonLogOut.Name = "buttonLogOut";
            this.buttonLogOut.Size = new System.Drawing.Size(55, 24);
            this.buttonLogOut.TabIndex = 5;
            this.buttonLogOut.Text = "下线";
            this.buttonLogOut.UseVisualStyleBackColor = true;
            this.buttonLogOut.Click += new System.EventHandler(this.buttonLogOut_Click);
            // 
            // buttonLogIn
            // 
            this.buttonLogIn.Location = new System.Drawing.Point(8, 70);
            this.buttonLogIn.Name = "buttonLogIn";
            this.buttonLogIn.Size = new System.Drawing.Size(55, 24);
            this.buttonLogIn.TabIndex = 4;
            this.buttonLogIn.Text = "登录";
            this.buttonLogIn.UseVisualStyleBackColor = true;
            this.buttonLogIn.Click += new System.EventHandler(this.buttonLogIn_Click);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(59, 43);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(79, 21);
            this.textBoxPassword.TabIndex = 3;
            this.textBoxPassword.Text = "net2016";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = " 密码 ";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(59, 17);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(79, 21);
            this.textBoxUsername.TabIndex = 1;
            this.textBoxUsername.Text = "2014011493";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonDelFriend);
            this.groupBox2.Controls.Add(this.buttonAddFriend);
            this.groupBox2.Controls.Add(this.listBoxFriend);
            this.groupBox2.Location = new System.Drawing.Point(12, 119);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(147, 389);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "好友列表";
            // 
            // buttonDelFriend
            // 
            this.buttonDelFriend.Location = new System.Drawing.Point(6, 357);
            this.buttonDelFriend.Name = "buttonDelFriend";
            this.buttonDelFriend.Size = new System.Drawing.Size(132, 24);
            this.buttonDelFriend.TabIndex = 2;
            this.buttonDelFriend.Text = "删除好友";
            this.buttonDelFriend.UseVisualStyleBackColor = true;
            this.buttonDelFriend.Click += new System.EventHandler(this.buttonDelFriend_Click);
            // 
            // buttonAddFriend
            // 
            this.buttonAddFriend.Location = new System.Drawing.Point(6, 327);
            this.buttonAddFriend.Name = "buttonAddFriend";
            this.buttonAddFriend.Size = new System.Drawing.Size(132, 24);
            this.buttonAddFriend.TabIndex = 1;
            this.buttonAddFriend.Text = "添加好友";
            this.buttonAddFriend.UseVisualStyleBackColor = true;
            this.buttonAddFriend.Click += new System.EventHandler(this.buttonAddFriend_Click);
            // 
            // listBoxFriend
            // 
            this.listBoxFriend.FormattingEnabled = true;
            this.listBoxFriend.ItemHeight = 12;
            this.listBoxFriend.Location = new System.Drawing.Point(8, 17);
            this.listBoxFriend.Name = "listBoxFriend";
            this.listBoxFriend.Size = new System.Drawing.Size(130, 304);
            this.listBoxFriend.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.buttonSend);
            this.groupBox3.Controls.Add(this.textBoxMsgSend);
            this.groupBox3.Controls.Add(this.textBoxMsgReceive);
            this.groupBox3.Location = new System.Drawing.Point(164, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(529, 495);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "聊天窗口";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 353);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 21);
            this.button1.TabIndex = 3;
            this.button1.Text = "双开";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(451, 353);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(72, 21);
            this.buttonSend.TabIndex = 2;
            this.buttonSend.Text = "发送";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // textBoxMsgSend
            // 
            this.textBoxMsgSend.Location = new System.Drawing.Point(9, 380);
            this.textBoxMsgSend.Multiline = true;
            this.textBoxMsgSend.Name = "textBoxMsgSend";
            this.textBoxMsgSend.Size = new System.Drawing.Size(513, 108);
            this.textBoxMsgSend.TabIndex = 1;
            // 
            // textBoxMsgReceive
            // 
            this.textBoxMsgReceive.Location = new System.Drawing.Point(9, 17);
            this.textBoxMsgReceive.Multiline = true;
            this.textBoxMsgReceive.Name = "textBoxMsgReceive";
            this.textBoxMsgReceive.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMsgReceive.Size = new System.Drawing.Size(514, 330);
            this.textBoxMsgReceive.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(705, 519);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormMain";
            this.Text = "计算机网络大作业";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonLogOut;
        private System.Windows.Forms.Button buttonLogIn;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonDelFriend;
        private System.Windows.Forms.Button buttonAddFriend;
        private System.Windows.Forms.ListBox listBoxFriend;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBoxMsgReceive;
        private System.Windows.Forms.TextBox textBoxMsgSend;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Button button1;
    }
}

