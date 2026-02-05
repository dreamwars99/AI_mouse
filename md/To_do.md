# ✅ 할 일 목록 (To-Do List)

## 📋 현재 상태 요약 (Current Status Summary)

**프로젝트 상태:** Phase 2.2 완료 ✅ (NAudio 기반 마이크 음성 녹음 서비스 구현 완료)

**완료된 주요 기능:**
- ✅ 프로젝트 생성 및 환경 설정 (.NET 8 WPF)
- ✅ NuGet 패키지 설치 (CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Hardcodet.NotifyIcon.Wpf)
- ✅ 기본 폴더 구조 구축 (Views/, ViewModels/, Services/, Helpers/)
- ✅ MainViewModel 클래스 생성
- ✅ MVVM 및 DI 컨테이너 구성 완료
- ✅ 트레이 아이콘 구현 완료
- ✅ 전역 마우스 훅 구현 완료 (NativeMethods, IGlobalHookService, GlobalHookService)
- ✅ 투명 오버레이 윈도우 구현 완료 (OverlayWindow, OverlayViewModel)
- ✅ 드래그 사각형 시각화 구현 완료 (MainViewModel 이벤트 구독 및 오버레이 제어)
- ✅ DPI 보정 유틸리티 구현 완료 (DpiHelper)
- ✅ 화면 캡처 서비스 구현 완료 (IScreenCaptureService, ScreenCaptureService)
- ✅ 클립보드 복사 기능 구현 완료
- ✅ NAudio 패키지 설치 완료 (v2.2.1)
- ✅ 오디오 녹음 서비스 구현 완료 (IAudioRecorderService, AudioRecorderService)
- ✅ 마이크 음성 녹음 및 WAV 파일 저장 기능 구현 완료

---

## 🔥 Today's Sprint (오늘 작업)

### Phase 1.1: 프로젝트 세팅 (Project Setup)
- [x] **프로젝트 생성 및 환경 설정** ✅ 완료
  - [x] .NET 8 WPF 솔루션 `AI_Mouse` 생성
  - [x] NuGet 패키지 설치:
    - [x] `CommunityToolkit.Mvvm` (MVVM) ✅ Version 8.2.2
    - [x] `Microsoft.Extensions.DependencyInjection` (DI) ✅ Version 8.0.0
    - [x] `Hardcodet.NotifyIcon.Wpf` (System Tray) ✅ Version 1.1.0
  - [ ] `app.manifest` 파일 생성/수정: `<dpiAwareness>PerMonitorV2</dpiAwareness>` 설정

- [x] **폴더 구조 구축** ✅ 부분 완료
  - [x] `Views/` 폴더 생성 및 `MainWindow.xaml` 이동 ✅
  - [x] `ViewModels/` 폴더 생성 ✅
  - [x] 네임스페이스 수정 (`AI_Mouse.Views` 등) ✅
  - [x] `Services/Interfaces/` 폴더 생성 ✅
  - [x] `Services/Implementations/` 폴더 생성 ✅
  - [ ] `Models/` 폴더 생성 (Phase 2 예정)
  - [x] `Helpers/` 폴더 생성 ✅

- [x] **MVVM 기본 구조** ✅ 완료
  - [x] `ViewModels/MainViewModel.cs` 생성 (`ObservableObject` 상속) ✅
  - [x] `App.xaml.cs`에서 `ServiceCollection` 초기화 ✅
  - [x] `MainWindow`와 `MainViewModel` DI 등록 ✅
  - [x] `App.xaml`에서 `StartupUri` 제거 ✅

- [x] **트레이 아이콘 및 생명주기 구현** ✅ 완료
  - [x] `TaskbarIcon` 리소스 정의 (`App.xaml`에 추가) ✅
  - [x] 앱 실행 시 트레이 아이콘만 표시 ✅
  - [x] ContextMenu 구현 ('설정', '종료') ✅
  - [x] `MainWindow` 시작 시 `Hide()` 호출로 숨김 처리 ✅
  - [x] 윈도우 닫기 버튼 클릭 시 숨기기 처리 (`Closing` 이벤트) ✅

