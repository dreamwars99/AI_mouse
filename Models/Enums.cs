namespace AI_Mouse.Models
{
    /// <summary>
    /// 트리거 버튼 열거형 (설정에서 선택 가능한 마우스 버튼)
    /// </summary>
    public enum TriggerButton
    {
        /// <summary>
        /// 왼쪽 버튼 (비추천: 일반 클릭이 안 될 수 있음)
        /// </summary>
        Left,

        /// <summary>
        /// 오른쪽 버튼
        /// </summary>
        Right,

        /// <summary>
        /// 휠 클릭 (중간 버튼)
        /// </summary>
        Middle,

        /// <summary>
        /// XButton1 (뒤로 가기 버튼, 기본값)
        /// </summary>
        XButton1,

        /// <summary>
        /// XButton2 (앞으로 가기 버튼)
        /// </summary>
        XButton2
    }
}
