using System;
using System.Windows.Media;

namespace Snake
{
    public static class Audio
    {
        public readonly static MediaPlayer GameOver = LoadAudio("game-over.mp3");
        private static MediaPlayer LoadAudio(string filename)
        {
            MediaPlayer player = new();
            player.Open(new Uri($"Assets/{filename}", UriKind.Relative));
            return player;
        }
    }
}
