# [기획서] AI 마우스: 음성 기반 화면 질의 도구

## 1. 프로젝트 개요 (Overview)
**프로젝트명:** AI Mouse (가칭)

**목표:** 별도의 타이핑이나 복잡한 캡처 도구 실행 없이, **"드래그 + 음성"**의 단일 동작만으로 화면의 특정 영역에 대해 AI에게 질문하고 답변을 얻는 윈도우 유틸리티 개발.

**핵심 가치:** 속도와 직관성. 사용자의 작업 흐름(Context)을 끊지 않고 즉각적인 정보 획득.

**타겟 플랫폼:** Windows 10/11 Desktop (WPF Application).

## 2. 시스템 아키텍처 (System Architecture)

### 2.1 기술 스택 (Tech Stack)
문서의 추천 스택인 Windows 데스크탑 제어에 최적화된 구성을 따릅니다.

* **Framework:** .NET 8 (C#)
* **UI Engine:** WPF (Windows Presentation Foundation) - 투명 오버레이 구현 용이
* **Audio:** NAudio (마이크 녹음 및 WAV 저장)
* **AI Engine:** Google Gemini API (Google.GenAI SDK 사용)

**전략:** 멀티모달(이미지+오디오) 입력을 직접 지원하므로, MVP 단계에서는 별도의 로컬 STT 없이 Gemini에게 원본 오디오와 이미지를 전송하여 처리 속도 최적화. (문서의 Phase 6 방식 채택)

### 2.2 핵심 구성 요소 (Components)
* **Input Listener (Global Hook):** 마우스/키보드 입력을 전역에서 감지하여 트리거 신호 생성.
* **Capture Overlay (View):** 전체 화면을 덮는 투명 윈도우로, 드래그 영역 시각화 담당.
* **Media Manager (Controller):** 스크린샷 캡처(GDI+) 및 오디오 녹음(NAudio) 제어.
* **AI Client (Service):** Gemini API와의 통신 및 세션 관리.
* **Result Viewer (View):** AI의 응답을 표시하는 채팅형 UI.

## 3. 기능 명세 (Functional Specifications)

### 3.1 트리거 및 입력 (Input Layer)
* **발동 조건:** 사용자가 설정한 '트리거 버튼'을 누르고 있는 상태(Down).
* **기본값:** 마우스 측면 버튼 (XButton1/2) 또는 F13 등 사용하지 않는 키 매핑.
* **대안:** 마우스에 측면 버튼이 없는 경우를 대비해 `Ctrl` + 마우스 좌클릭 지원 고려.
* **상태 전이:**
    * Button Down → 오버레이 활성화 + 녹음 시작.
    * Mouse Move → 캡처 영역(사각형) 그리기.
    * Button Up → 캡처 확정 + 녹음 종료 + 처리 시작.

### 3.2 캡처 및 녹음 (Capture & Audio Layer)
* **화면 캡처:**
    * 자체 캡처 UI 사용 (Windows 기본 캡처 X).
    * Button Up 순간의 드래그 영역 좌표를 기준으로 비트맵(PNG) 생성.
    * **편의 기능:** 캡처된 이미지는 자동으로 시스템 클립보드에도 저장.
* **음성 녹음:**
    * 트리거 시작 시점부터 종료 시점까지의 오디오를 메모리 스트림 또는 임시 파일(WAV)로 저장.
    * **Format:** Gemini API 호환성을 위해 PCM (16bit, Mono 권장) 포맷 사용.

### 3.3 AI 처리 및 전송 (Processing Layer)
* **API 요청 구성:**
    * 입력 1: 캡처된 이미지 (MIME: `image/png`).
    * 입력 2: 녹음된 오디오 (MIME: `audio/wav`).
    * 시스템 프롬프트: "사용자가 제공한 이미지와 오디오 질문을 바탕으로 명확하게 답변하라.".
* **전송 방식:** .NET용 Google Generative AI SDK를 사용하여 비동기 전송.

### 3.4 결과 표시 (UX/UI Layer)
* **Tray Icon:** 앱의 백그라운드 상주 상태 표시 (Idle/Processing 등 상태 아이콘 변경).
* **Chat Window:**
    * 응답 수신 시 마우스 커서 근처 또는 우측 하단에 대화창 팝업.
    * 마크다운(Markdown) 렌더링 지원 (코드 블럭, 볼드체 등 표현).
    * **옵션:** '닫기' 버튼을 누르기 전까지 창 유지 (멀티턴 대화 가능성 열어둠).

## 4. 사용자 시나리오 (User Flow)
1.  **준비 (Idle):** 앱이 실행되어 트레이 아이콘에 상주 중.
2.  **질문 발생:** 사용자가 웹 서핑 중 모르는 에러 코드 발견.
3.  **트리거 (Action):** 마우스 측면 버튼을 누른 채로 에러 코드 영역을 드래그하며 말함.
    > "이 에러 코드가 뭔지 설명해주고 해결 방법 알려줘."
4.  **완료 (Release):** 마우스 버튼을 놓음.
    * 오버레이 사라짐.
    * 앱이 이미지와 음성을 패키징하여 Gemini로 전송.
5.  **확인 (Result):** 2~3초 후 화면 한구석에 팝업창이 뜨며 해결 방법 출력.

## 5. 개발 로드맵 (Development Roadmap)
문서의 단계별 접근법을 따르되, MVP 달성을 위해 최적화된 순서입니다.

### Phase 1: 기본 골격 및 오버레이 (Foundation)
* WPF 프로젝트 생성 및 Tray Icon 구현.
* Global Mouse Hook 연결 (LowLevelMouseProc).
* 투명 오버레이 윈도우 생성 및 마우스 드래그 좌표 계산 로직 구현.
* **산출물:** 버튼을 누르면 화면이 어두워지고 드래그 사각형이 그려지는 앱.

### Phase 2: 캡처 및 녹음 구현 (Capture Logic)
* GDI+를 이용한 화면 영역 캡처 및 파일/클립보드 저장.
* NAudio 연동: 버튼 Down/Up 이벤트에 맞춰 녹음 Start/Stop.
* **산출물:** 드래그 후 놓으면 `capture.png`와 `audio.wav`가 생성되는 앱.

### Phase 3: Gemini API 연동 (Intelligence)
* Google Cloud Console API Key 발급 및 프로젝트 설정.
* 이미지+오디오 전송 로직 구현 (SDK 활용).
* 응답 수신 및 디버그 콘솔 출력 확인.
* **산출물:** 동작 수행 후 로그창에 AI의 답변 텍스트가 뜨는 앱.

### Phase 4: UI 완성 및 패키징 (Productization)
* 결과 표시용 팝업 UI 디자인 (WPF).
* 설정 창 구현 (API Key 입력, 마이크 선택, 단축키 변경).
* 예외 처리 (네트워크 실패, API 한도 초과 등).
* **산출물:** 배포 가능한 Setup.exe 또는 실행 파일.

## 6. 주요 고려 사항 (Technical Constraints & Risks)
* **DPI 이슈:** 고해상도 모니터나 배율이 다른 멀티 모니터 환경에서 좌표가 어긋나는 문제 해결 필요 (Per-monitor DPI Awareness 설정).
* **API 보안:** API Key를 코드에 하드코딩하지 않고, Windows Credential Manager 등을 이용해 로컬에 암호화 저장.
* **후킹 안정성:** 훅 콜백 처리가 늦어지면 OS가 훅을 끊어버릴 수 있으므로, 무거운 작업은 반드시 별도 스레드(Task)로 분리.****