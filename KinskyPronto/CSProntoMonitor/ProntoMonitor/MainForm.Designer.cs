/*
 * Created by SharpDevelop.
 * User: graham
 * Date: 25/03/2010
 * Time: 21:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace ProntoMonitor
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonClear = new System.Windows.Forms.Button();
			this.iTextBoxOutput = new System.Windows.Forms.TextBox();
			this.iComboBoxIp = new System.Windows.Forms.ComboBox();
			this.labelIpHeader = new System.Windows.Forms.Label();
			this.iLabelIpValue = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonClear
			// 
			this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClear.Location = new System.Drawing.Point(594, 386);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(75, 23);
			this.buttonClear.TabIndex = 0;
			this.buttonClear.Text = "Clear";
			this.buttonClear.UseVisualStyleBackColor = true;
			this.buttonClear.Click += new System.EventHandler(this.OnButtonClearClick);
			// 
			// iTextBoxOutput
			// 
			this.iTextBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.iTextBoxOutput.Location = new System.Drawing.Point(-1, 1);
			this.iTextBoxOutput.Multiline = true;
			this.iTextBoxOutput.Name = "iTextBoxOutput";
			this.iTextBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.iTextBoxOutput.Size = new System.Drawing.Size(708, 379);
			this.iTextBoxOutput.TabIndex = 1;
			// 
			// iComboBoxIp
			// 
			this.iComboBoxIp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.iComboBoxIp.FormattingEnabled = true;
			this.iComboBoxIp.Location = new System.Drawing.Point(87, 386);
			this.iComboBoxIp.Name = "iComboBoxIp";
			this.iComboBoxIp.Size = new System.Drawing.Size(121, 21);
			this.iComboBoxIp.TabIndex = 2;
			this.iComboBoxIp.SelectedIndexChanged += new System.EventHandler(this.OnComboBoxIpChange);
			// 
			// labelIpHeader
			// 
			this.labelIpHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelIpHeader.Location = new System.Drawing.Point(12, 383);
			this.labelIpHeader.Name = "labelIpHeader";
			this.labelIpHeader.Size = new System.Drawing.Size(69, 23);
			this.labelIpHeader.TabIndex = 3;
			this.labelIpHeader.Text = "Monitor IP:";
			this.labelIpHeader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// iLabelIpValue
			// 
			this.iLabelIpValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.iLabelIpValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.iLabelIpValue.Location = new System.Drawing.Point(78, 383);
			this.iLabelIpValue.Name = "iLabelIpValue";
			this.iLabelIpValue.Size = new System.Drawing.Size(81, 23);
			this.iLabelIpValue.TabIndex = 4;
			this.iLabelIpValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.iLabelIpValue.Visible = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(707, 410);
			this.Controls.Add(this.iLabelIpValue);
			this.Controls.Add(this.labelIpHeader);
			this.Controls.Add(this.iComboBoxIp);
			this.Controls.Add(this.iTextBoxOutput);
			this.Controls.Add(this.buttonClear);
			this.Name = "MainForm";
			this.Text = "ProntoMonitor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Shutdown);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Label iLabelIpValue;
		private System.Windows.Forms.ComboBox iComboBoxIp;
		private System.Windows.Forms.TextBox iTextBoxOutput;
		private System.Windows.Forms.Label labelIpHeader;
		private System.Windows.Forms.Button buttonClear;		
	}
}
