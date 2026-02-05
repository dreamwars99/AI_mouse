using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AI_Mouse.Services.Interfaces
{
    /// <summary>
    /// Google Gemini API와 통신하는 서비스 인터페이스
    /// </summary>
    public interface IGeminiService
    {
        /// <summary>
        /// 이미지와 오디오를 Gemini API에 전송하여 응답을 받습니다.
        /// </summary>
        /// <param name="image">캡처된 이미지</param>
        /// <param name="audioPath">녹음된 오디오 파일 경로</param>
        /// <param name="apiKey">Google AI Studio API Key</param>
        /// <returns>Gemini API 응답 텍스트</returns>
        Task<string> GetResponseAsync(BitmapSource image, string audioPath, string apiKey);
    }
}