- [x] **UX 피드백 및 검증 (Verification)** ✅ 완료
  - [x] `OnStartup` 시 실행 확인용 `MessageBox` 출력 ✅
  - [x] 트레이 아이콘 '설정' 메뉴 클릭 시 임시 팝업 구현 ✅
  - [x] 빈 아이콘 방지 (`SystemIcons.Application` 할당) ✅

---

## ✅ Completed (완료된 작업)

### Phase 1.1: 프로젝트 세팅 ✅ 완료
- ✅ .NET 8 WPF 프로젝트 생성 (`AI_Mouse.csproj`)
- ✅ NuGet 패키지 설치 완료:
  - `CommunityToolkit.Mvvm` v8.2.2
  - `Microsoft.Extensions.DependencyInjection` v8.0.0
  - `Hardcodet.NotifyIcon.Wpf` v1.1.0
- ✅ 기본 폴더 구조 생성:
  - `Views/` 폴더 및 `MainWindow.xaml`, `MainWindow.xaml.cs`
  - `ViewModels/` 폴더 및 `MainViewModel.cs`
- ✅ 네임스페이스 구조 설정 (`AI_Mouse.Views`, `AI_Mouse.ViewModels`)
- ✅ `MainViewModel` 클래스 생성 (`ObservableObject` 상속)
- ✅ DI 컨테이너 구성 (`App.xaml.cs`에서 `ServiceCollection` 사용)
- ✅ `MainWindow`와 `MainViewModel` 의존성 주입 구현
- ✅ 시스템 트레이 아이콘 구현 (`TaskbarIcon` 리소스 및 ContextMenu)
- ✅ 앱 생명주기 관리 (초기 숨김 상태, Closing 이벤트 처리)

### Phase 1.2: 전역 입력 감지 ✅ 완료
- ✅ `Helpers/NativeMethods.cs` 생성 (Win32 API P/Invoke 선언)
- ✅ `Services/Interfaces/IGlobalHookService.cs` 생성 (인터페이스 정의)
- ✅ `Services/Implementations/GlobalHookService.cs` 생성 (훅 서비스 구현)
- ✅ 전역 마우스 훅 설치/해제 로직 구현 (`SetWindowsHookEx`, `UnhookWindowsHookEx`)
- ✅ 마우스 이벤트 감지 및 디버그 로그 출력
- ✅ 비동기 이벤트 처리로 콜백 경량화 (`Task.Run`)
- ✅ `IDisposable` 패턴 구현으로 리소스 안전 해제
- ✅ `App.xaml.cs`에서 싱글톤 등록 및 `Start()` 호출
- ✅ `OnExit`에서 `Stop()` 호출하여 훅 해제 보장

### Phase 1.3: 시각적 피드백 (Overlay View) ✅ 완료
- ✅ `Views/OverlayWindow.xaml` 생성 (투명 윈도우, Canvas, Rectangle UI 구성)
- ✅ `Views/OverlayWindow.xaml.cs` 생성 (Code-behind 구현)
- ✅ `ViewModels/OverlayViewModel.cs` 생성 (`ObservableObject` 상속, 드래그 영역 속성 구현)
- ✅ `App.xaml.cs`에 `OverlayViewModel`과 `OverlayWindow` DI 등록 및 초기화
- ✅ `MainViewModel`에 `IGlobalHookService`와 `OverlayWindow` 주입 및 이벤트 구독
- ✅ 트리거 버튼 Down → 오버레이 Show, Up → 오버레이 Hide 로직 구현
- ✅ 드래그 중 사각형 시각화 로직 구현 (`UpdateRect` 메서드)

