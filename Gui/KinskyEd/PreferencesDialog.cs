using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {

public partial class PreferencesDialog : System.Windows.Forms.Form
{
    public PreferencesDialog()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();

        //
        // TODO: Add any constructor code after InitializeComponent call
        //
        packageTextBox.Text = PackageManager.Instance.RootDirectory;
        textureTextBox.Text = TextureManager.Instance.RootDirectory;
    }
       
    private void packageButtonBrowse_Click(object sender, System.EventArgs e) {
        System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        folderBrowserDialog.Description = "Select the directory that you want to use as the default package cache.";
        folderBrowserDialog.SelectedPath = PackageManager.Instance.RootDirectory;
        if(folderBrowserDialog.ShowDialog() == DialogResult.OK) {
            Trace.WriteLine(Trace.kKinskyEd, "Package Cache: " + folderBrowserDialog.SelectedPath);
            packageTextBox.Text = folderBrowserDialog.SelectedPath;
        }
    }
    
    private void textureButtonBrowse_Click(object sender, System.EventArgs e) {
        System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        folderBrowserDialog.Description = "Select the directory that you want to use as the default texture cache.";
        folderBrowserDialog.SelectedPath = TextureManager.Instance.RootDirectory;
        if(folderBrowserDialog.ShowDialog() == DialogResult.OK) {
            Trace.WriteLine(Trace.kKinskyEd, "Texture Cache: " + folderBrowserDialog.SelectedPath);
            textureTextBox.Text = folderBrowserDialog.SelectedPath;
        }
    }
    
    private void saveButton_Click(object sender, System.EventArgs e) {
        PackageManager.Instance.PathList[0] = packageTextBox.Text;
        TextureManager.Instance.PathList[0] = textureTextBox.Text;
    }
}

} // Editor
} // Gui
} // Linn
