using CybersecurityBotGUI.Core;
using System;
using System;
using System.Drawing;

using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable CS8618 

namespace CybersecurityBotGUI.UI

 
    public class SplashForm;
    {
        private Label _lblLogo;
        private Label _lblSubtitle;
        private Label _lblVoice;
        private Label _lblPrompt;
    private TextBox _txtName;
        private Button _btnStart;
        private Label _lblError;
        private Panel _pnlTop;
        private Panel _pnlBottom;

        private readonly ChatbotController _controller;

        private readonly Color _darkBg = Color.FromArgb(13, 17, 23);
        private readonly Color _cyanAccent = Color.FromArgb(0, 212, 212);
        private readonly Color _panelBg = Color.FromArgb(22, 27, 34);
        private readonly Color _textLight = Color.FromArgb(230, 237, 243);

        public SplashForm()
        {
            _controller = new ChatbotController();
            InitialiseForm();
            BuildUI();
        }

        
        private void InitialiseForm()
        {
            Text = "CyberBot — Cybersecurity Awareness Bot";
            Size = new Size(780, 620);
            MinimumSize = new Size(700, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = _darkBg;
            ForeColor = _textLight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Consolas", 10f);
        }

        
        private void BuildUI()
        {
            _pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 300,
                BackColor = _panelBg,
            };

            _lblLogo = new Label
            {
                Text = GetAsciiLogo(),
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = _cyanAccent,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
            };

            _lblSubtitle = new Label
            {
                Text = "[ Keeping you safe in the digital world — one tip at a time ]",
                Font = new Font("Consolas", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 200, 200),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 30,
            };

            _pnlTop.Controls.Add(_lblLogo);
            _pnlTop.Controls.Add(_lblSubtitle);

            _pnlBottom = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _darkBg,
                Padding = new Padding(60, 20, 60, 20),
            };

            _lblVoice = new Label
            {
                Text = "🔊  " + _controller.VoiceGreetingText,
                Font = new Font("Consolas", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 200, 80),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50,
            };

            _lblPrompt = new Label
            {
                Text = "Please enter your name to begin:",
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                ForeColor = _textLight,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 45,
            };

            _txtName = new TextBox
            {
                Font = new Font("Consolas", 13f),
                BackColor = Color.FromArgb(30, 35, 45),
                ForeColor = _cyanAccent,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center,
                Height = 36,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 8, 0, 8),
            };
            _txtName.KeyDown += TxtName_KeyDown;

            _lblError = new Label
            {
                Text = string.Empty,
                Font = new Font("Consolas", 9f),
                ForeColor = Color.FromArgb(255, 80, 80),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30,
            };

            _btnStart = new Button
            {
                Text = "Start Chatting  ▶",
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                BackColor = _cyanAccent,
                ForeColor = _darkBg,
                FlatStyle = FlatStyle.Flat,
                Height = 44,
                Dock = DockStyle.Top,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 12, 0, 0),
            };
            _btnStart.FlatAppearance.BorderSize = 0;
            _btnStart.Click += BtnStart_Click;

            _pnlBottom.Controls.Add(_btnStart);
            _pnlBottom.Controls.Add(_lblError);
            _pnlBottom.Controls.Add(_txtName);
            _pnlBottom.Controls.Add(_lblPrompt);
            _pnlBottom.Controls.Add(_lblVoice);

            Controls.Add(_pnlBottom);
            Controls.Add(_pnlTop);

            Load += async (s, e) =>
            {
                await Task.Delay(300);
                _controller.PlayVoiceGreeting();
                _txtName.Focus();
            };
        }

        private void TxtName_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                TryProceed();
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e) => TryProceed();

        private void TryProceed()
        {
            _lblError.Text = string.Empty;

            if (!_controller.SetUserName(_txtName.Text))
            {
                _lblError.Text = "⚠  Name cannot be empty. Please enter your name.";
                _txtName.Focus();
                return;
            }

            MainForm mainForm = new MainForm(_controller);
            mainForm.FormClosed += (s, e) => Close();
            Hide();
            mainForm.Show();
        }

        private static string GetAsciiLogo()
        {
            return
                "  ╔══════════════════════════════════════════════════════════════╗\r\n" +
                "  ║                                                              ║\r\n" +
                "  ║   ██████╗██╗   ██╗██████╗ ███████╗██████╗  ██████╗         ║\r\n" +
                "  ║  ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗██╔═══██╗        ║\r\n" +
                "  ║  ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝██║   ██║        ║\r\n" +
                "  ║  ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗██║   ██║        ║\r\n" +
                "  ║  ╚██████╗   ██║   ██████╔╝███████╗██║  ██║╚██████╔╝        ║\r\n" +
                "  ║   ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝ ╚═════╝         ║\r\n" +
                "  ║                                                              ║\r\n" +
                "  ║           🛡  CYBERSECURITY  AWARENESS  BOT  🛡              ║\r\n" +
                "  ║                                                              ║\r\n" +
                "  ╚══════════════════════════════════════════════════════════════╝";
        }
    }
}
#pragma warning restore CS8618

