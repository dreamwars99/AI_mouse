using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AI_Mouse.Services.Interfaces
{
    /// <summary>
    /// 화면 캡처 서비스 인터페이스
    /// 지정된 화면 영역을 이미지로 캡처하고 클립보드에 복사하는 기능을 제공합니다.
    /// </summary>
    public interface IScreenCaptureService
    {
        /// <summary>
        /// 지정된 화면 영역을 캡처하여 BitmapSource로 반환합니다.
        /// </summary>
        /// <param name="region">캡처할 영역 (물리 좌표계)</param>
        /// <returns>캡처된 이미지 (WPF BitmapSource)</returns>
        Task<BitmapSource> CaptureRegionAsync(System.Windows.Rect region);

        /// <summary>
        /// BitmapSource 이미지를 클립보드에 복사합니다.
        /// </summary>
        /// <param name="image">클립보드에 복사할 이미지</param>
        Task CopyToClipboardAsync(BitmapSource image);
    }
}
