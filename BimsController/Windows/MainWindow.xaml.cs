﻿using BimsController.Logics;
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
                string uri = "pack://application:,,,/BimsController;component/Assets/Images/"+ (logic.settings.windowSettings.isDetailedMainWindow ? "detailed.png" : "brief.png");

                DetailsButtonImage.Source = new BitmapImage(new Uri(uri));
                MainWindowDetailsControl.SelectedIndex = logic.settings.windowSettings.isDetailedMainWindow ? 1 : 0;
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
            Logic.Execute(logic => logic.logs.Log(0, "123"));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => logic.settings.appSettings.OpenAppSettingsWindow());
        }
    }
}
