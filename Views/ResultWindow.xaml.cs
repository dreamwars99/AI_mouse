using System.Windows;
using System.Windows.Input;
using AI_Mouse.ViewModels;
using AI_Mouse.Helpers;

namespace AI_Mouse.Views
{
    /// <summary>
    /// AI 응답 결과를 표시하는 윈도우
    /// </summary>
    public partial class ResultWindow : Window
    {
        private ResultViewModel? _viewModel;

        /// <summary>
        /// ResultWindow 생성자
        /// </summary>
        public ResultWindow()
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
            
            // 외부 클릭 시 창 닫기 (Deactivated 이벤트) - 간단한 구현
            // 주의: 이 구현은 완벽하지 않을 수 있으며, 필요시 개선이 필요합니다.
            LostFocus += (s, e) =>
            {
                // 포커스를 잃었을 때 창 닫기 (ESC 키로 닫는 것이 더 안전함)
                // 이 기능은 선택 사항이므로 주석 처리할 수 있습니다.
            };
        }

        /// <summary>
        /// ViewModel을 설정합니다 (DI 주입)
        /// </summary>
        /// <param name="viewModel">ResultViewModel 인스턴스</param>
        public void SetViewModel(ResultViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        /// <summary>
        /// 최소화 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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
        /// 창이 표시될 때 마우스 커서 근처 또는 우측 하단에 위치시킵니다
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // 마우스 커서 위치 가져오기 (Win32 API 사용)
            if (NativeMethods.GetCursorPos(out System.Drawing.Point cursorPos))
            {
                // 화면 크기 가져오기
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                
                // 창 위치 계산 (우측 하단 또는 마우스 커서 근처)
                double left = cursorPos.X + 20; // 마우스 커서에서 20픽셀 오른쪽
                double top = cursorPos.Y + 20;  // 마우스 커서에서 20픽셀 아래
                
                // 화면 밖으로 나가지 않도록 조정
                if (left + Width > screenWidth)
                {
                    left = screenWidth - Width - 20; // 우측 여백 20픽셀
                }
                
                if (top + Height > screenHeight)
                {
                    top = screenHeight - Height - 20; // 하단 여백 20픽셀
                }
                
                // 최소 위치 보장
                if (left < 0) left = 20;
                if (top < 0) top = 20;
                
                Left = left;
                Top = top;
            }
            else
            {
                // 마우스 위치를 가져올 수 없는 경우 우측 하단에 배치
                Left = SystemParameters.PrimaryScreenWidth - Width - 20;
                Top = SystemParameters.PrimaryScreenHeight - Height - 20;
            }
        }
    }
}
