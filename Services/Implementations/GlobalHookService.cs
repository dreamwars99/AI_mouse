using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using AI_Mouse.Helpers;
using AI_Mouse.Models;
using AI_Mouse.Services.Interfaces;

namespace AI_Mouse.Services.Implementations
{
    /// <summary>
    /// 전역 마우스 이벤트를 감지하는 서비스 구현
    /// </summary>
    public class GlobalHookService : IGlobalHookService
    {
        private IntPtr _hookId = IntPtr.Zero;
        private NativeMethods.LowLevelMouseProc _hookProc;
        private bool _disposed = false;
        private TriggerButton _currentTrigger = TriggerButton.XButton1; // 기본값: XButton1
        private bool _isDragging = false; // 드래그 중인지 추적

        /// <summary>
        /// 마우스 액션이 발생했을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<MouseActionEventArgs>? MouseAction;

        /// <summary>
        /// 현재 트리거 버튼 (설정에서 변경 가능)
        /// </summary>
        public TriggerButton CurrentTrigger
        {
            get => _currentTrigger;
            set
            {
                _currentTrigger = value;
                Debug.WriteLine($"[GlobalHookService] 트리거 버튼 변경: {value}");
            }
        }

        /// <summary>
        /// 훅이 활성화되어 있는지 여부
        /// </summary>
        public bool IsActive => _hookId != IntPtr.Zero;

        /// <summary>
        /// 생성자
        /// </summary>
        public GlobalHookService()
        {
            _hookProc = HookCallback;
        }

        /// <summary>
        /// 전역 훅을 시작합니다.
        /// </summary>
        public void Start()
        {
            if (IsActive)
            {
                Debug.WriteLine("[GlobalHookService] 훅이 이미 활성화되어 있습니다.");
                return;
            }

            try
            {
                // 현재 실행 파일의 모듈 핸들 가져오기 (null 전달 시 현재 모듈)
                var moduleHandle = NativeMethods.GetModuleHandle(null);
                
                if (moduleHandle == IntPtr.Zero)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"모듈 핸들 가져오기 실패. Win32 오류 코드: {error}");
                }

                // Low-level 마우스 훅 설치 (전역 훅: dwThreadId = 0)
                _hookId = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_MOUSE_LL,
                    _hookProc,
                    moduleHandle,
                    0);

