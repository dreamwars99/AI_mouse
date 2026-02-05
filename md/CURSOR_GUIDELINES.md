# 🐭 AI Mouse: AI Coding Guidelines
이 문서는 'AI Mouse (WPF Desktop Utility)' 개발 시 Cursor AI가 반드시 준수해야 할 **절대 원칙(Core Principles)**과 **작업 규칙(Operational Rules)**을 정의합니다.

## 🛡️ 1. 3대 절대 원칙 (The Golden Rules)

### 1.1. Architecture Compliance (MVVM & DI 준수)
본 프로젝트는 Model - View - ViewModel (MVVM) 패턴과 **의존성 주입(Dependency Injection)**을 엄격히 따릅니다.

| 계층 | 역할 | 규칙 | 예시 |
| :--- | :--- | :--- | :--- |
| **View (UI)** | 화면 표시 | 로직 작성 금지 (Code-behind 최소화) | `MainWindow.xaml`, `OverlayWindow.xaml` |
| **ViewModel** | 상태 및 로직 | UI와 데이터 바인딩, 명령 처리 | `MainViewModel.cs` |
| **Service** | 핵심 기능 수행 | 비즈니스 로직, I/O, API 통신 | `GlobalHookService`, `GeminiService` |
| **Model** | 데이터 구조 | 순수 데이터 객체 (DTO) | `ChatResponse.cs` |

**⚠️ 절대 금지:**
- `x:Name`을 사용하여 Code-behind(.xaml.cs)에서 UI 컨트롤을 직접 제어하지 마십시오. (단, 윈도우 이동/종료 등 순수 View 로직 제외)
- 모든 로직은 ViewModel의 Command와 Data Binding으로 처리하십시오.

### 1.2. Non-Blocking UI (UI 응답성 보장)
WPF 애플리케이션, 특히 시스템 트레이에 상주하는 유틸리티는 절대 멈추면 안 됩니다.

**원칙:**
- 모든 I/O 작업(파일 저장, 네트워크 통신)은 비동기(`async`/`await`)로 처리하십시오.
- 무거운 연산(이미지 처리, 오디오 인코딩)은 `Task.Run()`을 사용하여 백그라운드 스레드에서 수행하십시오.
- **Hook Callback 경량화:** `GlobalHookService`의 콜백 메서드는 최소한의 시간 내에 리턴해야 합니다. 로직이 길어지면 비동기로 이벤트를 던지고 즉시 리턴하십시오.

### 1.3. Resource Safety (리소스 안전성)
시스템 레벨(Win32 API, GDI+)을 다루므로 메모리 누수 방지가 필수적입니다.

**원칙:**
- **IDisposable 준수:** `Bitmap`, `Graphics`, `MemoryStream`, `WaveInEvent` 등 사용 후 반드시 `Dispose()` 또는 `using` 문을 사용하십시오.
- **Hook 해제:** 앱 종료 시 `UnhookWindowsHookEx`가 반드시 호출되어야 합니다. 그렇지 않으면 OS 전체의 마우스 입력이 먹통될 수 있습니다.

---

## 🛠️ 2. 작업 세부 규칙 (Operational Rules)

### 2.1. Toolkit Usage (라이브러리 사용 규칙)

**CommunityToolkit.Mvvm**
보일러플레이트 코드를 줄이기 위해 툴킷 기능을 적극 활용하십시오.
- `INotifyPropertyChanged` 수동 구현 금지.
- **ObservableProperty:** `[ObservableProperty]` 속성을 사용하여 필드를 선언하십시오.
- **RelayCommand:** `[RelayCommand]` 속성을 사용하여 메서드를 커맨드로 노출하십시오.

```csharp
// ✅ 좋은 예
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage;

    [RelayCommand]
    private async Task CaptureAndQueryAsync() { ... }
}
**P/Invoke & Win32**
- `User32.dll`, `Gdi32.dll` 등 외부 API 호출 시 `LibraryImport` 또는 `DllImport`를 사용하되, 적절한 마샬링(Marshaling) 속성을 지정하십시오.
- Win32 API 관련 코드는 `NativeMethods` 클래스나 별도 `Interop` 네임스페이스로 격리하십시오.

### 2.2. UI/UX Implementation (XAML 규칙)
- **Layout:** 절대 좌표(Canvas + Margin)보다는 `Grid`, `StackPanel`, `DockPanel`을 사용하여 창 크기 변경에 대응하십시오. (오버레이의 드래그 사각형 그리기는 예외적으로 `Canvas` 사용 허용)
- **Style:** 색상, 폰트 크기 등은 `App.xaml`의 `Resources`에 정의하여 재사용성을 높이십시오.
- **Resources:** 이미지는 **Build Action: Resource**로 설정하고 URI Pack 구문(`pack://application:,,,/`)을 사용하십시오.

### 2.3. Project Structure (폴더 구조)
```text
AI_Mouse/
├── App.xaml.cs              # DI 컨테이너 구성 (Startup)
├── Views/                   # .xaml 및 .xaml.cs
├── ViewModels/              # ObservableObject 상속 클래스
├── Services/
│   ├── Interfaces/          # IGlobalHookService, IGeminiService 등
│   └── Implementations/     # 실제 구현체
├── Models/                  # 데이터 클래스
├── Helpers/                 # Interop, Converters, Extensions
└── Resources/               # 아이콘, 이미지 에셋