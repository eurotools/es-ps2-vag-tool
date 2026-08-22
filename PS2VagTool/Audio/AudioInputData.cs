//-------------------------------------------------------------------------------------------------------------------------------
//  _____   _____ ___   __      __     _____
// |  __ \ / ____|__ \  \ \    / /\   / ____|
// | |__) | (___    ) |  \ \  / /  \ | |  __
// |  ___/ \___ \  / /    \ \/ / /\ \| | |_ |
// | |     ____) |/ /_     \  / ____ \ |__| |
// |_|    |_____/|____|     \/_/    \_\_____|
//
//-------------------------------------------------------------------------------------------------------------------------------
// Decoded Audio Input Data
//-------------------------------------------------------------------------------------------------------------------------------
namespace PS2VagTool.Audio
{
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------------
    internal sealed class AudioInputData
    {
        internal AudioInputData(short[] pcmSamples, int sampleRate, int channels, AudioLoopInfo loopInfo)
        {
            PcmSamples = pcmSamples;
            SampleRate = sampleRate;
            Channels = channels;
            LoopInfo = loopInfo;
        }

        internal short[] PcmSamples { get; private set; }
        internal int SampleRate { get; private set; }
        internal int Channels { get; private set; }
        internal AudioLoopInfo LoopInfo { get; private set; }

        internal int SampleFrames
        {
            get { return Channels <= 0 ? 0 : PcmSamples.Length / Channels; }
        }
    }
}
