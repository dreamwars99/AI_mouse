using System;
using System.Drawing;
using System.Windows;
using WpfPoint = System.Windows.Point;

namespace AI_Mouse.Helpers
{
    /// <summary>
    /// DPI 보정을 위한 유틸리티 클래스
    /// Win32 API를 사용하여 모니터별 DPI 정보를 가져오고 좌표 변환을 수행합니다.
    /// </summary>
    public static class DpiHelper
    {
        /// <summary>
        /// 지정된 화면 좌표의 모니터 DPI를 가져옵니다.
        /// </summary>
        /// <param name="x">화면 X 좌표 (물리 좌표)</param>
        /// <param name="y">화면 Y 좌표 (물리 좌표)</param>
        /// <returns>DPI 값 (96 = 100%, 192 = 200% 등)</returns>
        public static uint GetDpiForPoint(int x, int y)
        {
            var point = new Point(x, y); // System.Drawing.Point (Win32 API용)
            var monitorHandle = NativeMethods.MonitorFromPoint(point, NativeMethods.MonitorFromPointFlags.MONITOR_DEFAULTTONEAREST);

            if (monitorHandle == IntPtr.Zero)
            {
                // 실패 시 기본값 96 DPI 반환
                return 96;
            }

            if (NativeMethods.GetDpiForMonitor(monitorHandle, NativeMethods.MonitorDpiType.EffectiveDpi, out uint dpiX, out uint dpiY))
            {
                // X와 Y DPI가 다를 수 있지만, 일반적으로 같으므로 X를 반환
                return dpiX;
            }

            // 실패 시 기본값 96 DPI 반환
            return 96;
        }

        /// <summary>
        /// 물리 좌표(Physical Pixel)를 논리 좌표(Logical DPI-independent)로 변환합니다.
        /// WPF는 논리 좌표를 사용하므로, 물리 좌표를 논리 좌표로 변환해야 올바르게 표시됩니다.
        /// </summary>
        /// <param name="physicalX">물리 X 좌표</param>
        /// <param name="physicalY">물리 Y 좌표</param>
        /// <param name="dpi">DPI 값 (기본값: null이면 자동 감지)</param>
        /// <returns>논리 좌표 (WPF Point)</returns>
        public static WpfPoint PhysicalToLogical(int physicalX, int physicalY, uint? dpi = null)
        {
            uint actualDpi = dpi ?? GetDpiForPoint(physicalX, physicalY);
            double scaleFactor = 96.0 / actualDpi; // WPF는 96 DPI를 기준으로 함

            return new WpfPoint(physicalX * scaleFactor, physicalY * scaleFactor);
        }

        /// <summary>
        /// 논리 좌표(Logical DPI-independent)를 물리 좌표(Physical Pixel)로 변환합니다.
        /// </summary>
        /// <param name="logicalX">논리 X 좌표</param>
        /// <param name="logicalY">논리 Y 좌표</param>
        /// <param name="dpi">DPI 값 (기본값: null이면 자동 감지)</param>
        /// <returns>물리 좌표 (System.Drawing.Point)</returns>
        public static Point LogicalToPhysical(double logicalX, double logicalY, uint? dpi = null)
        {
            // 논리 좌표를 물리 좌표로 변환하려면 기준점이 필요하므로,
            // 일반적으로는 사용하지 않지만 필요시 구현
            uint actualDpi = dpi ?? 96;
            double scaleFactor = actualDpi / 96.0;

            return new Point((int)(logicalX * scaleFactor), (int)(logicalY * scaleFactor));
        }

        /// <summary>
        /// 물리 Rect를 논리 Rect로 변환합니다.
        /// </summary>
        /// <param name="physicalRect">물리 Rect (물리 좌표)</param>
        /// <param name="dpi">DPI 값 (기본값: null이면 자동 감지)</param>
        /// <returns>논리 Rect (WPF Rect)</returns>
        public static Rect PhysicalToLogicalRect(Rect physicalRect, uint? dpi = null)
        {
            uint actualDpi = dpi ?? GetDpiForPoint((int)physicalRect.X, (int)physicalRect.Y);
            double scaleFactor = 96.0 / actualDpi;

            return new Rect(
                physicalRect.X * scaleFactor,
                physicalRect.Y * scaleFactor,
                physicalRect.Width * scaleFactor,
                physicalRect.Height * scaleFactor);
        }

        /// <summary>
        /// 논리 Rect를 물리 Rect로 변환합니다.
        /// </summary>
        /// <param name="logicalRect">논리 Rect (WPF Rect)</param>
        /// <param name="dpi">DPI 값 (기본값: null이면 자동 감지)</param>
        /// <returns>물리 Rect (물리 좌표)</returns>
        public static Rect LogicalToPhysicalRect(Rect logicalRect, uint? dpi = null)
        {
            uint actualDpi = dpi ?? 96;
            double scaleFactor = actualDpi / 96.0;

            return new Rect(
                logicalRect.X * scaleFactor,
                logicalRect.Y * scaleFactor,
                logicalRect.Width * scaleFactor,
                logicalRect.Height * scaleFactor);
        }
    }
}