### Phase 2.1: 화면 캡처 (Screen Capture) ✅ 완료
- ✅ `Helpers/DpiHelper.cs` 생성 완료 (Win32 API P/Invoke 선언 및 좌표 변환 메서드)
- ✅ `Helpers/NativeMethods.cs`에 DPI 관련 API 선언 추가 완료 (`GetDpiForMonitor`, `MonitorFromPoint`)
- ✅ `Services/Interfaces/IScreenCaptureService.cs` 생성 완료 (인터페이스 정의)
- ✅ `Services/Implementations/ScreenCaptureService.cs` 생성 완료 (GDI+ 기반 캡처 및 BitmapSource 변환)
- ✅ `System.Drawing.Common` 패키지 추가 완료 (v8.0.0)
- ✅ `MainViewModel`에 `IScreenCaptureService` 주입 및 캡처 로직 구현 완료
- ✅ `HandleMouseMove`에 DPI 변환 로직 적용 완료 (`DpiHelper.PhysicalToLogicalRect`)
- ✅ `HandleMouseUp`에 화면 캡처 및 클립보드 복사 로직 구현 완료
- ✅ `App.xaml.cs`에 `IScreenCaptureService` 싱글톤 등록 완료

### Phase 2.2: 음성 녹음 (Audio Recording) ✅ 완료
- ✅ `NAudio` 패키지 설치 완료 (v2.2.1)
- ✅ `Services/Interfaces/IAudioRecorderService.cs` 생성 완료 (인터페이스 정의)
- ✅ `Services/Implementations/AudioRecorderService.cs` 생성 완료 (NAudio 기반 녹음 및 WAV 저장)
- ✅ `WaveInEvent` 시작/중지 로직 구현 완료
- ✅ 트리거 Down → 녹음 시작 로직 구현 완료 (`HandleMouseDown`)
- ✅ 트리거 Up → 녹음 중지 및 파일 경로 반환 로직 구현 완료 (`HandleMouseUp`)
- ✅ PCM 16bit, Mono, 16kHz WAV 포맷 저장 완료 (Gemini API 호환)
- ✅ 임시 폴더 관리 및 파일 정리 로직 구현 완료 (`Path.GetTempPath()/AI_Mouse/audio_temp.wav`)
- ✅ `TaskCompletionSource`를 사용한 비동기 처리 구현 완료
- ✅ `WaveFileWriter` Dispose로 파일 잠금 해제 보장 완료
- ✅ `MainViewModel`에 `IAudioRecorderService` 주입 및 녹음 로직 구현 완료
- ✅ `App.xaml.cs`에 `IAudioRecorderService` 싱글톤 등록 완료
- ✅ `OnExit`에서 `AudioRecorderService` Dispose 호출 추가 완료

---

## 🧊 Backlog (예정된 작업)

### 🔴 High Priority (높은 우선순위)

#### Phase 1.2: 전역 입력 감지 (Global Input Hook) ✅ 완료
- [x] **User32.dll P/Invoke 구현** ✅
  - [x] `Helpers/NativeMethods.cs` 생성 ✅
  - [x] `LowLevelMouseProc` 콜백 구현 ✅
  - [x] `SetWindowsHookEx(WH_MOUSE_LL)` 연동 ✅
  - [x] `UnhookWindowsHookEx` 해제 로직 ✅
  - [x] `MSLLHOOKSTRUCT` 구조체 정의 ✅
  - [x] 마우스 메시지 상수 정의 (WM_MOUSEMOVE, WM_LBUTTONDOWN 등) ✅

- [x] **마우스 이벤트 필터링** ✅
  - [x] `WM_XBUTTONDOWN` 처리 ✅
  - [x] `WM_XBUTTONUP` 처리 ✅
  - [x] `WM_MOUSEMOVE` 처리 (드래그 추적) ✅
  - [x] `WM_LBUTTONDOWN/UP`, `WM_RBUTTONDOWN/UP`, `WM_MBUTTONDOWN/UP` 처리 ✅
  - [ ] 키보드 대안 (`Ctrl + LeftClick`) 지원 (선택 사항)

