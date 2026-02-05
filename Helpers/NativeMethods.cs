using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace AI_Mouse.Helpers
{
    /// <summary>
    /// Win32 API를 P/Invoke로 호출하기 위한 네이티브 메서드 선언 클래스
    /// </summary>
    public static class NativeMethods
    {
        /// <summary>
        /// Low-level 마우스 훅 타입 (WH_MOUSE_LL = 14)
        /// </summary>
        public const int WH_MOUSE_LL = 14;

        /// <summary>
        /// Windows Hook 프로시저 델리게이트 타입
        /// </summary>
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Windows Hook을 설치합니다.
        /// </summary>
        /// <param name="idHook">훅 타입 (WH_MOUSE_LL 등)</param>
        /// <param name="lpfn">훅 프로시저</param>
        /// <param name="hMod">모듈 핸들</param>
        /// <param name="dwThreadId">스레드 ID (0이면 전역 훅)</param>
        /// <returns>훅 핸들</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Windows Hook을 해제합니다.
        /// </summary>
        /// <param name="hhk">훅 핸들</param>
        /// <returns>성공 여부</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// 다음 훅 프로시저로 이벤트를 전달합니다.
        /// </summary>
        /// <param name="hhk">훅 핸들</param>
        /// <param name="nCode">훅 코드</param>
        /// <param name="wParam">메시지 파라미터</param>
        /// <param name="lParam">메시지 파라미터</param>
        /// <returns>다음 훅 프로시저의 반환값</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 모듈 핸들을 가져옵니다.
        /// </summary>
        /// <param name="lpModuleName">모듈 이름 (null이면 현재 모듈)</param>
        /// <returns>모듈 핸들</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string? lpModuleName);

        /// <summary>
        /// Low-level 마우스 훅 구조체
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            /// <summary>
            /// 마우스 좌표 (화면 좌표계, System.Drawing.Point 사용)
            /// </summary>
            public Point pt;

            /// <summary>
            /// 마우스 데이터 (휠 이벤트 등)
            /// </summary>
            public uint mouseData;

            /// <summary>
            /// 플래그
            /// </summary>
            public uint flags;

            /// <summary>
            /// 타임스탬프
            /// </summary>
            public uint time;

            /// <summary>
            /// 추가 정보
            /// </summary>
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// 마우스 메시지 상수
        /// </summary>
        public static class MouseMessages
        {
            public const int WM_MOUSEMOVE = 0x0200;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_RBUTTONDOWN = 0x0204;
            public const int WM_RBUTTONUP = 0x0205;
            public const int WM_MBUTTONDOWN = 0x0207;
            public const int WM_MBUTTONUP = 0x0208;
            public const int WM_MOUSEWHEEL = 0x020A;
            public const int WM_XBUTTONDOWN = 0x020B;
            public const int WM_XBUTTONUP = 0x020C;
        }
    }
}
