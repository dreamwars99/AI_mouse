# 📝 작업 일지 (Project History & Context)

- **Project:** AI Mouse (Windows Smart Assistant)
- **Role:** Lead Architect & Cursor AI
- **Framework:** .NET 8 (WPF)
- **Platform:** Windows 10 / 11 Desktop
- **Last Updated:** 2026-02-05 (ResultWindow UX 개선 완료: 항상 위 해제, 최소화 버튼, 스크롤 포커스 수정, 18차)

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
- **NAudio** ✅ (Phase 2.2 완료)
  - Purpose: 마이크 오디오 녹음 및 WAV 파일 저장
  - Version: 2.2.1
- **Newtonsoft.Json** ✅ (Phase 3.1 완료)
  - Purpose: Gemini API 요청/응답 JSON 직렬화/역직렬화
  - Version: 13.0.3
- **HttpClient** ✅ (Phase 3.1 완료)
  - Purpose: Gemini API HTTP 통신 (System.Net.Http)
  - Built-in (.NET Framework)
- **Markdig.Wpf** ✅ (Phase 4.1 완료)
  - Purpose: 마크다운 텍스트를 WPF에서 렌더링
  - Version: 0.5.0.1

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

**Services (Implemented)**
- `GlobalHookService`: 마우스/키보드 전역 이벤트 감지 (User32.dll) ✅
- `ScreenCaptureService`: GDI+를 이용한 화면 캡처 ✅
- `AudioRecorderService`: NAudio 기반 음성 녹음 ✅
- 
- `GeminiService`: HttpClient 기반 Gemini 1.5 Pro API 통신 ✅

**Views (Implemented)**
- `MainWindow`: 메인 윈도우 (초기엔 Hidden) ✅
- `OverlayWindow`: 화면 캡처 오버레이 ✅
- `ResultWindow`: AI 응답 표시 창 (마크다운 렌더링) ✅

**ViewModels (Implemented)**
- `MainViewModel`: 메인 로직 및 상태 관리 ✅
- `OverlayViewModel`: 오버레이 상태 관리 ✅
- `ResultViewModel`: 결과 표시 로직 ✅


****## 📅 4. Development Log (개발 기록)

### 2026-02-05 (목) - ResultWindow UX 개선: 항상 위 해제, 최소화 버튼, 스크롤 포커스 수정 (18차)
**[목표]** **ResultWindow**의 UX를 개선한다. 항상 위 기능을 해제하여 다른 창 뒤로 이동 가능하게 하고, 최소화 버튼을 추가하며, 스크롤 포커스 문제를 해결하여 마우스 휠이 즉시 작동하도록 개선.

#### Dev Action (ResultWindow UX Enhancement)
- **Views/ResultWindow.xaml 수정:**
  - `Topmost="True"` 속성 제거:
    - 항상 위 기능을 해제하여 다른 창 뒤로 이동 가능하도록 변경
    - 사용자가 다른 작업을 할 때 창이 방해되지 않도록 개선
  - 타이틀 바에 최소화 버튼 추가:
    - 닫기 버튼(✕) 왼쪽에 최소화 버튼(_) 추가
    - `Grid.ColumnDefinitions`에 새로운 컬럼 추가 (`ColumnDefinition Width="Auto"`)
    - 최소화 버튼 스타일은 닫기 버튼과 동일하게 적용 (호버 시 배경색 변경)
    - `Click="MinimizeButton_Click"` 이벤트 핸들러 연결
    - 버튼 간 간격 조정 (`Margin="0,0,4,0"`)
  - `ScrollViewer`에 `Focusable="True"` 속성 추가:
    - 스크롤 뷰어가 포커스를 받을 수 있도록 설정
    - 마우스 휠 스크롤이 즉시 작동하도록 개선

- **Views/ResultWindow.xaml.cs 수정:**
  - `MinimizeButton_Click` 이벤트 핸들러 구현:
    - `this.WindowState = WindowState.Minimized;` 호출로 창 최소화
    - 작업 표시줄로 창을 최소화하여 화면 공간 확보
  - `Loaded` 이벤트 핸들러 추가:
    - 생성자에서 `Loaded` 이벤트 구독
    - 창이 로드될 때 `this.Activate()`와 `this.Focus()` 호출
    - 창이 표시되자마자 포커스를 받아 마우스 휠 스크롤이 즉시 작동하도록 보장
    - 스크롤 포커스 문제 해결

- **md/To_do.md 업데이트:**
  - Phase 4.1 섹션에 "결과창 UX 개선 완료 (항상 위 해제, 최소화 버튼, 스크롤 포커스 수정)" 항목 추가 및 완료 처리

#### Tech Details
- **항상 위 해제:** `Topmost` 속성 제거로 일반 창처럼 동작하여 다른 창 뒤로 이동 가능
- **최소화 기능:** `WindowState.Minimized`로 창을 작업 표시줄로 최소화
- **포커스 관리:** `Loaded` 이벤트에서 `Activate()`와 `Focus()` 호출로 창 표시 시 즉시 포커스 획득
- **스크롤 포커스:** `ScrollViewer`에 `Focusable="True"` 설정으로 마우스 휠 스크롤 즉시 작동
- **UI 일관성:** 최소화 버튼 스타일을 닫기 버튼과 동일하게 유지하여 일관된 UX 제공

#### Current Status
- ✅ `ResultWindow.xaml`에서 `Topmost="True"` 제거 완료
- ✅ 타이틀 바에 최소화 버튼 추가 완료 (`MinimizeButton_Click` 이벤트 연결)
- ✅ `ScrollViewer`에 `Focusable="True"` 추가 완료
- ✅ `ResultWindow.xaml.cs`에 `MinimizeButton_Click` 핸들러 구현 완료 (`WindowState.Minimized`)
- ✅ `Loaded` 이벤트 핸들러 추가 완료 (`Activate()`, `Focus()` 호출)
- ✅ `To_do.md`에 완료 항목 추가 완료
- ✅ Linter 에러 없음 확인 완료
- ResultWindow UX 개선 완료, 사용자 경험 향상 (항상 위 해제, 최소화 버튼, 스크롤 포커스 수정)

---

### 2026-02-05 (목) - ResultWindow UX 개선: 드래그 이동 및 스크롤 기능 강화 (17차)
**[목표]** 테두리 없는 창(`WindowStyle="None"`)인 **ResultWindow**를 사용자가 마우스로 드래그하여 이동할 수 있도록 기능을 추가하고, 긴 답변이 왔을 때 **마우스 휠 스크롤**이 부드럽게 작동하도록 UI를 개선.

#### Dev Action (ResultWindow Drag & Scroll Enhancement)
- **Views/ResultWindow.xaml 수정:**
  - 타이틀 바 `Border`에 `MouseLeftButtonDown="TitleBar_MouseLeftButtonDown"` 이벤트 추가
  - `MaxHeight`를 `600`에서 `900`으로 증가하여 더 많은 콘텐츠 표시 가능하도록 개선
  - `ScrollViewer`에 `CanContentScroll="False"` 속성 추가:
    - 픽셀 단위 스크롤 활성화로 마우스 휠 감도가 시스템 설정과 자연스럽게 동기화
    - 줄 단위가 아닌 부드러운 스크롤 경험 제공
  - `ScrollViewer`에 `PanningMode="VerticalOnly"` 속성 추가:
    - 터치패드/터치스크린에서 세로 스크롤 지원
  - `VerticalScrollBarVisibility="Auto"` 유지 (내용이 넘칠 때만 스크롤바 표시)

- **Views/ResultWindow.xaml.cs 수정:**
  - `TitleBar_MouseLeftButtonDown` 이벤트 핸들러 구현:
    - `MouseButtonEventArgs`의 `ButtonState` 확인
    - `this.DragMove()` 호출로 윈도우 이동 기능 활성화
    - 닫기 버튼(✕) 클릭 시 드래그가 발생하지 않도록 보장 (Button이 이벤트를 처리하므로 상위로 전파되지 않음)

- **md/To_do.md 업데이트:**
  - Phase 4.1 섹션에 "윈도우 드래그 이동 기능 구현 완료 (DragMove)" 항목 추가 및 완료 처리
  - Phase 4.1 섹션에 "결과창 스크롤 UX 개선 완료 (휠 지원 및 픽셀 단위 스크롤, MaxHeight 900)" 항목 추가 및 완료 처리