- [x] **GlobalHookService 구현** ✅
  - [x] `Services/Interfaces/IGlobalHookService.cs` 생성 ✅
  - [x] `Services/Implementations/GlobalHookService.cs` 구현 ✅
  - [x] 이벤트 전파 로직 (`MouseAction` 이벤트) ✅
  - [x] `MouseActionEventArgs`, `MouseActionType`, `MouseButton` 정의 ✅
  - [x] 비동기 이벤트 처리 (`Task.Run`으로 경량화) ✅
  - [x] `IDisposable` 구현 (Hook 해제) ✅
  - [x] `App.xaml.cs`에서 싱글톤 등록 및 `Start()` 호출 ✅
  - [x] `OnExit`에서 `Stop()` 호출하여 리소스 해제 보장 ✅
  - [x] `MainViewModel`에 이벤트 구독 연결 ✅ (Phase 1.3 완료)

#### Phase 1.3: 시각적 피드백 (Overlay View) ✅ 완료
- [x] **OverlayWindow 구현** ✅
  - [x] `Views/OverlayWindow.xaml` 생성 ✅
  - [x] `WindowStyle="None"`, `AllowsTransparency="True"` 설정 ✅
  - [x] `Background="#01000000"` (클릭 통과 방지) ✅
  - [x] 전체 화면 덮기 로직 (`WindowState="Maximized"`) ✅

- [x] **드래그 사각형 그리기** ✅
  - [x] `OverlayViewModel` 생성 ✅
  - [x] Canvas를 이용한 사각형 렌더링 ✅
  - [x] 마우스 좌표 계산 및 업데이트 로직 (`UpdateRect` 메서드) ✅

- [x] **트리거 연결** ✅
  - [x] 트리거 버튼 Down → 오버레이 Show ✅
  - [x] 트리거 버튼 Up → 오버레이 Hide ✅
  - [x] `MainViewModel`에서 상태 관리 ✅

### 🟡 Medium Priority (중간 우선순위)

#### Phase 2.1: 화면 캡처 (Screen Capture) ✅ 완료
- [x] **ScreenCaptureService 구현** ✅
  - [x] `Services/Interfaces/IScreenCaptureService.cs` 생성 ✅
  - [x] `Services/Implementations/ScreenCaptureService.cs` 구현 ✅
  - [x] GDI+ (`System.Drawing.Common`) 설치 및 사용 ✅
  - [x] 지정된 Rect 영역 캡처 로직 ✅
  - [x] `BitmapSource` 변환 (WPF 호환) ✅
  - [x] 시스템 클립보드 복사 기능 ✅

- [x] **DPI 보정** ✅
  - [x] `Helpers/DpiHelper.cs` 생성 ✅
  - [x] Per-Monitor DPI 좌표 변환 로직 ✅
  - [x] 멀티 모니터 환경 대응 ✅
  - [x] `NativeMethods`에 DPI 관련 P/Invoke 선언 추가 ✅
  - [x] `MainViewModel`의 `HandleMouseMove`에 DPI 변환 적용 ✅

#### Phase 2.2: 음성 녹음 (Audio Recording) ✅ 완료
- [x] **NAudio 패키지 설치** ✅
  - [x] `NAudio` NuGet 패키지 설치 ✅ (v2.2.1)

- [x] **AudioRecorderService 구현** ✅
  - [x] `Services/Interfaces/IAudioRecorderService.cs` 생성 ✅
  - [x] `Services/Implementations/AudioRecorderService.cs` 구현 ✅
  - [x] `WaveInEvent` 시작/중지 로직 ✅
  - [x] 트리거 Down → 녹음 시작 ✅
  - [x] 트리거 Up → 녹음 중지 ✅
  - [x] PCM 16bit, Mono, 16kHz WAV 포맷 저장 ✅ (Gemini API 호환)
  - [x] 임시 폴더 관리 및 파일 정리 로직 ✅ (`Path.GetTempPath()/AI_Mouse/audio_temp.wav`)
  - [ ] 오디오 레벨 미터링 (선택 사항)

