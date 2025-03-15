namespace door.Domain.Events
{
    /// <summary>
    /// 状態変更のドメインイベント
    /// </summary>
    public class StateChangedEvent
    {
        public string Message { get; }

        public StateChangedEvent(string message)
        {
            Message = message;
        }
    }
}