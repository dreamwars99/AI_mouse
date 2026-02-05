using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AI_Mouse.Models;
using AI_Mouse.Services.Interfaces;

namespace AI_Mouse.ViewModels
{
    /// <summary>
    /// 설정 창의 ViewModel
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IGlobalHookService _hookService;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// 설정 저장 완료 이벤트 (창 닫기용)
        /// </summary>
        public event EventHandler? SettingsSaved;

        /// <summary>
        /// API Key (apikey.txt에서 로드)
        /// </summary>
        [ObservableProperty]
        private string _apiKey = string.Empty;

        /// <summary>
        /// 선택된 트리거 버튼 (콤보박스 바인딩용)
        /// </summary>
        [ObservableProperty]
        private TriggerButton _selectedButton = TriggerButton.XButton1;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="hookService">전역 마우스 훅 서비스 (DI 주입)</param>
        /// <param name="settingsService">설정 서비스 (DI 주입)</param>
        public SettingsViewModel(IGlobalHookService hookService, ISettingsService settingsService)
        {
            _hookService = hookService ?? throw new ArgumentNullException(nameof(hookService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // 기존 설정 로드
            LoadSettings();
        }

        /// <summary>
        /// SelectedButton 속성이 변경될 때 GlobalHookService에 즉시 반영
        /// </summary>
        partial void OnSelectedButtonChanged(TriggerButton value)
        {
            if (_hookService != null)
            {
                _hookService.CurrentTrigger = value;
                Debug.WriteLine($"[SettingsViewModel] 트리거 버튼 변경: {value}");
            }
        }

        /// <summary>
        /// 설정 저장 명령
        /// </summary>
        [RelayCommand]
        private void Save()
        {
            try
            {
                // SettingsService를 통해 설정 저장 (JSON 파일)
                var config = new AppConfig
                {
                    ApiKey = ApiKey.Trim(),
                    TriggerButton = SelectedButton
                };
                _settingsService.Save(config);
                Debug.WriteLine("[SettingsViewModel] 설정 저장 완료 (JSON)");

                MessageBox.Show(
                    "설정이 저장되었습니다.",
                    "저장 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 설정 저장 완료 이벤트 발생 (창 닫기용)
                SettingsSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsViewModel] 설정 저장 중 오류: {ex.Message}");
                MessageBox.Show(
                    $"설정 저장 중 오류가 발생했습니다:\n\n{ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 임시 파일 폴더 열기 명령
        /// </summary>
        [RelayCommand]
        private void OpenFolder()
        {
            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "AI_Mouse");
                
                // 폴더가 없으면 생성
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                // 탐색기로 폴더 열기
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = tempFolder,
                    UseShellExecute = true
                });

                Debug.WriteLine($"[SettingsViewModel] 임시 폴더 열기: {tempFolder}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsViewModel] 폴더 열기 중 오류: {ex.Message}");
                MessageBox.Show(
                    $"폴더 열기 중 오류가 발생했습니다:\n\n{ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 기존 설정을 로드합니다.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // SettingsService를 통해 설정 로드 (JSON 파일)
                var config = _settingsService.Load();
                ApiKey = config.ApiKey;
                SelectedButton = config.TriggerButton;

                // GlobalHookService에도 반영
                _hookService.CurrentTrigger = config.TriggerButton;

                Debug.WriteLine("[SettingsViewModel] 설정 로드 완료 (JSON)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsViewModel] 설정 로드 중 오류: {ex.Message}");
            }
        }
    }
}
