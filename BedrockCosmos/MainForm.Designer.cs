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
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.HomePage = new System.Windows.Forms.TabPage();
            this.SettingsPage = new System.Windows.Forms.TabPage();
            this.DevPage = new System.Windows.Forms.TabPage();
            this.DevBackButton = new System.Windows.Forms.Button();
            this.DevConsole = new System.Windows.Forms.RichTextBox();
            this.EnableLoggingLabel = new System.Windows.Forms.Label();
            this.DownloadZipProgressLabel = new System.Windows.Forms.Label();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.AppIcon = new System.Windows.Forms.PictureBox();
            this.TopLabel = new System.Windows.Forms.Label();
            this.MinimizeButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.LaunchButton = new BedrockCosmos.App.UI.RoundGradientButton();
            this.DisableDevMenuButton = new BedrockCosmos.App.UI.RoundButton();
            this.ClearLogsButton = new BedrockCosmos.App.UI.RoundButton();
            this.EnableLoggingSwitch = new BedrockCosmos.App.UI.Switch();
            this.DownloadZipButton = new BedrockCosmos.App.UI.RoundButton();
            this.ExportLogsButton = new BedrockCosmos.App.UI.RoundButton();
            this.TabControl.SuspendLayout();
            this.HomePage.SuspendLayout();
            this.DevPage.SuspendLayout();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.StatusLabel.ForeColor = System.Drawing.Color.White;
            this.StatusLabel.Location = new System.Drawing.Point(227, 278);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(57, 13);
            this.StatusLabel.TabIndex = 2;
            this.StatusLabel.Text = "Waiting...";
            this.StatusLabel.Visible = false;
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
            this.DevPage.Controls.Add(this.DevBackButton);
            this.DevPage.Controls.Add(this.DisableDevMenuButton);
            this.DevPage.Controls.Add(this.DevConsole);
            this.DevPage.Controls.Add(this.ClearLogsButton);
            this.DevPage.Controls.Add(this.EnableLoggingLabel);
            this.DevPage.Controls.Add(this.EnableLoggingSwitch);
            this.DevPage.Controls.Add(this.DownloadZipButton);
            this.DevPage.Controls.Add(this.ExportLogsButton);
            this.DevPage.Controls.Add(this.DownloadZipProgressLabel);
            this.DevPage.Location = new System.Drawing.Point(4, 22);
            this.DevPage.Name = "DevPage";
            this.DevPage.Size = new System.Drawing.Size(806, 440);
            this.DevPage.TabIndex = 2;
            this.DevPage.Text = "Dev Page";
            // 
            // DevBackButton
            // 
            this.DevBackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DevBackButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DevBackButton.BackgroundImage")));
            this.DevBackButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.DevBackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DevBackButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.DevBackButton.FlatAppearance.BorderSize = 0;
            this.DevBackButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.DevBackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(15)))));
            this.DevBackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DevBackButton.Location = new System.Drawing.Point(15, 391);
            this.DevBackButton.Name = "DevBackButton";
            this.DevBackButton.Size = new System.Drawing.Size(25, 25);
            this.DevBackButton.TabIndex = 4;
            this.DevBackButton.UseVisualStyleBackColor = true;
            this.DevBackButton.Click += new System.EventHandler(this.DevBackButton_Click);
            // 
            // DevConsole
            // 
            this.DevConsole.BackColor = System.Drawing.Color.Black;
            this.DevConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DevConsole.ForeColor = System.Drawing.Color.White;
            this.DevConsole.Location = new System.Drawing.Point(290, 41);
            this.DevConsole.Name = "DevConsole";
            this.DevConsole.ReadOnly = true;
            this.DevConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.DevConsole.Size = new System.Drawing.Size(499, 340);
            this.DevConsole.TabIndex = 13;
            this.DevConsole.Text = "";
            // 
            // EnableLoggingLabel
            // 
            this.EnableLoggingLabel.AutoSize = true;
            this.EnableLoggingLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.EnableLoggingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.EnableLoggingLabel.Location = new System.Drawing.Point(641, 394);
            this.EnableLoggingLabel.Name = "EnableLoggingLabel";
            this.EnableLoggingLabel.Size = new System.Drawing.Size(103, 19);
            this.EnableLoggingLabel.TabIndex = 11;
            this.EnableLoggingLabel.Text = "Enable Logging";
            // 
            // DownloadZipProgressLabel
            // 
            this.DownloadZipProgressLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.DownloadZipProgressLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DownloadZipProgressLabel.Location = new System.Drawing.Point(161, 43);
            this.DownloadZipProgressLabel.Name = "DownloadZipProgressLabel";
            this.DownloadZipProgressLabel.Size = new System.Drawing.Size(121, 38);
            this.DownloadZipProgressLabel.TabIndex = 4;
            this.DownloadZipProgressLabel.Text = "Downloading...";
            this.DownloadZipProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DownloadZipProgressLabel.Visible = false;
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
            this.LaunchButton.FilledBackColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(47)))));
            this.LaunchButton.FilledBackColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(188)))), ((int)(((byte)(71)))));
            this.LaunchButton.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold);
            this.LaunchButton.ForeColor = System.Drawing.Color.White;
            this.LaunchButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(188)))), ((int)(((byte)(71)))));
            this.LaunchButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(188)))), ((int)(((byte)(71)))));
            this.LaunchButton.HoverForeColor = System.Drawing.Color.White;
            this.LaunchButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.LaunchButton.Location = new System.Drawing.Point(230, 175);
            this.LaunchButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.LaunchButton.Name = "LaunchButton";
            this.LaunchButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(188)))), ((int)(((byte)(71)))));
            this.LaunchButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.LaunchButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(188)))), ((int)(((byte)(71)))));
            this.LaunchButton.PressedForeColor = System.Drawing.Color.White;
            this.LaunchButton.Radius = 10;
            this.LaunchButton.Size = new System.Drawing.Size(340, 100);
            this.LaunchButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.LaunchButton.TabIndex = 6;
            this.LaunchButton.Text = "LAUNCH";
            this.LaunchButton.Click += new System.EventHandler(this.LaunchButton_Click);
            // 
            // DisableDevMenuButton
            // 
            this.DisableDevMenuButton.BackColor = System.Drawing.Color.Transparent;
            this.DisableDevMenuButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DisableDevMenuButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.DisableDevMenuButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.DisableDevMenuButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.DisableDevMenuButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DisableDevMenuButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DisableDevMenuButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DisableDevMenuButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DisableDevMenuButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.DisableDevMenuButton.Location = new System.Drawing.Point(13, 190);
            this.DisableDevMenuButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.DisableDevMenuButton.Name = "DisableDevMenuButton";
            this.DisableDevMenuButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DisableDevMenuButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.DisableDevMenuButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DisableDevMenuButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DisableDevMenuButton.Radius = 5;
            this.DisableDevMenuButton.Size = new System.Drawing.Size(144, 47);
            this.DisableDevMenuButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.DisableDevMenuButton.TabIndex = 14;
            this.DisableDevMenuButton.Text = "Disable Dev Menu";
            this.DisableDevMenuButton.Click += new System.EventHandler(this.DisableDevMenuButton_Click);
            // 
            // ClearLogsButton
            // 
            this.ClearLogsButton.BackColor = System.Drawing.Color.Transparent;
            this.ClearLogsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ClearLogsButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.ClearLogsButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClearLogsButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ClearLogsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ClearLogsButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ClearLogsButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ClearLogsButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ClearLogsButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.ClearLogsButton.Location = new System.Drawing.Point(13, 90);
            this.ClearLogsButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.ClearLogsButton.Name = "ClearLogsButton";
            this.ClearLogsButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ClearLogsButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.ClearLogsButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ClearLogsButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ClearLogsButton.Radius = 5;
            this.ClearLogsButton.Size = new System.Drawing.Size(144, 47);
            this.ClearLogsButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.ClearLogsButton.TabIndex = 12;
            this.ClearLogsButton.Text = "Clear Logs";
            this.ClearLogsButton.Click += new System.EventHandler(this.ClearLogsButton_Click);
            // 
            // EnableLoggingSwitch
            // 
            this.EnableLoggingSwitch.AutoSize = true;
            this.EnableLoggingSwitch.BaseColor = System.Drawing.Color.White;
            this.EnableLoggingSwitch.BaseOffColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.EnableLoggingSwitch.BaseOnColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.EnableLoggingSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EnableLoggingSwitch.Location = new System.Drawing.Point(749, 393);
            this.EnableLoggingSwitch.Name = "EnableLoggingSwitch";
            this.EnableLoggingSwitch.Size = new System.Drawing.Size(40, 20);
            this.EnableLoggingSwitch.TabIndex = 10;
            this.EnableLoggingSwitch.Text = "switch1";
            this.EnableLoggingSwitch.UseVisualStyleBackColor = true;
            this.EnableLoggingSwitch.CheckedChanged += new System.EventHandler(this.EnableLoggingSwitch_CheckedChanged);
            // 
            // DownloadZipButton
            // 
            this.DownloadZipButton.BackColor = System.Drawing.Color.Transparent;
            this.DownloadZipButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DownloadZipButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.DownloadZipButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.DownloadZipButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.DownloadZipButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DownloadZipButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DownloadZipButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DownloadZipButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DownloadZipButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.DownloadZipButton.Location = new System.Drawing.Point(13, 40);
            this.DownloadZipButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.DownloadZipButton.Name = "DownloadZipButton";
            this.DownloadZipButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DownloadZipButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.DownloadZipButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DownloadZipButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DownloadZipButton.Radius = 5;
            this.DownloadZipButton.Size = new System.Drawing.Size(144, 47);
            this.DownloadZipButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.DownloadZipButton.TabIndex = 9;
            this.DownloadZipButton.Text = "Force Zip Download";
            this.DownloadZipButton.Click += new System.EventHandler(this.DownloadZipButton_Click);
            // 
            // ExportLogsButton
            // 
            this.ExportLogsButton.BackColor = System.Drawing.Color.Transparent;
            this.ExportLogsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ExportLogsButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.ExportLogsButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ExportLogsButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ExportLogsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ExportLogsButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ExportLogsButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ExportLogsButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ExportLogsButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.ExportLogsButton.Location = new System.Drawing.Point(13, 140);
            this.ExportLogsButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.ExportLogsButton.Name = "ExportLogsButton";
            this.ExportLogsButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ExportLogsButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.ExportLogsButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ExportLogsButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ExportLogsButton.Radius = 5;
            this.ExportLogsButton.Size = new System.Drawing.Size(144, 47);
            this.ExportLogsButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.ExportLogsButton.TabIndex = 8;
            this.ExportLogsButton.Text = "Export Logs";
            this.ExportLogsButton.Click += new System.EventHandler(this.ExportLogsButton_Click);
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
        private System.Windows.Forms.Label DownloadZipProgressLabel;
        private BedrockCosmos.App.UI.RoundGradientButton LaunchButton;
        private BedrockCosmos.App.UI.RoundButton ExportLogsButton;
        private BedrockCosmos.App.UI.RoundButton DownloadZipButton;
        private App.UI.Switch EnableLoggingSwitch;
        private System.Windows.Forms.Label EnableLoggingLabel;
        private App.UI.RoundButton ClearLogsButton;
        private System.Windows.Forms.RichTextBox DevConsole;
        private App.UI.RoundButton DisableDevMenuButton;
        private System.Windows.Forms.Button DevBackButton;
    }
}

