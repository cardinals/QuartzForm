namespace Script.I200.QuartzJob
{
    public static class MessageCenter
    {
        public static event PushMessageHandler PushMessage;

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="message"></param>
        public static void Push(string message)
        {
            if (PushMessage != null)
            {
                PushMessage(new Message(message));
            }
        }
    }
}
