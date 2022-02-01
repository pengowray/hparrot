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

    //https://github.com/ar1st0crat/NWaves
    //https://github.com/xenolightning/AudioSwitcher/tree/master/AudioSwitcher.AudioApi.CoreAudio

    string inputFile = @"c:\samples\sample.wav";
    string outputFile = @"c:\samples\output_mono.wav";

    public void Test() {

        // loads a sample, processes it, and saves it again

        // docs: https://github.com/ar1st0crat/NWaves

        WaveFile waveContainer;

        // load

        using (var stream = new FileStream(inputFile, FileMode.Open)) {
            waveContainer = new WaveFile(stream);
        }

        DiscreteSignal left = waveContainer[Channels.Left];
        //DiscreteSignal right = waveContainer[Channels.Right];

        // process

        var signal = left;

        var normalize = Operation.NormalizePeak(signal, -3.0); // or is normalization is automatic?
        //var normalizeRms = Operation.NormalizeRms(signal, -3.0);


        //var interpolated = Operation.Interpolate(signal, 3);
        var decimated = Operation.Decimate(signal, 2);
        //var updown = Operation.ResampleUpDown(signal, 3, 2);

        // time scale modification

        var stretch = Operation.TimeStretch(signal, 1.1, TsmAlgorithm.Wsola);
        //var cool = Operation.TimeStretch(signal, 16, TsmAlgorithm.PaulStretch);

        //bandpass (todo)
        
        var tf = new NWaves.Filters.Butterworth.BandPassFilter(0.1, 0.16, 7).Tf;
        // get array of SOS from TF:
        
        TransferFunction[] sos = DesignFilter.TfToSos(tf);
        var sosFilter = new FilterChain(sos);

        ////System.NotImplementedException: The method or operation is not implemented.
        //var pitch = new NWaves.Effects.PitchShiftEffect(2.0); // or pitch shift when time stretching or resampling?
        //sosFilter.Add(pitch);

        var y = sosFilter.ApplyTo(signal);

        // resample now or earlier?
        var resampled = Operation.Resample(signal, 8000);

        var waveFileOut = new WaveFile(resampled);

        using (var stream = new FileStream(outputFile, FileMode.Create)) {
            waveFileOut.SaveTo(stream);
        }

    }
}
