using NWaves;
using NWaves.Audio;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Operations;
using NWaves.Operations.Tsm;
using NWaves.Signals;
using System.Buffers;
using System.Linq;

namespace HParrotFilterLib;

public class HPFilter {

    // Main libraries etc:
    // https://github.com/ar1st0crat/NWaves -- filtering
    // https://github.com/naudio/NAudio -- system audio
    // https://github.com/xenolightning/AudioSwitcher/tree/master/AudioSwitcher.AudioApi.CoreAudio -- lower level system audio (not used)
    // https://github.com/ar1st0crat/NWaves.Samples -- example projects (ParrotLive based on DemoMfccOnline)

    public string InputFile = @"c:\samples\sample.wav";
    public string OutputFile = @"c:\samples\output_mono.wav";

    public DiscreteSignal ParrotFilter(string inputFilename) {
        var file = Load(inputFilename);
        return ParrotFilter(file, false);
    }
    public DiscreteSignal ParrotFilter(WaveFile waveContainer, bool clone) {
        DiscreteSignal left = waveContainer[Channels.Left].Copy();
        //DiscreteSignal right = waveContainer[Channels.Right];
        return ParrotFilter(left);
    }
    public DiscreteSignal ParrotFilter(DiscreteSignal signal) {

        // process

        var normalize = Operation.NormalizePeak(signal, -3.0); // or is normalization automatic?
        //var normalizeRms = Operation.NormalizeRms(signal, -3.0);


        //var interpolated = Operation.Interpolate(signal, 3);
        //var decimated = Operation.Decimate(signal, 2);
        //var updown = Operation.ResampleUpDown(signal, 3, 2);


        // time scale modification
        //var stretched = Operation.TimeStretch(signal, 2.0, algorithm: TsmAlgorithm.PhaseVocoderPhaseLocking); // silence?

        var stretch = Operation.TimeStretch(signal, 1.1, TsmAlgorithm.Wsola);
        //var cool = Operation.TimeStretch(signal, 16, TsmAlgorithm.PaulStretch);

        //bandpass (todo)
        /*
        var tf = new NWaves.Filters.Butterworth.BandPassFilter(0.1, 0.16, 7).Tf;
        // get array of SOS from TF:
        TransferFunction[] sos = DesignFilter.TfToSos(tf);
        var sosFilter = new FilterChain(sos);
        var y = sosFilter.ApplyTo(stretch);
        */

        //https://github.com/ar1st0crat/NWaves/blob/41e9fef88fa58171eeed1e3464832271e7b76ddb/NWaves/Effects/PitchShiftEffect.cs
        //var pitch = new NWaves.Effects.PitchShiftEffect(1.5, 1024, 128, TsmAlgorithm.Wsola);
        //var z = pitch.ApplyTo(y);

        var speedy = new DiscreteSignal(stretch.SamplingRate * 2, stretch.Samples);
        var resampled = Operation.Resample(speedy, 8000);

        return resampled;
    }

    public void Test() {

        // loads a sample, processes it, and saves it again

        // docs: https://github.com/ar1st0crat/NWaves

        WaveFile waveContainer = Load();

        var resampled = ParrotFilter(waveContainer, clone:false);

        Save(resampled);

    }
    
    public WaveFile Load(string filename = null) {
        if (filename == null) {
            filename = InputFile;
        }

        WaveFile waveContainer;
        using (var stream = new FileStream(filename, FileMode.Open)) {
            waveContainer = new WaveFile(stream);
        }
        return waveContainer;
    }


    public void Save(DiscreteSignal signal, string filename = null) {
        if (filename == null) {
            filename = OutputFile;
        }

        var waveFileOut = new WaveFile(signal);

        using (var stream = new FileStream(OutputFile, FileMode.Create)) {
            waveFileOut.SaveTo(stream);
        }
    }
}
