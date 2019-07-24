using BimsController.Defines;
using BimsController.Logics;
using BimsController.Managers;
using BimsController.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BimsController
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Logic.LogicAction renderAction;

        public MainWindow()
        {
            InitializeComponent();

            renderAction = logic =>
            {
                _mainWindow.Height = logic.settings.windowSettings.isDetailedMainWindow ? 700 : 410;
                string uri = "pack://application:,,,/BimsController;component/Assets/Images/" + (logic.settings.windowSettings.isDetailedMainWindow ? "detailed.png" : "brief.png");
                DetailsButtonImage.Source = new BitmapImage(new Uri(uri));
                uri = "pack://application:,,,/BimsController;component/Assets/Images/" + (logic.bot.IsRunning() ? "pause.png" : "play.png");
                StartPauseButtonImage.Source = new BitmapImage(new Uri(uri));
                uri = "pack://application:,,,/BimsController;component/Assets/Images/" + (LocksManager.getInstance().CheckLock(LocksManager.SettingsWindowLock) ? "settings_locked.png" : "settings.png");
                SettingsButtonImage.Source = new BitmapImage(new Uri(uri));
                MainWindowDetailsControl.SelectedIndex = logic.settings.windowSettings.isDetailedMainWindow ? 1 : 0;
                SettingsButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.SettingsWindowLock);
                Session1StatusIndicator.Fill = logic.bot.Infos[0].State.Equals(ProcessStates.Stopped) ? Brushes.Red : (logic.bot.Infos[0].State.Equals(ProcessStates.Running) || logic.bot.Infos[0].State.Equals(ProcessStates.AutoReconnect) ? Brushes.Green : Brushes.Orange);
                Session1StateIndicator.Fill = logic.bot.Infos[0].State.Equals(ProcessStates.Stopped) ? Brushes.Red : (logic.bot.Infos[0].State.Equals(ProcessStates.Running) || logic.bot.Infos[0].State.Equals(ProcessStates.AutoReconnect) ? Brushes.Green : Brushes.Orange);
                Session1StateLabel.Content = logic.bot.Infos[0].State.description;
                Session1StartButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.BotStartLock) && logic.bot.Infos[0].State.Equals(ProcessStates.Stopped) && logic.settings.appSettings.profilesSettings[0].enabled;
                Session1StopButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.BotStopLock) && !logic.bot.Infos[0].State.Equals(ProcessStates.Stopped);
                Session2StatusIndicator.Fill = logic.bot.Infos[1].State.Equals(ProcessStates.Stopped) ? Brushes.Red : (logic.bot.Infos[1].State.Equals(ProcessStates.Running) || logic.bot.Infos[1].State.Equals(ProcessStates.AutoReconnect) ? Brushes.Green : Brushes.Orange);
                Session2StateIndicator.Fill = logic.bot.Infos[1].State.Equals(ProcessStates.Stopped) ? Brushes.Red : (logic.bot.Infos[1].State.Equals(ProcessStates.Running) || logic.bot.Infos[1].State.Equals(ProcessStates.AutoReconnect) ? Brushes.Green : Brushes.Orange);
                Session2StateLabel.Content = logic.bot.Infos[1].State.description;
                Session2StartButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.BotStartLock) && logic.bot.Infos[1].State.Equals(ProcessStates.Stopped) && logic.settings.appSettings.profilesSettings[1].enabled;
                Session2StopButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.BotStopLock) && !logic.bot.Infos[1].State.Equals(ProcessStates.Stopped);
                Session3StatusIndicator.Fill = logic.bot.Infos[2].State.Equals(ProcessStates.Stopped) ? Brushes.Red : (logic.bot.Infos[2].State.Equals(ProcessStates.Running) || logic.bot.Infos[2].State.Equals(ProcessStates.AutoReconnect) ? Brushes.Green : Brushes.Orange);
                Session3StateIndicator.Fill = logic.bot.Infos[2].State.Equals(ProcessStates.Stopped) ? Brushes.Red : (logic.bot.Infos[2].State.Equals(ProcessStates.Running) || logic.bot.Infos[2].State.Equals(ProcessStates.AutoReconnect) ? Brushes.Green : Brushes.Orange);
                Session3StateLabel.Content = logic.bot.Infos[2].State.description;
                Session3StartButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.BotStartLock) && logic.bot.Infos[2].State.Equals(ProcessStates.Stopped) && logic.settings.appSettings.profilesSettings[2].enabled;
                Session3StopButton.IsEnabled = !LocksManager.getInstance().CheckLock(LocksManager.BotStopLock) && !logic.bot.Infos[2].State.Equals(ProcessStates.Stopped);
                StartPauseButton.IsEnabled =  !LocksManager.getInstance().CheckLock(LocksManager.BotStopLock);
            };

            Logic.Subscribe(renderAction);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Logic.Unsubscribe(renderAction);
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic =>
            {
                logic.settings.windowSettings.isDetailedMainWindow = !logic.settings.windowSettings.isDetailedMainWindow;
            });
        }

        private void Session3LogsButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.logs.OpenLogsWindow(2));
        }

        private void Session2LogsButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.logs.OpenLogsWindow(1));
        }

        private void Session1LogsButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.logs.OpenLogsWindow(0));
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.SwitchBot());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.settings.appSettings.OpenAppSettingsWindow());
        }

        private void Session1StartButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.StartBot(0));
        }

        private void Session1StopButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.StopBot(0));
        }

        private void Session2StartButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.StartBot(1));
        }

        private void Session2StopButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.StopBot(1));
        }

        private void Session3StartButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.StartBot(2));
        }

        private void Session3StopButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.bot.StopBot(2));
        }
    }
}
