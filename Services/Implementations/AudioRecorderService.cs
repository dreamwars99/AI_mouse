using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AI_Mouse.Services.Interfaces;
using NAudio.Wave;

namespace AI_Mouse.Services.Implementations
{
    /// <summary>
    /// NAudio 기반 마이크 음성 녹음 서비스 구현체
    /// WaveInEvent를 사용하여 마이크 입력을 캡처하고 WAV 파일로 저장합니다.
    /// </summary>
    public class AudioRecorderService : IAudioRecorderService
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _waveFileWriter;
        private string? _outputFilePath;
        private bool _isRecording;
        private TaskCompletionSource<string>? _stopCompletionSource;

        /// <summary>
        /// 녹음을 시작합니다.
        /// </summary>
        public void StartRecording()
        {
            // 이미 녹음 중이면 무시
            if (_isRecording)
            {
                Debug.WriteLine("[AudioRecorderService] 이미 녹음 중입니다.");
                return;
            }

            try
            {
                // 임시 폴더 경로 생성 (Path.GetTempPath() 하위에 AI_Mouse 폴더)
                var tempPath = Path.GetTempPath();
                var aiMouseFolder = Path.Combine(tempPath, "AI_Mouse");
                
                // 폴더가 없으면 생성
                if (!Directory.Exists(aiMouseFolder))
                {
                    Directory.CreateDirectory(aiMouseFolder);
                }

                // 출력 파일 경로 설정 (덮어쓰기 모드)
                _outputFilePath = Path.Combine(aiMouseFolder, "audio_temp.wav");

                // WaveInEvent 초기화 (16kHz, 16bit, Mono - Gemini API 호환성)
                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz, 16bit, Mono
                };

                // 데이터 수신 이벤트 핸들러 등록
                _waveIn.DataAvailable += OnDataAvailable;

                // 녹음 중지 이벤트 핸들러 등록
                _waveIn.RecordingStopped += OnRecordingStopped;

                // WaveFileWriter 초기화
                _waveFileWriter = new WaveFileWriter(_outputFilePath, _waveIn.WaveFormat);

                // 녹음 시작
                _waveIn.StartRecording();
                _isRecording = true;

                Debug.WriteLine($"[AudioRecorderService] 녹음 시작: {_outputFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioRecorderService] 녹음 시작 중 오류: {ex.Message}");
                // 리소스 정리
                Cleanup();
                throw new InvalidOperationException($"마이크 녹음을 시작할 수 없습니다: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 녹음을 중지하고 WAV 파일로 저장합니다.
        /// </summary>
        /// <returns>저장된 WAV 파일의 전체 경로</returns>
        public async Task<string> StopRecordingAsync()
        {
            if (!_isRecording || _waveIn == null)
            {
                Debug.WriteLine("[AudioRecorderService] 녹음 중이 아닙니다.");
                return string.Empty;
            }

            try
            {
                // TaskCompletionSource 생성 (RecordingStopped 이벤트 대기용)
                _stopCompletionSource = new TaskCompletionSource<string>();

                // 녹음 중지
                _waveIn.StopRecording();

                // RecordingStopped 이벤트가 발생할 때까지 대기
                var filePath = await _stopCompletionSource.Task;

                Debug.WriteLine($"[AudioRecorderService] 녹음 중지 완료: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioRecorderService] 녹음 중지 중 오류: {ex.Message}");
                Cleanup();
                throw new InvalidOperationException($"녹음을 중지하는 중 오류가 발생했습니다: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 오디오 데이터 수신 이벤트 핸들러
        /// </summary>
        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            try
            {
                // WaveFileWriter에 데이터 쓰기
                _waveFileWriter?.Write(e.Buffer, 0, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioRecorderService] 데이터 쓰기 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 녹음 중지 이벤트 핸들러
        /// </summary>
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _isRecording = false;
            
            if (e.Exception != null)
            {
                Debug.WriteLine($"[AudioRecorderService] 녹음 중지 중 예외 발생: {e.Exception.Message}");
            }

            // 파일 경로 저장 (Cleanup 전에 저장)
            var filePath = _outputFilePath ?? string.Empty;

            // 리소스 정리
            Cleanup();

            // TaskCompletionSource 완료 (파일 경로 반환)
            _stopCompletionSource?.TrySetResult(filePath);
        }

        /// <summary>
        /// 리소스를 정리합니다.
        /// </summary>
        private void Cleanup()
        {
            try
            {
                // WaveFileWriter 정리 (파일 스트림 닫기 - 중요!)
                if (_waveFileWriter != null)
                {
                    _waveFileWriter.Dispose();
                    _waveFileWriter = null;
                }

                // WaveInEvent 정리
                if (_waveIn != null)
                {
                    _waveIn.DataAvailable -= OnDataAvailable;
                    _waveIn.RecordingStopped -= OnRecordingStopped;
                    _waveIn.Dispose();
                    _waveIn = null;
                }

                _isRecording = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioRecorderService] 리소스 정리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// IDisposable 구현
        /// </summary>
        public void Dispose()
        {
            Cleanup();
        }
    }
}
