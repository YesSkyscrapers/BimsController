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
using System.Windows.Shapes;

namespace BimsController.Windows
{
    /// <summary>
    /// Логика взаимодействия для LogsWindow.xaml
    /// </summary>
    public partial class LogsWindow : Window
    {
        public int _processId = -1;

        private Logics.Logic.LogicAction renderAction;

        public LogsWindow()
        {
            InitializeComponent();

            renderAction = logic =>
            {
                LogsTextBox.Document.Blocks.Clear();
                LogsTextBox.Document.Blocks.Add(new Paragraph(new Run(logic.logs.ReadLogs(_processId))));
                LogsTextBox.ScrollToEnd();
            };

            Logics.Logic.Subscribe(renderAction);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Logics.Logic.Unsubscribe(renderAction);
            Logics.Logic.Execute(logic => logic.logs.CloseLogsWindow(_processId));
        }
    }
}
