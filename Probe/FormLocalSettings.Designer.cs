namespace Probe
{
    partial class FormLocalSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLocalSettings));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.textReplayFolders = new System.Windows.Forms.TextBox();
            this.btnReplayFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textSc2Path = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.replaysFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.sc2pathDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(310, 72);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(229, 72);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // textReplayFolders
            // 
            this.textReplayFolders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textReplayFolders.Location = new System.Drawing.Point(91, 12);
            this.textReplayFolders.Name = "textReplayFolders";
            this.textReplayFolders.Size = new System.Drawing.Size(259, 20);
            this.textReplayFolders.TabIndex = 2;
            // 
            // btnReplayFolder
            // 
            this.btnReplayFolder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnReplayFolder.Image = global::Probe.Properties.Resources.folder;
            this.btnReplayFolder.Location = new System.Drawing.Point(356, 10);
            this.btnReplayFolder.Name = "btnReplayFolder";
            this.btnReplayFolder.Size = new System.Drawing.Size(28, 23);
            this.btnReplayFolder.TabIndex = 3;
            this.btnReplayFolder.UseVisualStyleBackColor = true;
            this.btnReplayFolder.Click += new System.EventHandler(this.btnReplayFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Replay Folder";
            // 
            // textSc2Path
            // 
            this.textSc2Path.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textSc2Path.Location = new System.Drawing.Point(91, 44);
            this.textSc2Path.Name = "textSc2Path";
            this.textSc2Path.Size = new System.Drawing.Size(259, 20);
            this.textSc2Path.TabIndex = 6;
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(356, 41);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 23);
            this.button2.TabIndex = 7;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Starcraft exe";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // sc2pathDialog
            // 
            this.sc2pathDialog.DefaultExt = "exe";
            this.sc2pathDialog.Filter = "Executable|*.exe|All files|*.*";
            // 
            // FormLocalSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 107);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textSc2Path);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReplayFolder);
            this.Controls.Add(this.textReplayFolders);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLocalSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Local settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLocalSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormLocalSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox textReplayFolders;
        private System.Windows.Forms.Button btnReplayFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textSc2Path;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FolderBrowserDialog replaysFolderDialog;
        private System.Windows.Forms.OpenFileDialog sc2pathDialog;
    }
}