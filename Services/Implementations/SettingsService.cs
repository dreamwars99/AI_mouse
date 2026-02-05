using System;
using System.Diagnostics;
using System.IO;
using AI_Mouse.Helpers;
using AI_Mouse.Models;
using AI_Mouse.Services.Interfaces;
using Newtonsoft.Json;

namespace AI_Mouse.Services.Implementations
{
    /// <summary>
    /// 설정 영구 저장 서비스 구현 (JSON 파일 기반)
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;

        /// <summary>
        /// 생성자
        /// </summary>
        public SettingsService()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _settingsPath = Path.Combine(baseDirectory, "settings.json");
        }

        /// <summary>
        /// 설정을 로드합니다. (settings.json이 없으면 기본값 반환, apikey.txt가 있으면 마이그레이션)
        /// </summary>
        /// <returns>로드된 설정 또는 기본값</returns>
        public AppConfig Load()
        {
            try
            {
                // settings.json이 있으면 로드
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);
                    
                    if (config != null)
                    {
                        Debug.WriteLine("[SettingsService] 설정 파일 로드 완료");
                        return config;
                    }
                }

                // settings.json이 없으면 apikey.txt 마이그레이션 시도
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string apiKeyPath = Path.Combine(baseDirectory, "apikey.txt");
                
                if (File.Exists(apiKeyPath))
                {
                    // apikey.txt에서 API Key 읽기
                    string apiKey = File.ReadAllText(apiKeyPath).Trim();
                    
                    // 새 설정 객체 생성 (기본값 사용)
                    var config = new AppConfig
                    {
                        ApiKey = apiKey,
                        TriggerButton = TriggerButton.XButton1 // 기본값
                    };
                    
                    // settings.json에 저장
                    Save(config);
                    
                    // 기존 apikey.txt 삭제 (마이그레이션 완료)
                    try
                    {
                        File.Delete(apiKeyPath);
                        Debug.WriteLine("[SettingsService] apikey.txt 마이그레이션 완료 및 삭제");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SettingsService] apikey.txt 삭제 실패: {ex.Message}");
                        // 삭제 실패해도 계속 진행
                    }
                    
                    return config;
                }

                // 둘 다 없으면 기본값 반환
                Debug.WriteLine("[SettingsService] 설정 파일이 없어 기본값 사용");
                return new AppConfig();
            }
            catch (Exception ex)
            {
                Logger.Error("설정 로드 중 오류", ex);
                Debug.WriteLine($"[SettingsService] 설정 로드 중 오류: {ex.Message}");
                // 오류 발생 시 기본값 반환
                return new AppConfig();
            }
        }

        /// <summary>
        /// 설정을 저장합니다. (JSON 직렬화하여 settings.json에 저장)
        /// </summary>
        /// <param name="config">저장할 설정</param>
        public void Save(AppConfig config)
        {
            try
            {
                if (config == null)
                {
                    throw new ArgumentNullException(nameof(config));
                }

                // JSON 직렬화
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                
                // 파일에 저장
                File.WriteAllText(_settingsPath, json);
                
                Debug.WriteLine("[SettingsService] 설정 저장 완료");
            }
            catch (Exception ex)
            {
                Logger.Error("설정 저장 중 오류", ex);
                Debug.WriteLine($"[SettingsService] 설정 저장 중 오류: {ex.Message}");
                throw;
            }
        }
    }
}
