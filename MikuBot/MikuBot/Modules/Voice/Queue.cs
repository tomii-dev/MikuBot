using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YoutubeExplode.Videos.Streams;

namespace MikuBot.Modules.Voice
{
    class Queue
    {
        private static Queue _currentQueue = null;
        private Queue<IStreamInfo> _queue;
        private IStreamInfo currentStream = null;
        private IStreamInfo nextStream = null;
        public Queue()
        {
            _queue = new Queue<IStreamInfo>();
        }

        public void AddStream(IStreamInfo stream)
        {
            try
            {
                _queue.Enqueue(stream);
                Console.WriteLine(_queue.Peek().ToString());
            }catch(Exception e) { Console.WriteLine(e.ToString()); }
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
            currentStream = _queue.Peek();
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
        public static void SetCurrentQueue(Queue queue)
        {
            _currentQueue = queue;
        }
        public static Queue GetCurrentQueue()
        {
            return _currentQueue;
        }
    }
}
