using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YoutubeExplode.Videos.Streams;

namespace MikuBot.Modules.Voice
{
    class Queue
    {
        private Queue<IStreamInfo> _queue;
        public Queue()
        {
            _queue = new Queue<IStreamInfo>();
        }

        public void AddStream(IStreamInfo stream)
        {
            try
            {
                _queue.Enqueue(stream);
            }catch(Exception e) { Console.WriteLine(e.ToString()); }
        }

        public bool IsEmpty()
        {
            return _queue.Count == 0;
        }
        
        public IStreamInfo GetCurrentStream()
        {
            return _queue.Peek();
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
