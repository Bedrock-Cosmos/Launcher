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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.HomePage = new System.Windows.Forms.TabPage();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.AboutButton = new System.Windows.Forms.Button();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.LaunchButton = new BedrockCosmos.App.UI.RoundGradientButton();
            this.AboutPage = new System.Windows.Forms.TabPage();
            this.WebsiteLabel = new System.Windows.Forms.LinkLabel();
            this.WebsiteIcon = new System.Windows.Forms.PictureBox();
            this.GitHubLabel = new System.Windows.Forms.LinkLabel();
            this.DiscordLabel = new System.Windows.Forms.LinkLabel();
            this.DiscordIcon = new System.Windows.Forms.PictureBox();
            this.GitHubIcon = new System.Windows.Forms.PictureBox();
            this.AboutLabel = new System.Windows.Forms.Label();
            this.AboutBackButton = new System.Windows.Forms.Button();
            this.SettingsPage = new System.Windows.Forms.TabPage();
            this.LanguageDescriptionLabel = new System.Windows.Forms.Label();
            this.LanguageTitleLabel = new System.Windows.Forms.Label();
            this.LanguageComboBox = new BedrockCosmos.App.UI.GradientComboBox();
            this.SettingsBackButton = new System.Windows.Forms.Button();
            this.BackgroundModeDescriptionLabel = new System.Windows.Forms.Label();
            this.BackgroundModeTitleLabel = new System.Windows.Forms.Label();
            this.BackgroundModeSwitch = new BedrockCosmos.App.UI.Switch();
            this.UpdatePage = new System.Windows.Forms.TabPage();
            this.ChangelogLabel = new System.Windows.Forms.LinkLabel();
            this.UpdateLabel = new System.Windows.Forms.Label();
            this.CancelUpdateButton = new BedrockCosmos.App.UI.RoundButton();
            this.UpdateButton = new BedrockCosmos.App.UI.RoundButton();
            this.DevPage = new System.Windows.Forms.TabPage();
            this.DetailedLoggingLabel = new System.Windows.Forms.Label();
            this.DevBackButton = new System.Windows.Forms.Button();
            this.DevConsole = new System.Windows.Forms.RichTextBox();
            this.EnableLoggingLabel = new System.Windows.Forms.Label();
            this.DownloadZipProgressLabel = new System.Windows.Forms.Label();
            this.DetailedLoggingSwitch = new BedrockCosmos.App.UI.Switch();
            this.FixProxyHangButton = new BedrockCosmos.App.UI.RoundButton();
            this.ResetNewsButton = new BedrockCosmos.App.UI.RoundButton();
            this.DisableDevMenuButton = new BedrockCosmos.App.UI.RoundButton();
            this.ClearLogsButton = new BedrockCosmos.App.UI.RoundButton();
            this.EnableLoggingSwitch = new BedrockCosmos.App.UI.Switch();
            this.DownloadZipButton = new BedrockCosmos.App.UI.RoundButton();
            this.ExportLogsButton = new BedrockCosmos.App.UI.RoundButton();
            this.TopPanel = new System.Windows.Forms.Panel();
            this.AppIcon = new System.Windows.Forms.PictureBox();
            this.TopLabel = new System.Windows.Forms.Label();
            this.MinimizeButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.BackgroundModeTimer = new System.Windows.Forms.Timer(this.components);
            this.TabControl.SuspendLayout();
            this.HomePage.SuspendLayout();
            this.AboutPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WebsiteIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DiscordIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GitHubIcon)).BeginInit();
            this.SettingsPage.SuspendLayout();
            this.UpdatePage.SuspendLayout();
            this.DevPage.SuspendLayout();
            this.TopPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AppIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // StatusLabel
            // 
            this.StatusLabel.BackColor = System.Drawing.Color.Transparent;
            this.StatusLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.StatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.StatusLabel.Location = new System.Drawing.Point(230, 278);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(340, 68);
            this.StatusLabel.TabIndex = 2;
            this.StatusLabel.Text = "";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.HomePage);
            this.TabControl.Controls.Add(this.AboutPage);
            this.TabControl.Controls.Add(this.SettingsPage);
            this.TabControl.Controls.Add(this.UpdatePage);
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
            this.HomePage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.HomePage.Controls.Add(this.VersionLabel);
            this.HomePage.Controls.Add(this.AboutButton);
            this.HomePage.Controls.Add(this.SettingsButton);
            this.HomePage.Controls.Add(this.LaunchButton);
            this.HomePage.Controls.Add(this.StatusLabel);
            this.HomePage.Location = new System.Drawing.Point(4, 22);
            this.HomePage.Name = "HomePage";
            this.HomePage.Padding = new System.Windows.Forms.Padding(3);
            this.HomePage.Size = new System.Drawing.Size(806, 440);
            this.HomePage.TabIndex = 0;
            this.HomePage.Text = "";
            // 
            // VersionLabel
            // 
            this.VersionLabel.BackColor = System.Drawing.Color.Transparent;
            this.VersionLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.VersionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.VersionLabel.Location = new System.Drawing.Point(234, 398);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(336, 19);
            this.VersionLabel.TabIndex = 18;
            this.VersionLabel.Text = "";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // AboutButton
            // 
            this.AboutButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AboutButton.BackColor = System.Drawing.Color.Transparent;
            this.AboutButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AboutButton.BackgroundImage")));
            this.AboutButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.AboutButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.AboutButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.AboutButton.FlatAppearance.BorderSize = 0;
            this.AboutButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.AboutButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.AboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AboutButton.Location = new System.Drawing.Point(15, 391);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(25, 25);
            this.AboutButton.TabIndex = 17;
            this.AboutButton.UseVisualStyleBackColor = false;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // SettingsButton
            // 
            this.SettingsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SettingsButton.BackColor = System.Drawing.Color.Transparent;
            this.SettingsButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SettingsButton.BackgroundImage")));
            this.SettingsButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SettingsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SettingsButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.SettingsButton.FlatAppearance.BorderSize = 0;
            this.SettingsButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.SettingsButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.SettingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SettingsButton.Location = new System.Drawing.Point(760, 391);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(25, 25);
            this.SettingsButton.TabIndex = 16;
            this.SettingsButton.UseVisualStyleBackColor = false;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
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
            // AboutPage
            // 
            this.AboutPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.AboutPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.AboutPage.Controls.Add(this.WebsiteLabel);
            this.AboutPage.Controls.Add(this.WebsiteIcon);
            this.AboutPage.Controls.Add(this.GitHubLabel);
            this.AboutPage.Controls.Add(this.DiscordLabel);
            this.AboutPage.Controls.Add(this.DiscordIcon);
            this.AboutPage.Controls.Add(this.GitHubIcon);
            this.AboutPage.Controls.Add(this.AboutLabel);
            this.AboutPage.Controls.Add(this.AboutBackButton);
            this.AboutPage.Location = new System.Drawing.Point(4, 22);
            this.AboutPage.Name = "AboutPage";
            this.AboutPage.Size = new System.Drawing.Size(806, 440);
            this.AboutPage.TabIndex = 3;
            this.AboutPage.Text = "";
            // 
            // WebsiteLabel
            // 
            this.WebsiteLabel.ActiveLinkColor = System.Drawing.Color.Cyan;
            this.WebsiteLabel.AutoSize = true;
            this.WebsiteLabel.BackColor = System.Drawing.Color.Transparent;
            this.WebsiteLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.WebsiteLabel.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.WebsiteLabel.Location = new System.Drawing.Point(540, 298);
            this.WebsiteLabel.Name = "WebsiteLabel";
            this.WebsiteLabel.Size = new System.Drawing.Size(65, 21);
            this.WebsiteLabel.TabIndex = 14;
            this.WebsiteLabel.TabStop = true;
            this.WebsiteLabel.Text = "";
            this.WebsiteLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.WebsiteLabel.Click += new System.EventHandler(this.OpenWebsiteLink);
            // 
            // WebsiteIcon
            // 
            this.WebsiteIcon.BackColor = System.Drawing.Color.Transparent;
            this.WebsiteIcon.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WebsiteIcon.BackgroundImage")));
            this.WebsiteIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.WebsiteIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.WebsiteIcon.Location = new System.Drawing.Point(500, 293);
            this.WebsiteIcon.Name = "WebsiteIcon";
            this.WebsiteIcon.Size = new System.Drawing.Size(32, 32);
            this.WebsiteIcon.TabIndex = 13;
            this.WebsiteIcon.TabStop = false;
            this.WebsiteIcon.Click += new System.EventHandler(this.OpenWebsiteLink);
            // 
            // GitHubLabel
            // 
            this.GitHubLabel.ActiveLinkColor = System.Drawing.Color.Cyan;
            this.GitHubLabel.AutoSize = true;
            this.GitHubLabel.BackColor = System.Drawing.Color.Transparent;
            this.GitHubLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.GitHubLabel.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.GitHubLabel.Location = new System.Drawing.Point(392, 298);
            this.GitHubLabel.Name = "GitHubLabel";
            this.GitHubLabel.Size = new System.Drawing.Size(59, 21);
            this.GitHubLabel.TabIndex = 12;
            this.GitHubLabel.TabStop = true;
            this.GitHubLabel.Text = "";
            this.GitHubLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.GitHubLabel.Click += new System.EventHandler(this.OpenGitHubLink);
            // 
            // DiscordLabel
            // 
            this.DiscordLabel.ActiveLinkColor = System.Drawing.Color.Cyan;
            this.DiscordLabel.AutoSize = true;
            this.DiscordLabel.BackColor = System.Drawing.Color.Transparent;
            this.DiscordLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.DiscordLabel.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.DiscordLabel.Location = new System.Drawing.Point(235, 298);
            this.DiscordLabel.Name = "DiscordLabel";
            this.DiscordLabel.Size = new System.Drawing.Size(63, 21);
            this.DiscordLabel.TabIndex = 11;
            this.DiscordLabel.TabStop = true;
            this.DiscordLabel.Text = "";
            this.DiscordLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.DiscordLabel.Click += new System.EventHandler(this.OpenDiscordLink);
            // 
            // DiscordIcon
            // 
            this.DiscordIcon.BackColor = System.Drawing.Color.Transparent;
            this.DiscordIcon.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DiscordIcon.BackgroundImage")));
            this.DiscordIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.DiscordIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DiscordIcon.Location = new System.Drawing.Point(195, 293);
            this.DiscordIcon.Name = "DiscordIcon";
            this.DiscordIcon.Size = new System.Drawing.Size(32, 32);
            this.DiscordIcon.TabIndex = 9;
            this.DiscordIcon.TabStop = false;
            this.DiscordIcon.Click += new System.EventHandler(this.OpenDiscordLink);
            // 
            // GitHubIcon
            // 
            this.GitHubIcon.BackColor = System.Drawing.Color.Transparent;
            this.GitHubIcon.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("GitHubIcon.BackgroundImage")));
            this.GitHubIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.GitHubIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitHubIcon.Location = new System.Drawing.Point(352, 293);
            this.GitHubIcon.Name = "GitHubIcon";
            this.GitHubIcon.Size = new System.Drawing.Size(32, 32);
            this.GitHubIcon.TabIndex = 4;
            this.GitHubIcon.TabStop = false;
            // 
            // AboutLabel
            // 
            this.AboutLabel.BackColor = System.Drawing.Color.Transparent;
            this.AboutLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.AboutLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.AboutLabel.Location = new System.Drawing.Point(120, 86);
            this.AboutLabel.Name = "AboutLabel";
            this.AboutLabel.Size = new System.Drawing.Size(560, 185);
            this.AboutLabel.TabIndex = 6;
            this.AboutLabel.Text = resources.GetString("AboutLabel.Text");
            this.AboutLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // AboutBackButton
            // 
            this.AboutBackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AboutBackButton.BackColor = System.Drawing.Color.Transparent;
            this.AboutBackButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AboutBackButton.BackgroundImage")));
            this.AboutBackButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.AboutBackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.AboutBackButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.AboutBackButton.FlatAppearance.BorderSize = 0;
            this.AboutBackButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.AboutBackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.AboutBackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AboutBackButton.Location = new System.Drawing.Point(15, 391);
            this.AboutBackButton.Name = "AboutBackButton";
            this.AboutBackButton.Size = new System.Drawing.Size(25, 25);
            this.AboutBackButton.TabIndex = 5;
            this.AboutBackButton.UseVisualStyleBackColor = false;
            this.AboutBackButton.Click += new System.EventHandler(this.ReturnToHomeScreen);
            // 
            // SettingsPage
            // 
            this.SettingsPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.SettingsPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SettingsPage.Controls.Add(this.LanguageDescriptionLabel);
            this.SettingsPage.Controls.Add(this.LanguageTitleLabel);
            this.SettingsPage.Controls.Add(this.LanguageComboBox);
            this.SettingsPage.Controls.Add(this.SettingsBackButton);
            this.SettingsPage.Controls.Add(this.BackgroundModeDescriptionLabel);
            this.SettingsPage.Controls.Add(this.BackgroundModeTitleLabel);
            this.SettingsPage.Controls.Add(this.BackgroundModeSwitch);
            this.SettingsPage.Location = new System.Drawing.Point(4, 22);
            this.SettingsPage.Name = "SettingsPage";
            this.SettingsPage.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsPage.Size = new System.Drawing.Size(806, 440);
            this.SettingsPage.TabIndex = 1;
            this.SettingsPage.Text = "";
            // 
            // LanguageDescriptionLabel
            // 
            this.LanguageDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.LanguageDescriptionLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.LanguageDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageDescriptionLabel.Location = new System.Drawing.Point(270, 115);
            this.LanguageDescriptionLabel.Name = "LanguageDescriptionLabel";
            this.LanguageDescriptionLabel.Size = new System.Drawing.Size(506, 23);
            this.LanguageDescriptionLabel.TabIndex = 18;
            this.LanguageDescriptionLabel.Text = "";
            this.LanguageDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LanguageTitleLabel
            // 
            this.LanguageTitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.LanguageTitleLabel.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.LanguageTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageTitleLabel.Location = new System.Drawing.Point(160, 109);
            this.LanguageTitleLabel.Name = "LanguageTitleLabel";
            this.LanguageTitleLabel.Size = new System.Drawing.Size(107, 30);
            this.LanguageTitleLabel.TabIndex = 17;
            this.LanguageTitleLabel.Text = "";
            this.LanguageTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LanguageComboBox
            // 
            this.LanguageComboBox.BackColor = System.Drawing.Color.Transparent;
            this.LanguageComboBox.ColorA = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageComboBox.ColorB = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageComboBox.ColorC = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.LanguageComboBox.ColorD = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.LanguageComboBox.ColorE = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.LanguageComboBox.ColorF = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageComboBox.ColorG = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageComboBox.ColorH = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.LanguageComboBox.ColorI = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.LanguageComboBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.LanguageComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LanguageComboBox.DropDownHeight = 100;
            this.LanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LanguageComboBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.LanguageComboBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.LanguageComboBox.FormattingEnabled = true;
            this.LanguageComboBox.HoverSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.LanguageComboBox.IntegralHeight = false;
            this.LanguageComboBox.ItemHeight = 24;
            this.LanguageComboBox.Items.AddRange(new object[] {
            "English",
            "Deutsch",
            "বাংলা",
            "Español",
            "Indonesia",
            "日本語",
            "Tiếng Việt"});
            this.LanguageComboBox.Location = new System.Drawing.Point(19, 114);
            this.LanguageComboBox.Name = "LanguageComboBox";
            this.LanguageComboBox.Size = new System.Drawing.Size(132, 26);
            this.LanguageComboBox.StartIndex = 0;
            this.LanguageComboBox.TabIndex = 16;
            this.LanguageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBox_SelectedIndexChanged);
            // 
            // SettingsBackButton
            // 
            this.SettingsBackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SettingsBackButton.BackColor = System.Drawing.Color.Transparent;
            this.SettingsBackButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SettingsBackButton.BackgroundImage")));
            this.SettingsBackButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.SettingsBackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SettingsBackButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.SettingsBackButton.FlatAppearance.BorderSize = 0;
            this.SettingsBackButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.SettingsBackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.SettingsBackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SettingsBackButton.Location = new System.Drawing.Point(15, 391);
            this.SettingsBackButton.Name = "SettingsBackButton";
            this.SettingsBackButton.Size = new System.Drawing.Size(25, 25);
            this.SettingsBackButton.TabIndex = 15;
            this.SettingsBackButton.UseVisualStyleBackColor = false;
            this.SettingsBackButton.Click += new System.EventHandler(this.ReturnToHomeScreen);
            // 
            // BackgroundModeDescriptionLabel
            // 
            this.BackgroundModeDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundModeDescriptionLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.BackgroundModeDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.BackgroundModeDescriptionLabel.Location = new System.Drawing.Point(270, 40);
            this.BackgroundModeDescriptionLabel.Name = "BackgroundModeDescriptionLabel";
            this.BackgroundModeDescriptionLabel.Size = new System.Drawing.Size(506, 61);
            this.BackgroundModeDescriptionLabel.TabIndex = 14;
            this.BackgroundModeDescriptionLabel.Text = "";
            this.BackgroundModeDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BackgroundModeTitleLabel
            // 
            this.BackgroundModeTitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundModeTitleLabel.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.BackgroundModeTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.BackgroundModeTitleLabel.Location = new System.Drawing.Point(74, 40);
            this.BackgroundModeTitleLabel.Name = "BackgroundModeTitleLabel";
            this.BackgroundModeTitleLabel.Size = new System.Drawing.Size(193, 61);
            this.BackgroundModeTitleLabel.TabIndex = 13;
            this.BackgroundModeTitleLabel.Text = "";
            this.BackgroundModeTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BackgroundModeSwitch
            // 
            this.BackgroundModeSwitch.AutoSize = true;
            this.BackgroundModeSwitch.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundModeSwitch.BaseColor = System.Drawing.Color.White;
            this.BackgroundModeSwitch.BaseOffColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.BackgroundModeSwitch.BaseOnColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.BackgroundModeSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BackgroundModeSwitch.Location = new System.Drawing.Point(19, 61);
            this.BackgroundModeSwitch.Name = "BackgroundModeSwitch";
            this.BackgroundModeSwitch.Size = new System.Drawing.Size(40, 20);
            this.BackgroundModeSwitch.TabIndex = 12;
            this.BackgroundModeSwitch.UseVisualStyleBackColor = false;
            this.BackgroundModeSwitch.CheckedChanged += new System.EventHandler(this.BackgroundModeToggle_CheckedChanged);
            // 
            // UpdatePage
            // 
            this.UpdatePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.UpdatePage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.UpdatePage.Controls.Add(this.ChangelogLabel);
            this.UpdatePage.Controls.Add(this.UpdateLabel);
            this.UpdatePage.Controls.Add(this.CancelUpdateButton);
            this.UpdatePage.Controls.Add(this.UpdateButton);
            this.UpdatePage.Location = new System.Drawing.Point(4, 22);
            this.UpdatePage.Name = "UpdatePage";
            this.UpdatePage.Size = new System.Drawing.Size(806, 440);
            this.UpdatePage.TabIndex = 4;
            this.UpdatePage.Text = "";
            // 
            // ChangelogLabel
            // 
            this.ChangelogLabel.ActiveLinkColor = System.Drawing.Color.Cyan;
            this.ChangelogLabel.BackColor = System.Drawing.Color.Transparent;
            this.ChangelogLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.ChangelogLabel.LinkColor = System.Drawing.Color.LightSkyBlue;
            this.ChangelogLabel.Location = new System.Drawing.Point(334, 220);
            this.ChangelogLabel.Name = "ChangelogLabel";
            this.ChangelogLabel.Size = new System.Drawing.Size(133, 36);
            this.ChangelogLabel.TabIndex = 13;
            this.ChangelogLabel.TabStop = true;
            this.ChangelogLabel.Text = "";
            this.ChangelogLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ChangelogLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.ChangelogLabel.Click += new System.EventHandler(this.ChangelogLabel_Click);
            // 
            // UpdateLabel
            // 
            this.UpdateLabel.BackColor = System.Drawing.Color.Transparent;
            this.UpdateLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.UpdateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.UpdateLabel.Location = new System.Drawing.Point(180, 124);
            this.UpdateLabel.Name = "UpdateLabel";
            this.UpdateLabel.Size = new System.Drawing.Size(440, 93);
            this.UpdateLabel.TabIndex = 11;
            this.UpdateLabel.Text = "";
            this.UpdateLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CancelUpdateButton
            // 
            this.CancelUpdateButton.BackColor = System.Drawing.Color.Transparent;
            this.CancelUpdateButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CancelUpdateButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.CancelUpdateButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.CancelUpdateButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.CancelUpdateButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.CancelUpdateButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.CancelUpdateButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.CancelUpdateButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.CancelUpdateButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.CancelUpdateButton.Location = new System.Drawing.Point(412, 283);
            this.CancelUpdateButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.CancelUpdateButton.Name = "CancelUpdateButton";
            this.CancelUpdateButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.CancelUpdateButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.CancelUpdateButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.CancelUpdateButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.CancelUpdateButton.Radius = 5;
            this.CancelUpdateButton.Size = new System.Drawing.Size(144, 57);
            this.CancelUpdateButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.CancelUpdateButton.TabIndex = 12;
            this.CancelUpdateButton.Text = "";
            this.CancelUpdateButton.Click += new System.EventHandler(this.CancelUpdateButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.BackColor = System.Drawing.Color.Transparent;
            this.UpdateButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.UpdateButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.UpdateButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.UpdateButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.UpdateButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.UpdateButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.UpdateButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.UpdateButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.UpdateButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.UpdateButton.Location = new System.Drawing.Point(245, 283);
            this.UpdateButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.UpdateButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.UpdateButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.UpdateButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.UpdateButton.Radius = 5;
            this.UpdateButton.Size = new System.Drawing.Size(144, 57);
            this.UpdateButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.UpdateButton.TabIndex = 10;
            this.UpdateButton.Text = "";
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // DevPage
            // 
            this.DevPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.DevPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DevPage.Controls.Add(this.DetailedLoggingLabel);
            this.DevPage.Controls.Add(this.DevBackButton);
            this.DevPage.Controls.Add(this.DevConsole);
            this.DevPage.Controls.Add(this.EnableLoggingLabel);
            this.DevPage.Controls.Add(this.DownloadZipProgressLabel);
            this.DevPage.Controls.Add(this.DetailedLoggingSwitch);
            this.DevPage.Controls.Add(this.FixProxyHangButton);
            this.DevPage.Controls.Add(this.ResetNewsButton);
            this.DevPage.Controls.Add(this.DisableDevMenuButton);
            this.DevPage.Controls.Add(this.ClearLogsButton);
            this.DevPage.Controls.Add(this.EnableLoggingSwitch);
            this.DevPage.Controls.Add(this.DownloadZipButton);
            this.DevPage.Controls.Add(this.ExportLogsButton);
            this.DevPage.Location = new System.Drawing.Point(4, 22);
            this.DevPage.Name = "DevPage";
            this.DevPage.Size = new System.Drawing.Size(806, 440);
            this.DevPage.TabIndex = 2;
            this.DevPage.Text = "";
            // 
            // DetailedLoggingLabel
            // 
            this.DetailedLoggingLabel.AutoSize = true;
            this.DetailedLoggingLabel.BackColor = System.Drawing.Color.Transparent;
            this.DetailedLoggingLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.DetailedLoggingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DetailedLoggingLabel.Location = new System.Drawing.Point(502, 394);
            this.DetailedLoggingLabel.Name = "DetailedLoggingLabel";
            this.DetailedLoggingLabel.Size = new System.Drawing.Size(221, 19);
            this.DetailedLoggingLabel.TabIndex = 18;
            this.DetailedLoggingLabel.Text = "";
            // 
            // DevBackButton
            // 
            this.DevBackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DevBackButton.BackColor = System.Drawing.Color.Transparent;
            this.DevBackButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DevBackButton.BackgroundImage")));
            this.DevBackButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.DevBackButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DevBackButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.DevBackButton.FlatAppearance.BorderSize = 0;
            this.DevBackButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.DevBackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.DevBackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DevBackButton.Location = new System.Drawing.Point(15, 391);
            this.DevBackButton.Name = "DevBackButton";
            this.DevBackButton.Size = new System.Drawing.Size(25, 25);
            this.DevBackButton.TabIndex = 4;
            this.DevBackButton.UseVisualStyleBackColor = false;
            this.DevBackButton.Click += new System.EventHandler(this.ReturnToHomeScreen);
            // 
            // DevConsole
            // 
            this.DevConsole.BackColor = System.Drawing.Color.Black;
            this.DevConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DevConsole.DetectUrls = false;
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
            this.EnableLoggingLabel.BackColor = System.Drawing.Color.Transparent;
            this.EnableLoggingLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.EnableLoggingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.EnableLoggingLabel.Location = new System.Drawing.Point(306, 394);
            this.EnableLoggingLabel.Name = "EnableLoggingLabel";
            this.EnableLoggingLabel.Size = new System.Drawing.Size(103, 19);
            this.EnableLoggingLabel.TabIndex = 11;
            this.EnableLoggingLabel.Text = "";
            // 
            // DownloadZipProgressLabel
            // 
            this.DownloadZipProgressLabel.BackColor = System.Drawing.Color.Transparent;
            this.DownloadZipProgressLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.DownloadZipProgressLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DownloadZipProgressLabel.Location = new System.Drawing.Point(162, 41);
            this.DownloadZipProgressLabel.Name = "DownloadZipProgressLabel";
            this.DownloadZipProgressLabel.Size = new System.Drawing.Size(121, 46);
            this.DownloadZipProgressLabel.TabIndex = 4;
            this.DownloadZipProgressLabel.Text = "";
            this.DownloadZipProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DownloadZipProgressLabel.Visible = false;
            // 
            // DetailedLoggingSwitch
            // 
            this.DetailedLoggingSwitch.AutoSize = true;
            this.DetailedLoggingSwitch.BackColor = System.Drawing.Color.Transparent;
            this.DetailedLoggingSwitch.BaseColor = System.Drawing.Color.White;
            this.DetailedLoggingSwitch.BaseOffColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.DetailedLoggingSwitch.BaseOnColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.DetailedLoggingSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DetailedLoggingSwitch.Location = new System.Drawing.Point(729, 393);
            this.DetailedLoggingSwitch.Name = "DetailedLoggingSwitch";
            this.DetailedLoggingSwitch.Size = new System.Drawing.Size(40, 20);
            this.DetailedLoggingSwitch.TabIndex = 17;
            this.DetailedLoggingSwitch.UseVisualStyleBackColor = true;
            this.DetailedLoggingSwitch.CheckedChanged += new System.EventHandler(this.DetailedLoggingSwitch_CheckedChanged);
            // 
            // FixProxyHangButton
            // 
            this.FixProxyHangButton.BackColor = System.Drawing.Color.Transparent;
            this.FixProxyHangButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.FixProxyHangButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.FixProxyHangButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.FixProxyHangButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FixProxyHangButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.FixProxyHangButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.FixProxyHangButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.FixProxyHangButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.FixProxyHangButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.FixProxyHangButton.Location = new System.Drawing.Point(13, 190);
            this.FixProxyHangButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.FixProxyHangButton.Name = "FixProxyHangButton";
            this.FixProxyHangButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.FixProxyHangButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.FixProxyHangButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.FixProxyHangButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.FixProxyHangButton.Radius = 5;
            this.FixProxyHangButton.Size = new System.Drawing.Size(144, 47);
            this.FixProxyHangButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.FixProxyHangButton.TabIndex = 16;
            this.FixProxyHangButton.Text = "";
            this.FixProxyHangButton.Click += new System.EventHandler(this.FixProxyHangButton_Click);
            // 
            // ResetNewsButton
            // 
            this.ResetNewsButton.BackColor = System.Drawing.Color.Transparent;
            this.ResetNewsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetNewsButton.DialogResult = System.Windows.Forms.DialogResult.None;
            this.ResetNewsButton.FilledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ResetNewsButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ResetNewsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ResetNewsButton.HoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ResetNewsButton.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ResetNewsButton.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ResetNewsButton.InterpolationType = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            this.ResetNewsButton.Location = new System.Drawing.Point(13, 240);
            this.ResetNewsButton.MinimumSize = new System.Drawing.Size(144, 47);
            this.ResetNewsButton.Name = "ResetNewsButton";
            this.ResetNewsButton.NormalBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ResetNewsButton.PixelOffsetType = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            this.ResetNewsButton.PressedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.ResetNewsButton.PressedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.ResetNewsButton.Radius = 5;
            this.ResetNewsButton.Size = new System.Drawing.Size(144, 47);
            this.ResetNewsButton.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.ResetNewsButton.TabIndex = 15;
            this.ResetNewsButton.Text = "";
            this.ResetNewsButton.Click += new System.EventHandler(this.ResetNewsButton_Click);
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
            this.DisableDevMenuButton.Location = new System.Drawing.Point(13, 290);
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
            this.DisableDevMenuButton.Text = "";
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
            this.ClearLogsButton.Location = new System.Drawing.Point(13, 140);
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
            this.ClearLogsButton.Text = "";
            this.ClearLogsButton.Click += new System.EventHandler(this.ClearLogsButton_Click);
            // 
            // EnableLoggingSwitch
            // 
            this.EnableLoggingSwitch.AutoSize = true;
            this.EnableLoggingSwitch.BackColor = System.Drawing.Color.Transparent;
            this.EnableLoggingSwitch.BaseColor = System.Drawing.Color.White;
            this.EnableLoggingSwitch.BaseOffColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.EnableLoggingSwitch.BaseOnColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.EnableLoggingSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EnableLoggingSwitch.Location = new System.Drawing.Point(414, 393);
            this.EnableLoggingSwitch.Name = "EnableLoggingSwitch";
            this.EnableLoggingSwitch.Size = new System.Drawing.Size(40, 20);
            this.EnableLoggingSwitch.TabIndex = 10;
            this.EnableLoggingSwitch.UseVisualStyleBackColor = false;
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
            this.DownloadZipButton.Text = "";
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
            this.ExportLogsButton.Location = new System.Drawing.Point(13, 90);
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
            this.ExportLogsButton.Text = "";
            this.ExportLogsButton.Click += new System.EventHandler(this.ExportLogsButton_Click);
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
            this.TopLabel.Text = "";
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
            // TrayIcon
            // 
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "";
            this.TrayIcon.Click += new System.EventHandler(this.TrayIcon_Click);
            // 
            // BackgroundModeTimer
            // 
            this.BackgroundModeTimer.Interval = 2500;
            this.BackgroundModeTimer.Tick += new System.EventHandler(this.BackgroundModeTimer_Tick);
            // 
            // MainForm
            // 
            this.AccessibleName = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.TabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.TabControl.ResumeLayout(false);
            this.HomePage.ResumeLayout(false);
            this.AboutPage.ResumeLayout(false);
            this.AboutPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WebsiteIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DiscordIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GitHubIcon)).EndInit();
            this.SettingsPage.ResumeLayout(false);
            this.SettingsPage.PerformLayout();
            this.UpdatePage.ResumeLayout(false);
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
        private System.Windows.Forms.Label BackgroundModeDescriptionLabel;
        private System.Windows.Forms.Label BackgroundModeTitleLabel;
        private App.UI.Switch BackgroundModeSwitch;
        private System.Windows.Forms.Button SettingsBackButton;
        private System.Windows.Forms.Button SettingsButton;
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.Timer BackgroundModeTimer;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.TabPage AboutPage;
        private System.Windows.Forms.Button AboutBackButton;
        private System.Windows.Forms.Label AboutLabel;
        private System.Windows.Forms.PictureBox DiscordIcon;
        private System.Windows.Forms.PictureBox GitHubIcon;
        private System.Windows.Forms.LinkLabel DiscordLabel;
        private System.Windows.Forms.LinkLabel GitHubLabel;
        private App.UI.RoundButton ResetNewsButton;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.TabPage UpdatePage;
        private App.UI.RoundButton UpdateButton;
        private App.UI.RoundButton CancelUpdateButton;
        private System.Windows.Forms.Label UpdateLabel;
        private App.UI.GradientComboBox LanguageComboBox;
        private System.Windows.Forms.Label LanguageDescriptionLabel;
        private System.Windows.Forms.Label LanguageTitleLabel;
        private App.UI.RoundButton FixProxyHangButton;
        private System.Windows.Forms.LinkLabel WebsiteLabel;
        private System.Windows.Forms.PictureBox WebsiteIcon;
        private System.Windows.Forms.LinkLabel ChangelogLabel;
        private System.Windows.Forms.Label DetailedLoggingLabel;
        private App.UI.Switch DetailedLoggingSwitch;
    }
}

