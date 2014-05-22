using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.Net;
using MediaFoundation.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    class TranscodeSample
    {
        const string sourceFile = @"C:\Users\dean\Documents\Bandicam\bandicam 2014-05-18 20-06-55-540.avi";
        const string destinationFile = @"C:\Users\dean\Documents\Bandicam\sample3.wmv";
        const int VideoBitRate = 3*1000000;
        const int AudioBitRate = 48000;

        //static Guid TARGET_AUDIO_FORMAT = MFMediaType.AAC;
        //static Guid TARGET_VIDEO_FORMAT = MFMediaType.H264;

        static Guid TARGET_AUDIO_FORMAT = MFMediaType.WMAudioV9;
        static Guid TARGET_VIDEO_FORMAT = MFMediaType.WMV3;

        public void Process()
        {
            using (MFSystem.Start())
            {
                var readWriteFactory = new ReadWriteClassFactory();

                var attributes = new Attributes
                {
                    ReadWriterEnableHardwareTransforms = true,
                    SourceReaderEnableVideoProcessing = true,
                };

                var destAttributes = new Attributes
                {
                    ReadWriterEnableHardwareTransforms = true,
                    SourceReaderEnableVideoProcessing = true,
                    MaxKeyFrameSpacing = 3000
                    //TranscodeContainerType = MFTranscodeContainer.Mpeg4
        //            H264Profile = eAVEncH264VProfile._444,
          //          Mpeg2Level = eAVEncH264VLevel.eAVEncH264VLevel1
                };

                var sourceReader = readWriteFactory.CreateSourceReaderFromURL(sourceFile, attributes);
                var sinkWriter = readWriteFactory.CreateSinkWriterFromURL(destinationFile, destAttributes);

            /*    var x = (IMFTransform)new CColorConvertDMO();

                x.SetInputType(0, GetMediaTypeRGB().instance, MFTSetTypeFlags.None).Hr();
                x.SetOutputType(0, GetMediaTypeYUY2().instance, MFTSetTypeFlags.None).Hr();
                MFTOutputStreamInfo info;
                x.GetOutputStreamInfo(0, out info).Hr();


                var samples = new List<Sample>();
                */

                var writeToSink = ConnectStreams(sourceReader, sinkWriter);

                using (sinkWriter.BeginWriting())
                {
                    sourceReader.Samples(sample =>
                        {
                            Console.Clear();
                            Console.WriteLine(TimeSpan.FromSeconds(sample.Timestamp.FromNanoToSeconds()));

                            return writeToSink(sample);
                        });
                }
            }
        }

        /*          if (sample.Sample != null && sample.Stream.NativeMediaType.IsVideo)
                            {
                                x.ProcessInput(0, sample.Sample.instance, 0).Hr();

                                var sample2 = Sample.Create();
                                var buffer = MFMediaBuffer.CreateMemoryBuffer(info.cbSize);
                                sample2.instance.AddBuffer(buffer.instance);

                                var outSamples = new MFTOutputDataBuffer[1];
                                outSamples[0].dwStreamID = 0;
                                outSamples[0].dwStatus = MFTOutputDataBufferFlags.None;
                                outSamples[0].pSample = Marshal.GetComInterfaceForObject(sample2.instance, typeof(IMFSample));
                                
                                ProcessOutputStatus status;
                                x.ProcessOutput(MFTProcessOutputFlags.None, 1, outSamples, out status).Hr();

                                sample.Sample = sample2;
                                var result = writeToSink(sample);

                                samples.Add(sample2);
                                if( samples.Count > 120)
                                {
                                    var b = samples.First();
                                    b.instance.RemoveAllBuffers();
                                    samples.RemoveAt(0);
                                    b.Dispose();
                                }

                                //sample2.instance.RemoveAllBuffers().Hr();
                                buffer.Dispose();
                                //sample2.Dispose();

                                return result;
                            }
                            else
                    */            
        private ProcessSample ConnectAudioStreams(SourceReader sourceReader, SinkWriter sinkWriter)
        {
            var sourceAudioStream = SetAudioMediaType(sourceReader);

            var sinkAudioStream = AddStream(sinkWriter, sourceAudioStream.CurrentMediaType, CreateTargetAudioMediaType(sourceAudioStream.NativeMediaType));

            return AVOperations.MediaTypeChange(sinkAudioStream, AVOperations.SaveTo(sinkAudioStream));
        }

        private ProcessSample ConnectVideoStreams(SourceReader sourceReader, SinkWriter sinkWriter)
        {
            var sourceVideoStream = SetVideoMediaType(sourceReader);
            /*sourceVideoStream.CurrentMediaType*/
            //GetMediaTypeYUY2()
            var sinkVideoStream = AddStream(sinkWriter, sourceVideoStream.CurrentMediaType, CreateTargetVideoMediaType(sourceVideoStream.NativeMediaType));

            return AVOperations.MediaTypeChange(sinkVideoStream, AVOperations.SaveTo(sinkVideoStream));
        }

        private ProcessSample ConnectStreams(SourceReader sourceReader, SinkWriter sinkWriter)
        {
            return AVOperations.SeperateAudioVideo(
                ConnectAudioStreams(sourceReader, sinkWriter),
                ConnectVideoStreams(sourceReader, sinkWriter));
        }

        SourceStream SetAudioMediaType(SourceReader sourceReader)
        {
            var sourceStream = sourceReader.Streams.First(s => s.IsSelected && s.NativeMediaType.IsAudio);

            sourceStream.CurrentMediaType = new MediaType() { MajorType = MFMediaType.Audio, SubType = MFMediaType.PCM };

            return sourceStream;
        }

        SourceStream SetVideoMediaType(SourceReader sourceReader)
        {
            var sourceStream = sourceReader.Streams.First(s => s.IsSelected && s.NativeMediaType.IsVideo);

            sourceStream.CurrentMediaType = GetMediaTypeRGB();

            return sourceStream;
        }

        private static MediaType GetMediaTypeRGB()
        {
            return new MediaType() { 
                MajorType = MFMediaType.Video,
                SubType = MFMediaType.RGB32,
                AspectRatio = new AspectRatio(1,1),
                FrameSize = new System.Drawing.Size(1280, 720),
                InterlaceMode = MFVideoInterlaceMode.Progressive
            };
        }

        private static MediaType xGetMediaTypeYUY2()
        {
            return new MediaType()
            {
                MajorType = MFMediaType.Video,
                SubType = MFMediaType.YUY2,
                AspectRatio = new AspectRatio(1, 1),
                FrameSize = new System.Drawing.Size(1280, 720),
                InterlaceMode = MFVideoInterlaceMode.Progressive
            };
        }

        SinkStream AddStream(SinkWriter sinkWriter, MediaType input, MediaType encoding)
        {
            var sinkStream = sinkWriter.AddStream(encoding);
            sinkStream.InputMediaType = input;
            return sinkStream;
        }

        MediaType CreateTargetAudioMediaType(MediaType nativeMediaType)
        {
            var numberOfChannels = nativeMediaType.AudioNumberOfChannels;
            var sampleRate = nativeMediaType.AudioSamplesPerSecond;

            var availableTypes = MFSystem.TranscodeGetAudioOutputAvailableTypes(TARGET_AUDIO_FORMAT, MFT_EnumFlag.All);

            var type = availableTypes
                .FirstOrDefault(t => t.AudioNumberOfChannels == numberOfChannels &&
                    t.AudioSamplesPerSecond == sampleRate &&
                    t.AudioAverageBytesPerSecond == AudioBitRate);

            type = availableTypes
                .FirstOrDefault(t => t.AudioNumberOfChannels == numberOfChannels &&
                    t.AudioSamplesPerSecond == sampleRate);

            if (type != null)
                return new MediaType(type);

            throw new Exception("Unable to find target audio format");
        }

        MediaType CreateTargetVideoMediaType(MediaType nativeMediaType)
        {
            var size = nativeMediaType.FrameSize;
            var rate = nativeMediaType.FrameRate;
            var aspect = nativeMediaType.AspectRatio;
            var bitRate = VideoBitRate;

            var mediaType = new MediaType()
            {
                MajorType = MFMediaType.Video,
                SubType = TARGET_VIDEO_FORMAT,
                FrameSize = size,
                FrameRate = rate,
                AspectRatio = aspect,
                BitRate = bitRate,
                InterlaceMode = MFVideoInterlaceMode.Progressive
            };

            return mediaType;
        }

    }
}
