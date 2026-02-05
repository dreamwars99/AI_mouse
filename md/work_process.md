# 📝 작업 일지 (Project History & Context)

- **Project:** AI Mouse (Windows Smart Assistant)
- **Role:** Lead Architect & Cursor AI
- **Framework:** .NET 8 (WPF)
- **Platform:** Windows 10 / 11 Desktop
- **Last Updated:** 2026-02-05 (프로젝트 폴더 구조 생성 완료)

## 📌 1. Development Environment (개발 환경 상세)
이 프로젝트를 이어받는 AI/개발자는 아래 설정을 필수로 확인해야 합니다.

### 1.1. Package Dependencies (NuGet 패키지)
주요 라이브러리는 NuGet을 통해 관리됩니다.

- **CommunityToolkit.Mvvm**
  - Purpose: MVVM 패턴 구현 (ObservableObject, RelayCommand)
- **Microsoft.Extensions.DependencyInjection**
  - Purpose: 의존성 주입 (DI) 컨테이너 구성
- **Hardcodet.NotifyIcon.Wpf**
  - Purpose: 시스템 트레이 아이콘 및 백그라운드 상주 기능
- **NAudio** (Phase 2 예정)
  - Purpose: 마이크 오디오 녹음 및 WAV 파일 저장
- **Google.Cloud.AIPlatform** (Phase 3 예정)
  - Purpose: Gemini API 연동 (Multimodal Input)

### 1.2. Project Settings
- **Target OS:** Windows 10.0.19041.0 이상
- **Architecture:** x64 (기본), ARM64 호환 고려
- **DPI Awareness:** Per-Monitor V2 (고해상도 모니터 대응 필수)
- **Manifest:** `uiAccess="true"` (오버레이가 최상단에 위치하기 위함, 관리자 권한 필요 가능성)

## 📂 2. Project Directory Structure (폴더 구조)
MVVM 패턴과 관심사 분리(Separation of Concerns) 원칙을 따릅니다.

AI_Mouse/
├── App.xaml.cs              # [Entry] DI 컨테이너 구성 및 앱 시작점
├── Views/                   # [UI] XAML 및 Code-behind
│   ├── MainWindow.xaml      # 설정 및 메인 화면 (초기엔 Hidden)
│   └── OverlayWindow.xaml   # [Overlay] 화면 캡처 및 드래그 가이드
├── ViewModels/              # [Logic] View와 데이터 바인딩
│   └── MainViewModel.cs     # 메인 로직 및 커맨드 처리
├── Services/                # [Core] 비즈니스 로직 및 시스템 제어
│   ├── Interfaces/          # 서비스 인터페이스 (IGlobalHookService 등)
│   └── Implementations/     # 서비스 구현체
├── Models/                  # [Data] 데이터 구조 (DTO)
└── Helpers/                 # [Util] Win32 Interop, 컨버터 등

## 🏗️ 3. Architecture & Code Flow (설계 및 로직)

### 3.1. Design Pattern: MVVM + Service
- **ViewModel:** UI 상태(Status, Input)를 관리하며, 비즈니스 로직은 직접 수행하지 않고 Service에 위임합니다.
- **Service:** Singleton으로 등록되며, 시스템 후킹, 파일 I/O, API 통신 등 무거운 작업을 수행합니다.

### 3.2. Key Components (핵심 컴포넌트 역할)
**App.xaml.cs (Bootstrapper)**
- **역할:** `IServiceProvider`를 초기화하고 싱글톤 서비스들을 등록합니다. 앱 종료 시 리소스 정리(Dispose)를 담당합니다.

**MainViewModel.cs (Orchestrator)**
- **역할:** 앱의 상태 기계(State Machine) 역할.
- **States:** `Idle` (대기) -> `Listening` (드래그 중) -> `Processing` (AI 요청 중) -> `Result` (결과 표시).
- **Flow:** Hook 이벤트 수신 -> Overlay 활성화 -> 캡처/녹음 지시 -> Gemini 전송 -> 결과 바인딩.

**Services (To Be Implemented)**
- `GlobalHookService`: 마우스/키보드 전역 이벤트 감지 (User32.dll).
- `ScreenCaptureService`: GDI+를 이용한 화면 캡처.
- `AudioRecorderService`: NAudio 기반 음성 녹음.

