namespace ImageOperations
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.bOpen = new System.Windows.Forms.Button();
            this.openImageDialog = new System.Windows.Forms.OpenFileDialog();
            this.pictureSource = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textThreshold = new System.Windows.Forms.TextBox();
            this.pictureResult = new System.Windows.Forms.PictureBox();
            this.textGrayTolerance = new System.Windows.Forms.TextBox();
            this.bFindLetters = new System.Windows.Forms.Button();
            this.textLetterThreshold = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btnBenchmark = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureResult)).BeginInit();
            this.SuspendLayout();
            // 
            // bOpen
            // 
            this.bOpen.Location = new System.Drawing.Point(12, 12);
            this.bOpen.Name = "bOpen";
            this.bOpen.Size = new System.Drawing.Size(75, 23);
            this.bOpen.TabIndex = 0;
            this.bOpen.Text = "Open...";
            this.bOpen.UseVisualStyleBackColor = true;
            this.bOpen.Click += new System.EventHandler(this.button1_Click);
            // 
            // openImageDialog
            // 
            this.openImageDialog.FileName = "openImageDialog";
            // 
            // pictureSource
            // 
            this.pictureSource.Image = global::ImageOperations.Properties.Resources.samples;
            this.pictureSource.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureSource.InitialImage")));
            this.pictureSource.Location = new System.Drawing.Point(12, 41);
            this.pictureSource.Name = "pictureSource";
            this.pictureSource.Size = new System.Drawing.Size(371, 50);
            this.pictureSource.TabIndex = 1;
            this.pictureSource.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(93, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Threshold";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // textThreshold
            // 
            this.textThreshold.Location = new System.Drawing.Point(175, 15);
            this.textThreshold.Name = "textThreshold";
            this.textThreshold.Size = new System.Drawing.Size(39, 20);
            this.textThreshold.TabIndex = 3;
            this.textThreshold.Text = "200";
            // 
            // pictureResult
            // 
            this.pictureResult.Location = new System.Drawing.Point(12, 107);
            this.pictureResult.Name = "pictureResult";
            this.pictureResult.Size = new System.Drawing.Size(279, 50);
            this.pictureResult.TabIndex = 4;
            this.pictureResult.TabStop = false;
            // 
            // textGrayTolerance
            // 
            this.textGrayTolerance.Location = new System.Drawing.Point(220, 15);
            this.textGrayTolerance.Name = "textGrayTolerance";
            this.textGrayTolerance.Size = new System.Drawing.Size(39, 20);
            this.textGrayTolerance.TabIndex = 5;
            this.textGrayTolerance.Text = "90";
            // 
            // bFindLetters
            // 
            this.bFindLetters.Location = new System.Drawing.Point(266, 12);
            this.bFindLetters.Name = "bFindLetters";
            this.bFindLetters.Size = new System.Drawing.Size(75, 23);
            this.bFindLetters.TabIndex = 6;
            this.bFindLetters.Text = "FindLetters";
            this.bFindLetters.UseVisualStyleBackColor = true;
            this.bFindLetters.Click += new System.EventHandler(this.bFindLetters_Click);
            // 
            // textLetterThreshold
            // 
            this.textLetterThreshold.Location = new System.Drawing.Point(347, 12);
            this.textLetterThreshold.Name = "textLetterThreshold";
            this.textLetterThreshold.Size = new System.Drawing.Size(39, 20);
            this.textLetterThreshold.TabIndex = 7;
            this.textLetterThreshold.Text = "50";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(393, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "Recognize";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnBenchmark
            // 
            this.btnBenchmark.Location = new System.Drawing.Point(474, 13);
            this.btnBenchmark.Name = "btnBenchmark";
            this.btnBenchmark.Size = new System.Drawing.Size(75, 23);
            this.btnBenchmark.TabIndex = 9;
            this.btnBenchmark.Text = "Benchmark";
            this.btnBenchmark.UseVisualStyleBackColor = true;
            this.btnBenchmark.Click += new System.EventHandler(this.btnBenchmark_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 262);
            this.Controls.Add(this.btnBenchmark);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textLetterThreshold);
            this.Controls.Add(this.bFindLetters);
            this.Controls.Add(this.textGrayTolerance);
            this.Controls.Add(this.pictureResult);
            this.Controls.Add(this.textThreshold);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureSource);
            this.Controls.Add(this.bOpen);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bOpen;
        private System.Windows.Forms.OpenFileDialog openImageDialog;
        private System.Windows.Forms.PictureBox pictureSource;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textThreshold;
        private System.Windows.Forms.PictureBox pictureResult;
        private System.Windows.Forms.TextBox textGrayTolerance;
        private System.Windows.Forms.Button bFindLetters;
        private System.Windows.Forms.TextBox textLetterThreshold;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnBenchmark;
    }
}

