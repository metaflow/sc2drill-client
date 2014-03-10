using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Probe.Common;
using SC2.Ram;

namespace matchOverlays
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _gameScreenWaiter, _gameStartWaiter, _gameEndWaiter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SimpleLog.OnTempLog += (s) =>
                                       {
                                           Action a = () => listBox1.Items.Insert(0, s);
                                           Dispatcher.Invoke(a);
                                       };
            Sc2Application.WaitForApplicationCompleted += (success) =>
                                                              {
                                                                  if (!success)
                                                                  {
                                                                      SimpleLog.Log("cannot find client");
                                                                      return;
                                                                  }
                                                                  SimpleLog.LogTemp("client found");
                                                                  WaitForLoadScreen();
                                                              };
            SimpleLog.LogTemp("waiting for sc2");
            Sc2Application.WaitForApplication(new TimeSpan(10, 0, 0, 0));
        }

        private void WaitForLoadScreen()
        {
            SimpleLog.LogTemp("waiting for game load screen");
            _gameScreenWaiter = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _gameScreenWaiter.DoWork +=
                (a, b) =>
                {
                    b.Result = false;
                    while (!_gameScreenWaiter.CancellationPending &&
                        String.IsNullOrWhiteSpace(Sc2RamStructure.GetPlayerName(0)))
                        Thread.Sleep(100);
                    if (_gameScreenWaiter.CancellationPending) return;
                    SimpleLog.LogTemp("players found");
                    var players = Sc2RamStructure.GetPlayers();
                    foreach (var player in players)
                    {
                        SimpleLog.LogTemp(string.Format("{0} : {1}", player.Name, player.AccountNumber));
                    }
                    b.Result = true;
                };
            _gameScreenWaiter.RunWorkerAsync();
            _gameScreenWaiter.RunWorkerCompleted +=
                (c, d) =>
                {
                    _gameStartWaiter = new BackgroundWorker()
                                            {WorkerSupportsCancellation = true};
                    _gameStartWaiter.DoWork +=
                        (a, b) =>
                            {
                                while (
                                    RamHelper.GetInt(
                                        Sc2RamStructure.GetPointer(
                                            Sc2RamStructure.AddressType.GameTime)) ==
                                    0)
                                {
                                    Thread.Sleep(50);
                                }
                                SimpleLog.LogTemp("game started");
                                _gameEndWaiter = new BackgroundWorker();
                                _gameEndWaiter.DoWork +=
                                    (e, f) =>
                                        {
                                            SimpleLog.LogTemp("waiting for game end");
                                            while (Sc2Application.GetProcess() != null &&
                                                   !string.IsNullOrWhiteSpace(Sc2RamStructure.GetPlayerName(0)))
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            SimpleLog.LogTemp("game ended");
                                        };
                                _gameEndWaiter.RunWorkerAsync();
                            };
                    SimpleLog.LogTemp("waiting for game start");
                    _gameStartWaiter.RunWorkerAsync();
                };
            //throw new NotImplementedException();
        }
    }
}
