using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using AI_Mouse.Helpers;
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
        private readonly IScreenCaptureService _captureService;
        private readonly IAudioRecorderService _audioService;
        private readonly IGeminiService _geminiService;
        private OverlayWindow? _overlayWindow;
        private OverlayViewModel? _overlayViewModel;

        // 드래그 시작점 좌표 (화면 좌표계 - 물리 좌표)
        private int _dragStartX;
        private int _dragStartY;

        // TODO: 여기에 Google AI Studio API Key를 입력하세요
        private const string ApiKey = "";

        /// <summary>
        /// MainViewModel 생성자
        /// </summary>
        /// <param name="hookService">전역 마우스 훅 서비스 (DI 주입)</param>
        /// <param name="captureService">화면 캡처 서비스 (DI 주입)</param>
        /// <param name="audioService">오디오 녹음 서비스 (DI 주입)</param>
        /// <param name="geminiService">Gemini API 서비스 (DI 주입)</param>
        public MainViewModel(IGlobalHookService hookService, IScreenCaptureService captureService, IAudioRecorderService audioService, IGeminiService geminiService)
        {
            _hookService = hookService ?? throw new ArgumentNullException(nameof(hookService));
            _captureService = captureService ?? throw new ArgumentNullException(nameof(captureService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));

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

            // 오디오 녹음 시작
            try
            {
                _audioService.StartRecording();
                Debug.WriteLine("[MainViewModel] 오디오 녹음 시작");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainViewModel] 오디오 녹음 시작 중 오류: {ex.Message}");
                // 마이크가 없거나 권한이 없는 경우에도 계속 진행
            }

            Debug.WriteLine($"[MainViewModel] 드래그 시작: ({x}, {y})");
        }

        /// <summary>
        /// 마우스 Move 이벤트 처리 (드래그 중)
        /// </summary>
        /// <remarks>
        /// 주의: MouseMove 이벤트는 매우 빈번하므로 로직을 가볍게 유지해야 합니다.
        /// 마우스 훅은 물리 좌표를 제공하지만, WPF OverlayWindow는 논리 좌표를 사용하므로
        /// DpiHelper를 통해 물리 좌표를 논리 좌표로 변환해야 올바르게 표시됩니다.
        /// </remarks>
        private void HandleMouseMove(int x, int y)
        {
            // OverlayWindow가 표시되어 있을 때만 처리
            if (_overlayWindow == null || !_overlayWindow.IsVisible || _overlayViewModel == null)
            {
                return;
            }

            // 물리 좌표로 Rect 계산 (훅이 물리 좌표를 제공)
            var physicalRect = new Rect(
                Math.Min(_dragStartX, x),
                Math.Min(_dragStartY, y),
                Math.Abs(x - _dragStartX),
                Math.Abs(y - _dragStartY));

            // 물리 좌표를 논리 좌표로 변환 (WPF OverlayWindow에 표시하기 위해)
            var logicalRect = DpiHelper.PhysicalToLogicalRect(physicalRect);

            // OverlayViewModel에 사각형 업데이트 (논리 좌표 사용)
            _overlayViewModel.UpdateRect(logicalRect);
        }

        /// <summary>
        /// 마우스 Up 이벤트 처리 (트리거 종료)
        /// </summary>
        private async void HandleMouseUp(int x, int y)
        {
            // OverlayWindow 숨김
            if (_overlayWindow != null)
            {
                _overlayWindow.Hide();
            }

            // 최종 Rect 계산 (물리 좌표 - 캡처 서비스가 물리 좌표를 사용)
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

            // 유효한 영역인 경우에만 캡처 수행
            if (finalRect.Width > 0 && finalRect.Height > 0)
            {
                try
                {
                    // 화면 캡처 (물리 좌표 사용)
                    var capturedImage = await _captureService.CaptureRegionAsync(finalRect);

                    // 클립보드에 복사
                    await _captureService.CopyToClipboardAsync(capturedImage);

                    // 오디오 녹음 중지 및 파일 경로 받기
                    string audioPath = string.Empty;
                    try
                    {
                        audioPath = await _audioService.StopRecordingAsync();
                        Debug.WriteLine($"[MainViewModel] 오디오 녹음 중지 완료: {audioPath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[MainViewModel] 오디오 녹음 중지 중 오류: {ex.Message}");
                        // 오디오 녹음 오류는 무시하고 계속 진행
                    }

                    // Gemini API 호출
                    if (string.IsNullOrWhiteSpace(ApiKey))
                    {
                        MessageBox.Show(
                            "API 키를 설정해주세요.",
                            "API 키 필요",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    try
                    {
                        // Gemini API 호출
                        string response = await _geminiService.GetResponseAsync(capturedImage, audioPath, ApiKey);

                        // 결과를 클립보드에 복사
                        Clipboard.SetText(response);

                        // 결과를 메시지 박스로 출력
                        MessageBox.Show(
                            $"Gemini 응답:\n\n{response}\n\n(결과가 클립보드에 복사되었습니다.)",
                            "Gemini 응답",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        Debug.WriteLine($"[MainViewModel] Gemini API 응답 수신 완료");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[MainViewModel] Gemini API 호출 중 오류: {ex.Message}");
                        MessageBox.Show(
                            $"Gemini API 호출 중 오류가 발생했습니다:\n{ex.Message}",
                            "API 오류",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    Debug.WriteLine($"[MainViewModel] 화면 캡처 및 클립보드 복사 완료: {finalRect.Width}x{finalRect.Height}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainViewModel] 화면 캡처 중 오류: {ex.Message}");
                    MessageBox.Show(
                        $"화면 캡처 중 오류가 발생했습니다:\n{ex.Message}",
                        "오류",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                // 영역이 유효하지 않아도 오디오 녹음은 중지해야 함
                try
                {
                    await _audioService.StopRecordingAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainViewModel] 오디오 녹음 중지 중 오류: {ex.Message}");
                }
            }
        }
    }
}
