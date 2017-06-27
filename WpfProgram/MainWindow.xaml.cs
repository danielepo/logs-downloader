using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace WpfProgram
{
    public static class Enumeration
    {
        public static IList<string> GetAll<TEnum>() where TEnum : struct
        {
            var enumerationType = typeof(TEnum);

            if (!enumerationType.IsEnum)
                throw new ArgumentException("Enumeration type is expected.");

            var dictionary = new List<string>();

            foreach (int value in Enum.GetValues(enumerationType))
            {
                var name = Enum.GetName(enumerationType, value);
                dictionary.Add(name);
            }

            return dictionary;
        }
    }

    enum Prova
    {
        Try, Try1
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Internallogger logger;
        public MainWindow()
        {
            InitializeComponent();
            environment.ItemsSource = Enumeration.GetAll<Types.Environment>();
            program.ItemsSource = Enumeration.GetAll<Types.Program>();
            type.ItemsSource = Enumeration.GetAll<Types.LogType>();
            logger = new Internallogger(log);
        }

        //private void InitDropdown(Type enm)
        //{
        //    var itemValues = Enum.GetValues(enm);
        //    var itemNames = Enum.GetNames(enm);

        //    for (int i = 0; i <= itemNames.Length - 1; i++)
        //    {
        //        ListItem item = new ListItem((itemNames[i], itemValues[i]);
        //        dropdownlist.Items.Add(item);
        //    }
        //}
        class Internallogger : Logger.ILogger
        {
            private readonly ListBox _log;

            public Internallogger(ListBox log)
            {
                _log = log;
            }
            public void debug(string messaggio)
            {
                LogMessage(messaggio);
            }

            private void LogMessage(string item)
            {
                if (Thread.CurrentThread.IsBackground)
                {
                    _log.Items.Dispatcher.Invoke(() => //dispatch to UI Thread
                    {
                        _log.Items.Add(item);
                    });
                }
                else
                {
                    _log.Items.Add(item);
                }
            }

            public void debug(string messaggio, Exception ex)
            {
                LogMessageWithExeption(messaggio, ex);
            }

            private void LogMessageWithExeption(string messaggio, Exception ex)
            {
                LogMessage(messaggio);
                LogMessage(ex.Message);
            }

            public void error(string messaggio)
            {
                LogMessage(messaggio);
            }

            public void error(string messaggio, Exception ex)
            {
                LogMessageWithExeption(messaggio, ex);
            }

            public void info(string messaggio)
            {
                LogMessage(messaggio);
            }

            public void warn(string messaggio)
            {
                LogMessage(messaggio);
            }

            public void warn(string messaggio, Exception ex)
            {
                LogMessageWithExeption(messaggio, ex);
            }
        }
        private void go_Click(object sender, RoutedEventArgs e)
        {
            log.Items.Clear();
            var dto = new Downloader.DownloaderDtoExtended(
                date.Text,
                time.Text,
                Parse<Types.Environment>(environment.Text),
                Parse<Types.Program>("IncassoDA"),
                search.Text,
                folder.Text,
                Parse<Types.LogType>(type.Text),
                logger);
            Task.Run(() => Downloader.DownladLogs(dto));
        }

        private static TEnum Parse<TEnum>(string value) where TEnum : struct
        {
            TEnum parsed;
            Enum.TryParse(value, true, out parsed);
            return parsed;
        }
    }
}
