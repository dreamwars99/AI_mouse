using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace AI_Mouse.ViewModels
{
    /// <summary>
    /// 오버레이 윈도우의 드래그 사각형 상태를 관리하는 ViewModel
    /// </summary>
    public partial class OverlayViewModel : ObservableObject
    {
        /// <summary>
        /// 드래그 사각형의 왼쪽 좌표 (Canvas.Left)
        /// </summary>
        [ObservableProperty]
        private double _left;

        /// <summary>
        /// 드래그 사각형의 위쪽 좌표 (Canvas.Top)
        /// </summary>
        [ObservableProperty]
        private double _top;

        /// <summary>
        /// 드래그 사각형의 너비
        /// </summary>
        [ObservableProperty]
        private double _width;

        /// <summary>
        /// 드래그 사각형의 높이
        /// </summary>
        [ObservableProperty]
        private double _height;

        /// <summary>
        /// 드래그 사각형의 표시 여부
        /// </summary>
        [ObservableProperty]
        private bool _isVisible;

        /// <summary>
        /// 드래그 영역을 한 번에 업데이트합니다.
        /// </summary>
        /// <param name="rect">업데이트할 사각형 영역 (화면 좌표계)</param>
        /// <remarks>
        /// 주의: 현재는 1:1 매핑으로 구현되어 있습니다.
        /// 향후 DPI 보정이 필요할 경우 DpiHelper를 통해 좌표 변환을 수행해야 합니다.
        /// WPF는 논리 좌표(DPI 독립)를 사용하고, 훅은 물리 좌표(Pixel)를 제공하므로
        /// VisualTreeHelper나 DpiHelper를 통해 좌표 변환이 필요할 수 있습니다.
        /// </remarks>
        public void UpdateRect(Rect rect)
        {
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
            IsVisible = rect.Width > 0 && rect.Height > 0;
        }

        /// <summary>
        /// 드래그 사각형을 초기화합니다 (숨김 처리).
        /// </summary>
        public void Reset()
        {
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
            IsVisible = false;
        }
    }
}
