using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Hardcodet.Wpf.TaskbarNotification;
using AI_Mouse.Views;
using AI_Mouse.ViewModels;
using System.Drawing;
using AI_Mouse.Services.Interfaces;
using AI_Mouse.Services.Implementations;
using System.Net.Http;
using System;
using AI_Mouse.Helpers;

namespace AI_Mouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon? _trayIcon;
        private ServiceProvider? _serviceProvider;
        private OverlayWindow? _overlayWindow;
        private SettingsWindow? _settingsWindow; // 중복 실행 방지용

        /// <summary>
        /// ServiceProvider를 외부에서 접근할 수 있도록 제공합니다.
        /// </summary>
        public static IServiceProvider? Services => ((App)Current)._serviceProvider;

        /// <summary>
        /// 애플리케이션 시작 시 DI 컨테이너를 구성하고 MainWindow를 생성합니다.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 0. 로거 초기화 및 앱 시작 로그 기록
            try
            {
                Logger.Initialize();
                Logger.Info("앱 시작됨");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] 로거 초기화 실패: {ex.Message}");
            }

            // 전역 예외 처리 이벤트 구독
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            // 1. ServiceCollection 인스턴스 생성
            var services = new ServiceCollection();

            // 2. MainViewModel과 MainWindow를 AddTransient로 등록
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            // 3. OverlayViewModel과 OverlayWindow를 AddTransient로 등록
            services.AddTransient<OverlayViewModel>();
            services.AddTransient<OverlayWindow>();

            // 4. ResultViewModel과 ResultWindow를 AddTransient로 등록 (질문할 때마다 새 창을 띄우기 위함)
            services.AddTransient<ResultViewModel>();
            services.AddTransient<ResultWindow>();

            // 5. SettingsViewModel과 SettingsWindow를 AddTransient로 등록
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsWindow>();

            // 6. GlobalHookService를 싱글톤으로 등록
            services.AddSingleton<IGlobalHookService, GlobalHookService>();

            // 7. ScreenCaptureService를 싱글톤으로 등록
            services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();

            // 8. AudioRecorderService를 싱글톤으로 등록
            services.AddSingleton<IAudioRecorderService, AudioRecorderService>();

            // 9. HttpClient를 싱글톤으로 등록
            services.AddSingleton<HttpClient>();

            // 10. GeminiService를 싱글톤으로 등록 (HttpClient 주입)
            services.AddSingleton<IGeminiService, GeminiService>();

            // 11. ServiceProvider 빌드
            _serviceProvider = services.BuildServiceProvider();

            // 11. MainWindow 인스턴스를 DI로 생성
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // 12. MainViewModel을 DI로 생성 (IGlobalHookService, IScreenCaptureService, IAudioRecorderService, IGeminiService 자동 주입)
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            // 13. MainWindow.DataContext에 MainViewModel 설정
            mainWindow.DataContext = mainViewModel;

            // 14. MainWindow를 숨김 상태로 유지 (초기 상태)
            mainWindow.Hide();

            // 15. OverlayWindow를 미리 생성하되 Hide() 상태로 대기 (반응 속도 최적화)
            _overlayWindow = _serviceProvider.GetRequiredService<OverlayWindow>();
            _overlayWindow.Hide();

            // 16. MainViewModel에 OverlayWindow 참조 전달 (Show/Hide를 위해 필요)
            mainViewModel.SetOverlayWindow(_overlayWindow);

            // 17. 리소스에서 TaskbarIcon을 찾아 _trayIcon 멤버 변수에 할당
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            
            // 빈 아이콘 에러 방지를 위해 System.Drawing.SystemIcons.Application 할당
            if (_trayIcon != null)
            {
                _trayIcon.Icon = SystemIcons.Application;
            }

            // 18. GlobalHookService를 가져와서 기본 트리거 설정 및 훅 시작
            var hookService = _serviceProvider.GetRequiredService<IGlobalHookService>();
            hookService.CurrentTrigger = Models.TriggerButton.XButton1; // 기본값: XButton1 (뒤로 가기)
            hookService.Start();

            // 검증: 앱 실행 확인용 메시지 박스 출력
            MessageBox.Show(
                "AI Mouse가 백그라운드에서 실행되었습니다.\n트레이 아이콘을 우클릭해보세요.", 
                "실행 성공", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        /// <summary>
        /// 애플리케이션 종료 시 리소스를 정리합니다.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // GlobalHookService 중지 (훅 해제)
                if (_serviceProvider != null)
                {
                    try
                    {
                        var hookService = _serviceProvider.GetService<IGlobalHookService>();
                        hookService?.Stop();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("훅 중지 중 오류", ex);
                    }

                    try
                    {
                        // AudioRecorderService 정리 (IDisposable)
                        var audioService = _serviceProvider.GetService<IAudioRecorderService>();
                        audioService?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("오디오 서비스 정리 중 오류", ex);
                    }

                    try
                    {
                        // HttpClient 정리
                        var httpClient = _serviceProvider.GetService<HttpClient>();
                        httpClient?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("HttpClient 정리 중 오류", ex);
                    }

                    // ServiceProvider가 IDisposable이면 Dispose 호출
                    if (_serviceProvider is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                // 트레이 아이콘 정리
                _trayIcon?.Dispose();

                // 앱 종료 로그 기록
                Logger.Info("앱 종료됨");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] 종료 처리 중 오류: {ex.Message}");
            }

            base.OnExit(e);
        }

        /// <summary>
        /// WPF 디스패처에서 처리되지 않은 예외를 처리합니다.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Logger.Error("처리되지 않은 예외 발생 (Dispatcher)", e.Exception);
                
                MessageBox.Show(
                    "오류가 발생했습니다. 로그를 확인하세요.\n\n" + e.Exception.Message,
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // 가능하면 앱을 유지
                e.Handled = true;
            }
            catch
            {
                // 로거 자체가 실패하면 무시
            }
        }

        /// <summary>
        /// AppDomain에서 처리되지 않은 예외를 처리합니다 (복구 불가).
        /// </summary>
        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                {
                    Logger.Error("처리되지 않은 예외 발생 (AppDomain)", ex);
                }
                else
                {
                    Logger.Error($"처리되지 않은 예외 발생 (AppDomain): {e.ExceptionObject}");
                }
            }
            catch
            {
                // 로거 자체가 실패하면 무시
            }
        }

        /// <summary>
        /// 트레이 메뉴의 '설정' 항목 클릭 이벤트 핸들러
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_serviceProvider == null)
            {
                MessageBox.Show(
                    "서비스 프로바이더가 초기화되지 않았습니다.",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                // 이미 창이 열려있으면 활성화만 수행
                if (_settingsWindow != null && _settingsWindow.IsVisible)
                {
                    _settingsWindow.Activate();
                    _settingsWindow.Focus();
                    return;
                }

                // SettingsWindow와 SettingsViewModel 생성 (DI)
                _settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
                var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();

                // 창이 닫힐 때 참조 제거
                _settingsWindow.Closed += (s, args) =>
                {
                    _settingsWindow = null;
                };

                // ViewModel 설정
                _settingsWindow.SetViewModel(settingsViewModel);

                // 창 표시
                _settingsWindow.Show();
                _settingsWindow.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"설정 창을 열 수 없습니다:\n\n{ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 트레이 메뉴의 '종료' 항목 클릭 이벤트 핸들러
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
