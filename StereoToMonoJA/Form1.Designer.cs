namespace StereoToMonoJA
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
            this.groupBoxFiles = new System.Windows.Forms.GroupBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.lblInput = new System.Windows.Forms.Label();
            this.groupBoxDLL = new System.Windows.Forms.GroupBox();
            this.radioASM = new System.Windows.Forms.RadioButton();
            this.radioCPP = new System.Windows.Forms.RadioButton();
            this.lblDLLVersion = new System.Windows.Forms.Label();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.chkDithering = new System.Windows.Forms.CheckBox();
            this.chkRemoveDC = new System.Windows.Forms.CheckBox();
            this.chkNormalize = new System.Windows.Forms.CheckBox();
            this.chkAntiClipping = new System.Windows.Forms.CheckBox();
            this.radioEqualPower = new System.Windows.Forms.RadioButton();
            this.radioArithmetic = new System.Windows.Forms.RadioButton();
            this.lblMixMode = new System.Windows.Forms.Label();
            this.groupBoxThreads = new System.Windows.Forms.GroupBox();
            this.lblThreadsValue = new System.Windows.Forms.Label();
            this.trackBarThreads = new System.Windows.Forms.TrackBar();
            this.lblThreads = new System.Windows.Forms.Label();
            this.btnProcess = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblProcessingTime = new System.Windows.Forms.Label();
            this.groupBoxFiles.SuspendLayout();
            this.groupBoxDLL.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.groupBoxThreads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThreads)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxFiles
            // 
            this.groupBoxFiles.Controls.Add(this.btnBrowseOutput);
            this.groupBoxFiles.Controls.Add(this.btnBrowseInput);
            this.groupBoxFiles.Controls.Add(this.txtOutputFile);
            this.groupBoxFiles.Controls.Add(this.txtInputFile);
            this.groupBoxFiles.Controls.Add(this.lblOutput);
            this.groupBoxFiles.Controls.Add(this.lblInput);
            this.groupBoxFiles.Location = new System.Drawing.Point(12, 12);
            this.groupBoxFiles.Name = "groupBoxFiles";
            this.groupBoxFiles.Size = new System.Drawing.Size(760, 90);
            this.groupBoxFiles.TabIndex = 0;
            this.groupBoxFiles.TabStop = false;
            this.groupBoxFiles.Text = "Files";
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(670, 54);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOutput.TabIndex = 5;
            this.btnBrowseOutput.Text = "Browse...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(670, 24);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseInput.TabIndex = 2;
            this.btnBrowseInput.Text = "Browse...";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(100, 56);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(560, 20);
            this.txtOutputFile.TabIndex = 4;
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(100, 26);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(560, 20);
            this.txtInputFile.TabIndex = 1;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(15, 59);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(64, 13);
            this.lblOutput.TabIndex = 3;
            this.lblOutput.Text = "Output File:";
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(15, 29);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(56, 13);
            this.lblInput.TabIndex = 0;
            this.lblInput.Text = "Input File:";
            // 
            // groupBoxDLL
            // 
            this.groupBoxDLL.Controls.Add(this.radioASM);
            this.groupBoxDLL.Controls.Add(this.radioCPP);
            this.groupBoxDLL.Controls.Add(this.lblDLLVersion);
            this.groupBoxDLL.Location = new System.Drawing.Point(12, 108);
            this.groupBoxDLL.Name = "groupBoxDLL";
            this.groupBoxDLL.Size = new System.Drawing.Size(250, 90);
            this.groupBoxDLL.TabIndex = 1;
            this.groupBoxDLL.TabStop = false;
            this.groupBoxDLL.Text = "Processing Library";
            // 
            // radioASM
            // 
            this.radioASM.AutoSize = true;
            this.radioASM.Location = new System.Drawing.Point(130, 25);
            this.radioASM.Name = "radioASM";
            this.radioASM.Size = new System.Drawing.Size(109, 17);
            this.radioASM.TabIndex = 1;
            this.radioASM.Text = "Assembly (AVX2)";
            this.radioASM.UseVisualStyleBackColor = true;
            this.radioASM.CheckedChanged += new System.EventHandler(this.radioDLL_CheckedChanged);
            // 
            // radioCPP
            // 
            this.radioCPP.AutoSize = true;
            this.radioCPP.Checked = true;
            this.radioCPP.Location = new System.Drawing.Point(15, 25);
            this.radioCPP.Name = "radioCPP";
            this.radioCPP.Size = new System.Drawing.Size(45, 17);
            this.radioCPP.TabIndex = 0;
            this.radioCPP.TabStop = true;
            this.radioCPP.Text = "C++";
            this.radioCPP.UseVisualStyleBackColor = true;
            this.radioCPP.CheckedChanged += new System.EventHandler(this.radioDLL_CheckedChanged);
            // 
            // lblDLLVersion
            // 
            this.lblDLLVersion.AutoSize = true;
            this.lblDLLVersion.ForeColor = System.Drawing.Color.Blue;
            this.lblDLLVersion.Location = new System.Drawing.Point(15, 55);
            this.lblDLLVersion.Name = "lblDLLVersion";
            this.lblDLLVersion.Size = new System.Drawing.Size(88, 13);
            this.lblDLLVersion.TabIndex = 2;
            this.lblDLLVersion.Text = "Version: Loading...";
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Controls.Add(this.chkDithering);
            this.groupBoxOptions.Controls.Add(this.chkRemoveDC);
            this.groupBoxOptions.Controls.Add(this.chkNormalize);
            this.groupBoxOptions.Controls.Add(this.chkAntiClipping);
            this.groupBoxOptions.Controls.Add(this.radioEqualPower);
            this.groupBoxOptions.Controls.Add(this.radioArithmetic);
            this.groupBoxOptions.Controls.Add(this.lblMixMode);
            this.groupBoxOptions.Location = new System.Drawing.Point(268, 108);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(250, 150);
            this.groupBoxOptions.TabIndex = 2;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Processing Options";
            // 
            // chkDithering
            // 
            this.chkDithering.AutoSize = true;
            this.chkDithering.Location = new System.Drawing.Point(15, 120);
            this.chkDithering.Name = "chkDithering";
            this.chkDithering.Size = new System.Drawing.Size(68, 17);
            this.chkDithering.TabIndex = 6;
            this.chkDithering.Text = "Dithering";
            this.chkDithering.UseVisualStyleBackColor = true;
            // 
            // chkRemoveDC
            // 
            this.chkRemoveDC.AutoSize = true;
            this.chkRemoveDC.Location = new System.Drawing.Point(15, 100);
            this.chkRemoveDC.Name = "chkRemoveDC";
            this.chkRemoveDC.Size = new System.Drawing.Size(117, 17);
            this.chkRemoveDC.TabIndex = 5;
            this.chkRemoveDC.Text = "Remove DC Offset";
            this.chkRemoveDC.UseVisualStyleBackColor = true;
            // 
            // chkNormalize
            // 
            this.chkNormalize.AutoSize = true;
            this.chkNormalize.Location = new System.Drawing.Point(15, 80);
            this.chkNormalize.Name = "chkNormalize";
            this.chkNormalize.Size = new System.Drawing.Size(72, 17);
            this.chkNormalize.TabIndex = 4;
            this.chkNormalize.Text = "Normalize";
            this.chkNormalize.UseVisualStyleBackColor = true;
            // 
            // chkAntiClipping
            // 
            this.chkAntiClipping.AutoSize = true;
            this.chkAntiClipping.Checked = true;
            this.chkAntiClipping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAntiClipping.Location = new System.Drawing.Point(15, 60);
            this.chkAntiClipping.Name = "chkAntiClipping";
            this.chkAntiClipping.Size = new System.Drawing.Size(86, 17);
            this.chkAntiClipping.TabIndex = 3;
            this.chkAntiClipping.Text = "Anti-Clipping";
            this.chkAntiClipping.UseVisualStyleBackColor = true;
            // 
            // radioEqualPower
            // 
            this.radioEqualPower.AutoSize = true;
            this.radioEqualPower.Location = new System.Drawing.Point(130, 35);
            this.radioEqualPower.Name = "radioEqualPower";
            this.radioEqualPower.Size = new System.Drawing.Size(85, 17);
            this.radioEqualPower.TabIndex = 2;
            this.radioEqualPower.Text = "Equal-Power";
            this.radioEqualPower.UseVisualStyleBackColor = true;
            // 
            // radioArithmetic
            // 
            this.radioArithmetic.AutoSize = true;
            this.radioArithmetic.Checked = true;
            this.radioArithmetic.Location = new System.Drawing.Point(15, 35);
            this.radioArithmetic.Name = "radioArithmetic";
            this.radioArithmetic.Size = new System.Drawing.Size(73, 17);
            this.radioArithmetic.TabIndex = 1;
            this.radioArithmetic.TabStop = true;
            this.radioArithmetic.Text = "Arithmetic";
            this.radioArithmetic.UseVisualStyleBackColor = true;
            // 
            // lblMixMode
            // 
            this.lblMixMode.AutoSize = true;
            this.lblMixMode.Location = new System.Drawing.Point(15, 18);
            this.lblMixMode.Name = "lblMixMode";
            this.lblMixMode.Size = new System.Drawing.Size(59, 13);
            this.lblMixMode.TabIndex = 0;
            this.lblMixMode.Text = "Mix Mode:";
            // 
            // groupBoxThreads
            // 
            this.groupBoxThreads.Controls.Add(this.lblThreadsValue);
            this.groupBoxThreads.Controls.Add(this.trackBarThreads);
            this.groupBoxThreads.Controls.Add(this.lblThreads);
            this.groupBoxThreads.Location = new System.Drawing.Point(524, 108);
            this.groupBoxThreads.Name = "groupBoxThreads";
            this.groupBoxThreads.Size = new System.Drawing.Size(248, 90);
            this.groupBoxThreads.TabIndex = 3;
            this.groupBoxThreads.TabStop = false;
            this.groupBoxThreads.Text = "Multi-Threading";
            // 
            // lblThreadsValue
            // 
            this.lblThreadsValue.AutoSize = true;
            this.lblThreadsValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThreadsValue.Location = new System.Drawing.Point(115, 25);
            this.lblThreadsValue.Name = "lblThreadsValue";
            this.lblThreadsValue.Size = new System.Drawing.Size(16, 16);
            this.lblThreadsValue.TabIndex = 2;
            this.lblThreadsValue.Text = "4";
            // 
            // trackBarThreads
            // 
            this.trackBarThreads.Location = new System.Drawing.Point(15, 45);
            this.trackBarThreads.Maximum = 64;
            this.trackBarThreads.Minimum = 1;
            this.trackBarThreads.Name = "trackBarThreads";
            this.trackBarThreads.Size = new System.Drawing.Size(220, 45);
            this.trackBarThreads.TabIndex = 1;
            this.trackBarThreads.TickFrequency = 4;
            this.trackBarThreads.Value = 4;
            this.trackBarThreads.Scroll += new System.EventHandler(this.trackBarThreads_Scroll);
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Location = new System.Drawing.Point(15, 25);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(97, 13);
            this.lblThreads.TabIndex = 0;
            this.lblThreads.Text = "Number of threads:";
            // 
            // btnProcess
            // 
            this.btnProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProcess.Location = new System.Drawing.Point(524, 205);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(248, 53);
            this.btnProcess.TabIndex = 4;
            this.btnProcess.Text = "Convert to Mono";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 465);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(760, 23);
            this.progressBar.TabIndex = 7;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 445);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 13);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.Text = "Ready";
            // 
            // lblProcessingTime
            // 
            this.lblProcessingTime.AutoSize = true;
            this.lblProcessingTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProcessingTime.ForeColor = System.Drawing.Color.Green;
            this.lblProcessingTime.Location = new System.Drawing.Point(300, 445);
            this.lblProcessingTime.Name = "lblProcessingTime";
            this.lblProcessingTime.Size = new System.Drawing.Size(0, 15);
            this.lblProcessingTime.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 500);
            this.Controls.Add(this.lblProcessingTime);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.groupBoxThreads);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.groupBoxDLL);
            this.Controls.Add(this.groupBoxFiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stereo to Mono Converter - JAiO";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBoxFiles.ResumeLayout(false);
            this.groupBoxFiles.PerformLayout();
            this.groupBoxDLL.ResumeLayout(false);
            this.groupBoxDLL.PerformLayout();
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.groupBoxThreads.ResumeLayout(false);
            this.groupBoxThreads.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThreads)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxFiles;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.GroupBox groupBoxDLL;
        private System.Windows.Forms.RadioButton radioASM;
        private System.Windows.Forms.RadioButton radioCPP;
        private System.Windows.Forms.Label lblDLLVersion;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.CheckBox chkDithering;
        private System.Windows.Forms.CheckBox chkRemoveDC;
        private System.Windows.Forms.CheckBox chkNormalize;
        private System.Windows.Forms.CheckBox chkAntiClipping;
        private System.Windows.Forms.RadioButton radioEqualPower;
        private System.Windows.Forms.RadioButton radioArithmetic;
        private System.Windows.Forms.Label lblMixMode;
        private System.Windows.Forms.GroupBox groupBoxThreads;
        private System.Windows.Forms.Label lblThreadsValue;
        private System.Windows.Forms.TrackBar trackBarThreads;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblProcessingTime;
    }
}

