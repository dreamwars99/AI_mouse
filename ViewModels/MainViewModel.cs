using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;
using AI_Mouse.Services.Interfaces;
using AI_Mouse.Views;

namespace AI_Mouse.ViewModels
{
    /// <summary>
    /// 메인 로직 및 상태 관리를 담당하는 ViewModel (Orchestrator)
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IGlobalHookService _hookService;
        private OverlayWindow? _overlayWindow;
        private OverlayViewModel? _overlayViewModel;

        // 드래그 시작점 좌표 (화면 좌표계)
        private int _dragStartX;
        private int _dragStartY;

        /// <summary>
        /// MainViewModel 생성자
        /// </summary>
        /// <param name="hookService">전역 마우스 훅 서비스 (DI 주입)</param>
        public MainViewModel(IGlobalHookService hookService)
        {
            _hookService = hookService ?? throw new ArgumentNullException(nameof(hookService));

            // 마우스 액션 이벤트 구독
            _hookService.MouseAction += OnMouseAction;
        }

        /// <summary>
        /// OverlayWindow 참조를 설정합니다 (App.xaml.cs에서 호출)
        /// </summary>
        /// <param name="overlayWindow">OverlayWindow 인스턴스</param>
        public void SetOverlayWindow(OverlayWindow overlayWindow)
        {
            _overlayWindow = overlayWindow ?? throw new ArgumentNullException(nameof(overlayWindow));
            _overlayViewModel = overlayWindow.DataContext as OverlayViewModel;

            if (_overlayViewModel == null)
            {
                throw new InvalidOperationException("OverlayWindow의 DataContext가 OverlayViewModel이 아닙니다.");
            }
        }

        /// <summary>
        /// 전역 마우스 훅에서 발생한 마우스 액션을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 상태 기계(State Machine) 로직:
        /// - Down (트리거): 시작점 기록, OverlayWindow.Show(), 사각형 초기화
        /// - Move (드래그): 현재 좌표와 시작점을 이용해 Rect 계산, OverlayViewModel.UpdateRect() 호출
        /// - Up (종료): OverlayWindow.Hide(), 최종 Rect 로그 출력
        /// </remarks>
        private void OnMouseAction(object? sender, MouseActionEventArgs e)
        {
            // 트리거 버튼(XButton1)만 처리
            if (e.Button != MouseButton.XButton1)
            {
                return;
            }

            // UI 스레드에서 실행되도록 보장
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    switch (e.ActionType)
                    {
                        case MouseActionType.Down:
                            HandleMouseDown(e.X, e.Y);
                            break;

                        case MouseActionType.Move:
                            HandleMouseMove(e.X, e.Y);
                            break;

                        case MouseActionType.Up:
                            HandleMouseUp(e.X, e.Y);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainViewModel] 마우스 액션 처리 중 오류: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 마우스 Down 이벤트 처리 (트리거 시작)
        /// </summary>
        private void HandleMouseDown(int x, int y)
        {
            // 시작점 기록
            _dragStartX = x;
            _dragStartY = y;

            // OverlayWindow 표시
            if (_overlayWindow != null)
            {
                _overlayWindow.Show();
            }

            // 사각형 초기화
            if (_overlayViewModel != null)
            {
                _overlayViewModel.Reset();
            }

            Debug.WriteLine($"[MainViewModel] 드래그 시작: ({x}, {y})");
        }

        /// <summary>
        /// 마우스 Move 이벤트 처리 (드래그 중)
        /// </summary>
        /// <remarks>
        /// 주의: MouseMove 이벤트는 매우 빈번하므로 로직을 가볍게 유지해야 합니다.
        /// 현재는 1:1 매핑으로 구현되어 있으며, 향후 DPI 보정이 필요할 수 있습니다.
        /// </remarks>
        private void HandleMouseMove(int x, int y)
        {
            // OverlayWindow가 표시되어 있을 때만 처리
            if (_overlayWindow == null || !_overlayWindow.IsVisible || _overlayViewModel == null)
            {
                return;
            }

            // 현재 좌표와 시작점을 이용해 Rect 계산
            var rect = new Rect(
                Math.Min(_dragStartX, x),
                Math.Min(_dragStartY, y),
                Math.Abs(x - _dragStartX),
                Math.Abs(y - _dragStartY));

            // OverlayViewModel에 사각형 업데이트
            _overlayViewModel.UpdateRect(rect);
        }

        /// <summary>
        /// 마우스 Up 이벤트 처리 (트리거 종료)
        /// </summary>
        private void HandleMouseUp(int x, int y)
        {
            // OverlayWindow 숨김
            if (_overlayWindow != null)
            {
                _overlayWindow.Hide();
            }

            // 최종 Rect 계산 및 로그 출력
            var finalRect = new Rect(
                Math.Min(_dragStartX, x),
                Math.Min(_dragStartY, y),
                Math.Abs(x - _dragStartX),
                Math.Abs(y - _dragStartY));

            Debug.WriteLine($"[MainViewModel] 드래그 종료: Rect({finalRect.X}, {finalRect.Y}, {finalRect.Width}, {finalRect.Height})");

            // 사각형 초기화
            if (_overlayViewModel != null)
            {
                _overlayViewModel.Reset();
            }
        }
    }
}
