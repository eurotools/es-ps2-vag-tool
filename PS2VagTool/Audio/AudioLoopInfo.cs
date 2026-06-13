namespace PS2VagTool.Audio
{
    internal sealed class AudioLoopInfo
    {
        internal static readonly AudioLoopInfo None = new AudioLoopInfo(false, 0, 0, null, null);

        internal AudioLoopInfo(bool isLooped, uint startSample, uint endSample, string source, string warning)
        {
            IsLooped = isLooped;
            StartSample = startSample;
            EndSample = endSample;
            Source = source;
            Warning = warning;
        }

        internal bool IsLooped { get; private set; }
        internal uint StartSample { get; private set; }
        internal uint EndSample { get; private set; }
        internal string Source { get; private set; }
        internal string Warning { get; private set; }
    }
}
