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

namespace AI_Mouse.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Closing 이벤트 핸들러 등록
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// 윈도우 닫기 버튼 클릭 시 앱이 종료되지 않고 숨기도록 처리합니다.
        /// 실제 종료는 트레이 메뉴에서만 가능합니다.
        /// </summary>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // 닫기 동작을 취소하고 대신 숨김 처리
            e.Cancel = true;
            Hide();
        }
    }
}
