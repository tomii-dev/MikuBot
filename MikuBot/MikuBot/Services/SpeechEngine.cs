using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MikuBot.Services
{
    class SpeechEngine : IDisposable
    {
        private static ConcurrentQueue<SpeechEngine> Engines = new ConcurrentQueue<SpeechEngine>();
        internal static int Count
        {
            get
            {
                return Engines.Count;
            }
        }

        internal static CultureInfo Culture = new CultureInfo("en-US");
        internal static string[] Trigger;
        private static long State = long.MinValue;

        internal static Task<SpeechEngine> Get(EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted)
        {
            if (!Engines.TryDequeue(out SpeechEngine Engine))
            {
                Engine = new SpeechEngine
                {
                    Service = new SpeechRecognitionEngine(Culture)
                };
            }

            return Engine.Prepare(RecognizeCompleted);
        }

        internal static void Invalidate()
        {
            Interlocked.Increment(ref State);
        }

        private SpeechRecognitionEngine Service;
        private long OwnState;
        private EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted;

        private async Task<SpeechEngine> Prepare(EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted)
        {
            this.RecognizeCompleted = RecognizeCompleted;
            Service.RecognizeCompleted += RecognizeCompleted;

            if (!IsValid)
            {
                Service.UnloadAllGrammars();

                var Main = new GrammarBuilder();
                Main.Append(GetChoiceLibrary());

                var Waiter = new TaskCompletionSource<LoadGrammarCompletedEventArgs>();
                EventHandler<LoadGrammarCompletedEventArgs> Event = (s, e) => Waiter.SetResult(e);
                Service.LoadGrammarCompleted += Event;
                Service.LoadGrammarAsync(new Grammar(Main));
                await Waiter.Task;
                Service.LoadGrammarCompleted -= Event;

                OwnState = State;
            }

            return this;
        }

        internal void Recognize(MemoryStream Stream)
        {
            Service.SetInputToAudioStream(Stream, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
            Service.RecognizeAsync(RecognizeMode.Single);
        }

        internal bool IsValid
        {
            get
            {
                return OwnState == State;
            }
        }

        private Choices GetChoiceLibrary()
        {
            Choices choices = new Choices();
            choices.Add("hi miku");
            choices.Add("miku poo");
            choices.Add("miku fuck off");
            return choices;
        }

        public void Dispose()
        {
            Service.RecognizeCompleted -= RecognizeCompleted;
            Engines.Enqueue(this);
        }
    }
}