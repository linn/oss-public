using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;

using Linn.ProductSupport;

namespace LinnSetup
{
    public partial class BasicSetup : UserControl
    {
        public BasicSetup(Target aTarget) {
            iTarget = aTarget;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            if (iTarget.Box.State == Box.EState.eOn && !iTarget.Box.IsProxy) {
                Enable();
            }
            else {
                Disable();
            }
            base.OnLoad(e);
        }

        public void Enable() {
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                  textBox1.Enabled = true;
                  textBox2.Enabled = true;
                  textBox3.Enabled = true;
                  comboBox1.Enabled = true;
                  comboBox2.Enabled = true;
                  checkBox1.Enabled = true;
                  checkBox2.Enabled = true;
                  checkBox3.Enabled = true;
                  checkBox4.Enabled = true;
                  pictureBox1.Enabled = true;
                  pictureBox2.Enabled = true;
                  listView1.Enabled = true;
            });

            if (!iEnabled) {
                iEnabled = true;

                iTarget.Box.BasicSetup.EventSetupValueChanged += SetupValueChanged;
                iTarget.Box.BasicSetup.EventSetupError += SetupError;
            }
        }

        public void Disable() {
            this.BeginInvoke(
              (MethodInvoker)delegate() {
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "";
                    comboBox1.SelectedIndex = -1;
                    comboBox2.Text = "";
                    checkBox1.Checked = false;
                    checkBox2.Checked = false;
                    checkBox3.Checked = false;
                    checkBox4.Checked = false;
                    pictureBox1.Image = null;
                    pictureBox2.Image = null;
                    listView1.Clear();
                    listView1.Items.Clear();
                    imageList1.Images.Clear();
                    comboBox1.Items.Clear();

                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    textBox3.Enabled = false;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;
                    checkBox4.Enabled = false;
                    pictureBox1.Enabled = false;
                    pictureBox2.Enabled = false;
                    listView1.Enabled = false;
            });

            if (iEnabled) {
                iEnabled = false;

                iTarget.Box.BasicSetup.EventSetupValueChanged -= SetupValueChanged;
                iTarget.Box.BasicSetup.EventSetupError -= SetupError;
            }
        }

        private void SetupValueChanged(object obj, EventArgsSetupValue e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                textBox1.Text = e.Room;
                if (comboBox1.Items.Count < e.SourceInfoList.Count) {
                    foreach (SourceInfo source in e.SourceInfoList) {
                        comboBox1.Items.Add(source.SystemName);
                    }
                    if (comboBox1.Items.Count > 0) {
                        comboBox1.SelectedIndex = 0;
                    }
                }
                else {
                    SourceInfo source = iTarget.Box.BasicSetup.SourceInfoAt(comboBox1.Text);
                    if (source != null) {
                        textBox2.Text = source.Name;
                        checkBox1.Checked = source.Visible;
                        checkBox4.Checked = source.IsStartup;
                        pictureBox1.ImageLocation = source.DefaultIcon.ImageUri;
                        pictureBox2.ImageLocation = source.Icon.ImageUri;
                        SetSelectedIcon(source.Icon.Name);
                    }
                }
                textBox3.Text = e.StartupVolume.ToString();
                checkBox2.Checked = e.StartupVolumeEnabled;
                checkBox3.Checked = e.AutoPlay;
                comboBox2.Text = e.TuneInUsername;
                if (comboBox2.Items.Count != e.TuneInUsernameAllowedValues.Count) {
                    foreach (string allowedValue in e.TuneInUsernameAllowedValues) {
                        comboBox2.Items.Add(allowedValue);
                    }
                }
            });
        }

        private void SetupError(object obj, EventArgsSetupError e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                MessageBox.Show("Setup Error", e.ErrorMessage);
            });
        }

        private void panel1_Click(object sender, EventArgs e) {
            iTarget.Box.BasicSetup.SetRoom(textBox1.Text);
            iTarget.Box.BasicSetup.SetSourceName(comboBox1.Text, textBox2.Text);
            iTarget.Box.BasicSetup.SetSourceIcon(comboBox1.Text, listView1.SelectedItems[0].Text);
            uint volume;
            if (uint.TryParse(textBox3.Text, out volume)) {
                iTarget.Box.BasicSetup.SetStartupVolume(volume);
            }
            iTarget.Box.BasicSetup.SetTuneInUsername(comboBox2.Text);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                iTarget.Box.BasicSetup.SetRoom(textBox1.Text);
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                iTarget.Box.BasicSetup.SetSourceName(comboBox1.Text, textBox2.Text);
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                uint volume;
                if (uint.TryParse(textBox3.Text, out volume)) {
                    iTarget.Box.BasicSetup.SetStartupVolume(volume);
                }
            }
        }

        private void comboBox2_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                iTarget.Box.BasicSetup.SetTuneInUsername(comboBox2.Text);
            }
        }

        private void checkBox1_Click(object sender, EventArgs e) {
            iTarget.Box.BasicSetup.SetSourceVisible(comboBox1.Text, checkBox1.Checked);
        }

        private void checkBox2_Click(object sender, EventArgs e) {
            iTarget.Box.BasicSetup.SetStartupVolumeEnabled(checkBox2.Checked);
        }

        private void checkBox3_Click(object sender, EventArgs e) {
            iTarget.Box.BasicSetup.SetAutoPlay(checkBox3.Checked);
        }

        private void checkBox4_Click(object sender, EventArgs e) {
            iTarget.Box.BasicSetup.SetStartupSource(comboBox1.Text, checkBox4.Checked);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                SourceInfo source = iTarget.Box.BasicSetup.SourceInfoAt(comboBox1.Text);
                if (source != null) {
                    textBox2.Text = source.Name;
                    checkBox1.Checked = source.Visible;
                    checkBox4.Checked = source.IsStartup;
                    pictureBox1.ImageLocation = source.DefaultIcon.ImageUri;
                    pictureBox2.ImageLocation = source.Icon.ImageUri;

                    listView1.Clear();
                    listView1.Items.Clear();
                    imageList1.Images.Clear();
                    foreach (SourceIcon icon in source.AllowedIcons) {
                        try {
                            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(icon.ImageUri);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            Image img = Image.FromStream(response.GetResponseStream());
                            response.Close();
                            imageList1.Images.Add(img);
                        }
                        catch {
                            imageList1.Images.Add(Linn.Kinsky.Properties.Resources.NoAlbumArt);
                        }
                        ListViewItem item = new ListViewItem();
                        item.Text = icon.Name;
                        item.ImageIndex = source.AllowedIcons.IndexOf(icon);
                        listView1.Items.Add(item);
                    }
                    SetSelectedIcon(source.Icon.Name);
                }
            });
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                iTarget.Box.BasicSetup.SetTuneInUsername(comboBox2.Text);
            });
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                iTarget.Box.BasicSetup.SetSourceIcon(comboBox1.Text, listView1.SelectedItems[0].Text);
            });
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            this.BeginInvoke(
            (MethodInvoker)delegate() {
                if (listView1.SelectedItems.Count == 1) {
                    iTarget.Box.BasicSetup.SetSourceIcon(comboBox1.Text, listView1.SelectedItems[0].Text);
                }
            });
        }

        private void SetSelectedIcon(string aIconName) {
            if (aIconName != null) {
                ListViewItem item = listView1.FindItemWithText(aIconName);
                if (item != null) {
                    listView1.SelectedIndexChanged -= listView1_SelectedIndexChanged;
                    item.Selected = true;
                    listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
                }
            }
        }

        private Target iTarget;
        private bool iEnabled = false;
    }
}
