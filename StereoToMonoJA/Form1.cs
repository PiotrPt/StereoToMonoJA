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

            // NEW: reject mono input files early (read header on UI thread)
            try
            {
                int channels = GetNumChannelsFromWav(txtInputFile.Text);
                if (channels == 1)
                {
                    MessageBox.Show("Input file is already mono (1 channel). Please select a stereo WAV file.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot read WAV header: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // Read number of channels from WAV file by parsing the 'fmt ' chunk. Throws on invalid files.
        private int GetNumChannelsFromWav(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // RIFF
                var riff = br.ReadBytes(4);
                if (riff.Length < 4 || System.Text.Encoding.ASCII.GetString(riff) != "RIFF")
                    throw new Exception("Missing RIFF header");

                br.ReadUInt32(); // file size
                var wave = br.ReadBytes(4);
                if (wave.Length < 4 || System.Text.Encoding.ASCII.GetString(wave) != "WAVE")
                    throw new Exception("Missing WAVE header");

                // search chunks
                while (fs.Position + 8 <= fs.Length)
                {
                    string chunkId = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
                    uint chunkSize = br.ReadUInt32();

                    if (chunkId == "fmt ")
                    {
                        if (chunkSize < 16) throw new Exception("Invalid fmt chunk");
                        ushort audioFormat = br.ReadUInt16();
                        ushort numChannels = br.ReadUInt16();
                        return (int)numChannels;
                    }

                    // skip chunk data (word aligned)
                    long next = fs.Position + chunkSize;
                    if ((chunkSize & 1) == 1) next++;
                    fs.Position = next;
                }

                throw new Exception("fmt chunk not found");
            }
        }

        private void ProcessWavFile(string inputPath, string outputPath, ProcessingOptions options)
        {
            // Robust WAV reader: find "fmt " and "data" chunks (skips extra chunks like LIST, JUNK, fact)
            ushort audioFormat = 0;
            ushort numChannels = 0;
            uint sampleRate = 0;
            ushort bitsPerSample = 0;
            uint byteRate = 0;
            ushort blockAlign = 0;
            long dataChunkPos = -1;
            uint dataChunkSize = 0;

            using (FileStream fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                // RIFF header
                var riff = br.ReadBytes(4);
                if (riff.Length < 4 || System.Text.Encoding.ASCII.GetString(riff) != "RIFF")
                    throw new Exception("Invalid WAV file: missing RIFF.");

                br.ReadUInt32(); // file size
                var wave = br.ReadBytes(4);
                if (wave.Length < 4 || System.Text.Encoding.ASCII.GetString(wave) != "WAVE")
                    throw new Exception("Invalid WAV file: missing WAVE.");

                // Read chunks until we find 'fmt ' and 'data'
                while (fs.Position + 8 <= fs.Length)
                {
                    string chunkId = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
                    uint chunkSize = br.ReadUInt32();

                    long nextChunk = fs.Position + chunkSize;
                    // align to WORD (chunks are word-aligned)
                    if ((chunkSize & 1) == 1) nextChunk++;

                    if (chunkId == "fmt ")
                    {
                        // read format info (minimum 16 bytes)
                        if (chunkSize < 16) throw new Exception("Invalid fmt chunk.");

                        audioFormat = br.ReadUInt16();
                        numChannels = br.ReadUInt16();
                        sampleRate = br.ReadUInt32();
                        byteRate = br.ReadUInt32();
                        blockAlign = br.ReadUInt16();
                        bitsPerSample = br.ReadUInt16();

                        // If there are extra fmt bytes (WAVE_FORMAT_EXTENSIBLE or others), read them
                        int fmtExtra = (int)chunkSize - 16;
                        if (fmtExtra > 0)
                        {
                            byte[] extra = br.ReadBytes(fmtExtra);
                            // handle WAVE_FORMAT_EXTENSIBLE -> identify float subformat GUID
                            if (audioFormat == 0xFFFE && fmtExtra >= 22)
                            {
                                // extra layout: cbSize(2) + validBitsPerSample(2) + channelMask(4) + subFormat(16)
                                // subFormat starts at offset (fmtExtra - 16) from the start of extra only if cbSize >= 22
                                // but here extra[0..] contains cbSize first; we locate GUID at offset 4
                                int guidOffset = 4;
                                if (extra.Length >= guidOffset + 16)
                                {
                                    byte[] subGuid = new byte[16];
                                    Array.Copy(extra, guidOffset + 4, subGuid, 0, 12); // sometimes layout varies; fall back to best-effort below
                                }
                                // Simpler fallback: if bitsPerSample == 32 assume float for extensible
                            }
                            // nothing else required here - we just skip the extra bytes
                        }
                    }
                    else if (chunkId == "data")
                    {
                        dataChunkPos = fs.Position;
                        dataChunkSize = chunkSize;
                        // we found data - break (we may want fmt as well, ensure fmt was read earlier)
                        break;
                    }
                    

                    fs.Position = nextChunk;
                }
            }

            if (dataChunkPos < 0) throw new Exception("WAV 'data' chunk not found.");
            if (numChannels != 2) throw new Exception("Input file must be stereo (2 channels).");

            // Determine format: accept PCM (1) 16-bit or FLOAT (3) 32-bit.
            bool isFloat = false;
            if (audioFormat == 1 && bitsPerSample == 16)
            {
                isFloat = false;
            }
            else if (audioFormat == 3 && bitsPerSample == 32)
            {
                isFloat = true;
            }
            else if (audioFormat == 0xFFFE && bitsPerSample == 32)
            {
                // WAVE_FORMAT_EXTENSIBLE with 32 bits - treat as float if Audacity exported float
                // (best-effort) - assume float if BitsPerSample==32
                isFloat = true;
            }
            else
            {
                throw new Exception($"Unsupported WAV format: AudioFormat={audioFormat}, BitsPerSample={bitsPerSample}");
            }

            int bytesPerSample = bitsPerSample / 8;
            uint numSamples = dataChunkSize / blockAlign; // frames (samples per channel)

            // Read audio data from data chunk
            byte[] audioData = new byte[dataChunkSize];
            using (FileStream fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(dataChunkPos, SeekOrigin.Begin);
                int read = 0;
                while (read < audioData.Length)
                {
                    int r = fs.Read(audioData, read, audioData.Length - read);
                    if (r == 0) break;
                    read += r;
                }
                if (read != audioData.Length)
                    throw new Exception("Could not read WAV data chunk fully.");
            }

            // Allocate buffers
            int sampleSize = isFloat ? sizeof(float) : sizeof(short);
            byte[] leftData = new byte[numSamples * sampleSize];
            byte[] rightData = new byte[numSamples * sampleSize];
            byte[] monoData = new byte[numSamples * sampleSize];

            // De-interleave stereo to separate channels (interleaved LRLR...)
            for (uint i = 0; i < numSamples; i++)
            {
                int srcOffset = (int)(i * blockAlign);
                int dstOffset = (int)(i * sampleSize);

                // copy left channel
                Array.Copy(audioData, srcOffset, leftData, dstOffset, bytesPerSample);
                // copy right channel
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

            // Prepare output header (mono)
            WavHeader outHeader = new WavHeader();
            outHeader.ChunkID = 0x46464952; // "RIFF"
            outHeader.Format = 0x45564157;  // "WAVE"
            outHeader.Subchunk1ID = 0x20746D66; // "fmt "
            outHeader.Subchunk1Size = 16;
            outHeader.AudioFormat = (ushort)(isFloat ? 3 : 1);
            outHeader.NumChannels = 1;
            outHeader.SampleRate = sampleRate;
            outHeader.BitsPerSample = bitsPerSample;
            outHeader.BlockAlign = (ushort)(outHeader.NumChannels * (outHeader.BitsPerSample / 8));
            outHeader.ByteRate = outHeader.SampleRate * outHeader.BlockAlign;
            outHeader.Subchunk2ID = 0x61746164; // "data"
            outHeader.Subchunk2Size = (uint)(numSamples * outHeader.BlockAlign);
            outHeader.ChunkSize = 36 + outHeader.Subchunk2Size;

            using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {

                byte[] outHeaderBytes = new byte[Marshal.SizeOf(typeof(WavHeader))];
                GCHandle hOutHeader = GCHandle.Alloc(outHeaderBytes, GCHandleType.Pinned);
                Marshal.StructureToPtr(outHeader, hOutHeader.AddrOfPinnedObject(), false);
                hOutHeader.Free();
                fs.Write(outHeaderBytes, 0, outHeaderBytes.Length);

               
                fs.Write(monoData, 0, monoData.Length);
            }
        }

        
    }
}