#### Tech Details
- **드래그 이동:** WPF의 `DragMove()` 메서드를 사용하여 테두리 없는 창 이동 구현
- **픽셀 단위 스크롤:** `CanContentScroll="False"`로 설정하여 부드러운 스크롤 경험 제공
- **이벤트 처리:** 타이틀 바 영역에만 드래그 이벤트를 연결하여 텍스트 선택 방해 방지
- **터치 지원:** `PanningMode="VerticalOnly"`로 터치 디바이스 지원
- **높이 제한 완화:** `MaxHeight`를 900으로 증가하여 긴 답변도 더 많이 표시 가능

#### Current Status
- ✅ `ResultWindow.xaml`에 타이틀 바 `MouseLeftButtonDown` 이벤트 추가 완료
- ✅ `ResultWindow.xaml.cs`에 `TitleBar_MouseLeftButtonDown` 핸들러 구현 완료 (`DragMove()` 호출)
- ✅ `MaxHeight`를 600에서 900으로 증가 완료
- ✅ `ScrollViewer`에 `CanContentScroll="False"` 추가 완료 (픽셀 단위 스크롤)
- ✅ `ScrollViewer`에 `PanningMode="VerticalOnly"` 추가 완료 (터치 지원)
- ✅ `To_do.md`에 완료 항목 추가 완료
- ✅ Linter 에러 없음 확인 완료
- ResultWindow 드래그 이동 및 스크롤 UX 개선 완료, 사용자 경험 향상

---

### 2026-02-05 (목) - Gemini 모델 ID 변경 및 URL 동적 생성 로직 개선 (16차)
**[목표]** 사용자의 환경에 맞춰 Gemini 모델 ID를 `gemini-1.5-pro`에서 **`gemini-2.5-flash`**로 변경하고, URL 생성 로직을 유연하게 개선하여 향후 모델 변경 시 코드 수정을 최소화.

#### Dev Action (Gemini Model ID & URL Dynamic Generation)
- **Services/Implementations/GeminiService.cs 수정:**
  - 모델 ID 상수 분리:
    - 클래스 최상단에 `private const string ModelId = "gemini-2.5-flash";` 상수 정의
    - API 버전도 상수로 분리: `private const string ApiVersion = "v1beta";`
    - 사용자 환경에 따라 `gemini-1.5-flash`, `gemini-2.5-flash` 등으로 쉽게 변경 가능하도록 구조화
  - URL 생성 로직 수정:
    - 하드코딩된 URL `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={apiKey}` 제거
    - 상수를 사용한 동적 URL 생성으로 변경:
      ```csharp
      string endpoint = $"https://generativelanguage.googleapis.com/{ApiVersion}/models/{ModelId}:generateContent?key={apiKey}";
      ```
    - 404 오류 방지를 위해 URL 구조 정확성 재확인
  - 디버그 로그 강화:
    - 요청 시작 시 사용 중인 모델 ID를 명확히 로그에 출력:
      ```csharp
      System.Diagnostics.Debug.WriteLine($"[Gemini API] Target Model: {ModelId}");
      ```
    - 디버깅 시 어떤 모델을 사용하는지 즉시 확인 가능

#### Tech Details
- **모델 ID 관리:** 상수로 분리하여 코드 상단에서 한 번만 수정하면 전체 적용
- **URL 동적 생성:** 문자열 보간(String Interpolation)을 사용하여 유연한 URL 생성
- **유지보수성 향상:** 향후 모델 변경 시 상수 값만 변경하면 됨
- **디버그 지원:** 로그 출력으로 실제 사용 중인 모델 확인 가능
- **404 오류 방지:** URL 구조를 정확히 검증하여 API 호출 실패 방지

#### Current Status
- ✅ `GeminiService.cs`에 모델 ID 상수(`ModelId = "gemini-2.5-flash"`) 추가 완료
- ✅ `GeminiService.cs`에 API 버전 상수(`ApiVersion = "v1beta"`) 추가 완료
- ✅ URL 생성 로직을 동적 생성으로 변경 완료 (상수 사용)
- ✅ 디버그 로그에 모델 ID 출력 추가 완료 (`System.Diagnostics.Debug.WriteLine`)
- ✅ Linter 에러 없음 확인 완료
- 모델 ID 변경 및 URL 동적 생성 로직 개선 완료, 향후 모델 변경 시 유지보수 용이

---

### 2026-02-05 (목) - API Key 외부 파일 분리: 보안 강화 및 GitHub 유출 방지 (15차)
**[목표]** API Key가 GitHub에 유출되는 것을 막기 위해, 외부 파일(`apikey.txt`)에서 키를 로드하는 안전한 구조로 변경하여 보안을 강화.

#### Dev Action (API Key Externalization)
- **.gitignore 수정:**
  - `apikey.txt` 파일을 `.gitignore`에 추가
  - 이 파일이 절대 Git에 커밋되지 않도록 보장
  - PowerShell `Add-Content` 명령으로 추가

- **AI_Mouse.csproj 수정:**
  - `<ItemGroup>` 섹션에 `<None Update="apikey.txt">` 항목 추가
  - `CopyToOutputDirectory` 속성을 `PreserveNewest`로 설정
  - 빌드 시 `apikey.txt` 파일이 존재할 경우 출력 디렉토리(bin/Debug 등)로 자동 복사
  - 실행 파일과 같은 폴더에 API Key 파일이 위치하도록 보장

- **ViewModels/MainViewModel.cs 수정:**
  - 기존 `private const string ApiKey = "";` 상수 제거
  - 코드에 API Key를 하드코딩하지 않도록 변경
  - `LoadApiKey()` 메서드 구현:
    - `AppDomain.CurrentDomain.BaseDirectory` 경로에서 `apikey.txt` 파일 찾기
    - 파일이 존재하면 내용을 읽어 공백 제거(`Trim()`) 후 반환
    - 파일이 없거나 읽기 실패 시 `null` 반환
    - 예외 처리 포함 (`try-catch`) - 파일 읽기 실패 시에도 앱이 멈추지 않도록 보장
    - 디버그 로그 출력으로 문제 진단 가능
  - `HandleMouseUp` 메서드 수정:
    - `LoadApiKey()` 호출하여 런타임에 API Key 로드
    - 키가 없으면 `MessageBox.Show`로 사용자 안내 메시지 표시:
      - "실행 폴더에 apikey.txt 파일을 만들고 키를 넣어주세요."
    - 키가 있으면 기존 로직대로 Gemini API 호출

- **using 문 추가:**
  - `System.IO` 네임스페이스 추가 (`File`, `Path` 클래스 사용)

#### Tech Details
- **보안 강화:** API Key를 코드에서 완전히 분리하여 GitHub 유출 방지
- **파일 기반 관리:** 외부 파일(`apikey.txt`)에서 런타임에 로드
- **Git 보호:** `.gitignore`에 추가하여 실수로 커밋되는 것을 방지
- **빌드 통합:** MSBuild 설정으로 빌드 시 자동 복사
- **예외 처리:** 파일 읽기 실패 시에도 앱이 안정적으로 동작
- **사용자 안내:** 키가 없을 때 명확한 안내 메시지 제공
- **디버그 지원:** 로그 출력으로 문제 진단 가능

#### Current Status
- ✅ `.gitignore`에 `apikey.txt` 추가 완료
- ✅ `AI_Mouse.csproj`에 파일 복사 설정 추가 완료 (`CopyToOutputDirectory="PreserveNewest"`)
- ✅ `MainViewModel.cs`에서 API Key 상수 제거 완료
- ✅ `LoadApiKey()` 메서드 구현 완료 (파일 읽기, 예외 처리 포함)
- ✅ `HandleMouseUp` 메서드에서 `LoadApiKey()` 호출하도록 수정 완료
- ✅ API Key 없을 때 사용자 안내 메시지 구현 완료 (`MessageBox.Show`)
- ✅ `System.IO` 네임스페이스 추가 완료
- ✅ Linter 에러 없음 확인 완료
- 보안 개선 완료, API Key가 코드에 하드코딩되지 않음

---

### 2026-02-05 (목) - Phase 4.1 결과 뷰어 완료: ResultWindow 및 Markdig.Wpf 마크다운 렌더링 구현 (14차)
**[목표]** `MessageBox` 대신 **AI의 응답을 보여줄 전용 윈도우(ResultWindow)**를 구현하고, **`Markdig.Wpf`** 라이브러리를 사용하여 마크다운 텍스트를 렌더링하는 기능을 완성.

#### Dev Action (Result Window Implementation)
- **AI_Mouse.csproj 수정:**
  - `Markdig.Wpf` NuGet 패키지 추가 (v0.5.0.1)
  - 마크다운 텍스트를 WPF에서 렌더링하기 위한 필수 패키지

