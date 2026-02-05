using System.Windows;
using AI_Mouse.ViewModels;

namespace AI_Mouse.Views
{
    /// <summary>
    /// OverlayWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OverlayWindow : Window
    {
        /// <summary>
        /// OverlayWindow 생성자
        /// </summary>
        /// <param name="viewModel">OverlayViewModel 인스턴스 (DI 주입)</param>
        public OverlayWindow(OverlayViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
