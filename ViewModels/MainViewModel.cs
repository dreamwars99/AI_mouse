using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AI_Mouse.Helpers;
using AI_Mouse.Models;
using AI_Mouse.Services.Interfaces;
using AI_Mouse.Views;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService;
        private OverlayWindow? _overlayWindow;
        private OverlayViewModel? _overlayViewModel;

        // 드래그 시작점 좌표 (화면 좌표계 - 물리 좌표)
        private int _dragStartX;
        private int _dragStartY;
        private bool _isListening = false; // 드래그 중인지 추적 (취소 로직용)

        /// <summary>
        /// MainViewModel 생성자
        /// </summary>
        /// <param name="hookService">전역 마우스 훅 서비스 (DI 주입)</param>
        /// <param name="captureService">화면 캡처 서비스 (DI 주입)</param>
        /// <param name="audioService">오디오 녹음 서비스 (DI 주입)</param>
        /// <param name="geminiService">Gemini API 서비스 (DI 주입)</param>
        /// <param name="serviceProvider">서비스 프로바이더 (DI 주입)</param>
        /// <param name="settingsService">설정 서비스 (DI 주입)</param>
        public MainViewModel(IGlobalHookService hookService, IScreenCaptureService captureService, IAudioRecorderService audioService, IGeminiService geminiService, IServiceProvider serviceProvider, ISettingsService settingsService)
        {
            _hookService = hookService ?? throw new ArgumentNullException(nameof(hookService));
            _captureService = captureService ?? throw new ArgumentNullException(nameof(captureService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // 저장된 설정 로드 및 트리거 설정 적용
            var config = _settingsService.Load();
            _hookService.CurrentTrigger = config.TriggerButton;
            Debug.WriteLine($"[MainViewModel] 저장된 설정 로드 완료: TriggerButton={config.TriggerButton}");

            // 마우스 액션 이벤트 구독
            _hookService.MouseAction += OnMouseAction;

            // 취소 요청 이벤트 구독 (ESC 키 감지)
            _hookService.CancellationRequested += OnCancellationRequested;
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
            // 현재 트리거 버튼만 처리
            var currentTriggerButton = ConvertTriggerToMouseButton(_hookService.CurrentTrigger);
            if (e.Button != currentTriggerButton)
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
        /// 취소 요청 이벤트 핸들러 (ESC 키 감지 시 호출)
        /// </summary>
        private void OnCancellationRequested(object? sender, EventArgs e)
        {
            // UI 스레드에서 실행되도록 보장
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (_isListening)
                    {
                        Debug.WriteLine("[MainViewModel] ESC 키 감지: 작업 취소");

                        // OverlayWindow 숨김
                        if (_overlayWindow != null)
                        {
                            _overlayWindow.Hide();
                        }

                        // 사각형 초기화
                        if (_overlayViewModel != null)
                        {
                            _overlayViewModel.Reset();
                        }

                        // 오디오 녹음 중지 (Gemini 요청은 보내지 않음)
                        try
                        {
                            _audioService.StopRecordingAsync();
                            Debug.WriteLine("[MainViewModel] 오디오 녹음 중지 완료 (취소)");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MainViewModel] 오디오 녹음 중지 중 오류: {ex.Message}");
                        }

                        // 상태 리셋
                        _isListening = false;
                        _dragStartX = 0;
                        _dragStartY = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainViewModel] 취소 처리 중 오류: {ex.Message}");
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
            _isListening = true; // 드래그 시작 상태로 설정

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
            _isListening = false; // 드래그 종료 상태로 설정

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

                    // API Key 로드 (SettingsService 사용)
                    var config = _settingsService.Load();
                    string? apiKey = config.ApiKey;
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        // API Key가 없으면 사용자에게 안내
                        MessageBox.Show(
                            "설정에서 API Key를 입력해주세요.",
                            "API 키 필요",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // ResultWindow 생성 및 표시 (로딩 상태로 시작)
                    var resultWindow = _serviceProvider.GetRequiredService<ResultWindow>();
                    var resultViewModel = _serviceProvider.GetRequiredService<ResultViewModel>();
                    resultViewModel.IsLoading = true;
                    resultViewModel.ResponseText = string.Empty;
                    resultWindow.SetViewModel(resultViewModel);
                    resultWindow.Show();

                    try
                    {
                        // Gemini API 호출
                        string response = await _geminiService.GetResponseAsync(capturedImage, audioPath, apiKey);

                        // 결과를 클립보드에 복사
                        Clipboard.SetText(response);

                        // ResultWindow에 응답 표시
                        resultViewModel.ResponseText = response;
                        resultViewModel.IsLoading = false;

                        Debug.WriteLine($"[MainViewModel] Gemini API 응답 수신 완료");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[MainViewModel] Gemini API 호출 중 오류: {ex.Message}");
                        
                        // 오류 메시지를 ResultWindow에 표시
                        resultViewModel.ResponseText = $"## 오류 발생\n\nGemini API 호출 중 오류가 발생했습니다:\n\n```\n{ex.Message}\n```";
                        resultViewModel.IsLoading = false;
                    }

                    Debug.WriteLine($"[MainViewModel] 화면 캡처 및 클립보드 복사 완료: {finalRect.Width}x{finalRect.Height}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MainViewModel] 화면 캡처 중 오류: {ex.Message}");
                    
                    // ResultWindow를 통해 오류 메시지 표시
                    var errorWindow = _serviceProvider.GetRequiredService<ResultWindow>();
                    var errorViewModel = _serviceProvider.GetRequiredService<ResultViewModel>();
                    errorViewModel.ResponseText = $"## 오류 발생\n\n화면 캡처 중 오류가 발생했습니다:\n\n```\n{ex.Message}\n```";
                    errorViewModel.IsLoading = false;
                    errorWindow.SetViewModel(errorViewModel);
                    errorWindow.Show();
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

        /// <summary>
        /// 앱 종료 시 설정을 저장합니다. (App.xaml.cs의 OnExit에서 호출)
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var config = new AppConfig
                {
                    ApiKey = _settingsService.Load().ApiKey, // 현재 API Key 유지
                    TriggerButton = _hookService.CurrentTrigger // 현재 트리거 버튼 저장
                };
                _settingsService.Save(config);
                Debug.WriteLine("[MainViewModel] 설정 저장 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainViewModel] 설정 저장 중 오류: {ex.Message}");
                Logger.Error("설정 저장 중 오류", ex);
            }
        }

        /// <summary>
        /// TriggerButton을 MouseButton으로 변환합니다.
        /// </summary>
        private MouseButton ConvertTriggerToMouseButton(Models.TriggerButton trigger)
        {
            return trigger switch
            {
                Models.TriggerButton.Left => MouseButton.Left,
                Models.TriggerButton.Right => MouseButton.Right,
                Models.TriggerButton.Middle => MouseButton.Middle,
                Models.TriggerButton.XButton1 => MouseButton.XButton1,
                Models.TriggerButton.XButton2 => MouseButton.XButton2,
                _ => MouseButton.None
            };
        }
    }
}
