using AI_Mouse.Models;

namespace AI_Mouse.Models
{
    /// <summary>
    /// 앱 설정 모델 (JSON 직렬화용)
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// API Key (Gemini API)
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// 트리거 버튼 (기본값: XButton1)
        /// </summary>
        public TriggerButton TriggerButton { get; set; } = TriggerButton.XButton1;
    }
}
