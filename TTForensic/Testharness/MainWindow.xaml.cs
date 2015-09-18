using System.Windows;
using TTSkeleton;

namespace Testharness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SampleSettingControl settingControl = new SampleSettingControl();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            userControl1.OnSetSampleCount += userControl1_OnSetSampleCount;
        }

        void userControl1_OnSetSampleCount()
        {
            settingControl.MethodDistributer(userControl1, "CallDisableEnableNextButtonMethod", "", null);
        }
    }
}
