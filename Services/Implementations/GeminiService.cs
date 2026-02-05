using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using AI_Mouse.Services.Interfaces;

namespace AI_Mouse.Services.Implementations
{
    /// <summary>
    /// Google Gemini 1.5 Pro API와 통신하는 서비스 구현
    /// </summary>
    public class GeminiService : IGeminiService
    {
        // 사용자 환경에 따라 'gemini-1.5-flash', 'gemini-2.5-flash' 등으로 변경 가능
        private const string ModelId = "gemini-2.5-flash";
        private const string ApiVersion = "v1beta";

        private readonly HttpClient _httpClient;

        /// <summary>
        /// GeminiService 생성자
        /// </summary>
        /// <param name="httpClient">HttpClient 인스턴스 (DI 주입)</param>
        public GeminiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// 이미지와 오디오를 Gemini API에 전송하여 응답을 받습니다.
        /// </summary>
        /// <param name="image">캡처된 이미지</param>
        /// <param name="audioPath">녹음된 오디오 파일 경로</param>
        /// <param name="apiKey">Google AI Studio API Key</param>
        /// <returns>Gemini API 응답 텍스트</returns>
        public async Task<string> GetResponseAsync(BitmapSource image, string audioPath, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API 키가 설정되지 않았습니다.", nameof(apiKey));
            }

            try
            {
                // 사용 중인 모델 ID 로그 출력
                System.Diagnostics.Debug.WriteLine($"[Gemini API] Target Model: {ModelId}");

                // 이미지를 Base64로 변환
                string imageBase64 = ConvertImageToBase64(image);

                // 오디오를 Base64로 변환
                string audioBase64 = ConvertAudioToBase64(audioPath);

                // 요청 JSON 생성
                var requestBody = new GeminiRequest
                {
                    Contents = new[]
                    {
                        new Content
                        {
                            Parts = new Part[]
                            {
                                new Part { Text = "당신은 윈도우 AI 비서입니다. 화면과 음성을 보고 한국어로 답변하세요." },
                                new Part
                                {
                                    InlineData = new InlineData
                                    {
                                        MimeType = "image/jpeg",
                                        Data = imageBase64
                                    }
                                },
                                new Part
                                {
                                    InlineData = new InlineData
                                    {
                                        MimeType = "audio/wav",
                                        Data = audioBase64
                                    }
                                }
                            }
                        }
                    }
                };

                string jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // API 엔드포인트 (동적 생성)
                string endpoint = $"https://generativelanguage.googleapis.com/{ApiVersion}/models/{ModelId}:generateContent?key={apiKey}";

                // HTTP POST 요청
                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

                // 응답 확인
                response.EnsureSuccessStatusCode();

                // 응답 파싱
                string responseJson = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseJson);

                // 응답 텍스트 추출
                if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0)
                {
                    var candidate = geminiResponse.Candidates[0];
                    if (candidate.Content?.Parts != null && candidate.Content.Parts.Length > 0)
                    {
                        return candidate.Content.Parts[0].Text ?? "응답이 비어있습니다.";
                    }
                }

                return "응답을 파싱할 수 없습니다.";
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Gemini API 호출 중 오류가 발생했습니다: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"요청 처리 중 오류가 발생했습니다: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// BitmapSource를 JPEG로 인코딩하여 Base64 문자열로 변환합니다.
        /// </summary>
        private string ConvertImageToBase64(BitmapSource image)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        /// <summary>
        /// 오디오 파일을 읽어서 Base64 문자열로 변환합니다.
        /// </summary>
        private string ConvertAudioToBase64(string audioPath)
        {
            if (string.IsNullOrWhiteSpace(audioPath) || !File.Exists(audioPath))
            {
                throw new FileNotFoundException($"오디오 파일을 찾을 수 없습니다: {audioPath}");
            }

            byte[] audioBytes = File.ReadAllBytes(audioPath);
            return Convert.ToBase64String(audioBytes);
        }

        #region DTO Classes (Request/Response JSON 모델)

        /// <summary>
        /// Gemini API 요청 본문 구조
        /// </summary>
        private class GeminiRequest
        {
            [JsonProperty("contents")]
            public Content[]? Contents { get; set; }
        }

        /// <summary>
        /// 콘텐츠 구조
        /// </summary>
        private class Content
        {
            [JsonProperty("parts")]
            public Part[]? Parts { get; set; }
        }

        /// <summary>
        /// 파트 구조 (텍스트 또는 인라인 데이터)
        /// </summary>
        private class Part
        {
            [JsonProperty("text")]
            public string? Text { get; set; }

            [JsonProperty("inline_data")]
            public InlineData? InlineData { get; set; }
        }

        /// <summary>
        /// 인라인 데이터 구조 (이미지/오디오)
        /// </summary>
        private class InlineData
        {
            [JsonProperty("mime_type")]
            public string? MimeType { get; set; }

            [JsonProperty("data")]
            public string? Data { get; set; }
        }

        /// <summary>
        /// Gemini API 응답 구조
        /// </summary>
        private class GeminiResponse
        {
            [JsonProperty("candidates")]
            public Candidate[]? Candidates { get; set; }
        }

        /// <summary>
        /// 후보 응답 구조
        /// </summary>
        private class Candidate
        {
            [JsonProperty("content")]
            public Content? Content { get; set; }
        }

        #endregion
    }
}
