using System;
using System.Threading.Tasks;

namespace AI_Mouse.Services.Interfaces
{
    /// <summary>
    /// 마이크 음성 녹음 서비스 인터페이스
    /// NAudio를 사용하여 마이크 입력을 WAV 파일로 녹음하는 기능을 제공합니다.
    /// </summary>
    public interface IAudioRecorderService : IDisposable
    {
        /// <summary>
        /// 녹음을 시작합니다.
        /// </summary>
        void StartRecording();

        /// <summary>
        /// 녹음을 중지하고 WAV 파일로 저장합니다.
        /// </summary>
        /// <returns>저장된 WAV 파일의 전체 경로</returns>
        Task<string> StopRecordingAsync();
    }
}
