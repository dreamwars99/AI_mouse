using AI_Mouse.Models;

namespace AI_Mouse.Services.Interfaces
{
    /// <summary>
    /// 설정 영구 저장 서비스 인터페이스
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 설정을 로드합니다. (settings.json이 없으면 기본값 반환, apikey.txt가 있으면 마이그레이션)
        /// </summary>
        /// <returns>로드된 설정 또는 기본값</returns>
        AppConfig Load();

        /// <summary>
        /// 설정을 저장합니다. (JSON 직렬화하여 settings.json에 저장)
        /// </summary>
        /// <param name="config">저장할 설정</param>
        void Save(AppConfig config);
    }
}
