namespace PS2VagTool.Audio
{
    internal static class LoopValidator
    {
        internal static AudioLoopInfo Validate(uint start, uint end, long totalSampleFrames, string source)
        {
            if (totalSampleFrames > 0 && end >= (ulong)totalSampleFrames)
            {
                end = (uint)System.Math.Min(totalSampleFrames - 1, uint.MaxValue);
            }

            if (end <= start)
            {
                return new AudioLoopInfo(false, 0, 0, null, source + " loop end must be greater than loop start.");
            }

            return new AudioLoopInfo(true, start, end, source, null);
        }
    }
}