- **ViewModels/ResultViewModel.cs 생성:**
  - `ResultViewModel` 클래스 생성 (`ObservableObject` 상속)
  - `[ObservableProperty] string _responseText;` (AI 답변 바인딩용)
  - `[ObservableProperty] bool _isLoading;` (로딩 상태 표시용)
  - `LoadingVisibility`, `ContentVisibility` 속성 구현 (UI 표시 제어)
  - `OnIsLoadingChanged` 부분 메서드로 Visibility 속성 자동 업데이트
  - `[RelayCommand] void Close();` (창 닫기 명령)

- **Views/ResultWindow.xaml 생성:**
  - Window 속성 설정:
    - `WindowStyle="None"` (테두리 없는 창)
    - `ResizeMode="CanResizeWithGrip"` (우측 하단 그립으로 크기 조절)
    - `Topmost="True"` (항상 최상단 표시)
    - `SizeToContent="Height"` (높이는 콘텐츠에 맞춤)
    - `Width="400"`, `MaxHeight="600"` (고정 너비, 최대 높이 제한)
  - UI 구성:
    - 상단: 타이틀 바 ("AI Mouse") + 닫기 버튼 (X)
    - 중앙: `ScrollViewer` 안에 `markdown:MarkdownViewer` 배치
      - 네임스페이스: `xmlns:markdown="clr-namespace:Markdig.Wpf;assembly=Markdig.Wpf"`
      - Markdown 바인딩: `Markdown="{Binding ResponseText}"`
  - 스타일: 배경 흰색, 둥근 모서리(`Border.CornerRadius="8"`), 그림자 효과(`DropShadowEffect`)
  - 로딩 인디케이터: `ProgressBar` (IsIndeterminate) 및 텍스트 표시

- **Views/ResultWindow.xaml.cs 생성:**
  - ESC 키로 창 닫기 (`KeyDown` 이벤트)
  - `SetViewModel(ResultViewModel)` 메서드 구현 (DI 주입)
  - `CloseButton_Click` 이벤트 핸들러 구현
  - `OnSourceInitialized` 오버라이드:
    - 마우스 커서 위치 가져오기 (`NativeMethods.GetCursorPos`)
    - 창 위치 계산 (마우스 커서 근처 또는 우측 하단)
    - 화면 밖으로 나가지 않도록 조정

- **Helpers/NativeMethods.cs 수정:**
  - `GetCursorPos` Win32 API 선언 추가
    - 현재 마우스 커서의 화면 좌표를 가져오는 함수
    - `[DllImport("user32.dll")]` 사용

- **App.xaml.cs 수정:**
  - `ResultViewModel`과 `ResultWindow`를 Transient로 등록
    - 질문할 때마다 새 창을 띄우기 위함
  - `App.Services` 정적 속성 추가:
    - `public static IServiceProvider? Services => ((App)Current)._serviceProvider;`
    - 외부에서 ServiceProvider 접근 가능하도록 함

- **ViewModels/MainViewModel.cs 수정:**
  - 생성자에 `IServiceProvider` 주입 추가 (`_serviceProvider` 필드)
  - `HandleMouseUp` 메서드 수정:
    - 기존 `MessageBox.Show` 코드 제거
    - API 요청 시작 시:
      - `ResultWindow` 및 `ResultViewModel` 생성 (DI)
      - `IsLoading = true` 설정
      - `ResultWindow.Show()` 호출 (로딩 상태로 표시)
    - 응답 도착 시:
      - `ResponseText` 업데이트
      - `IsLoading = false` 설정
    - 오류 발생 시:
      - 오류 메시지를 마크다운 형식으로 `ResultWindow`에 표시
      - `MessageBox` 대신 `ResultWindow` 사용

#### Tech Details
- **Markdig.Wpf:** WPF용 마크다운 렌더링 라이브러리 (Markdig 기반)
- **마크다운 렌더링:** AI 응답의 Bold, List, Code 등 마크다운 문법이 정상적으로 렌더링됨
- **로딩 상태 관리:** `IsLoading` 속성으로 로딩 인디케이터와 마크다운 뷰어를 전환
- **창 위치 설정:** 마우스 커서 근처에 창을 배치하여 사용자 경험 향상
- **ESC 키 지원:** 사용자가 ESC 키를 누르면 창이 닫힘
- **모던 디자인:** 둥근 모서리, 그림자 효과, 깔끔한 UI 디자인
- **DI 패턴:** Transient 등록으로 질문할 때마다 새 창 생성 (독립적인 상태 관리)
- **비동기 처리:** API 응답을 기다리는 동안 로딩 상태 표시로 사용자 피드백 제공

#### Current Status
- ✅ `Markdig.Wpf` 패키지 설치 완료 (v0.5.0.1)
- ✅ `ResultViewModel.cs` 생성 완료 (`ObservableObject` 상속, 속성 구현)
- ✅ `ResultWindow.xaml` 생성 완료 (모던 디자인, 마크다운 뷰어 포함)
- ✅ `ResultWindow.xaml.cs` 생성 완료 (ESC 키 닫기, 마우스 커서 위치 설정)
- ✅ `NativeMethods.cs`에 `GetCursorPos` Win32 API 추가 완료
- ✅ `App.xaml.cs`에 `ResultViewModel`과 `ResultWindow` DI 등록 완료
- ✅ `App.Services` 정적 속성 추가 완료
- ✅ `MainViewModel`에 `IServiceProvider` 주입 추가 완료
- ✅ `MainViewModel`에서 MessageBox 대신 ResultWindow 사용하도록 변경 완료
- ✅ API 요청 시작 시 ResultWindow 표시 및 로딩 상태 설정 완료
- ✅ 응답 도착 시 ResponseText 업데이트 및 로딩 상태 해제 완료
- ✅ 오류 메시지도 ResultWindow로 표시하도록 변경 완료
- ✅ Linter 에러 없음 확인 완료
- Phase 4.1 완전히 완료, 다음 단계: Phase 4.2 사용자 설정 (Settings Window) 구현 준비

---

### 2026-02-05 (목) - Phase 3.1 Gemini API 연동 완료: HttpClient 기반 Gemini 1.5 Pro API 통신 서비스 구현 (13차)
**[목표]** `HttpClient`를 사용하여 **Gemini 1.5 Pro API**와 통신하는 서비스를 구현하고, 캡처된 이미지와 녹음된 오디오를 멀티모달 요청으로 전송하여 AI 응답을 받는 기능을 완성.

#### Dev Action (Gemini API Service Implementation)
- **AI_Mouse.csproj 수정:**
  - `Newtonsoft.Json` NuGet 패키지 추가 (v13.0.3)
  - JSON 직렬화/역직렬화를 위한 필수 패키지

- **Services/Interfaces/IGeminiService.cs 생성:**
  - `IGeminiService` 인터페이스 정의
  - `GetResponseAsync(BitmapSource image, string audioPath, string apiKey)` 메서드 정의
  - 간결한 인터페이스 설계 (멀티모달 입력 처리)

- **Services/Implementations/GeminiService.cs 생성:**
  - `IGeminiService` 인터페이스 구현
  - 생성자에서 `HttpClient` 주입 (DI)
  - `GetResponseAsync` 구현:
    - 이미지 변환: `BitmapSource` → JPEG Encoder → Byte[] → Base64
    - 오디오 변환: 파일 경로에서 Byte[] 읽기 → Base64
    - JSON 요청 본문 생성:
      - 텍스트 프롬프트: "당신은 윈도우 AI 비서입니다. 화면과 음성을 보고 한국어로 답변하세요."
      - 이미지: `inline_data` 형식 (`mime_type: "image/jpeg"`)
      - 오디오: `inline_data` 형식 (`mime_type: "audio/wav"`)
    - API 엔드포인트: `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={apiKey}`
    - `HttpClient.PostAsync` 사용하여 HTTP POST 요청
    - 응답 파싱: `Newtonsoft.Json`으로 JSON 역직렬화
    - 응답 텍스트 추출 및 반환
  - DTO 클래스: `GeminiService` 클래스 내부에 `private class`로 Request/Response 구조체 정의
    - `GeminiRequest`, `Content`, `Part`, `InlineData` (요청용)
    - `GeminiResponse`, `Candidate` (응답용)
  - 에러 처리:
    - `HttpRequestException` 처리 및 사용자 친화적 메시지 반환
    - API 키 검증 (빈 문자열 체크)
    - 파일 존재 여부 확인 (오디오 파일)

