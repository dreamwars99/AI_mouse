using System;
using System.IO;
using System.Threading;

namespace AI_Mouse.Helpers
{
    /// <summary>
    /// 파일 기반 로깅 유틸리티 클래스
    /// </summary>
    public static class Logger
    {
        private static readonly object _lockObject = new object();
        private static string? _logDirectory;

        /// <summary>
        /// 로그 디렉토리 경로를 초기화합니다.
        /// </summary>
        public static void Initialize()
        {
            if (_logDirectory == null)
            {
                _logDirectory = Path.Combine(Path.GetTempPath(), "AI_Mouse", "logs");
                
                // 로그 디렉토리가 없으면 생성
                try
                {
                    if (!Directory.Exists(_logDirectory))
                    {
                        Directory.CreateDirectory(_logDirectory);
                    }
                }
                catch (Exception ex)
                {
                    // 디렉토리 생성 실패 시 기본 경로 사용 (예외는 무시)
                    System.Diagnostics.Debug.WriteLine($"[Logger] 로그 디렉토리 생성 실패: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 현재 날짜를 기반으로 로그 파일 경로를 생성합니다.
        /// </summary>
        private static string GetLogFilePath()
        {
            if (_logDirectory == null)
            {
                Initialize();
            }

            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            string fileName = $"log_{dateString}.txt";
            return Path.Combine(_logDirectory ?? Path.GetTempPath(), fileName);
        }

        /// <summary>
        /// 정보 로그를 기록합니다.
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 오류 로그를 기록합니다.
        /// </summary>
        /// <param name="message">로그 메시지</param>
        /// <param name="ex">예외 객체 (선택사항)</param>
        public static void Error(string message, Exception? ex = null)
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $"\n예외: {ex.GetType().Name}\n메시지: {ex.Message}\n스택 트레이스:\n{ex.StackTrace}";
            }
            WriteLog("ERROR", fullMessage);
        }

        /// <summary>
        /// 로그를 파일에 기록합니다.
        /// </summary>
        private static void WriteLog(string level, string message)
        {
            lock (_lockObject)
            {
                try
                {
                    string logFilePath = GetLogFilePath();
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    string logEntry = $"[{timestamp}] [{level}] {message}\n";

                    // 파일에 로그 추가 (비동기 처리는 단순함을 위해 생략, lock으로 동기화)
                    File.AppendAllText(logFilePath, logEntry);
                }
                catch (Exception ex)
                {
                    // 로그 기록 실패 시 디버그 출력으로 대체
                    System.Diagnostics.Debug.WriteLine($"[Logger] 로그 기록 실패: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[Logger] 원본 메시지: [{level}] {message}");
                }
            }
        }
    }
}
