namespace BedrockCosmos
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.StartButtonOld = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.HomePage = new System.Windows.Forms.TabPage();
            this.SettingsPage = new System.Windows.Forms.TabPage();
            this.DevPage = new System.Windows.Forms.TabPage();
            this.DevStatusLabel = new System.Windows.Forms.Label();
            this.DownloadZipProgressBar = new System.Windows.Forms.ProgressBar();
            this.DownloadZipButton = new System.Windows.Forms.Button();
            this.DevBackButton = new System.Windows.Forms.Button();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.AppIcon = new System.Windows.Forms.PictureBox();
            this.TopLabel = new System.Windows.Forms.Label();
            this.MinimizeButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.LaunchButton = new BedrockCosmos.RoundButton();
            this.TabControl.SuspendLayout();
            this.HomePage.SuspendLayout();
            this.DevPage.SuspendLayout();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButtonOld
            // 
            this.StartButtonOld.BackColor = System.Drawing.Color.Transparent;
            this.StartButtonOld.Location = new System.Drawing.Point(29, 46);
            this.StartButtonOld.Name = "StartButtonOld";
            this.StartButtonOld.Size = new System.Drawing.Size(75, 25);
            this.StartButtonOld.TabIndex = 0;
            this.StartButtonOld.Text = "Start";
            this.StartButtonOld.UseVisualStyleBackColor = false;
            this.StartButtonOld.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.BackColor = System.Drawing.Color.Transparent;
            this.StopButton.Location = new System.Drawing.Point(29, 77);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 1;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = false;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.ForeColor = System.Drawing.Color.White;
            this.StatusLabel.Location = new System.Drawing.Point(26, 107);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(52, 13);
            this.StatusLabel.TabIndex = 2;
            this.StatusLabel.Text = "Waiting...";
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.HomePage);
            this.TabControl.Controls.Add(this.SettingsPage);
            this.TabControl.Controls.Add(this.DevPage);
            this.TabControl.Location = new System.Drawing.Point(-5, -5);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(814, 466);
            this.TabControl.TabIndex = 5;
            // 
            // HomePage
            // 
            this.HomePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.HomePage.Controls.Add(this.LaunchButton);
            this.HomePage.Controls.Add(this.StartButtonOld);
            this.HomePage.Controls.Add(this.StopButton);
            this.HomePage.Controls.Add(this.StatusLabel);
            this.HomePage.Location = new System.Drawing.Point(4, 22);
            this.HomePage.Name = "HomePage";
            this.HomePage.Padding = new System.Windows.Forms.Padding(3);
            this.HomePage.Size = new System.Drawing.Size(806, 440);
            this.HomePage.TabIndex = 0;
            this.HomePage.Text = "Home Page";
            // 
            // SettingsPage
            // 
            this.SettingsPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.SettingsPage.Location = new System.Drawing.Point(4, 22);
            this.SettingsPage.Name = "SettingsPage";
            this.SettingsPage.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsPage.Size = new System.Drawing.Size(806, 440);
            this.SettingsPage.TabIndex = 1;
            this.SettingsPage.Text = "Settings Page";
            // 
            // DevPage
            // 
            this.DevPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.DevPage.Controls.Add(this.DevStatusLabel);
            this.DevPage.Controls.Add(this.DownloadZipProgressBar);
            this.DevPage.Controls.Add(this.DownloadZipButton);
            this.DevPage.Controls.Add(this.DevBackButton);
            this.DevPage.Location = new System.Drawing.Point(4, 22);
            this.DevPage.Name = "DevPage";
            this.DevPage.Size = new System.Drawing.Size(806, 440);
            this.DevPage.TabIndex = 2;
            this.DevPage.Text = "Dev Page";
            // 
            // DevStatusLabel
            // 
            this.DevStatusLabel.AutoSize = true;
            this.DevStatusLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DevStatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DevStatusLabel.Location = new System.Drawing.Point(110, 46);
            this.DevStatusLabel.Name = "DevStatusLabel";
            this.DevStatusLabel.Size = new System.Drawing.Size(420, 21);
            this.DevStatusLabel.TabIndex = 4;
            this.DevStatusLabel.Text = "The super duper secret download progress text will go here";
            // 
            // DownloadZipProgressBar
            // 
            this.DownloadZipProgressBar.Location = new System.Drawing.Point(178, 77);
            this.DownloadZipProgressBar.Name = "DownloadZipProgressBar";
            this.DownloadZipProgressBar.Size = new System.Drawing.Size(136, 23);
            this.DownloadZipProgressBar.TabIndex = 3;
            // 
            // DownloadZipButton
            // 
            this.DownloadZipButton.BackColor = System.Drawing.Color.Transparent;
            this.DownloadZipButton.Location = new System.Drawing.Point(29, 77);
            this.DownloadZipButton.Name = "DownloadZipButton";
            this.DownloadZipButton.Size = new System.Drawing.Size(143, 25);
            this.DownloadZipButton.TabIndex = 2;
            this.DownloadZipButton.Text = "Download Zip";
            this.DownloadZipButton.UseVisualStyleBackColor = false;
            // 
            // DevBackButton
            // 
            this.DevBackButton.BackColor = System.Drawing.Color.Transparent;
            this.DevBackButton.Location = new System.Drawing.Point(29, 46);
            this.DevBackButton.Name = "DevBackButton";
            this.DevBackButton.Size = new System.Drawing.Size(75, 25);
            this.DevBackButton.TabIndex = 1;
            this.DevBackButton.Text = "Back";
            this.DevBackButton.UseVisualStyleBackColor = false;
            this.DevBackButton.Click += new System.EventHandler(this.DevBackButton_Click);
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.TopPanel.Controls.Add(this.AppIcon);
            this.TopPanel.Controls.Add(this.TopLabel);
            this.TopPanel.Controls.Add(this.MinimizeButton);
            this.TopPanel.Controls.Add(this.CloseButton);
            this.TopPanel.Location = new System.Drawing.Point(-1, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(803, 46);
            this.TopPanel.TabIndex = 6;
            this.TopPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseDown);
            this.TopPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseMove);
            this.TopPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseUp);
            // 
            // AppIcon
            // 
            this.AppIcon.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AppIcon.BackgroundImage")));
            this.AppIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.AppIcon.Location = new System.Drawing.Point(8, 7);
            this.AppIcon.Name = "AppIcon";
            this.AppIcon.Size = new System.Drawing.Size(32, 32);
            this.AppIcon.TabIndex = 3;
            this.AppIcon.TabStop = false;
            this.AppIcon.Click += new System.EventHandler(this.AppIcon_Click);
            this.AppIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseDown);
            this.AppIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseMove);
            this.AppIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseUp);
            // 
            // TopLabel
            // 
            this.TopLabel.AutoSize = true;
            this.TopLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TopLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.TopLabel.Location = new System.Drawing.Point(46, 12);
            this.TopLabel.Name = "TopLabel";
            this.TopLabel.Size = new System.Drawing.Size(126, 21);
            this.TopLabel.TabIndex = 2;
            this.TopLabel.Text = "Bedrock Cosmos";
            this.TopLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseDown);
            this.TopLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseMove);
            this.TopLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TopPanel_MouseUp);
            // 
            // MinimizeButton
            // 
            this.MinimizeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MinimizeButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MinimizeButton.BackgroundImage")));
            this.MinimizeButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.MinimizeButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.MinimizeButton.FlatAppearance.BorderSize = 0;
            this.MinimizeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.MinimizeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.MinimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MinimizeButton.Location = new System.Drawing.Point(732, 10);
            this.MinimizeButton.Name = "MinimizeButton";
            this.MinimizeButton.Size = new System.Drawing.Size(25, 25);
            this.MinimizeButton.TabIndex = 1;
            this.MinimizeButton.UseVisualStyleBackColor = true;
            this.MinimizeButton.Click += new System.EventHandler(this.MinimizeButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CloseButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CloseButton.BackgroundImage")));
            this.CloseButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CloseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.CloseButton.FlatAppearance.BorderSize = 0;
            this.CloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.CloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Location = new System.Drawing.Point(767, 10);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(25, 25);
            this.CloseButton.TabIndex = 0;
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // LaunchButton
            // 
            this.LaunchButton.BackColor = System.Drawing.Color.Transparent;
            this.LaunchButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.LaunchButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.LaunchButton.FilledBackColor = System.Drawing.Color.Purple;
            this.LaunchButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.LaunchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.LaunchButton.HoverBackColor = System.Drawing.Color.Purple;
            this.LaunchButton.HoverForeColor = System.Drawing.Color.Purple;
            this.LaunchButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.LaunchButton.Location = new System.Drawing.Point(123, 46);
            this.LaunchButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.LaunchButton.Name = "LaunchButton";
            this.LaunchButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.LaunchButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.LaunchButton.PressedBackColor = System.Drawing.Color.Purple;
            this.LaunchButton.PressedForeColor = System.Drawing.Color.Purple;
            this.LaunchButton.Radius = 20;
            this.LaunchButton.Size = new System.Drawing.Size(144, 47);
            this.LaunchButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.LaunchButton.TabIndex = 3;
            this.LaunchButton.Text = "Lunch Time";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.TabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Bedrock Cosmos";
            this.TabControl.ResumeLayout(false);
            this.HomePage.ResumeLayout(false);
            this.HomePage.PerformLayout();
            this.DevPage.ResumeLayout(false);
            this.DevPage.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AppIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button StartButtonOld;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage HomePage;
        private System.Windows.Forms.TabPage SettingsPage;
        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button MinimizeButton;
        private System.Windows.Forms.Label TopLabel;
        private System.Windows.Forms.PictureBox AppIcon;
        private System.Windows.Forms.TabPage DevPage;
        private System.Windows.Forms.Button DevBackButton;
        private System.Windows.Forms.Label DevStatusLabel;
        private System.Windows.Forms.ProgressBar DownloadZipProgressBar;
        private System.Windows.Forms.Button DownloadZipButton;
        private RoundButton LaunchButton;
    }
}