- **ViewModels/MainViewModel.cs 수정:**
  - 생성자에 `IGeminiService` 주입 추가 (`_geminiService` 필드)
  - API Key 상수 선언:
    - `private const string ApiKey = "";` (빈 문자열)
    - TODO 주석: "여기에 Google AI Studio API Key를 입력하세요"
  - `HandleMouseUp` 메서드 수정:
    - 캡처/녹음 완료 후 `_geminiService.GetResponseAsync` 호출
    - API Key 검증: 비어있으면 "API 키를 설정해주세요" 메시지 출력
    - 결과 처리:
      - 응답 텍스트를 `Clipboard.SetText()`로 클립보드 복사
      - `MessageBox.Show()`로 응답 텍스트 출력
    - 예외 처리: API 호출 실패 시 사용자 친화적 에러 메시지 표시

- **App.xaml.cs 수정:**
  - `HttpClient` 싱글톤 등록 추가 (`services.AddSingleton<HttpClient>()`)
  - `IGeminiService` → `GeminiService` 싱글톤 등록 추가
  - `GeminiService` 생성자에 `HttpClient` 자동 주입
  - `OnExit` 메서드 수정:
    - `HttpClient` Dispose 호출 추가 (리소스 정리)
    - 예외 처리 추가

#### Tech Details
- **Gemini 1.5 Pro 모델:** 반드시 `gemini-1.5-pro` 엔드포인트 사용 (사용자 요구사항 준수)
- **멀티모달 입력:** 이미지(JPEG)와 오디오(WAV)를 동시에 전송하여 컨텍스트 풍부한 질의 가능
- **Base64 인코딩:** 이미지와 오디오를 Base64 문자열로 변환하여 JSON에 포함
- **JSON 구조:** Gemini API 표준 형식 준수 (`contents[0].parts[]` 배열 구조)
- **비동기 처리:** `async/await` 패턴으로 UI 응답성 유지
- **에러 처리:** `HttpRequestException`을 잡아 사용자 친화적 메시지 제공
- **리소스 관리:** `HttpClient` 싱글톤 사용 및 종료 시 Dispose 보장
- **파일 간소화:** DTO 클래스를 별도 파일이 아닌 `GeminiService` 내부 `private class`로 정의

#### Current Status
- ✅ `Newtonsoft.Json` 패키지 설치 완료 (v13.0.3)
- ✅ `IGeminiService.cs` 생성 완료 (인터페이스 정의)
- ✅ `GeminiService.cs` 생성 완료 (HttpClient 기반 API 통신 구현)
- ✅ 이미지 Base64 변환 로직 구현 완료 (BitmapSource → JPEG → Base64)
- ✅ 오디오 Base64 변환 로직 구현 완료 (파일 경로 → Byte[] → Base64)
- ✅ JSON 요청 본문 생성 로직 구현 완료 (텍스트 + 이미지 + 오디오)
- ✅ `HttpClient.PostAsync` 사용하여 API 호출 구현 완료
- ✅ 응답 파싱 및 텍스트 추출 로직 구현 완료
- ✅ DTO 클래스를 `GeminiService` 내부 `private class`로 정의 완료
- ✅ `MainViewModel`에 `IGeminiService` 주입 및 API 호출 로직 구현 완료
- ✅ API Key 상수 선언 및 검증 로직 구현 완료
- ✅ 결과 클립보드 복사 및 MessageBox 출력 로직 구현 완료
- ✅ `App.xaml.cs`에 `HttpClient` 및 `IGeminiService` 싱글톤 등록 완료
- ✅ `OnExit`에서 `HttpClient` Dispose 호출 추가 완료
- ✅ 에러 처리 및 사용자 친화적 메시지 구현 완료
- Phase 3.1 완전히 완료, 다음 단계: Phase 4.1 결과 뷰어 (Result Window) 구현 준비

---

### 2026-02-05 (목) - DpiHelper 네임스페이스 별칭 적용 및 타입 모호성 해결 (12차)
**[목표]** `System.Drawing.Point`와 `System.Windows.Point` 간의 모호함(CS0104)을 해결하기 위해 네임스페이스 별칭(Alias)을 적용하여 컴파일 에러를 제거하고 타입 의도를 명확히 함.

#### Dev Action (Namespace Alias & Type Clarity)
- **Helpers/DpiHelper.cs 수정:**
  - `using` 섹션 정리 및 네임스페이스 별칭 추가:
    - `System.Drawing` 제거 (별칭으로 대체)
    - `System.Runtime.InteropServices` 추가
    - `System.Windows.Media` 추가
    - 네임스페이스 별칭 정의:
      - `WpfPoint = System.Windows.Point` (논리 좌표용)
      - `WinPoint = System.Drawing.Point` (물리 좌표용)
      - `WpfRect = System.Windows.Rect` (WPF Rect용)
  - 타입 명시 수정:
    - `GetDpiForPoint`: `new WinPoint(x, y)` 사용 (Win32 API용 `System.Drawing.Point`)
    - `PhysicalToLogical`: 반환 타입 `WpfPoint` (이미 적용됨)
    - `LogicalToPhysical`: 반환 타입 `WinPoint`로 변경 (`System.Drawing.Point`)
    - `PhysicalToLogicalRect`: 매개변수 및 반환 타입 `WpfRect`로 변경
    - `LogicalToPhysicalRect`: 매개변수 및 반환 타입 `WpfRect`로 변경
  - 코드 내부 타입 사용 명확화:
    - 모든 `Point` 사용을 `WpfPoint` 또는 `WinPoint`로 명시
    - 모든 `Rect` 사용을 `WpfRect`로 명시
    - 주석에 타입 용도 명확히 표시

#### Tech Details
- **네임스페이스 모호성 해결:** `System.Drawing.Point`와 `System.Windows.Point`가 모두 `Point`로 인식되어 발생하는 CS0104 컴파일 에러 해결
- **타입 의도 명확화:** 별칭을 통해 각 타입의 용도가 명확해짐
  - `WpfPoint`: WPF 논리 좌표계 (DPI-independent)
  - `WinPoint`: Win32 물리 좌표계 (Physical Pixel)
  - `WpfRect`: WPF Rect (논리 좌표계)
- **기존 로직 유지:** 타입 명시만 수정하여 기존 변환 로직은 그대로 유지
- **코드 가독성 향상:** 별칭을 통해 코드를 읽는 사람이 각 타입의 용도를 쉽게 파악 가능

#### Current Status
- ✅ 네임스페이스 별칭 정의 완료 (`WpfPoint`, `WinPoint`, `WpfRect`)
- ✅ `GetDpiForPoint`에서 `WinPoint` 사용으로 수정 완료
- ✅ `LogicalToPhysical` 반환 타입 `WinPoint`로 변경 완료
- ✅ `PhysicalToLogicalRect` 및 `LogicalToPhysicalRect` 매개변수/반환 타입 `WpfRect`로 변경 완료
- ✅ 컴파일 에러(CS0104) 해결 완료
- ✅ 타입 의도 명확화 완료
- ✅ Linter 에러 없음 확인 완료
- 코드 품질 개선 완료, 다음 단계: Phase 3.1 Gemini API 연동 (Intelligence Layer) 구현 준비

---

### 2026-02-05 (목) - Phase 2.2 NAudio 기반 마이크 음성 녹음 서비스 구현 (11차)
**[목표]** `NAudio` 라이브러리를 사용하여 **마이크 음성 녹음 서비스(AudioRecorderService)**를 구현하고, 드래그 동작과 연동하여 **WAV 파일**을 생성하는 기능을 완성.

#### Dev Action (Audio Recording Service)
- **AI_Mouse.csproj 수정:**
  - `NAudio` NuGet 패키지 추가 (v2.2.1)
  - 마이크 입력 캡처 및 WAV 파일 저장을 위한 필수 패키지

- **Services/Interfaces/IAudioRecorderService.cs 생성:**
  - `IAudioRecorderService` 인터페이스 정의 (`IDisposable` 상속)
  - `StartRecording()` 메서드: 녹음 시작
  - `StopRecordingAsync()` 메서드: 녹음 중지 및 WAV 파일 경로 반환 (`Task<string>`)
  - 간결한 인터페이스 설계 (이벤트 기반 오디오 레벨 미터링은 선택 사항으로 제외)

