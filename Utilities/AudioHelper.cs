using NAudio.Wave;

namespace Eryth.Utilities
{
    public static class AudioHelper
    {
        public static int GetAudioDurationInSeconds(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return 0;

                using var audioFile = new AudioFileReader(filePath);
                return (int)audioFile.TotalTime.TotalSeconds;
            }
            catch (Exception)
            {
                return 180;
            }
        }

        public static int GetAudioDurationInSecondsFromStream(Stream stream)
        {
            try
            {
                using var audioFile = new StreamMediaFoundationReader(stream);
                return (int)audioFile.TotalTime.TotalSeconds;
            }
            catch (Exception)
            {
                return 180;
            }
        }
    }
}