### 🟢 Low Priority (낮은 우선순위)

#### Phase 3.1: Gemini API 연동 (Intelligence Layer)
- [ ] **Google Generative AI SDK 설치**
  - [ ] `Google.GenerativeAI` 또는 `Google.Cloud.AIPlatform` NuGet 패키지 설치

- [ ] **GeminiService 구현**
  - [ ] `Services/Interfaces/IGeminiService.cs` 생성
  - [ ] `Services/Implementations/GeminiService.cs` 구현
  - [ ] API Key 보안 처리 (UserSecrets 또는 암호화 파일)
  - [ ] 멀티모달 요청 생성 (`GenerateContent` with Image, Audio, Prompt)
  - [ ] System Prompt 튜닝
  - [ ] 비동기 처리 (`async/await`)
  - [ ] 예외 처리 및 재시도 로직

#### Phase 4.1: 결과 뷰어 (Result Window)
- [ ] **ResultWindow 구현**
  - [ ] `Views/ResultWindow.xaml` 생성
  - [ ] 마우스 커서 위치 또는 화면 우측 하단에 팝업
  - [ ] Markdown 렌더링 (`Markdig.Wpf` 또는 `Markdown.Xaml`)
  - [ ] 로딩 인디케이터 (Skeleton UI 또는 Spinner)
  - [ ] 외부 클릭 시 닫기 (Light Dismiss) 또는 닫기 버튼

#### Phase 4.2: 사용자 설정 (Settings)
- [ ] **SettingsWindow 구현**
  - [ ] `Views/SettingsWindow.xaml` 생성
  - [ ] 입력 디바이스 선택 (마이크)
  - [ ] API Key 입력 필드
  - [ ] 시작 프로그램 등록 옵션
  - [ ] 트레이 아이콘 ContextMenu에서 '설정' 클릭 시 열기

#### Phase 4.3: 안정화 및 배포 (Stabilization)
- [ ] **메모리 누수 점검**
  - [ ] `Bitmap`, `Graphics` Dispose 확인
  - [ ] Hook 해제 확인
  - [ ] Audio Stream Dispose 확인

- [ ] **로깅 시스템 구축**
  - [ ] `Serilog` 또는 `NLog` 설치
  - [ ] 로그 파일 저장 및 관리

- [ ] **Release 빌드 최적화**
  - [ ] 설치 파일 생성 (MSI 또는 ClickOnce)
  - [ ] 코드 서명 (선택 사항)

---

## 📝 참고 사항

### 작업 우선순위 가이드
1. **High Priority**: Phase 1 완료 (기반 구축)
2. **Medium Priority**: Phase 2 완료 (데이터 획득)
3. **Low Priority**: Phase 3-4 완료 (AI 연동 및 UX)

### 다음 단계 추천
- **Phase 1.1 완료 후**: Phase 1.2 (전역 입력 감지) 진행
- **Phase 1 완료 후**: Phase 2 (화면 캡처 및 음성 녹음) 진행

### 개발 가이드라인
- 모든 작업은 `CURSOR_GUIDELINES.md`를 준수해야 함
- MVVM 패턴 엄격히 준수 (View에 로직 작성 금지)
- 모든 I/O 작업은 비동기(`async/await`) 처리
- 리소스 안전성 확보 (`IDisposable` 구현)
- 주석은 한국어로 작성

### 아키텍처 참고
- `Architecture.md`: 시스템 아키텍처 및 데이터 흐름
- `Dev_Roadmap.md`: 단계별 개발 로드맵
- `work_process.md`: 프로젝트 구조 및 개발 환경

---

**마지막 업데이트**: 2026-02-05 (Phase 2.2 완료 - NAudio 기반 마이크 음성 녹음 서비스 구현 완료)