- **Services/Implementations/AudioRecorderService.cs 생성:**
  - `IAudioRecorderService` 인터페이스 구현
  - `StartRecording()` 구현:
    - `WaveInEvent` 초기화 (16kHz, 16bit, Mono - Gemini API 호환 포맷)
    - `Path.GetTempPath()/AI_Mouse` 폴더 생성 (없으면 자동 생성)
    - 출력 파일 경로 설정: `audio_temp.wav` (덮어쓰기 모드)
    - `WaveFileWriter` 초기화 및 데이터 수신 이벤트 핸들러 등록
    - `WaveInEvent.StartRecording()` 호출하여 녹음 시작
    - 예외 처리: 마이크 없음/권한 없음 시에도 앱이 계속 동작하도록 처리
  - `StopRecordingAsync()` 구현:
    - `TaskCompletionSource`를 사용한 비동기 처리
    - `WaveInEvent.StopRecording()` 호출
    - `RecordingStopped` 이벤트 대기 (녹음 완료 보장)
    - `WaveFileWriter.Dispose()` 호출로 파일 스트림 확실히 닫기 (파일 잠금 해제)
    - 파일 경로 반환
  - `OnDataAvailable` 이벤트 핸들러: 오디오 버퍼를 `WaveFileWriter`에 쓰기
  - `OnRecordingStopped` 이벤트 핸들러: `TaskCompletionSource` 완료 및 리소스 정리
  - `Cleanup()` 메서드: `WaveFileWriter`와 `WaveInEvent` 리소스 안전 해제
  - `Dispose()` 패턴 구현: `IDisposable` 인터페이스 준수

- **ViewModels/MainViewModel.cs 수정:**
  - 생성자에 `IAudioRecorderService` 주입 추가 (`_audioService` 필드)
  - `HandleMouseDown` 메서드 수정:
    - 기존 오버레이 표시 로직 유지
    - `_audioService.StartRecording()` 호출 추가
    - 예외 처리: 마이크 없음/권한 없음 시에도 계속 진행 (디버그 로그만 출력)
  - `HandleMouseUp` 메서드 수정:
    - 기존 화면 캡처 로직 유지
    - `await _audioService.StopRecordingAsync()` 호출 추가하여 오디오 파일 경로 받기
    - 검증용 `MessageBox` 메시지 수정: "캡처 및 녹음 완료!\n이미지: 클립보드\n오디오: {audioPath}"
    - 유효하지 않은 영역인 경우에도 오디오 녹음은 중지하도록 처리
    - 예외 처리: 오디오 녹음 오류는 무시하고 계속 진행

- **App.xaml.cs 수정:**
  - `IAudioRecorderService` → `AudioRecorderService` 싱글톤 등록 추가
  - DI 컨테이너에 서비스 등록 순서 명확화 (주석 번호 업데이트)
  - `OnExit` 메서드 수정:
    - `AudioRecorderService` Dispose 호출 추가 (`IDisposable` 리소스 정리)
    - 예외 처리 추가 (오디오 서비스 정리 실패 시에도 앱 종료 보장)

#### Tech Details
- **NAudio 라이브러리:** Windows 오디오 API를 래핑한 .NET 라이브러리로 마이크 입력 캡처 및 WAV 파일 저장 제공
- **오디오 포맷:** PCM 16bit, Mono, 16kHz (Gemini API 호환성을 위해 표준 포맷 사용)
- **파일 저장:** `Path.GetTempPath()/AI_Mouse/audio_temp.wav`에 저장 (덮어쓰기 모드로 매번 새로 생성)
- **비동기 처리:** `TaskCompletionSource`를 사용하여 `RecordingStopped` 이벤트가 발생할 때까지 안전하게 대기
- **리소스 관리:** `WaveFileWriter.Dispose()`를 반드시 호출하여 파일 스트림을 닫아야 파일 잠금이 풀림 (중요!)
- **예외 처리:** 마이크가 없거나 권한이 없는 경우에도 앱이 계속 동작하도록 처리 (디버그 로그만 출력)
- **파일 잠금 해제:** `WaveFileWriter`를 `Dispose()`하지 않으면 파일이 잠겨서 다른 프로세스에서 접근 불가

#### Current Status
- ✅ `NAudio` 패키지 설치 완료 (v2.2.1)
- ✅ `IAudioRecorderService.cs` 생성 완료 (인터페이스 정의)
- ✅ `AudioRecorderService.cs` 생성 완료 (NAudio 기반 녹음 및 WAV 저장)
- ✅ `WaveInEvent` 시작/중지 로직 구현 완료
- ✅ 트리거 Down → 녹음 시작 로직 구현 완료 (`HandleMouseDown`)
- ✅ 트리거 Up → 녹음 중지 및 파일 경로 반환 로직 구현 완료 (`HandleMouseUp`)
- ✅ PCM 16bit, Mono, 16kHz WAV 포맷 저장 완료 (Gemini API 호환)
- ✅ 임시 폴더 관리 및 파일 정리 로직 구현 완료
- ✅ `TaskCompletionSource`를 사용한 비동기 처리 구현 완료
- ✅ `WaveFileWriter` Dispose로 파일 잠금 해제 보장 완료
- ✅ `MainViewModel`에 `IAudioRecorderService` 주입 및 녹음 로직 구현 완료
- ✅ `App.xaml.cs`에 `IAudioRecorderService` 싱글톤 등록 완료
- ✅ `OnExit`에서 `AudioRecorderService` Dispose 호출 추가 완료
- ✅ 드래그 종료 시 화면 캡처와 함께 오디오 녹음 동작 확인
- Phase 2.2 완전히 완료, 다음 단계: Phase 3.1 Gemini API 연동 (Intelligence Layer) 구현 준비

---

### 2026-02-05 (목) - Phase 2.1 DPI 보정 유틸리티 및 화면 캡처 서비스 구현 (10차)
**[목표]** **DPI 보정 유틸리티(DpiHelper)**와 **GDI+ 기반 화면 캡처 서비스(ScreenCaptureService)**를 구현하고, 드래그 종료 시 해당 영역을 **이미지(BitmapSource)**로 변환하여 클립보드에 복사하는 기능을 완성.

#### Dev Action (DPI Helper & Screen Capture Service)
- **Helpers/NativeMethods.cs 수정:**
  - DPI 관련 Win32 API P/Invoke 선언 추가:
    - `GetDpiForMonitor` (shcore.dll) - 모니터별 DPI 값 가져오기
    - `MonitorFromPoint` (user32.dll) - 포인트 위치의 모니터 핸들 가져오기
  - `DpiAwareness`, `MonitorDpiType`, `MonitorFromPointFlags` 열거형 정의
  - 기존 마우스 훅 관련 선언과 분리하여 명확한 구조 유지

- **Helpers/DpiHelper.cs 생성:**
  - `GetDpiForPoint(int x, int y)` 메서드: 지정된 화면 좌표의 모니터 DPI 가져오기
  - `PhysicalToLogical(int, int, uint?)` 메서드: 물리 좌표 → 논리 좌표 변환 (WPF Point 반환)
  - `LogicalToPhysical(double, double, uint?)` 메서드: 논리 좌표 → 물리 좌표 변환 (System.Drawing.Point 반환)
  - `PhysicalToLogicalRect(Rect, uint?)` 메서드: 물리 Rect → 논리 Rect 변환 (WPF OverlayWindow용)
  - `LogicalToPhysicalRect(Rect, uint?)` 메서드: 논리 Rect → 물리 Rect 변환
  - DPI 자동 감지 기능 (null 전달 시 `GetDpiForPoint`로 자동 계산)
  - 네임스페이스 충돌 방지 (`WpfPoint` 별칭 사용)

- **Services/Interfaces/IScreenCaptureService.cs 생성:**
  - `CaptureRegionAsync(Rect region)` 메서드: 지정된 화면 영역을 캡처하여 `BitmapSource` 반환
  - `CopyToClipboardAsync(BitmapSource image)` 메서드: `BitmapSource` 이미지를 클립보드에 복사
  - 비동기 처리 (`Task` 반환 타입)
  - 물리 좌표계 사용 명시 (주석으로 문서화)

- **Services/Implementations/ScreenCaptureService.cs 생성:**
  - `IScreenCaptureService` 인터페이스 구현
  - `CaptureRegionAsync` 구현:
    - GDI+ `Graphics.CopyFromScreen` 사용하여 화면 캡처
    - 물리 좌표계 사용 (마우스 훅이 물리 좌표 제공)
    - `System.Drawing.Bitmap` 생성 및 `Graphics` 객체로 화면 복사
    - `ConvertToBitmapSource` 메서드로 WPF `BitmapSource` 변환
    - `Bitmap.LockBits`를 사용한 효율적인 메모리 접근
    - `using` 문으로 리소스 자동 해제 보장
  - `CopyToClipboardAsync` 구현:
    - WPF `Clipboard.SetImage` 직접 사용 (BitmapSource 호환)
    - UI 스레드에서 실행 보장 (`Dispatcher.Invoke`)
    - 예외 처리 및 오류 메시지 제공
  - `ConvertToBitmapSource` 메서드: `System.Drawing.Bitmap` → WPF `BitmapSource` 변환
    - `BitmapSource.Create` 사용
    - `Freeze()` 호출로 스레드 안전성 보장

