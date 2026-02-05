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

        /// <summary>
        /// 애플리케이션 시작 시 DI 컨테이너를 구성하고 MainWindow를 생성합니다.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. ServiceCollection 인스턴스 생성
            var services = new ServiceCollection();

            // 2. MainViewModel과 MainWindow를 AddTransient로 등록
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            // 3. OverlayViewModel과 OverlayWindow를 AddTransient로 등록
            services.AddTransient<OverlayViewModel>();
            services.AddTransient<OverlayWindow>();

            // 4. GlobalHookService를 싱글톤으로 등록
            services.AddSingleton<IGlobalHookService, GlobalHookService>();

            // 5. ScreenCaptureService를 싱글톤으로 등록
            services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();

            // 6. AudioRecorderService를 싱글톤으로 등록
            services.AddSingleton<IAudioRecorderService, AudioRecorderService>();

            // 7. HttpClient를 싱글톤으로 등록
            services.AddSingleton<HttpClient>();

            // 8. GeminiService를 싱글톤으로 등록 (HttpClient 주입)
            services.AddSingleton<IGeminiService, GeminiService>();

            // 9. ServiceProvider 빌드
            _serviceProvider = services.BuildServiceProvider();

            // 10. MainWindow 인스턴스를 DI로 생성
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // 11. MainViewModel을 DI로 생성 (IGlobalHookService, IScreenCaptureService, IAudioRecorderService, IGeminiService 자동 주입)
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            // 12. MainWindow.DataContext에 MainViewModel 설정
            mainWindow.DataContext = mainViewModel;

            // 13. MainWindow를 숨김 상태로 유지 (초기 상태)
            mainWindow.Hide();

            // 14. OverlayWindow를 미리 생성하되 Hide() 상태로 대기 (반응 속도 최적화)
            _overlayWindow = _serviceProvider.GetRequiredService<OverlayWindow>();
            _overlayWindow.Hide();

            // 15. MainViewModel에 OverlayWindow 참조 전달 (Show/Hide를 위해 필요)
            mainViewModel.SetOverlayWindow(_overlayWindow);

            // 16. 리소스에서 TaskbarIcon을 찾아 _trayIcon 멤버 변수에 할당
            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            
            // 빈 아이콘 에러 방지를 위해 System.Drawing.SystemIcons.Application 할당
            if (_trayIcon != null)
            {
                _trayIcon.Icon = SystemIcons.Application;
            }

            // 17. GlobalHookService를 가져와서 훅 시작
            var hookService = _serviceProvider.GetRequiredService<IGlobalHookService>();
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
                    System.Diagnostics.Debug.WriteLine($"[App] 훅 중지 중 오류: {ex.Message}");
                }

                try
                {
                    // AudioRecorderService 정리 (IDisposable)
                    var audioService = _serviceProvider.GetService<IAudioRecorderService>();
                    audioService?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] 오디오 서비스 정리 중 오류: {ex.Message}");
                }

                try
                {
                    // HttpClient 정리
                    var httpClient = _serviceProvider.GetService<HttpClient>();
                    httpClient?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] HttpClient 정리 중 오류: {ex.Message}");
                }
            }

            // 트레이 아이콘 정리
            _trayIcon?.Dispose();

            // ServiceProvider 정리 (모든 IDisposable 서비스 해제)
            _serviceProvider?.Dispose();

            base.OnExit(e);
        }

        /// <summary>
        /// 트레이 메뉴의 '설정' 항목 클릭 이벤트 핸들러
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "설정 창은 추후 구현될 예정입니다. (Phase 4.2)", 
                "알림", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
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
