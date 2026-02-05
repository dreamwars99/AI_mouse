using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AI_Mouse.ViewModels
{
    /// <summary>
    /// AI 응답 결과를 표시하는 윈도우의 ViewModel
    /// </summary>
    public partial class ResultViewModel : ObservableObject
    {
        /// <summary>
        /// AI 응답 텍스트 (마크다운 형식)
        /// </summary>
        [ObservableProperty]
        private string _responseText = string.Empty;

        /// <summary>
        /// 로딩 상태 표시 여부
        /// </summary>
        [ObservableProperty]
        private bool _isLoading = false;

        /// <summary>
        /// 로딩 인디케이터 표시 여부 (IsLoading이 true일 때 표시)
        /// </summary>
        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 마크다운 뷰어 표시 여부 (IsLoading이 false일 때 표시)
        /// </summary>
        public Visibility ContentVisibility => IsLoading ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// IsLoading 속성이 변경될 때 Visibility 속성도 업데이트
        /// </summary>
        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(LoadingVisibility));
            OnPropertyChanged(nameof(ContentVisibility));
        }

        /// <summary>
        /// 창 닫기 명령
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            // View에서 Window를 닫기 위해 이벤트를 발생시키거나,
            // Window 참조를 통해 직접 닫을 수 있습니다.
            // 여기서는 View의 Code-behind에서 처리하도록 합니다.
        }
    }
}