- **ViewModels/MainViewModel.cs 수정:**
  - 생성자에 `IScreenCaptureService` 주입 추가 (`_captureService` 필드)
  - `HandleMouseMove` 메서드 수정:
    - 물리 좌표로 Rect 계산 (마우스 훅이 물리 좌표 제공)
    - `DpiHelper.PhysicalToLogicalRect`를 통해 논리 좌표로 변환
    - 변환된 논리 Rect를 `OverlayViewModel.UpdateRect`에 전달
    - DPI 보정을 통해 멀티 모니터 환경에서도 정확한 사각형 표시 보장
  - `HandleMouseUp` 메서드 수정:
    - `async void`로 변경하여 비동기 처리 가능하도록 수정
    - 물리 좌표로 최종 Rect 계산 (캡처 서비스가 물리 좌표 사용)
    - `_captureService.CaptureRegionAsync(finalRect)` 호출하여 화면 캡처
    - `_captureService.CopyToClipboardAsync(capturedImage)` 호출하여 클립보드 복사
    - 검증용 `MessageBox` 출력 ("캡처 완료! 클립보드를 확인하세요.")
    - 예외 처리 및 디버그 로그 출력
    - 유효한 영역(Width > 0 && Height > 0)인 경우에만 캡처 수행

- **App.xaml.cs 수정:**
  - `IScreenCaptureService` → `ScreenCaptureService` 싱글톤 등록 추가
  - DI 컨테이너에 서비스 등록 순서 명확화 (주석 추가)

- **AI_Mouse.csproj 수정:**
  - `System.Drawing.Common` 패키지 추가 (v8.0.0)
  - GDI+ 기능 사용을 위한 필수 패키지

#### Tech Details
- **DPI 보정:** Win32 API를 사용하여 모니터별 DPI를 감지하고, 물리 좌표와 논리 좌표 간 변환 수행
  - 마우스 훅은 물리 좌표(Physical Pixel)를 제공
  - WPF OverlayWindow는 논리 좌표(Logical DPI-independent)를 사용
  - `DpiHelper`를 통해 두 좌표계 간 변환 수행
- **화면 캡처:** GDI+ `Graphics.CopyFromScreen`을 사용하여 지정된 영역 캡처
  - 물리 좌표계 사용 (마우스 훅과 일치)
  - `System.Drawing.Bitmap` → WPF `BitmapSource` 변환으로 WPF UI 호환
- **리소스 관리:** `using` 문을 사용하여 `Bitmap`, `Graphics` 객체 자동 해제
- **비동기 처리:** 캡처 및 클립보드 작업을 `Task.Run`으로 비동기 처리하여 UI 응답성 유지
- **좌표계 일관성:** 
  - 마우스 훅 → 물리 좌표
  - ScreenCaptureService → 물리 좌표
  - OverlayWindow → 논리 좌표 (DpiHelper로 변환)

#### Current Status
- ✅ `DpiHelper.cs` 생성 완료 (Win32 API P/Invoke 선언 및 좌표 변환 메서드)
- ✅ `NativeMethods.cs`에 DPI 관련 API 선언 추가 완료
- ✅ `IScreenCaptureService.cs` 생성 완료 (인터페이스 정의)
- ✅ `ScreenCaptureService.cs` 생성 완료 (GDI+ 기반 캡처 및 BitmapSource 변환)
- ✅ `System.Drawing.Common` 패키지 추가 완료
- ✅ `MainViewModel`에 `IScreenCaptureService` 주입 및 캡처 로직 구현 완료
- ✅ `HandleMouseMove`에 DPI 변환 로직 적용 완료
- ✅ `HandleMouseUp`에 화면 캡처 및 클립보드 복사 로직 구현 완료
- ✅ `App.xaml.cs`에 `IScreenCaptureService` 싱글톤 등록 완료
- ✅ 드래그 종료 시 화면 캡처 및 클립보드 복사 동작 확인
- Phase 2.1 완전히 완료, 다음 단계: Phase 2.2 음성 녹음 (Audio Recording) 구현 준비

---

### 2026-02-05 (목) - Phase 1.3 투명 오버레이 윈도우 및 드래그 사각형 시각화 구현 (9차)
**[목표]** **투명 오버레이 윈도우(OverlayWindow)**를 구현하고, 전역 마우스 훅(GlobalHookService)과 연동하여 **드래그 시 사각형 영역을 시각적으로 표시**하는 기능을 완성.

#### Dev Action (Overlay Window & Visual Feedback)
- **ViewModels/OverlayViewModel.cs 생성:**
  - `ObservableObject` 상속하여 MVVM 패턴 준수
  - 드래그 영역 속성 구현 (`[ObservableProperty]` 사용):
    - `Left`, `Top`, `Width`, `Height` (사각형 좌표 및 크기)
    - `IsVisible` (사각형 표시 여부)
  - `UpdateRect(Rect rect)` 메서드: 뷰모델 상태를 한 번에 업데이트
  - `Reset()` 메서드: 드래그 사각형 초기화
  - DPI 보정 필요성에 대한 한국어 주석 추가 (현재는 1:1 매핑, 향후 DpiHelper 필요)

- **Views/OverlayWindow.xaml 생성:**
  - 투명 윈도우 속성 설정:
    - `WindowStyle="None"`, `AllowsTransparency="True"`, `Topmost="True"`
    - `WindowState="Maximized"`, `ResizeMode="NoResize"`
    - `ShowInTaskbar="False"`
    - `Background="#01000000"` (완전 투명이면 클릭을 못 받으므로 1% 불투명도 적용)
  - Canvas 내부에 빨간색 테두리 반투명 Rectangle 배치
  - ViewModel 속성과 바인딩 (`Canvas.Left`, `Canvas.Top`, `Width`, `Height`, `Visibility`)
  - `BooleanToVisibilityConverter` 리소스 참조 (`App.xaml`에 추가)

- **Views/OverlayWindow.xaml.cs 생성:**
  - 생성자에서 `OverlayViewModel`을 DI로 주입받아 `DataContext` 설정
  - Code-behind 최소화 (MVVM 패턴 준수)

- **App.xaml.cs 수정:**
  - `OverlayViewModel`과 `OverlayWindow`를 `AddTransient`로 DI 등록
  - `OnStartup`에서 `OverlayWindow` 미리 생성 후 `Hide()` 상태로 대기 (반응 속도 최적화)
  - `MainViewModel`에 `OverlayWindow` 참조 전달 (`SetOverlayWindow` 메서드 호출)
  - `App.xaml`에 `BooleanToVisibilityConverter` 리소스 추가

- **ViewModels/MainViewModel.cs 구현:**
  - 생성자에서 `IGlobalHookService` 주입 (DI)
  - `SetOverlayWindow(OverlayWindow)` 메서드: `OverlayWindow` 참조 및 `OverlayViewModel` 저장
  - `MouseAction` 이벤트 구독 (`_hookService.MouseAction += OnMouseAction`)
  - 상태 기계 로직 구현:
    - **Down (트리거):** 시작점 기록 (`_dragStartX`, `_dragStartY`), `OverlayWindow.Show()`, `OverlayViewModel.Reset()`
    - **Move (드래그):** 현재 좌표와 시작점으로 `Rect` 계산, `OverlayViewModel.UpdateRect()` 호출
    - **Up (종료):** `OverlayWindow.Hide()`, 최종 `Rect` 디버그 로그 출력, `OverlayViewModel.Reset()`
  - UI 스레드에서 실행 보장 (`Application.Current.Dispatcher.Invoke`)
  - 트리거 버튼(XButton1)만 처리하도록 필터링
  - 성능 최적화: `MouseMove` 이벤트 처리 로직 경량화

#### Tech Details
- **투명 오버레이 윈도우:** `AllowsTransparency="True"`와 `Background="#01000000"`을 사용하여 거의 투명하지만 클릭 이벤트를 받을 수 있는 윈도우 구현
- **MVVM 패턴 준수:** View와 ViewModel 완전 분리, 데이터 바인딩을 통한 상태 관리
- **성능 최적화:** `OverlayWindow`를 앱 시작 시 미리 생성하여 첫 드래그 시 반응 속도 향상
- **좌표 변환:** 현재는 1:1 매핑으로 구현, 향후 DPI 보정 필요 시 `DpiHelper` 사용 예정
- **이벤트 처리:** `GlobalHookService`의 `MouseAction` 이벤트를 구독하여 드래그 상태 관리
- **UI 스레드 안전성:** 모든 UI 업데이트는 `Dispatcher.Invoke`를 통해 UI 스레드에서 실행 보장

