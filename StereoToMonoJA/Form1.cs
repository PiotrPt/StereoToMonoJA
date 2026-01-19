using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace StereoToMonoJA
{
    public partial class Form1 : Form
    {
        // DLL Import for C++ library
        [DllImport("MonoCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProcessStereoToMonoPCM16(
            IntPtr leftChannel,
            IntPtr rightChannel,
            IntPtr monoOutput,
            UIntPtr sampleCount,
            int mixMode,
            bool antiClipping,
            bool normalize,
            bool removeDC,
            bool dithering,
            int numThreads);

        [DllImport("MonoCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProcessStereoToMonoFloat32(
            IntPtr leftChannel,
            IntPtr rightChannel,
            IntPtr monoOutput,
            UIntPtr sampleCount,
            int mixMode,
            bool antiClipping,
            bool normalize,
            bool removeDC,
            bool dithering,
            int numThreads);

        [DllImport("MonoCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetLibraryVersion();

        // DLL Import for Assembly library
        [DllImport("MonoASM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProcessStereoToMonoPCM16_AVX2(
            IntPtr leftChannel,
            IntPtr rightChannel,
            IntPtr monoOutput,
            UIntPtr sampleCount,
            int mixMode,
            bool antiClipping,
            bool normalize,
            bool removeDC,
            bool dithering,
            int numThreads);

        [DllImport("MonoASM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProcessStereoToMonoFloat32_AVX2(
            IntPtr leftChannel,
            IntPtr rightChannel,
            IntPtr monoOutput,
            UIntPtr sampleCount,
            int mixMode,
            bool antiClipping,
            bool normalize,
            bool removeDC,
            bool dithering,
            int numThreads);

        [DllImport("MonoASM.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetVersion_ASM")]
        private static extern IntPtr GetVersion_ASM();

        // WAV file structures
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WavHeader
        {
            public uint ChunkID;
            public uint ChunkSize;
            public uint Format;
            public uint Subchunk1ID;
            public uint Subchunk1Size;
            public ushort AudioFormat;
            public ushort NumChannels;
            public uint SampleRate;
            public uint ByteRate;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public uint Subchunk2ID;
            public uint Subchunk2Size;
        }

        private enum AudioFormat
        {
            PCM = 1,
            Float = 3
        }

        // Processing options class to hold UI values
        private class ProcessingOptions
        {
            public bool UseCppLibrary { get; set; }
            public bool UseArithmeticMix { get; set; }
            public bool AntiClipping { get; set; }
            public bool Normalize { get; set; }
            public bool RemoveDC { get; set; }
            public bool Dithering { get; set; }
            public int ThreadCount { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateDLLVersion();
            UpdateThreadsLabel();
        }

        private void UpdateDLLVersion()
        {
            try
            {
                IntPtr versionPtr;
                if (radioCPP.Checked)
                {
                    versionPtr = GetLibraryVersion();
                }
                else
                {
                    versionPtr = GetVersion_ASM();
                }
                string version = Marshal.PtrToStringAnsi(versionPtr);
                lblDLLVersion.Text = $"Version: {version}";
            }
            catch (Exception ex)
            {
                lblDLLVersion.Text = $"Error: {ex.Message}";
                lblDLLVersion.ForeColor = Color.Red;
            }
        }

        private void UpdateThreadsLabel()
        {
            lblThreadsValue.Text = trackBarThreads.Value.ToString();
        }

        private void radioDLL_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDLLVersion();
        }

        private void trackBarThreads_Scroll(object sender, EventArgs e)
        {
            UpdateThreadsLabel();
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
                ofd.Title = "Select Input WAV File";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = ofd.FileName;
                    if (string.IsNullOrEmpty(txtOutputFile.Text))
                    {
                        string dir = Path.GetDirectoryName(ofd.FileName);
                        string filename = Path.GetFileNameWithoutExtension(ofd.FileName);
                        txtOutputFile.Text = Path.Combine(dir, filename + "_mono.wav");
                    }
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
                sfd.Title = "Select Output WAV File";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = sfd.FileName;
                }
            }
        }

        private async void btnProcess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInputFile.Text) || !File.Exists(txtInputFile.Text))
            {
                MessageBox.Show("Please select a valid input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtOutputFile.Text))
            {
                MessageBox.Show("Please specify an output file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // READ ALL UI CONTROL VALUES ON UI THREAD BEFORE STARTING BACKGROUND TASK
            string inputPath = txtInputFile.Text;
            string outputPath = txtOutputFile.Text;
            
            ProcessingOptions options = new ProcessingOptions
            {
                UseCppLibrary = radioCPP.Checked,
                UseArithmeticMix = radioArithmetic.Checked,
                AntiClipping = chkAntiClipping.Checked,
                Normalize = chkNormalize.Checked,
                RemoveDC = chkRemoveDC.Checked,
                Dithering = chkDithering.Checked,
                ThreadCount = trackBarThreads.Value
            };

            btnProcess.Enabled = false;
            progressBar.Value = 0;
            lblStatus.Text = "Processing...";
            lblStatus.ForeColor = Color.Blue;
            lblProcessingTime.Text = "";

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                await Task.Run(() => ProcessWavFile(inputPath, outputPath, options));
                sw.Stop();

                lblStatus.Text = "Completed successfully!";
                lblStatus.ForeColor = Color.Green;
                lblProcessingTime.Text = $"Processing Time: {sw.ElapsedMilliseconds} ms";
                progressBar.Value = 100;

                MessageBox.Show($"Conversion completed in {sw.ElapsedMilliseconds} ms", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error occurred!";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnProcess.Enabled = true;
            }
        }

        private void ProcessWavFile(string inputPath, string outputPath, ProcessingOptions options)
        {
            // Read WAV header
            byte[] headerBytes = new byte[Marshal.SizeOf(typeof(WavHeader))];
            using (FileStream fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(headerBytes, 0, headerBytes.Length);
            }

            GCHandle handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
            WavHeader header = (WavHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(WavHeader));
            handle.Free();

            // Validate WAV file
            if (header.ChunkID != 0x46464952 || header.Format != 0x45564157) // "RIFF" and "WAVE"
            {
                throw new Exception("Invalid WAV file format.");
            }

            if (header.NumChannels != 2)
            {
                throw new Exception("Input file must be stereo (2 channels).");
            }

            AudioFormat audioFormat = (AudioFormat)header.AudioFormat;
            if (audioFormat != AudioFormat.PCM && audioFormat != AudioFormat.Float)
            {
                throw new Exception("Only PCM16 and Float32 formats are supported.");
            }

            bool isFloat = audioFormat == AudioFormat.Float;
            int bytesPerSample = header.BitsPerSample / 8;
            uint numSamples = header.Subchunk2Size / header.BlockAlign;

            // Read audio data
            byte[] audioData = new byte[header.Subchunk2Size];
            using (FileStream fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(headerBytes.Length, SeekOrigin.Begin);
                fs.Read(audioData, 0, audioData.Length);
            }

            // Allocate buffers
            int sampleSize = isFloat ? sizeof(float) : sizeof(short);
            byte[] leftData = new byte[numSamples * sampleSize];
            byte[] rightData = new byte[numSamples * sampleSize];
            byte[] monoData = new byte[numSamples * sampleSize];

            // De-interleave stereo to separate channels
            for (uint i = 0; i < numSamples; i++)
            {
                int srcOffset = (int)(i * header.BlockAlign);
                int dstOffset = (int)(i * sampleSize);

                Array.Copy(audioData, srcOffset, leftData, dstOffset, bytesPerSample);
                Array.Copy(audioData, srcOffset + bytesPerSample, rightData, dstOffset, bytesPerSample);
            }

            // Pin memory for native calls
            GCHandle hLeft = GCHandle.Alloc(leftData, GCHandleType.Pinned);
            GCHandle hRight = GCHandle.Alloc(rightData, GCHandleType.Pinned);
            GCHandle hMono = GCHandle.Alloc(monoData, GCHandleType.Pinned);

            try
            {
                int mixMode = options.UseArithmeticMix ? 0 : 1;
                int result;

                if (isFloat)
                {
                    if (options.UseCppLibrary)
                    {
                        result = ProcessStereoToMonoFloat32(
                            hLeft.AddrOfPinnedObject(),
                            hRight.AddrOfPinnedObject(),
                            hMono.AddrOfPinnedObject(),
                            (UIntPtr)numSamples,
                            mixMode,
                            options.AntiClipping,
                            options.Normalize,
                            options.RemoveDC,
                            options.Dithering,
                            options.ThreadCount);
                    }
                    else
                    {
                        result = ProcessStereoToMonoFloat32_AVX2(
                            hLeft.AddrOfPinnedObject(),
                            hRight.AddrOfPinnedObject(),
                            hMono.AddrOfPinnedObject(),
                            (UIntPtr)numSamples,
                            mixMode,
                            options.AntiClipping,
                            options.Normalize,
                            options.RemoveDC,
                            options.Dithering,
                            options.ThreadCount);
                    }
                }
                else
                {
                    if (options.UseCppLibrary)
                    {
                        result = ProcessStereoToMonoPCM16(
                            hLeft.AddrOfPinnedObject(),
                            hRight.AddrOfPinnedObject(),
                            hMono.AddrOfPinnedObject(),
                            (UIntPtr)numSamples,
                            mixMode,
                            options.AntiClipping,
                            options.Normalize,
                            options.RemoveDC,
                            options.Dithering,
                            options.ThreadCount);
                    }
                    else
                    {
                        result = ProcessStereoToMonoPCM16_AVX2(
                            hLeft.AddrOfPinnedObject(),
                            hRight.AddrOfPinnedObject(),
                            hMono.AddrOfPinnedObject(),
                            (UIntPtr)numSamples,
                            mixMode,
                            options.AntiClipping,
                            options.Normalize,
                            options.RemoveDC,
                            options.Dithering,
                            options.ThreadCount);
                    }
                }

                if (result != 0)
                {
                    throw new Exception("Processing failed with error code: " + result);
                }
            }
            finally
            {
                hLeft.Free();
                hRight.Free();
                hMono.Free();
            }

            // Write output WAV file
            WavHeader outHeader = header;
            outHeader.NumChannels = 1;
            outHeader.BlockAlign = (ushort)(bytesPerSample);
            outHeader.ByteRate = outHeader.SampleRate * outHeader.BlockAlign;
            outHeader.Subchunk2Size = (uint)(numSamples * bytesPerSample);
            outHeader.ChunkSize = 36 + outHeader.Subchunk2Size;

            using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                // Write header
                byte[] outHeaderBytes = new byte[Marshal.SizeOf(typeof(WavHeader))];
                GCHandle hOutHeader = GCHandle.Alloc(outHeaderBytes, GCHandleType.Pinned);
                Marshal.StructureToPtr(outHeader, hOutHeader.AddrOfPinnedObject(), false);
                hOutHeader.Free();
                fs.Write(outHeaderBytes, 0, outHeaderBytes.Length);

                // Write mono audio data
                fs.Write(monoData, 0, monoData.Length);
            }
        }

        private async void btnBenchmark_Click(object sender, EventArgs e)
        {
            btnBenchmark.Enabled = false;
            txtBenchmarkResults.Text = "Running benchmark...\r\n";

            try
            {
                string results = await Task.Run(() => RunBenchmark());
                txtBenchmarkResults.Text = results;
            }
            catch (Exception ex)
            {
                txtBenchmarkResults.Text = $"Benchmark error: {ex.Message}";
            }
            finally
            {
                btnBenchmark.Enabled = true;
            }
        }

        private string RunBenchmark()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Benchmark Results");
            sb.AppendLine("=================");
            sb.AppendLine();

            int[] sampleCounts = { 44100, 441000, 4410000 }; // 1s, 10s, 100s @ 44.1kHz
            int[] threadCounts = { 1, 2, 4, 8, 16 };

            foreach (int samples in sampleCounts)
            {
                sb.AppendLine($"Sample Count: {samples:N0} ({samples / 44100.0:F1}s @ 44.1kHz)");
                sb.AppendLine(new string('-', 70));

                // Allocate test data
                short[] leftPCM = new short[samples];
                short[] rightPCM = new short[samples];
                short[] monoPCM = new short[samples];

                Random rand = new Random(42);
                for (int i = 0; i < samples; i++)
                {
                    leftPCM[i] = (short)rand.Next(-32768, 32767);
                    rightPCM[i] = (short)rand.Next(-32768, 32767);
                }

                GCHandle hLeft = GCHandle.Alloc(leftPCM, GCHandleType.Pinned);
                GCHandle hRight = GCHandle.Alloc(rightPCM, GCHandleType.Pinned);
                GCHandle hMono = GCHandle.Alloc(monoPCM, GCHandleType.Pinned);

                try
                {
                    foreach (int threads in threadCounts)
                    {
                        // Benchmark C++
                        Stopwatch sw = Stopwatch.StartNew();
                        ProcessStereoToMonoPCM16(
                            hLeft.AddrOfPinnedObject(),
                            hRight.AddrOfPinnedObject(),
                            hMono.AddrOfPinnedObject(),
                            (UIntPtr)samples,
                            0, false, false, false, false, threads);
                        sw.Stop();
                        double cppTime = sw.Elapsed.TotalMilliseconds;

                        // Benchmark ASM
                        sw.Restart();
                        ProcessStereoToMonoPCM16_AVX2(
                            hLeft.AddrOfPinnedObject(),
                            hRight.AddrOfPinnedObject(),
                            hMono.AddrOfPinnedObject(),
                            (UIntPtr)samples,
                            0, false, false, false, false, threads);
                        sw.Stop();
                        double asmTime = sw.Elapsed.TotalMilliseconds;

                        double speedup = cppTime / asmTime;
                        sb.AppendLine($"  Threads: {threads,2}  |  C++: {cppTime,8:F2}ms  |  ASM: {asmTime,8:F2}ms  |  Speedup: {speedup:F2}x");
                    }
                }
                finally
                {
                    hLeft.Free();
                    hRight.Free();
                    hMono.Free();
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
