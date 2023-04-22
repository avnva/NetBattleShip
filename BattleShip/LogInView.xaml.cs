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
using Xceed.Wpf.Toolkit;

namespace BattleShip
{
    /// <summary>
    /// Логика взаимодействия для LogInView.xaml
    /// </summary>
    public partial class LogInView : Window
    {
        public LogInView()
        {
            InitializeComponent();
        }

        private void tbIP_LostFocus(object sender, RoutedEventArgs e)
        {
            MaskedTextBox tbIP = sender as MaskedTextBox;
            if (tbIP != null)
            {
                string[] octets = tbIP.Text.Split('.');
                bool isValid = true;
                foreach (string octet in octets)
                {
                    if (!byte.TryParse(octet, out byte result) || result > 255)
                    {
                        isValid = false;
                        break;
                    }
                }
                if (!isValid)
                {
                    System.Windows.MessageBox.Show("Неправильный формат IP-адреса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbIP.Text = "";
                }
            }
        }
    }
}