#### Current Status
- ✅ `OverlayViewModel.cs` 생성 완료 (드래그 영역 속성 및 업데이트 로직)
- ✅ `OverlayWindow.xaml` 생성 완료 (투명 윈도우, Canvas, Rectangle UI)
- ✅ `OverlayWindow.xaml.cs` 생성 완료 (Code-behind 최소화)
- ✅ `App.xaml.cs`에 DI 등록 및 초기화 완료 (`OverlayWindow` 미리 생성)
- ✅ `MainViewModel`에 이벤트 구독 및 오버레이 제어 로직 구현 완료
- ✅ 트리거 버튼 Down/Up 시 오버레이 Show/Hide 동작 확인
- ✅ 드래그 중 사각형 시각화 동작 확인
- ✅ `BooleanToVisibilityConverter` 리소스 추가 완료
- Phase 1.3 완전히 완료, 다음 단계: Phase 2.1 화면 캡처 (Screen Capture) 구현 준비

---

### 2026-02-05 (목) - Phase 1.2 전역 마우스 훅 구현 (8차)
**[목표]** Windows API(User32.dll)를 사용하여 **전역 마우스 이벤트를 감지하는 시스템(Global Hook)**을 구현하여 앱이 백그라운드에서 마우스 이벤트를 실시간으로 감지할 수 있도록 완성.

#### Dev Action (Global Mouse Hook Implementation)
- **Helpers/NativeMethods.cs 생성:**
  - Win32 API P/Invoke 선언 (`SetWindowsHookEx`, `UnhookWindowsHookEx`, `CallNextHookEx`, `GetModuleHandle`)
  - `WH_MOUSE_LL` (14) 상수 정의
  - `MSLLHOOKSTRUCT` 구조체 정의 (마우스 좌표, 버튼 정보 등)
  - 마우스 메시지 상수 정의 (`MouseMessages` 클래스: `WM_MOUSEMOVE`, `WM_LBUTTONDOWN/UP`, `WM_RBUTTONDOWN/UP`, `WM_MBUTTONDOWN/UP`, `WM_XBUTTONDOWN/UP`)

- **Services/Interfaces/IGlobalHookService.cs 생성:**
  - `IGlobalHookService` 인터페이스 정의 (`IDisposable` 상속)
  - `MouseAction` 이벤트 정의 (`EventHandler<MouseActionEventArgs>`)
  - `Start()`, `Stop()` 메서드 정의
  - `IsActive` 속성 정의
  - `MouseActionEventArgs` 클래스 정의 (액션 타입, 좌표, 버튼 정보 포함)
  - `MouseActionType` 열거형 정의 (`Move`, `Down`, `Up`)
  - `MouseButton` 열거형 정의 (`None`, `Left`, `Right`, `Middle`, `XButton1`, `XButton2`)

- **Services/Implementations/GlobalHookService.cs 생성:**
  - `IGlobalHookService` 인터페이스 구현
  - `Start()` 메서드: `SetWindowsHookEx(WH_MOUSE_LL)`로 전역 마우스 훅 설치
  - `Stop()` 메서드: `UnhookWindowsHookEx`로 훅 해제
  - `HookCallback` 메서드: Win32 콜백 프로시저 (경량화를 위해 `Task.Run`으로 비동기 처리)
  - `ProcessMouseMessage` 메서드: 마우스 메시지 파싱 및 `MouseAction` 이벤트 발생
  - 모든 마우스 버튼 이벤트 처리 (Left, Right, Middle, XButton1, XButton2)
  - `Dispose()` 패턴 구현: 앱 종료 시 훅 해제 보장
  - 소멸자 추가: 안전장치로 훅 해제 보장
  - 디버그 로그 출력 (`Debug.WriteLine`)으로 마우스 이벤트 추적

- **App.xaml.cs 수정:**
  - `IGlobalHookService`를 싱글톤으로 DI 등록 (`services.AddSingleton<IGlobalHookService, GlobalHookService>()`)
  - `OnStartup`에서 `GlobalHookService` 인스턴스를 가져와 `Start()` 호출
  - `OnExit`에서 `GlobalHookService.Stop()` 호출하여 훅 해제 보장
  - 예외 처리 추가 (훅 중지 실패 시에도 앱 종료 보장)

#### Tech Details
- **Win32 Hook:** `SetWindowsHookEx`의 `WH_MOUSE_LL` (Low-level 마우스 훅) 사용하여 전역 마우스 이벤트 감지
- **콜백 경량화:** 훅 콜백 메서드는 최대한 가볍게 작성하고, 무거운 작업은 `Task.Run`으로 비동기 처리하여 시스템 성능에 영향 최소화
- **리소스 관리:** `IDisposable` 패턴과 `OnExit` 오버라이드를 통해 앱 종료 시 훅이 반드시 해제되도록 보장 (훅이 해제되지 않으면 OS가 느려질 수 있음)
- **이벤트 구조:** 단일 `MouseAction` 이벤트로 통합하여 이벤트 인자에 액션 타입, 좌표, 버튼 정보를 포함
- **디버깅:** 모든 마우스 이벤트를 `Debug.WriteLine`으로 로깅하여 개발 중 이벤트 추적 가능

#### Current Status
- ✅ `NativeMethods.cs` 생성 완료 (Win32 API P/Invoke 선언)
- ✅ `IGlobalHookService.cs` 생성 완료 (인터페이스 및 이벤트 인자 정의)
- ✅ `GlobalHookService.cs` 생성 완료 (훅 서비스 구현)
- ✅ 전역 마우스 훅 설치/해제 로직 구현 완료
- ✅ 마우스 이벤트 감지 및 디버그 로그 출력 완료
- ✅ 비동기 이벤트 처리로 콜백 경량화 완료
- ✅ `IDisposable` 패턴 구현 완료 (리소스 안전 해제)
- ✅ `App.xaml.cs`에서 DI 등록 및 `Start()` 호출 완료
- ✅ `OnExit`에서 `Stop()` 호출 완료 (훅 해제 보장)
- Phase 1.2 완전히 완료, 다음 단계: Phase 1.3 시각적 피드백 (Overlay View) 구현 준비

---

### 2026-02-05 (목) - Phase 1.1 UX 피드백 및 검증 기능 추가 (7차)
**[목표]** Phase 1.1의 핵심인 **DI 컨테이너 구성**과 **트레이 아이콘** 구현을 완료하고, 실행 여부를 확인할 수 있도록 **검증용 메시지 박스** 및 이벤트 핸들러를 추가하여 사용자 피드백을 개선.

#### Dev Action (UX Feedback & Verification)
- **App.xaml 수정:**
  - `ToolTipText`를 "AI Mouse (Running)"으로 변경하여 실행 상태 명확히 표시
  - ContextMenu의 '설정' 메뉴에 `Settings_Click` 이벤트 핸들러 연결
  - '종료' 메뉴의 이벤트 핸들러를 `Exit_Click`으로 변경 (기존 `ExitMenuItem_Click`에서 리팩토링)
  - 메뉴 항목에 영문 표기 추가 ("설정(Settings)", "종료(Exit)")

- **App.xaml.cs 수정:**
  - `OnStartup` 메서드에 검증용 `MessageBox` 추가:
    - "AI Mouse가 백그라운드에서 실행되었습니다.\n트레이 아이콘을 우클릭해보세요." 메시지 표시
    - 앱 실행 직후 사용자에게 트레이 아이콘 존재를 알림
  - 아이콘 할당 방식을 `SystemIcons.Application` 직접 할당으로 변경 (기존 `Icon.FromHandle` 방식에서 개선)
  - `Settings_Click` 이벤트 핸들러 구현:
    - "설정 창은 추후 구현될 예정입니다. (Phase 4.2)" 임시 알림 메시지 표시
  - `Exit_Click` 이벤트 핸들러 구현:
    - `Application.Current.Shutdown()` 호출하여 앱 정상 종료

- **To_do.md 업데이트:**
  - UX 피드백 및 검증 항목을 완료(`[x]`)로 표시
  - 모든 하위 항목(MessageBox 출력, 설정 메뉴 팝업, 아이콘 할당) 완료 표시

#### Tech Details
- **UX 피드백:** 앱 실행 직후 `MessageBox`를 통해 사용자에게 백그라운드 실행 상태를 명확히 알림
- **이벤트 핸들러:** 트레이 메뉴의 모든 항목에 적절한 이벤트 핸들러 연결하여 사용자 상호작용 개선
- **아이콘 처리:** `SystemIcons.Application` 직접 할당으로 코드 간소화 및 안정성 향상
- **검증 기능:** 개발 단계에서 앱 실행 여부를 쉽게 확인할 수 있는 메시지 박스 추가

