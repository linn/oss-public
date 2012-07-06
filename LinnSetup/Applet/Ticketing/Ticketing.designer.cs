namespace LinnSetup
{
    partial class Ticketing
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.testInfo = new System.Windows.Forms.Label();
            this.runTests = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.response = new System.Windows.Forms.TextBox();
            this.clear = new System.Windows.Forms.Button();
            this.submit = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.contactNotes = new System.Windows.Forms.TextBox();
            this.faultDesc = new System.Windows.Forms.TextBox();
            this.phone = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lastName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.email = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.firstName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ticketXml = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(667, 0);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.testInfo);
            this.panel1.Controls.Add(this.runTests);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.response);
            this.panel1.Controls.Add(this.clear);
            this.panel1.Controls.Add(this.submit);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.contactNotes);
            this.panel1.Controls.Add(this.faultDesc);
            this.panel1.Controls.Add(this.phone);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lastName);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.email);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.firstName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(667, 224);
            this.panel1.TabIndex = 4;
            // 
            // testInfo
            // 
            this.testInfo.AutoSize = true;
            this.testInfo.Location = new System.Drawing.Point(87, 156);
            this.testInfo.Name = "testInfo";
            this.testInfo.Size = new System.Drawing.Size(0, 13);
            this.testInfo.TabIndex = 25;
            // 
            // runTests
            // 
            this.runTests.Location = new System.Drawing.Point(6, 151);
            this.runTests.Name = "runTests";
            this.runTests.Size = new System.Drawing.Size(75, 23);
            this.runTests.TabIndex = 24;
            this.runTests.Text = "Run Tests";
            this.runTests.UseVisualStyleBackColor = true;
            this.runTests.Click += new System.EventHandler(this.runTests_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(202, 200);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "Response";
            // 
            // response
            // 
            this.response.Location = new System.Drawing.Point(263, 195);
            this.response.Name = "response";
            this.response.Size = new System.Drawing.Size(374, 20);
            this.response.TabIndex = 22;
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(87, 195);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(75, 23);
            this.clear.TabIndex = 21;
            this.clear.Text = "Clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // submit
            // 
            this.submit.Location = new System.Drawing.Point(6, 195);
            this.submit.Name = "submit";
            this.submit.Size = new System.Drawing.Size(75, 23);
            this.submit.TabIndex = 20;
            this.submit.Text = "Submit";
            this.submit.UseVisualStyleBackColor = true;
            this.submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(323, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Contact Notes";
            // 
            // contactNotes
            // 
            this.contactNotes.Location = new System.Drawing.Point(421, 52);
            this.contactNotes.Multiline = true;
            this.contactNotes.Name = "contactNotes";
            this.contactNotes.Size = new System.Drawing.Size(216, 78);
            this.contactNotes.TabIndex = 18;
            // 
            // faultDesc
            // 
            this.faultDesc.Location = new System.Drawing.Point(101, 50);
            this.faultDesc.Multiline = true;
            this.faultDesc.Name = "faultDesc";
            this.faultDesc.Size = new System.Drawing.Size(216, 80);
            this.faultDesc.TabIndex = 17;
            // 
            // phone
            // 
            this.phone.Location = new System.Drawing.Point(421, 26);
            this.phone.Name = "phone";
            this.phone.Size = new System.Drawing.Size(216, 20);
            this.phone.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(323, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Phone";
            // 
            // lastName
            // 
            this.lastName.Location = new System.Drawing.Point(421, 3);
            this.lastName.Name = "lastName";
            this.lastName.Size = new System.Drawing.Size(216, 20);
            this.lastName.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(323, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Last Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Fault Description";
            // 
            // email
            // 
            this.email.Location = new System.Drawing.Point(101, 26);
            this.email.Name = "email";
            this.email.Size = new System.Drawing.Size(216, 20);
            this.email.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Email";
            // 
            // firstName
            // 
            this.firstName.Location = new System.Drawing.Point(101, 3);
            this.firstName.Name = "firstName";
            this.firstName.Size = new System.Drawing.Size(216, 20);
            this.firstName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "First Name";
            // 
            // ticketXml
            // 
            this.ticketXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ticketXml.Location = new System.Drawing.Point(0, 224);
            this.ticketXml.MaxLength = 99999;
            this.ticketXml.Multiline = true;
            this.ticketXml.Name = "ticketXml";
            this.ticketXml.ReadOnly = true;
            this.ticketXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ticketXml.Size = new System.Drawing.Size(667, 50);
            this.ticketXml.TabIndex = 5;
            // 
            // Ticketing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ticketXml);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "Ticketing";
            this.Size = new System.Drawing.Size(667, 274);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox firstName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox email;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox contactNotes;
        private System.Windows.Forms.TextBox faultDesc;
        private System.Windows.Forms.TextBox phone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox lastName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox response;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.Button submit;
        private System.Windows.Forms.Label testInfo;
        private System.Windows.Forms.Button runTests;
        private System.Windows.Forms.TextBox ticketXml;
    }
}