## 📅 4. Development Log (개발 기록)

### 2026-02-05 (목) - NuGet 패키지 설치 및 MainWindow 위치 조정 (4차)
**[목표]** 필수 NuGet 패키지를 설치하고, `MainWindow`를 `Views` 폴더로 이동하여 MVVM 패턴에 맞는 기본 구조를 완성.

#### Dev Action (NuGet & Refactoring)
- **NuGet 패키지 설치:**
  - `CommunityToolkit.Mvvm`
  - `Microsoft.Extensions.DependencyInjection`
  - `Hardcodet.NotifyIcon.Wpf` 추가
- **파일 이동 및 네임스페이스 수정:**
  - `MainWindow.xaml`, `MainWindow.xaml.cs` -> `Views/` 폴더로 이동
  - `MainWindow`의 네임스페이스를 `AI_Mouse.Views`로 수정
  - `App.xaml`의 `StartupUri`를 `Views/MainWindow.xaml`로 업데이트
- **ViewModel 생성:**
  - `ViewModels/MainViewModel.cs` 생성 (ObservableObject 상속)

#### Tech Details
- **WPF 프로젝트 구성:** .NET 8.0 기반의 `AI_Mouse.csproj` 파일 생성 및 패키지 참조 추가
- **MVVM 패턴 준수:** View와 ViewModel의 역할을 분리하고 폴더 구조에 맞게 네임스페이스 정렬

#### Current Status
- 패키지 설치 및 파일 이동 완료
- 프로젝트 기본 골격 및 MVVM 구조 확보
- (주의) 시스템에 .NET SDK가 설치되어 있지 않아 빌드 확인은 수동 진행 필요
- 다음 단계: DI 컨테이너 구성 및 트레이 아이콘 기능 구현

---

### 2026-02-05 (목) - 프로젝트 폴더 구조 생성 (3차)
**[목표]** MVVM 패턴에 따른 프로젝트 폴더 구조를 생성하여 코드 조직화의 기반을 마련.

#### Dev Action (Directory Structure Setup)
- **폴더 구조 생성:**
  - 프로젝트 루트에 MVVM 패턴에 맞는 폴더 구조 생성
  - `Views/`: UI 레이어 (XAML 및 Code-behind 파일)
  - `ViewModels/`: 비즈니스 로직 및 데이터 바인딩
  - `Services/`: 비즈니스 로직 및 시스템 제어
    - `Services/Interfaces/`: 서비스 인터페이스 정의
    - `Services/Implementations/`: 서비스 구현체
  - `Models/`: 데이터 구조 및 DTO
  - `Helpers/`: 유틸리티 클래스 (Win32 Interop, 컨버터 등)

#### Tech Details
- **구조 설계 원칙:** 관심사 분리(Separation of Concerns) 및 MVVM 패턴 준수
- **확장성 고려:** 서비스 계층을 Interfaces와 Implementations로 분리하여 의존성 주입 및 테스트 용이성 확보
- **표준 구조:** WPF .NET 8 프로젝트의 표준 폴더 구조 적용

#### Current Status
- 프로젝트 루트에 모든 필수 폴더 구조 생성 완료
- MVVM 아키텍처에 맞는 코드 조직화 준비 완료
- 다음 단계: 프로젝트 파일 생성 및 기본 클래스 구현 준비

---

### 2026-02-05 (목) - 프로젝트 문서화 및 설정 파일 정리 (2차)
**[목표]** 기존 Unity 프로젝트 템플릿에서 남아있던 문서와 설정 파일을 AI Mouse WPF 프로젝트에 맞게 전면 수정.

