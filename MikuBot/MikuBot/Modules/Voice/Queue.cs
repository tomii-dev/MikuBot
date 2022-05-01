using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YoutubeExplode.Videos.Streams;

namespace MikuBot.Modules.Voice
{
    class Queue
    {
        private static Dictionary<ulong, Queue> _queues = new Dictionary<ulong, Queue>();
        private Queue<IStreamInfo> _queue;
        private IStreamInfo currentStream = null;
        private IStreamInfo nextStream = null;
        private ulong _guildId;

        public Queue(ulong guildId)
        {
            _queue = new Queue<IStreamInfo>();
            _guildId = guildId;
            _queues.Add(guildId, this);
        }
        public static Queue GetQueue(ulong guildId)
        {
            return _queues[guildId];
        }

        public static bool QueueExists(ulong guildId)
        {
            return _queues.ContainsKey(guildId);
        }

        public void AddStream(IStreamInfo stream)
        {
            _queue.Enqueue(stream);
        }

        public bool IsEmpty()
        {
            return _queue.Count == 0;
        }

        public IStreamInfo GetNextStream()
        {
            return _queue.Peek();
        }
        
        public IStreamInfo GetCurrentStream()
        {
            return currentStream;
        }

        public void SetCurrentStream()
        {
            Console.WriteLine("jdfhgjidfjgi");
            currentStream = _queue.Peek();
        }

        public void Destroy()
        {
            _queues.Remove(_guildId);
        }

        public void SetCurrentStreamNull()
        {
            currentStream = null;
        }

        public void Dequeue()
        {
            _queue.Dequeue();
        }
        public int GetCount()
        {
            return _queue.Count;
        }
    }
}
