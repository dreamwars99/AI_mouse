# ✅ 할 일 목록 (To-Do List)

## 📋 현재 상태 요약 (Current Status Summary)

**프로젝트 상태:** Phase 1.1 진행 중 (프로젝트 초기 세팅)

**완료된 주요 기능:**
- ✅ 프로젝트 생성 및 환경 설정 (.NET 8 WPF)
- ✅ NuGet 패키지 설치 (CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Hardcodet.NotifyIcon.Wpf)
- ✅ 기본 폴더 구조 구축 (Views/, ViewModels/)
- ✅ MainViewModel 클래스 생성
- ⏳ MVVM 및 DI 컨테이너 구성 (다음 단계)
- ⏳ 트레이 아이콘 구현 (다음 단계)

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
  - [ ] `Services/Interfaces/` 폴더 생성 (Phase 1.2에서 생성 예정)
  - [ ] `Services/Implementations/` 폴더 생성 (Phase 1.2에서 생성 예정)
  - [ ] `Models/` 폴더 생성 (Phase 1.2에서 생성 예정)
  - [ ] `Helpers/` 폴더 생성 (Phase 1.2에서 생성 예정)

- [x] **MVVM 기본 구조** ✅ 부분 완료
  - [x] `ViewModels/MainViewModel.cs` 생성 (`ObservableObject` 상속) ✅
  - [ ] `App.xaml.cs`에서 `ServiceCollection` 초기화 (다음 단계)
  - [ ] `MainWindow`와 `MainViewModel` DI 등록 (다음 단계)
  - [ ] `App.xaml`에서 `StartupUri` 제거 (다음 단계)

- [ ] **트레이 아이콘 및 생명주기 구현** ⏳ 다음 단계
  - [ ] `TaskbarIcon` 리소스 정의 (`App.xaml`에 추가)
  - [ ] 앱 실행 시 트레이 아이콘만 표시
  - [ ] ContextMenu 구현 ('설정', '종료')
  - [ ] `MainWindow` 시작 시 `Visibility="Hidden"` 설정
  - [ ] 윈도우 닫기 버튼 클릭 시 숨기기 처리 (`Closing` 이벤트)

---

## ✅ Completed (완료된 작업)

### Phase 1.1: 프로젝트 세팅 (부분 완료)
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

---

## 🧊 Backlog (예정된 작업)

### 🔴 High Priority (높은 우선순위)

#### Phase 1.2: 전역 입력 감지 (Global Input Hook)
- [ ] **User32.dll P/Invoke 구현**
  - [ ] `Helpers/NativeMethods.cs` 생성
  - [ ] `LowLevelMouseProc` 콜백 구현
  - [ ] `SetWindowsHookEx(WH_MOUSE_LL)` 연동
  - [ ] `UnhookWindowsHookEx` 해제 로직

- [ ] **마우스 이벤트 필터링**
  - [ ] `WM_XBUTTONDOWN` 처리
  - [ ] `WM_XBUTTONUP` 처리
  - [ ] `WM_MOUSEMOVE` 처리 (드래그 추적)
  - [ ] 키보드 대안 (`Ctrl + LeftClick`) 지원 (선택 사항)

- [ ] **GlobalHookService 구현**
  - [ ] `Services/Interfaces/IGlobalHookService.cs` 생성
  - [ ] `Services/Implementations/GlobalHookService.cs` 구현
  - [ ] 이벤트 전파 로직 (`MouseDown`, `MouseUp`, `MouseMove`)
  - [ ] `MainViewModel`에 이벤트 구독 연결
  - [ ] `IDisposable` 구현 (Hook 해제)

#### Phase 1.3: 시각적 피드백 (Overlay View)
- [ ] **OverlayWindow 구현**
  - [ ] `Views/OverlayWindow.xaml` 생성
  - [ ] `WindowStyle="None"`, `AllowsTransparency="True"` 설정
  - [ ] `Background="#01000000"` (클릭 통과 방지)
  - [ ] 전체 화면 덮기 로직

- [ ] **드래그 사각형 그리기**
  - [ ] `OverlayViewModel` 생성
  - [ ] Canvas 또는 Adorner를 이용한 사각형 렌더링
  - [ ] 마우스 좌표 계산 및 업데이트 로직

- [ ] **트리거 연결**
  - [ ] 트리거 버튼 Down → 오버레이 Show
  - [ ] 트리거 버튼 Up → 오버레이 Hide
  - [ ] `MainViewModel`에서 상태 관리

### 🟡 Medium Priority (중간 우선순위)

#### Phase 2.1: 화면 캡처 (Screen Capture)
- [ ] **ScreenCaptureService 구현**
  - [ ] `Services/Interfaces/IScreenCaptureService.cs` 생성
  - [ ] `Services/Implementations/ScreenCaptureService.cs` 구현
  - [ ] GDI+ (`System.Drawing.Common`) 설치 및 사용
  - [ ] 지정된 Rect 영역 캡처 로직
  - [ ] `BitmapSource` 변환 (WPF 호환)
  - [ ] `MemoryStream` 저장 (API 전송용)
  - [ ] 시스템 클립보드 복사 기능

- [ ] **DPI 보정**
  - [ ] `Helpers/DpiHelper.cs` 생성
  - [ ] Per-Monitor DPI 좌표 변환 로직
  - [ ] 멀티 모니터 환경 대응

#### Phase 2.2: 음성 녹음 (Audio Recording)
- [ ] **NAudio 패키지 설치**
  - [ ] `NAudio` NuGet 패키지 설치

- [ ] **AudioRecorderService 구현**
  - [ ] `Services/Interfaces/IAudioRecorderService.cs` 생성
  - [ ] `Services/Implementations/AudioRecorderService.cs` 구현
  - [ ] `WaveInEvent` 시작/중지 로직
  - [ ] 트리거 Down → 녹음 시작
  - [ ] 트리거 Up → 녹음 중지
  - [ ] PCM 16bit, Mono, 16kHz/24kHz WAV 포맷 저장
  - [ ] 임시 폴더 관리 및 파일 정리 로직
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

**마지막 업데이트**: 2026-02-05 (Phase 1.1 진행 중 - 기본 구조 생성 완료, DI 컨테이너 구성 및 트레이 아이콘 구현 예정)
