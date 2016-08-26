using System;

namespace Script.I200.QuartzJob
{
    public class Message
    {
        public DateTime CreateTime { get; private set; }

        public string Content { get; private set; }

        public Message(string content)
        {
            this.CreateTime = DateTime.Now;
            this.Content = content;
        }
    }
}
