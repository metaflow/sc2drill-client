using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Overlays;
using Probe.Engine;
using Probe.Game;
using Probe.Properties;
using Probe.Tools;
using Probe.Utility;
using Probe.WebClient;
using Probe.AutoUpdate;
using Timer = System.Timers.Timer;

namespace Probe
{
    public partial class MainForm : Form
    {
        // ReSharper disable UnaccessedField.Local
        //This field is used to prevent running multiple copies of the application
        private static Mutex _instanceMutex;
        // ReSharper restore UnaccessedField.Local
        private int _trayAnimationIndex = 0;
        private Timer _animationTimer = new Timer(300) { Enabled = false, AutoReset = true };
        private bool _balloonTipShowed;
        private enum BalloonTipActionEnum
        {
            None,
            Url
        }

        private BalloonTipActionEnum _balloonAction = BalloonTipActionEnum.None;
        private string _ballonUrl = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void StartTrayIconAnimation()
        {
            if (_animationTimer.Enabled) return;
            _trayAnimationIndex = 0;
            _animationTimer.Start();
        }

        private void StopTrayIconAnimation()
        {
            _animationTimer.Stop();
            //trayIcon.Icon = Controller.Instance.Connected ? Resources.probe : Resources.probe_exclamation;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitTrayMenu();

            UIController.ShowStateIndicator();
            _animationTimer.Elapsed += _animationTimer_Elapsed;
            StartTrayIconAnimation();
            CustomEvents.Instance.OnEvent += OnCustomEvent;
            if (!IsFirstInstance())
            {
                CustomEvents.Instance.Add(EventsType.AnotherInstanceExists);
                Thread.Sleep(4000);
                Close();
                return;
            }
            Controller.Instance.ToString(); //to initialize object
#if !DEBUG
            var task = new AutoUpdateTask();

            Controller.Instance.AddUniqueAsyncTask(task);

            while (!task.Success)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
#endif
#if DEBUG
            UIController.ShowHotkeyBar();
            UIController.SetHotkeyBarMode(ButtonsOverlayMode.Banner);
#endif
            //UIController.PreloadUI();
            PreloadSounds();
            CheckSettings();
            ServerConnection.Instance.Connect();
        }

        private void CheckSettings()
        {
            var save = false;
            if (string.IsNullOrEmpty(Settings.Default.sc2path))
            {
                Settings.Default.sc2path = "sc2.exe";
                save = true;
            }
            if(string.IsNullOrEmpty(Settings.Default.ReplayFolders))
            {
                Settings.Default.ReplayFolders = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "StarCraft II");
                save = true;
            }
            if (save) Settings.Default.Save();
         }

