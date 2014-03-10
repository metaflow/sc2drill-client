using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Reflection;
using System.Speech.Synthesis;
using System.Threading;

namespace Probe.Utility
{
    internal static class Speaker
    {
        private const string WaveFileExtension = ".wav";

        private static readonly SpeechSynthesizer _synthesizer;
        private static readonly Dictionary<string, SoundPlayer> _players = new Dictionary<string, SoundPlayer>();
        private static readonly BackgroundWorker _speaker = new BackgroundWorker();
        public static bool TestFlag;

        private static readonly PriorityQueue<SoundPlayer> _priorityQueue = new PriorityQueue<SoundPlayer>(); 

        static Speaker()
        {
            try
            {
                _synthesizer = new SpeechSynthesizer();
            }
            catch
            {
                _synthesizer = null;
            }

            _speaker.DoWork += _speaker_DoWork;
            _speaker.RunWorkerAsync();
        }

        private static void _speaker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_speaker.CancellationPending)
            {
                SoundPlayer player = null;

                if (_priorityQueue.Count > 0)
                {
                    lock (_priorityQueue)
                    {
                        if (_priorityQueue.Count > 0)
                        {
                            player = _priorityQueue.Dequeue();
                        }
                    }
                }

                if (player != null)
                {
                    player.PlaySync();
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
        }

        public static void PreLoad(IEnumerable<string> texts)
        {
            if (texts == null) return;

            foreach (var text in texts)
            {
                GetPlayer(text);
            }
        }

        public static void Speak(string text, bool isHightPriority = false)
        {
            if (string.IsNullOrEmpty(text)) return;

            var player = GetPlayer(text);

            if (player == null) return;

            lock (_priorityQueue)
            {
                _priorityQueue.Enqueue(player, isHightPriority ? 1 : 0);
            }
        }

        private static string FindSoundFile(string path)
        {
            var suggestions = new List<string>();
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (directory != null)
            {
                suggestions.Add(Path.Combine(directory, path));
                suggestions.Add(Path.Combine(Path.Combine(directory, "sounds"), path));
            }
            directory = Path.GetDirectoryName(directory);
            if (directory != null)
            {
                suggestions.Add(Path.Combine(directory, path));
                suggestions.Add(Path.Combine(Path.Combine(directory, "sounds"), path));
            }

            foreach (var suggestion in suggestions)
            {
                if (File.Exists(suggestion)) return suggestion;
            }

            if (Path.GetExtension(path.ToLower()) != WaveFileExtension)
            {
                return FindSoundFile(string.Format("{0}{1}", path, WaveFileExtension));
            }

            return String.Empty;
        }

        private static SoundPlayer GetPlayer(string text)
        {
            if (_players.ContainsKey(text)) return _players[text];
            
            SoundPlayer player;

            var findFile = FindSoundFile(text);

            if (File.Exists(findFile))
            {
                player = new SoundPlayer(findFile);
                player.Load();
            }
            else
            {
                if (_synthesizer == null) return null;

                var hash = text.GetHashCode();
                var file = string.Format("probe_spkr_{0}.wav", hash);
                var path = Path.Combine(Path.GetTempPath(), file);

                if (!File.Exists(path))
                {
                    _synthesizer.SetOutputToWaveFile(path);
                    _synthesizer.Speak(text.Replace(WaveFileExtension, string.Empty).Trim());
                }

                player = new SoundPlayer(path);
                player.Load();
            }

            _players.Add(text, player);

            return player;
        }

        public static void ClearPlaylist()
        {
            lock (_priorityQueue)
            {
                _priorityQueue.Clear();
            }
        }
    }
}
