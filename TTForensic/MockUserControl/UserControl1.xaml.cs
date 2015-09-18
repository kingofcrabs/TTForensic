using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MockUserControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public delegate void delSetSampleCount();
        public event delSetSampleCount OnSetSampleCount;
        public UserControl1()
        {
            InitializeComponent();
            this.Loaded += UserControl1_Loaded;
        }

        void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnSetSampleCount != null)
                OnSetSampleCount();
        }
    }
}