        private void InitTrayMenu()
        {
            foreach (var toolController in ToolManager.Controllers)
            {
                var item = new ToolStripMenuItem(toolController.Name, toolController.Icon, ToolItemClick);
                item.ToolTipText = toolController.Description;
                item.Tag = toolController.Name;
                item.ImageScaling = ToolStripItemImageScaling.None;


                toolsToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        void ToolItemClick(object sender, EventArgs e)
        {
            var item = (ToolStripItem) sender;

            ToolManager.Run((string)item.Tag);
        }

        private void PreloadSounds()
        {
            var sounds = new[]
                             {
                                 "recording completed.wav", "recording paused.wav", "recording started.wav",
                                 "notify minimap.wav", "notify resources.wav", "notify production.wav"
                             };

            Speaker.PreLoad(sounds);
        }

        void _animationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _trayAnimationIndex++;
            if (_trayAnimationIndex > 3) _trayAnimationIndex = 1;
            trayIcon.Icon = (Icon)Resources.ResourceManager.GetObject(string.Format("probe_dots{0}", _trayAnimationIndex));
        }

        private static bool IsFirstInstance()
        {
            bool b;
            _instanceMutex = new Mutex(true, "b0547943-d1fd-4f87-bb56-57517f018a05", out b);
            return b;
        }

        private delegate void OnCustomEventDelegate(EventsType eventsType, object details);

        private void OnCustomEvent(EventsType eventsType, object details)
        {
            if (InvokeRequired)
            {
                Invoke(new OnCustomEventDelegate(OnCustomEvent), new[] { eventsType, details });
                return;
            }
            switch (eventsType)
            {
                case EventsType.RecordingStarted:
                case EventsType.RecordingUnPaused:
                    SetTrayText("mining");
                    break;
                case EventsType.RecordingPaused:
                    SetTrayText("paused");
                    break;
                case EventsType.RecordingStopped:
                    CustomEvents.Instance.Add(EventsType.ReadyState);
                    break;
                case EventsType.ConnectionError:
                    if (ServerConnection.Instance.ConnectionMode != ServerConnection.ConnectionModeEnum.SilentErrors)
                        ShowBalloonTip("Cannot connect\r\nCheck your internet connection.");
                    break;
                case EventsType.JSONRequestException:
                case EventsType.GeneralError:
                    SetErrorTrayIcon();
                    ShowBalloonTip("Error occurred\r\nSubmitting...");
                    SetTrayText("error occurred");
                    break;
                case EventsType.ServerMessage:
                    ShowBalloonTip(details.ToString());
                    break;
                case EventsType.ErrorSentToServer:
                    ShowBalloonTip("Error successfully submitted");
                    break;
                case EventsType.ConnectingState:
                    StartTrayIconAnimation();
                    TrayMenuItemConnect.Enabled = false;
                    SetTrayText("connecting...");
                    break;
                case EventsType.ReadyState:
                    TrayMenuItemConnect.Visible = false;
                    SetTrayText("ready");
                    break;
                case EventsType.DisconnectedState:
                    SetErrorTrayIcon();
                    TrayMenuItemConnect.Enabled = true;
                    TrayMenuItemConnect.Visible = true;
                    SetTrayText("offline");
                    break;
                case EventsType.AnotherInstanceExists:
                    ShowBalloonTip("Already open");
                    break;
                case EventsType.Connected:
                    ShowBalloonTip("Successfully connected");
                    StopTrayIconAnimation();
                    SetNormalTrayIcon();
                    break;
                case EventsType.CannotStartSession:
                    ShowBalloonTip("Click here to connect", BalloonTipActionEnum.Url, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.BindClient));
                    break;
                case EventsType.SessionOfOtherInstance:
                    ShowBalloonTip("Another client already uses your account.\r\nClick here to use this", BalloonTipActionEnum.Url, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.ForceOpenClientSession));
                    break;
                case EventsType.UnexpectedCase:
                    ShowBalloonTip("Unexpected error occurred.\r\nTry to update client");
                    break;
                case EventsType.SessionStarted:
                    break;
                case EventsType.ErrorSavedToFile:
                    ShowBalloonTip("Error saved");
                    break;
                case EventsType.Disconnected:
                    ShowBalloonTip("Disconnected");
                    break;
                case EventsType.CloseRequest:
                    break;
                case EventsType.ServerAction:
                    var a = (JToken)details;
                    var e = a.AsJEnumerable().GetEnumerator();
                    while (e.MoveNext())
                    {
                        var action = e.Current;
                        if (action != null)
                        {
                            switch (action["type"].Value<String>())
                            {
                                case "message_url":
                                    if (ServerConnection.Instance.ConnectionMode != ServerConnection.ConnectionModeEnum.SilentErrors)
                                        ShowBalloonTip(action["message"].Value<string>(), BalloonTipActionEnum.Url,
                                                       action["url"].Value<string>());
                                    break;
                            }
                        }
                    }
                    break;
                case EventsType.NewVersionInstalled:
                    ShowBalloonTip("New version has been installed\r\nRestarting...");
                    break;
                case EventsType.ServerUnavailable:
                    if (ServerConnection.Instance.ConnectionMode != ServerConnection.ConnectionModeEnum.SilentErrors)
                        ShowBalloonTip("Server is under maintenance now.\r\nPlease check website for details", BalloonTipActionEnum.Url, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.Root));
                    break;
                case EventsType.WillRestartForUpdate:
                    ShowBalloonTip("Application need to restart to apply updates.\r\nPlease wait for a while.", BalloonTipActionEnum.Url, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.Root));
                    break;
                case EventsType.ClientNeedToBeReinstalled:
                    ShowBalloonTip("Cannot auto-update\r\nPlease manually install latest version\r\nClick here to download installer", BalloonTipActionEnum.Url, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.DownloadClient));
                    SetTrayText("please reinstall application");
                    break;
                case EventsType.Message:
                    ShowBalloonTip(details.ToString());
                    break;
                case EventsType.ConnectionCycleCompleted:
                    StopTrayIconAnimation();
                    SetNormalTrayIcon();
                    break;
                case EventsType.Close:
                    Close();
                    break;
                case EventsType.ReplayFileUploaded:
                    ShowBalloonTip("Replay uploaded");
                    break;
                default:
                    break;
            }
        }

        private void SetErrorTrayIcon()
        {
            _animationTimer.Stop();
            trayIcon.Icon = Resources.probe_exclamation;
        }

        private void SetNormalTrayIcon()
        {
            _animationTimer.Stop();
            trayIcon.Icon = Resources.probe;
        }

        private void ShowBalloonTip(string text, BalloonTipActionEnum tipAction = BalloonTipActionEnum.None, string url = "")
        {
            if (_balloonTipShowed)
            {
                var t = new Timer(1000) { AutoReset = false };
                t.Elapsed += delegate { ShowBalloonTip(text, tipAction, url); };
                t.Start();
                return;
            }
            _balloonTipShowed = true;
            _balloonAction = tipAction;
            _ballonUrl = url;
            trayIcon.BalloonTipText = text;
            trayIcon.ShowBalloonTip(4000);
            var t2 = new Timer(4000) { AutoReset = false };
            t2.Elapsed += delegate { _balloonTipShowed = false; };
            t2.Start();
        }

        void trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            switch (_balloonAction)
            {
                case BalloonTipActionEnum.None:
                    break;
                case BalloonTipActionEnum.Url:
                    if (string.IsNullOrEmpty(_ballonUrl)) return;
                    WebLayer.OpenBrowser(_ballonUrl);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetTrayText(string text)
        {
            trayIcon.Text = string.Format("{0}\r\n{1}", Application.ProductName, text);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Visible = false;
        }

        private void iExit_Click(object sender, EventArgs e)
        {
            if (GameClock.Instance.Started)
            {
                if (MessageBox.Show(Resources.cancel_recording_confirmation, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            CustomEvents.Instance.Add(EventsType.CloseRequest);
            CustomEvents.Instance.Add(EventsType.Close);
        }


        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GameClock.Instance.Started)
            {
                if (MessageBox.Show(Resources.cancel_recording_confirmation, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            CustomEvents.Instance.Add(EventsType.CloseRequest);
            CustomEvents.Instance.Add(EventsType.Restart);
        }

        private void TrayMenuItemConnect_Click(object sender, EventArgs e)
        {
            ServerConnection.Instance.Connect(ServerConnection.ConnectionModeEnum.Force);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var box = new AboutBox();
            box.Show();
        }

        private void trayIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || string.IsNullOrEmpty(trayIcon.BalloonTipText)) return;
            trayIcon.ShowBalloonTip(4000);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebLayer.OpenBrowser(WebLayer.PredefinedUrl.Settings);
        }

        private void contactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebLayer.OpenBrowser(WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.Contact) + new QueryString().Add("edit[submitted][message]",
                String.Format(" \r\n-----\r\nProbe: {0} ({1})\r\nOS: {2}\r\nFramework / runtime: {3}/{4}", 
                Assembly.GetExecutingAssembly().GetName().Version,
                Settings.Default.InstanceCode, 
                Environment.OSVersion.VersionString,
                Program.GetFramevorkVersion(),
                Environment.Version)));
        }

        private void localToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLocalSettings f = new FormLocalSettings();
            if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        }
    }
}