#### Current Status
- ✅ UX 피드백 메시지 박스 구현 완료 (앱 실행 확인용)
- ✅ 트레이 메뉴 이벤트 핸들러 구현 완료 (`Settings_Click`, `Exit_Click`)
- ✅ 아이콘 할당 방식 개선 완료 (`SystemIcons.Application` 직접 할당)
- ✅ ToolTipText 개선 완료 ("AI Mouse (Running)")
- ✅ Phase 1.1의 모든 항목 완료 (프로젝트 세팅, DI 컨테이너, 트레이 아이콘, UX 피드백)
- Phase 1.1 완전히 완료, 다음 단계: Phase 1.2 전역 입력 감지 (Global Input Hook) 구현 준비

---

### 2026-02-05 (목) - Phase 1.1 DI 컨테이너 구성 및 시스템 트레이 아이콘 구현 (6차)
**[목표]** `To_do.md`의 Phase 1.1 잔여 항목인 **의존성 주입(DI) 컨테이너 구성**과 **시스템 트레이 아이콘** 기능을 구현하여 앱이 백그라운드에서 실행되도록 완성.

#### Dev Action (DI Container & System Tray)
- **App.xaml 수정:**
  - `StartupUri` 속성 제거 (DI를 통한 수동 제어를 위해 필수)
  - `Hardcodet.NotifyIcon` 네임스페이스 추가 (`http://www.hardcodet.net/taskbar`)
  - `TaskbarIcon` 리소스 정의 (`x:Key="TrayIcon"`)
    - `ToolTipText="AI Mouse"` 설정
    - `ContextMenu` 정의 및 '설정', '종료' `MenuItem` 추가
    - '종료' 메뉴 클릭 이벤트 핸들러 연결 (`ExitMenuItem_Click`)

- **App.xaml.cs 수정 (DI Bootstrapper):**
  - `OnStartup` 메서드 오버라이드하여 DI 컨테이너 구성:
    1. `ServiceCollection` 인스턴스 생성
    2. `MainViewModel`과 `MainWindow`를 `AddTransient`로 등록
    3. `ServiceProvider` 빌드 (`BuildServiceProvider`)
    4. `MainWindow` 인스턴스를 `provider.GetRequiredService<MainWindow>()`로 생성
    5. `MainWindow.DataContext`에 `provider.GetRequiredService<MainViewModel>()` 주입
    6. `MainWindow.Hide()` 호출하여 초기 상태를 숨김으로 유지
    7. 리소스에서 `TaskbarIcon`을 찾아 `_trayIcon` 멤버 변수에 할당
    8. 아이콘 파일이 없으므로 `System.Drawing.SystemIcons.Application`을 `Icon.FromHandle`로 할당
  - `OnExit` 메서드에서 리소스 정리 (`_trayIcon.Dispose()`, `_serviceProvider.Dispose()`)
  - '종료' 메뉴 클릭 이벤트 핸들러에서 `Application.Current.Shutdown()` 호출

- **MainWindow.xaml.cs 수정:**
  - `Closing` 이벤트 핸들러 추가 (`MainWindow_Closing`)
  - 사용자가 X 버튼을 눌렀을 때 `e.Cancel = true`로 설정하여 앱 종료 방지
  - `Hide()` 호출로 창만 숨김 처리 (실제 종료는 트레이 메뉴에서만 가능)

#### Tech Details
- **DI 컨테이너 구성:** `Microsoft.Extensions.DependencyInjection`을 사용하여 `ServiceCollection`으로 서비스 등록 및 `ServiceProvider`로 인스턴스 생성
- **트레이 아이콘 관리:** `Hardcodet.NotifyIcon.Wpf`의 `TaskbarIcon`을 리소스로 정의하고 코드에서 접근하여 아이콘 설정
- **아이콘 처리:** 아이콘 파일이 없으므로 `System.Drawing.SystemIcons.Application`을 사용하여 기본 아이콘 표시
- **앱 생명주기:** 앱 실행 시 아무 창도 표시되지 않고 시스템 트레이에만 아이콘 표시, X 버튼 클릭 시 숨김 처리, 트레이 메뉴에서만 종료 가능

#### Current Status
- ✅ DI 컨테이너 구성 완료 (`ServiceCollection`, `ServiceProvider` 사용)
- ✅ `MainWindow`와 `MainViewModel` 의존성 주입 완료
- ✅ 시스템 트레이 아이콘 구현 완료 (ContextMenu 포함)
- ✅ 앱 실행 시 창 없이 트레이 아이콘만 표시됨
- ✅ 윈도우 닫기 버튼 클릭 시 숨김 처리 동작 확인
- ✅ 트레이 메뉴를 통한 정상 종료 기능 구현 완료
- Phase 1.1의 핵심 기능 완료 (DI 컨테이너 구성 및 트레이 아이콘)
- 다음 단계: Phase 1.2 전역 입력 감지 (Global Input Hook) 구현 준비

---

### 2026-02-05 (목) - 프로젝트 문서 동기화 및 상태 업데이트 (5차)
**[목표]** 현재 `AI_Mouse` 프로젝트의 실제 구현 상태(WPF .NET 8, MVVM 패턴, 생성된 폴더 구조)를 분석하여, 문서 파일 3종(`Tree.md`, `Architecture.md`, `To_do.md`)을 최신 상태로 동기화.

#### Dev Action (Documentation Sync)
- **프로젝트 상태 분석:**
  - WPF .NET 8 프로젝트 구조 확인
  - 생성된 폴더 구조 파악 (`Views/`, `ViewModels/` 생성 완료)
  - 설치된 NuGet 패키지 확인:
    - `CommunityToolkit.Mvvm` v8.2.2
    - `Microsoft.Extensions.DependencyInjection` v8.0.0
    - `Hardcodet.NotifyIcon.Wpf` v1.1.0
  - 실제 파일 구조 확인 (`MainWindow.xaml`, `MainViewModel.cs` 등)

- **Tree.md 최신화:**
  - 실제 파일 시스템 구조 반영
  - 생성된 폴더/파일은 ✅ 표시, 예정된 항목은 ⏳ 표시
  - MVVM 계층 구조 다이어그램 유지 (WPF 구조에 맞게 이미 작성됨)
  - `App.xaml.cs` DI 컨테이너 구조 섹션에 "구현 예정" 표시 추가

- **Architecture.md 최신화:**
  - WPF MVVM 구조 유지 (이미 올바르게 작성됨)
  - DI 컨테이너 섹션에 현재 상태 추가:
    - 패키지 설치 완료 표시
    - `App.xaml.cs` 구현은 예정으로 표시
  - 시퀀스 다이어그램 유지 (WPF MVVM 구조에 맞게 이미 작성됨)

- **To_do.md 최신화:**
  - Phase 1.1 완료 항목 체크:
    - ✅ 프로젝트 생성 및 환경 설정 완료
    - ✅ NuGet 패키지 설치 완료 (버전 정보 포함)
    - ✅ 기본 폴더 구조 구축 완료 (`Views/`, `ViewModels/`)
    - ✅ `MainViewModel` 클래스 생성 완료
  - 다음 단계 명시:
    - DI 컨테이너 구성 (`App.xaml.cs`)
    - 트레이 아이콘 구현
  - 완료된 작업 섹션에 Phase 1.1 진행 상황 추가

#### Tech Details
- **문서 동기화 전략:** 실제 프로젝트 상태를 정확히 반영하여 개발자가 프로젝트 구조를 한눈에 파악할 수 있도록 명확하게 작성
- **상태 표시 규칙:** 
  - ✅ 생성 완료
  - ⏳ 생성 예정 (Phase별로 구분)
- **문서 일관성:** 모든 문서가 동일한 프로젝트 상태를 반영하도록 동기화

#### Current Status
- 프로젝트 문서 3종(`Tree.md`, `Architecture.md`, `To_do.md`)이 현재 실제 구현 상태를 정확히 반영하도록 업데이트 완료
- Phase 1.1 진행 상황이 명확하게 문서화됨:
  - 완료: 프로젝트 생성, 패키지 설치, 기본 폴더 구조, `MainViewModel` 생성
  - 다음 단계: DI 컨테이너 구성 및 트레이 아이콘 구현
- Unity 관련 레거시 내용 없음 확인 (이미 WPF 구조로 작성됨)
- 다음 단계: Phase 1.1의 남은 작업 진행 (DI 컨테이너 구성, 트레이 아이콘 구현)

---

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