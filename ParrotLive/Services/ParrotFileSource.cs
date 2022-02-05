using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;
using NWaves.DemoMfccOnline.Interfaces;
using HParrotFilterLib;
using NWaves.Audio;
using System.IO;
using NWaves.Signals;
using System.Media;

namespace NWaves.DemoMfccOnline.Services {
    public class ParrotFileSource /* : ISampleProvider */ {

        private string OGFilename;
        private WaveFile? OGWave; // wave container
        private DiscreteSignal? FilteredWave;
        private SoundPlayer Player;

        public ParrotFileSource(string filename) {
            // Load full file for simple playback or filtered playback
            // Choose between playback devices: preview (aka monitor), output, or both

            // TODO: stop button -- hook into events from one or just provide a Stop()
            // TODO (future): allow streaming of files (incremental load; more importanting for mic and waveform display)

            if (string.IsNullOrWhiteSpace(filename)) {
                throw new ArgumentNullException("filename missing");
            }

            OGFilename = filename;
            using (var stream = new FileStream(filename, FileMode.Open)) {
                OGWave = new WaveFile(stream);
            }
            
            var parrot = new HPFilter();
            FilteredWave = parrot.ParrotFilter(OGWave, clone:true);

            //parrot.Save(filtered);

            //_reader = new AudioFileReader(parrot.OutputFile);
            //Channels = _reader.WaveFormat.Channels;
            //_waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_reader.WaveFormat.SampleRate, Channels);

        }

        public void Play(bool Filtered) {
            //TODO: option to stop first or not? or managed elsewhere and use event?
            //TODO: choice of playback device

            Player?.Stop();
            Player?.Dispose();

            if (!Filtered) {
                PlayOriginalFromFile();

            } else {
                PlayFiltered();
            }
        }

        void PlayOriginalFromFile() {
            // doesn't work?
            if (OGFilename == null)
                return;
            Player = new SoundPlayer(OGFilename);
            //Player.Stream.Seek(0, SeekOrigin.Begin);
            Player.Play();

        }

        void PlayFiltered() {
            if (FilteredWave == null)
                return;
            var stream = new MemoryStream();
            short bitdepth = 16;
            var wave = new WaveFile(FilteredWave, bitdepth);
            wave.SaveTo(stream);
            stream = new MemoryStream(stream.ToArray());

            Player = new SoundPlayer(stream);
            Player.Stream.Seek(0, SeekOrigin.Begin);
            Player.Play();

        }
        /*
        public WaveFormat WaveFormat => throw new NotImplementedException();

        public int Read(float[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }
        */

    }
}
