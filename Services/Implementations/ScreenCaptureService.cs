using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AI_Mouse.Services.Interfaces;

namespace AI_Mouse.Services.Implementations
{
    /// <summary>
    /// GDI+ 기반 화면 캡처 서비스 구현체
    /// 물리 좌표계를 사용하여 화면 영역을 캡처하고 WPF BitmapSource로 변환합니다.
    /// </summary>
    public class ScreenCaptureService : IScreenCaptureService
    {
        /// <summary>
        /// 지정된 화면 영역을 캡처하여 BitmapSource로 반환합니다.
        /// </summary>
        /// <param name="region">캡처할 영역 (물리 좌표계)</param>
        /// <returns>캡처된 이미지 (WPF BitmapSource)</returns>
        public Task<BitmapSource> CaptureRegionAsync(Rect region)
        {
            return Task.Run(() =>
            {
                // Rect를 정수 좌표로 변환 (GDI+는 정수 좌표 사용)
                int x = (int)Math.Round(region.X);
                int y = (int)Math.Round(region.Y);
                int width = (int)Math.Round(region.Width);
                int height = (int)Math.Round(region.Height);

                // 유효성 검사
                if (width <= 0 || height <= 0)
                {
                    throw new ArgumentException("캡처 영역의 너비와 높이는 0보다 커야 합니다.", nameof(region));
                }

                // GDI+ Bitmap 생성
                using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    // Graphics 객체 생성
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        // 화면에서 지정된 영역을 비트맵으로 복사
                        graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
                    }

                    // BitmapSource로 변환
                    return ConvertToBitmapSource(bitmap);
                }
            });
        }

        /// <summary>
        /// BitmapSource 이미지를 클립보드에 복사합니다.
        /// </summary>
        /// <param name="image">클립보드에 복사할 이미지</param>
        public Task CopyToClipboardAsync(BitmapSource image)
        {
            return Task.Run(() =>
            {
                // UI 스레드에서 클립보드 작업 수행
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // WPF Clipboard.SetImage는 BitmapSource를 직접 받을 수 있음
                        Clipboard.SetImage(image);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"클립보드에 이미지를 복사하는 중 오류가 발생했습니다: {ex.Message}", ex);
                    }
                });
            });
        }

        /// <summary>
        /// System.Drawing.Bitmap을 WPF BitmapSource로 변환합니다.
        /// </summary>
        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            // 비트맵 데이터를 잠금
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                // BitmapSource 생성
                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width,
                    bitmapData.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    System.Windows.Media.PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    bitmapData.Stride * bitmapData.Height,
                    bitmapData.Stride);

                // 메모리 고정 (가비지 컬렉션 방지)
                bitmapSource.Freeze();

                return bitmapSource;
            }
            finally
            {
                // 비트맵 데이터 잠금 해제
                bitmap.UnlockBits(bitmapData);
            }
        }

    }
}
