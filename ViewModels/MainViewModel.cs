using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AI_Mouse.Helpers;
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
        private OverlayWindow? _overlayWindow;
        private OverlayViewModel? _overlayViewModel;

        // 드래그 시작점 좌표 (화면 좌표계 - 물리 좌표)
        private int _dragStartX;
        private int _dragStartY;

        /// <summary>
        /// MainViewModel 생성자
        /// </summary>
        /// <param name="hookService">전역 마우스 훅 서비스 (DI 주입)</param>
        /// <param name="captureService">화면 캡처 서비스 (DI 주입)</param>
        /// <param name="audioService">오디오 녹음 서비스 (DI 주입)</param>
        /// <param name="geminiService">Gemini API 서비스 (DI 주입)</param>
        /// <param name="serviceProvider">서비스 프로바이더 (DI 주입)</param>
        public MainViewModel(IGlobalHookService hookService, IScreenCaptureService captureService, IAudioRecorderService audioService, IGeminiService geminiService, IServiceProvider serviceProvider)
        {
            _hookService = hookService ?? throw new ArgumentNullException(nameof(hookService));
            _captureService = captureService ?? throw new ArgumentNullException(nameof(captureService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

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

                    // API Key 로드
                    string? apiKey = LoadApiKey();
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        // API Key가 없으면 사용자에게 안내
                        MessageBox.Show(
                            "실행 폴더에 apikey.txt 파일을 만들고 키를 넣어주세요.",
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
        /// 외부 파일(apikey.txt)에서 API Key를 로드합니다.
        /// </summary>
        /// <returns>API Key 문자열 (파일이 없거나 읽기 실패 시 null 또는 빈 문자열)</returns>
        /// <remarks>
        /// 보안: API Key는 코드에 하드코딩하지 않고 외부 파일에서 로드하여 GitHub 유출을 방지합니다.
        /// 파일 경로: AppDomain.CurrentDomain.BaseDirectory (실행 파일과 같은 폴더)
        /// </remarks>
        private string? LoadApiKey()
        {
            try
            {
                // 실행 파일과 같은 폴더에서 apikey.txt 찾기
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string apiKeyPath = Path.Combine(baseDirectory, "apikey.txt");

                // 파일이 존재하는지 확인
                if (!File.Exists(apiKeyPath))
                {
                    Debug.WriteLine($"[MainViewModel] API Key 파일을 찾을 수 없습니다: {apiKeyPath}");
                    return null;
                }

                // 파일 내용 읽기 (공백 제거)
                string apiKey = File.ReadAllText(apiKeyPath).Trim();

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    Debug.WriteLine("[MainViewModel] API Key 파일이 비어있습니다.");
                    return null;
                }

                Debug.WriteLine("[MainViewModel] API Key 로드 성공");
                return apiKey;
            }
            catch (Exception ex)
            {
                // 파일 읽기 실패 시 예외 처리 (앱이 멈추지 않도록)
                Debug.WriteLine($"[MainViewModel] API Key 로드 중 오류 발생: {ex.Message}");
                return null;
            }
        }
    }
}
