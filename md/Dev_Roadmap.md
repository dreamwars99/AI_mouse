# AI Mouse 개발 로드맵 (Development Roadmap)

## 프로젝트 개요
- **목표:** 드래그+음성(Drag & Speak) 기반의 직관적인 Windows AI 질의 도구 개발.
- **핵심 아키텍처:** WPF (UI) + Global Hook (Input) + Gemini API (Brain).
- **관리:** 이 문서는 프로젝트의 진행 상황을 추적하고, Cursor에게 작업을 지시하는 기준이 됩니다.

## Phase 1: 기반 구축 (Foundation & Input Layer)
**목표:** 앱 실행 시 트레이 아이콘에 상주하며, 마우스 이벤트를 전역에서 감지하여 오버레이를 띄웁니다.

- [ ] **1.1 프로젝트 세팅 (Project Setup)**
  - [ ] .NET 8 WPF 프로젝트 생성 (AI_Mouse).
  - [ ] MVVM Toolkit (CommunityToolkit.Mvvm) 및 DI 컨테이너(Microsoft.Extensions.DependencyInjection) 구성.
  - [ ] Hardcodet.NotifyIcon.Wpf 패키지를 이용한 Tray Icon 구현 (ContextMenu: 종료, 설정).
  - [ ] 앱 실행 시 메인 윈도우 숨김 처리 및 백그라운드 상주 로직.
- [ ] **1.2 전역 입력 감지 (Global Input Hook)**
  - [ ] User32.dll P/Invoke를 이용한 LowLevelMouseProc 구현.
  - [ ] WM_XBUTTONDOWN, WM_XBUTTONUP, WM_MOUSEMOVE 메시지 필터링.
  - [ ] 키보드 대안(Ctrl + LeftClick) 지원을 위한 LowLevelKeyboardProc (선택 사항, 우선순위 낮음).
  - [ ] HookService 구현: 이벤트를 MainViewModel로 전파하는 로직.
- [ ] **1.3 시각적 피드백 (Overlay View)**
  - [ ] OverlayWindow.xaml 구현: WindowStyle="None", AllowsTransparency="True", Background="#01000000" (완전 투명 시 클릭 통과 문제 방지).
  - [ ] 마우스 드래그 시 좌표 계산 및 사각형 그리기 (Canvas 또는 Adorner 사용).
  - [ ] 트리거 버튼 Down -> 오버레이 Show, Up -> 오버레이 Hide 연결.

## Phase 2: 데이터 획득 (Sensory Layer)
**목표:** 사용자가 지정한 화면 영역을 이미지로, 음성을 오디오 파일로 추출합니다.

- [ ] **2.1 화면 캡처 (Screen Capture)**
  - [ ] ScreenCaptureService 구현.
  - [ ] GDI+ (System.Drawing.Common)를 이용하여 지정된 Rect 영역 캡처.
  - [ ] Per-Monitor DPI Awareness 설정 (app.manifest)으로 좌표 오차 보정.
  - [ ] 캡처된 이미지를 BitmapSource로 변환(UI 표시용) 및 MemoryStream(전송용) 저장.
  - [ ] 시스템 클립보드 복사 기능.
- [ ] **2.2 음성 녹음 (Audio Recording)**
  - [ ] NAudio 패키지 설치.
  - [ ] AudioRecorderService 구현.
  - [ ] 트리거 Down: WaveInEvent 시작 -> Up: 녹음 중지.
  - [ ] 오디오 레벨 미터링 (선택 사항: 오버레이에 시각화).
  - [ ] PCM 16bit, 16kHz/24kHz Mono WAV 포맷으로 저장 (Gemini 호환성 확보).
  - [ ] System.IO.Directory를 이용한 임시 폴더 관리 및 파일 정리 로직.

## Phase 3: 지능 연동 (Intelligence Layer)
**목표:** 수집된 데이터를 Google Gemini API로 전송하고 답변을 받아옵니다.

- [ ] **3.1 API 클라이언트 구현 (Gemini Integration)**
  - [ ] Google.Cloud.AIPlatform 또는 Google.GenerativeAI SDK 설치.
  - [ ] GeminiService 구현.
  - [ ] API Key 보안 처리 (UserSecrets 또는 암호화된 로컬 파일).
  - [ ] 멀티모달 요청 생성: GenerateContent(Image, Audio, Prompt).
  - [ ] System Prompt 튜닝: "화면의 문맥과 사용자의 음성을 결합하여 답변하라."
- [ ] **3.2 비동기 처리 및 예외 관리**
  - [ ] Task.Run 및 async/await를 통한 UI 스레드 차단 방지.
  - [ ] 네트워크 오류, API Quota 초과 등에 대한 예외 처리 및 재시도 로직.

## Phase 4: 결과 표시 및 UX (Presentation Layer)
**목표:** AI의 응답을 사용자에게 직관적으로 보여주고 제품의 완성도를 높입니다.

- [ ] **4.1 결과 뷰어 (Result Window)**
  - [ ] ResultWindow.xaml 구현: 마우스 커서 위치 또는 화면 우측 하단에 팝업.
  - [ ] Markdown.Xaml 또는 Markdig.Wpf를 이용한 마크다운 렌더링.
  - [ ] 로딩 인디케이터 (Skeleton UI 또는 Spinner).
  - [ ] 창 닫기: 외부 클릭 시 닫기 (Light Dismiss) 또는 닫기 버튼.
- [ ] **4.2 사용자 설정 (Settings)**
  - [ ] 입력 디바이스 선택 (마이크).
  - [ ] API Key 입력 필드.
  - [ ] 시작 프로그램 등록 옵션.
- [ ] **4.3 안정화 및 배포 (Stabilization)**
  - [ ] 메모리 누수 점검 (Bitmap, Hook, Audio Stream의 Dispose 확인).
  - [ ] 로깅 시스템 구축 (Serilog 등).
  - [ ] Release 빌드 최적화 및 설치 파일 생성.