using System;
using AI_Mouse.Models;

namespace AI_Mouse.Services.Interfaces
{
    /// <summary>
    /// 전역 마우스 이벤트를 감지하는 서비스 인터페이스
    /// </summary>
    public interface IGlobalHookService : IDisposable
    {
        /// <summary>
        /// 마우스 액션이 발생했을 때 발생하는 이벤트
        /// </summary>
        event EventHandler<MouseActionEventArgs>? MouseAction;

        /// <summary>
        /// 현재 트리거 버튼 (설정에서 변경 가능)
        /// </summary>
        TriggerButton CurrentTrigger { get; set; }

        /// <summary>
        /// 전역 훅을 시작합니다.
        /// </summary>
        void Start();

        /// <summary>
        /// 전역 훅을 중지합니다.
        /// </summary>
        void Stop();

        /// <summary>
        /// 훅이 활성화되어 있는지 여부
        /// </summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// 마우스 액션 이벤트 인자
    /// </summary>
    public class MouseActionEventArgs : EventArgs
    {
        /// <summary>
        /// 마우스 액션 타입
        /// </summary>
        public MouseActionType ActionType { get; }

        /// <summary>
        /// 마우스 X 좌표 (화면 좌표계)
        /// </summary>
        public int X { get; }

        /// <summary>
        /// 마우스 Y 좌표 (화면 좌표계)
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// 마우스 버튼 (Left, Right, Middle, XButton1, XButton2)
        /// </summary>
        public MouseButton Button { get; }

        public MouseActionEventArgs(MouseActionType actionType, int x, int y, MouseButton button = MouseButton.None)
        {
            ActionType = actionType;
            X = x;
            Y = y;
            Button = button;
        }
    }

    /// <summary>
    /// 마우스 액션 타입
    /// </summary>
    public enum MouseActionType
    {
        Move,
        Down,
        Up
    }

    /// <summary>
    /// 마우스 버튼 타입
    /// </summary>
    public enum MouseButton
    {
        None,
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }
}