                if (_hookId == IntPtr.Zero)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"훅 설치 실패. Win32 오류 코드: {error}");
                }

                Debug.WriteLine("[GlobalHookService] 전역 마우스 훅이 시작되었습니다.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GlobalHookService] 훅 시작 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 전역 훅을 중지합니다.
        /// </summary>
        public void Stop()
        {
            if (!IsActive)
            {
                Debug.WriteLine("[GlobalHookService] 훅이 이미 비활성화되어 있습니다.");
                return;
            }

            try
            {
                var result = NativeMethods.UnhookWindowsHookEx(_hookId);
                if (!result)
                {
                    var error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"[GlobalHookService] 훅 해제 실패. Win32 오류 코드: {error}");
                }
                else
                {
                    Debug.WriteLine("[GlobalHookService] 전역 마우스 훅이 중지되었습니다.");
                }

                _hookId = IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GlobalHookService] 훅 중지 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 훅 콜백 메서드 (Win32에서 호출)
        /// 주의: 이 메서드는 최대한 가볍게 작성해야 하며, 무거운 작업은 비동기로 처리해야 합니다.
        /// </summary>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // nCode가 0보다 작으면 CallNextHookEx를 호출하고 즉시 반환
            if (nCode < 0)
            {
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            var message = wParam.ToInt32();
            var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);

            // 현재 트리거 버튼에 해당하는 메시지인지 확인
            bool isTriggerMessage = IsTriggerMessage(message, hookStruct.mouseData);

            if (isTriggerMessage)
            {
                // 트리거 버튼의 Down/Up 이벤트 처리
                if (message == GetDownMessage(_currentTrigger) || message == GetUpMessage(_currentTrigger))
                {
                    // 드래그 상태 업데이트
                    if (message == GetDownMessage(_currentTrigger))
                    {
                        _isDragging = true;
                    }
                    else if (message == GetUpMessage(_currentTrigger))
                    {
                        _isDragging = false;
                    }

                    // 마우스 메시지 처리 (비동기로 전파하여 콜백을 경량화)
                    Task.Run(() =>
                    {
                        try
                        {
                            ProcessMouseMessage(wParam, lParam);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[GlobalHookService] 마우스 메시지 처리 중 오류: {ex.Message}");
                        }
                    });

                    // 기본 동작(예: 뒤로가기, 우클릭 메뉴)을 막기 위해 이벤트 전파 차단
                    return (IntPtr)1;
                }
            }
            else if (_isDragging && message == NativeMethods.MouseMessages.WM_MOUSEMOVE)
            {
                // 드래그 중 Move 이벤트는 처리하되 기본 동작은 허용
                Task.Run(() =>
                {
                    try
                    {
                        ProcessMouseMessage(wParam, lParam);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[GlobalHookService] 마우스 메시지 처리 중 오류: {ex.Message}");
                    }
                });
            }

            // 다음 훅으로 전달
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// 현재 메시지가 트리거 버튼에 해당하는지 확인합니다.
        /// </summary>
        private bool IsTriggerMessage(int message, uint mouseData)
        {
            return _currentTrigger switch
            {
                TriggerButton.Left => message == NativeMethods.MouseMessages.WM_LBUTTONDOWN || 
                                      message == NativeMethods.MouseMessages.WM_LBUTTONUP,
                TriggerButton.Right => message == NativeMethods.MouseMessages.WM_RBUTTONDOWN || 
                                       message == NativeMethods.MouseMessages.WM_RBUTTONUP,
                TriggerButton.Middle => message == NativeMethods.MouseMessages.WM_MBUTTONDOWN || 
                                        message == NativeMethods.MouseMessages.WM_MBUTTONUP,
                TriggerButton.XButton1 => (message == NativeMethods.MouseMessages.WM_XBUTTONDOWN || 
                                           message == NativeMethods.MouseMessages.WM_XBUTTONUP) && 
                                          (mouseData >> 16) == 1,
                TriggerButton.XButton2 => (message == NativeMethods.MouseMessages.WM_XBUTTONDOWN || 
                                           message == NativeMethods.MouseMessages.WM_XBUTTONUP) && 
                                          (mouseData >> 16) == 2,
                _ => false
            };
        }

        /// <summary>
        /// 트리거 버튼에 해당하는 Down 메시지를 반환합니다.
        /// </summary>
        private int GetDownMessage(TriggerButton trigger)
        {
            return trigger switch
            {
                TriggerButton.Left => NativeMethods.MouseMessages.WM_LBUTTONDOWN,
                TriggerButton.Right => NativeMethods.MouseMessages.WM_RBUTTONDOWN,
                TriggerButton.Middle => NativeMethods.MouseMessages.WM_MBUTTONDOWN,
                TriggerButton.XButton1 => NativeMethods.MouseMessages.WM_XBUTTONDOWN,
                TriggerButton.XButton2 => NativeMethods.MouseMessages.WM_XBUTTONDOWN,
                _ => 0
            };
        }

        /// <summary>
        /// 트리거 버튼에 해당하는 Up 메시지를 반환합니다.
        /// </summary>
        private int GetUpMessage(TriggerButton trigger)
        {
            return trigger switch
            {
                TriggerButton.Left => NativeMethods.MouseMessages.WM_LBUTTONUP,
                TriggerButton.Right => NativeMethods.MouseMessages.WM_RBUTTONUP,
                TriggerButton.Middle => NativeMethods.MouseMessages.WM_MBUTTONUP,
                TriggerButton.XButton1 => NativeMethods.MouseMessages.WM_XBUTTONUP,
                TriggerButton.XButton2 => NativeMethods.MouseMessages.WM_XBUTTONUP,
                _ => 0
            };
        }

        /// <summary>
        /// 마우스 메시지를 처리하고 이벤트를 발생시킵니다.
        /// </summary>
        private void ProcessMouseMessage(IntPtr wParam, IntPtr lParam)
        {
            // MSLLHOOKSTRUCT 구조체 역직렬화
            var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            var message = wParam.ToInt32();

            MouseActionType actionType;
            MouseButton button = MouseButton.None;

            // 메시지 타입에 따라 액션 타입과 버튼 결정
            switch (message)
            {
                case NativeMethods.MouseMessages.WM_MOUSEMOVE:
                    actionType = MouseActionType.Move;
                    // Move 이벤트는 버튼 정보가 없으므로 현재 트리거 버튼 사용
                    button = ConvertTriggerToMouseButton(_currentTrigger);
                    break;

                case NativeMethods.MouseMessages.WM_LBUTTONDOWN:
                    actionType = MouseActionType.Down;
                    button = MouseButton.Left;
                    break;

                case NativeMethods.MouseMessages.WM_LBUTTONUP:
                    actionType = MouseActionType.Up;
                    button = MouseButton.Left;
                    break;

                case NativeMethods.MouseMessages.WM_RBUTTONDOWN:
                    actionType = MouseActionType.Down;
                    button = MouseButton.Right;
                    break;

                case NativeMethods.MouseMessages.WM_RBUTTONUP:
                    actionType = MouseActionType.Up;
                    button = MouseButton.Right;
                    break;

                case NativeMethods.MouseMessages.WM_MBUTTONDOWN:
                    actionType = MouseActionType.Down;
                    button = MouseButton.Middle;
                    break;

                case NativeMethods.MouseMessages.WM_MBUTTONUP:
                    actionType = MouseActionType.Up;
                    button = MouseButton.Middle;
                    break;

                case NativeMethods.MouseMessages.WM_XBUTTONDOWN:
                    actionType = MouseActionType.Down;
                    // mouseData의 상위 16비트에 버튼 정보가 있음
                    button = (hookStruct.mouseData >> 16) == 1 ? MouseButton.XButton1 : MouseButton.XButton2;
                    break;

                case NativeMethods.MouseMessages.WM_XBUTTONUP:
                    actionType = MouseActionType.Up;
                    button = (hookStruct.mouseData >> 16) == 1 ? MouseButton.XButton1 : MouseButton.XButton2;
                    break;

                default:
                    // 처리하지 않는 메시지는 무시
                    return;
            }

            // 이벤트 발생 (디버그 로그 포함)
            var args = new MouseActionEventArgs(
                actionType,
                hookStruct.pt.X,
                hookStruct.pt.Y,
                button);

            Debug.WriteLine($"[GlobalHookService] 마우스 이벤트: {actionType}, 버튼: {button}, 좌표: ({hookStruct.pt.X}, {hookStruct.pt.Y})");

            MouseAction?.Invoke(this, args);
        }

        /// <summary>
        /// TriggerButton을 MouseButton으로 변환합니다.
        /// </summary>
        private MouseButton ConvertTriggerToMouseButton(TriggerButton trigger)
        {
            return trigger switch
            {
                TriggerButton.Left => MouseButton.Left,
                TriggerButton.Right => MouseButton.Right,
                TriggerButton.Middle => MouseButton.Middle,
                TriggerButton.XButton1 => MouseButton.XButton1,
                TriggerButton.XButton2 => MouseButton.XButton2,
                _ => MouseButton.None
            };
        }

        /// <summary>
        /// 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 리소스 해제 구현
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리 리소스 해제
                }

                // 훅이 아직 활성화되어 있으면 반드시 해제
                if (IsActive)
                {
                    Debug.WriteLine("[GlobalHookService] Dispose에서 훅을 해제합니다.");
                    Stop();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// 소멸자 (안전장치)
        /// </summary>
        ~GlobalHookService()
        {
            Dispose(false);
        }
    }
}