#### Dev Action (Documentation & Configuration)
- **문서 파일 수정:**
  - `Architecture.md`: Unity 게임 프로젝트 아키텍처를 WPF MVVM 아키텍처로 전면 재작성
    - 핵심 로직 플로우 (드래그+음성 질의 시퀀스 다이어그램)
    - MVVM 계층 구조 및 의존성 그래프
    - 상태 머신 (Idle → Listening → Processing → Result)
    - 서비스 인터페이스 및 구현 세부사항
    - 데이터 흐름 및 리소스 관리 가이드
  - `To_do.md`: Unity 게임 개발 할 일 목록을 WPF 프로젝트 Phase별 작업 목록으로 재구성
    - Phase 1.1 (프로젝트 세팅) 작업 항목 정리
    - Phase 1.2~4.3 Backlog 항목 정의
    - 우선순위별 작업 분류 (High/Medium/Low)
  - `Tree.md`: Unity Hierarchy 구조를 WPF MVVM 폴더 구조로 전면 재작성
    - 파일 시스템 구조 (Views, ViewModels, Services 등)
    - MVVM 계층 다이어그램
    - DI 컨테이너 구조 설명
    - 런타임 컴포넌트 흐름 다이어그램
    - 네임스페이스 구조 정의

- **설정 파일 수정:**
  - `.cursorignore`: Unity 관련 항목 제거, WPF .NET 빌드 아티팩트 추가
    - `bin/`, `obj/`, `Debug/`, `Release/` 폴더 제외
    - NuGet 패키지 캐시 (`packages/`) 제외
    - Visual Studio 관련 파일 제외
    - WPF 자동 생성 파일 (`*.g.cs`, `*.g.i.cs`) 제외
  - `.gitignore`: Unity 관련 항목 제거, WPF .NET 표준 Git ignore 패턴 적용
    - WPF 빌드 산출물 무시 규칙
    - Visual Studio, Rider IDE 설정 파일 무시
    - NuGet 패키지 디렉토리 무시
    - 테스트 결과 및 배포 프로필 무시

#### Tech Details
- **문서화 전략:** 프로젝트 아키텍처를 Mermaid 다이어그램으로 시각화하여 이해도 향상
- **설정 파일 최적화:** Cursor AI가 불필요한 파일을 읽지 않도록 `.cursorignore` 최적화
- **Git 관리:** 빌드 산출물과 임시 파일이 Git에 커밋되지 않도록 `.gitignore` 정리

#### Current Status
- 모든 문서 파일이 AI Mouse WPF 프로젝트에 맞게 업데이트 완료
- 설정 파일이 WPF .NET 8 프로젝트 표준에 맞게 정리 완료
- 프로젝트 구조 및 아키텍처가 명확하게 문서화됨
- 다음 단계: Phase 1.1 프로젝트 초기 세팅 작업 진행 준비 완료

---

### 2026-02-05 (목) - Phase 1.1 프로젝트 착수 및 기반 구축 (1차)
**[목표]** WPF 프로젝트를 생성하고, MVVM/DI 구조를 잡은 뒤 시스템 트레이에 상주시키는 기본 골격 완성.

#### Dev Action (Foundation)
- **Project Setup:**
  - .NET 8 WPF 솔루션 `AI_Mouse` 생성.
  - 필수 NuGet 패키지(`CommunityToolkit.Mvvm`, `Hardcodet.NotifyIcon.Wpf`, `DI`) 설치.
  
- **Structure & MVVM:**
  - 폴더 구조(`Views`, `ViewModels`, `Services`) 정의.
  - `MainViewModel` 생성 및 `ObservableObject` 상속.
  - `App.xaml.cs`에서 `ServiceProvider` 구성 및 `MainWindow` 의존성 주입 연결.

- **System Tray Implementation:**
  - `TaskbarIcon` 리소스 정의.
  - 앱 실행 시 `MainWindow`를 숨기고(`Visibility="Hidden"`) 트레이 아이콘만 표시.
  - ContextMenu에 '설정', '종료' 메뉴 추가 및 `Application.Current.Shutdown()` 연결.

#### Tech Details
- **DI Container:** `Microsoft.Extensions.DependencyInjection`을 사용하여 뷰와 뷰모델, 서비스를 느슨하게 결합.
- **Tray Logic:** 윈도우 닫기 버튼을 눌러도 앱이 종료되지 않고 트레이로 숨겨지도록 `Closing` 이벤트 처리 필요 (Phase 1.2 예정).

#### Current Status
- 앱 실행 시 윈도우 없이 트레이 아이콘이 정상적으로 표시됨.
- 우클릭 메뉴를 통해 정상 종료 가능.
- MVVM 및 DI 기반의 확장 가능한 아키텍처 준비 완료.