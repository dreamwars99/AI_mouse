using System.Linq;
using System.Windows;
using System.Windows.Input;
using AI_Mouse.Models;
using AI_Mouse.ViewModels;

namespace AI_Mouse.Views
{
    /// <summary>
    /// 설정 창
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel? _viewModel;

        /// <summary>
        /// SettingsWindow 생성자
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
            
            // 창이 로드될 때 포커스 설정
            Loaded += (s, e) =>
            {
                this.Activate();
                this.Focus();
            };
            
            // ESC 키로 창 닫기
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                }
            };
        }

        /// <summary>
        /// ViewModel을 설정합니다 (DI 주입)
        /// </summary>
        /// <param name="viewModel">SettingsViewModel 인스턴스</param>
        public void SetViewModel(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            // ComboBox에 Enum 값들 추가
            TriggerButtonComboBox.ItemsSource = System.Enum.GetValues(typeof(TriggerButton)).Cast<TriggerButton>();

            // PasswordBox에 기존 API Key 설정 (보안을 위해 별도 처리)
            if (_viewModel != null && !string.IsNullOrEmpty(_viewModel.ApiKey))
            {
                ApiKeyPasswordBox.Password = _viewModel.ApiKey;
            }

            // 설정 저장 완료 이벤트 구독 (저장 후 창 닫기)
            if (_viewModel != null)
            {
                _viewModel.SettingsSaved += (s, e) =>
                {
                    this.Close();
                };
            }
        }

        /// <summary>
        /// 닫기 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 타이틀 바 드래그 이동 이벤트 핸들러
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// API Key PasswordBox 변경 이벤트 핸들러
        /// </summary>
        private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.ApiKey = ApiKeyPasswordBox.Password;
            }
        }

        /// <summary>
        /// 트리거 버튼 ComboBox 선택 변경 이벤트 핸들러
        /// </summary>
        private void TriggerButtonComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TriggerButtonComboBox.SelectedItem is TriggerButton selectedButton)
            {
                // Left 버튼 선택 시 경고 표시
                if (selectedButton == TriggerButton.Left)
                {
                    LeftButtonWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    LeftButtonWarning.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
