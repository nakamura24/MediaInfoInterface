using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MediaInfoLib;

namespace MediaInfoInterface
{
    public class StreamInfo
    {
        private string version;
        private TimeSpan duration;
        private long bitrate;
        private List<Video> videos = new List<Video>();
        private List<Audio> audios = new List<Audio>();

        public StreamInfo()
        {
            try
            {
                MediaInfo MI = new MediaInfo();
                version = MI.Option("Info_Version", "0.7.0.0;MediaInfoDLL_Example_CS;0.7.0.0");
                if (version.Length == 0)
                {
                    //Logging.Writer(System.Diagnostics.TraceEventType.Warning, "MediaInfoDLL Errer");
                    return;
                }
            }
            catch (Exception ex) { /*Logging.Writer(ex);*/ }
        }

        public StreamInfo(string fullpath)
        {
            try
            {
                MediaInfo MI = new MediaInfo();
                version = MI.Option("Info_Version", "0.7.0.0;MediaInfoDLL_Example_CS;0.7.0.0");
                if (version.Length == 0)
                {
                    //Logging.Writer(System.Diagnostics.TraceEventType.Warning, "MediaInfoDLL Errer");
                    return;
                }
                if (MI.Open(fullpath) == 0)
                {
                    //Logging.Writer(System.Diagnostics.TraceEventType.Warning, "MediaInfoDLL File Open Errer File=" + fullpath);
                    return;
                }
                MI.Option("Complete", "1");
                string result = MI.Inform();
                MI.Option("Inform", "General;%Duration/String3%");
                result = MI.Inform();
                if (result.Length <= 0)
                {
                    //Logging.Writer(System.Diagnostics.TraceEventType.Warning, "MediaInfoDLL Get Duration");
                    return;
                }
                int Hour = int.Parse(result.Substring(0, 2));
                int Minute = int.Parse(result.Substring(3, 2));
                int Second = int.Parse(result.Substring(6, 2));
                this.duration = new TimeSpan(Hour, Minute, Second);
                this.bitrate = long.Parse(MI.Get(StreamKind.General, 0, "OverallBitRate"));
                int count = MI.Count_Get(StreamKind.Video);
                for (int i = 0; i < count; i++)
                {
                    Video video = new Video(MI, i);
                    this.Videos.Add(video);
                }
                count = MI.Count_Get(StreamKind.Audio);
                for (int i = 0; i < count; i++)
                {
                    Audio audio = new Audio(MI,i);
                    this.Audios.Add(audio);
                }
            }
            catch (Exception ex) { /*Logging.Writer(ex);*/ }
        }

        public string Version_MediaInfo { get { return this.version; } }
        public string Version_MediaInfoInterface
        {
            get
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                Version ver = asm.GetName().Version;
                string copyright = ((System.Reflection.AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyCopyrightAttribute))).Copyright;
                return "MediaInfoInterface Ver." + ver.ToString() + " " + copyright;
            }
        }
        public List<string> Streams
        {
            get
            {
                List<string> streams = new List<string>();
                int i = 0;
                foreach (Video video in this.videos)
                {
                    streams.Add("Video:" + i.ToString() + ":Codec: " + video.codec);
                    streams.Add("Video:" + i.ToString() + ":Profile: " + video.profile);
                    if (video.bitrate_mode.Contains("Constant"))
                    {
                        streams.Add("Video:" + i.ToString() + ":bitrate: " + video.bitrate);
                    }
                    else
                    {
                        streams.Add("Video:" + i.ToString() + ":bitrate: " + video.bitrate_mode);
                    }
                    streams.Add("Video:" + i.ToString() + ":WidthxHeight: " + video.angle_of_view);
                    streams.Add("Video:" + i.ToString() + ":Aspect Ratio: " + video.aspect_ratio);
                    if (video.framerate_mode.Contains("Variable"))
                    {
                        streams.Add("Video:" + i.ToString() + ":Frame Rate: " + video.framerate_mode);
                    }
                    else
                    {
                        streams.Add("Video:" + i.ToString() + ":Frame Rate: " + video.framerate);
                    }
                    streams.Add("Video:" + i.ToString() + ":Scan Type: " + video.scan_type);
                    i++;
                }
                foreach (Audio audio in this.audios)
                {
                    streams.Add("Audio:" + i.ToString() + ":Codec: " + audio.codec);
                    //streams.Add("Audio:" + i.ToString() + ":Profile: " + audio.profile);
                    if (audio.bitrate_mode.Contains("Constant"))
                    {
                        streams.Add("Audio:" + i.ToString() + ":bitrate: " + audio.bitrate);
                    }
                    else
                    {
                        streams.Add("Audio:" + i.ToString() + ":bitrate: " + audio.bitrate_mode);
                    }
                    streams.Add("Audio:" + i.ToString() + ":Sampling: " + audio.sampling_frequency);
                    streams.Add("Audio:" + i.ToString() + ":Channels: " + audio.channels);
                    i++;
                }
                return streams;
            }
        }
        public TimeSpan Duration { get { return this.duration; } }
        public long Bitrate { get { return this.bitrate; } }
        public List<Video> Videos { get { return this.videos; } }
        public List<Audio> Audios { get { return this.audios; } }
    }

    public class Video
    {
        public string codec { get; set; }
        public string profile { get; set; }
        public string bitrate_mode { get; set; }
        public string bitrate { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string angle_of_view { get; set; }
        public string aspect_ratio { get; set; }
        public string framerate_mode { get; set; }
        public string framerate { get; set; }
        public string scan_type { get; set; }

        public const int E_Video_Codec = 0x0001;
        public const int E_Video_AngleOfView = 0x0002;
        public const int E_Video_FrameRate = 0x0004;
        public const int E_Video_AspectRatio = 0x0008;

        public Video()
        {
        }

        public Video(MediaInfo MI,int i)
        {
            this.codec = MI.Get(StreamKind.Video, i, "Codec/String");
            this.profile = MI.Get(StreamKind.Video, i, "Codec_Profile");
            this.bitrate_mode = MI.Get(StreamKind.Video, i, "BitRate_Mode/String");
            this.bitrate = MI.Get(StreamKind.Video, i, "BitRate/String");
            this.width = MI.Get(StreamKind.Video, i, "Width");
            this.height = MI.Get(StreamKind.Video, i, "Height");
            this.angle_of_view = this.width + "x" + this.height;
            this.aspect_ratio = MI.Get(StreamKind.Video, i, "DisplayAspectRatio/String");
            this.framerate_mode = MI.Get(StreamKind.Video, i, "FrameRate_Mode/String");
            this.framerate = MI.Get(StreamKind.Video, i, "FrameRate");
            this.scan_type = MI.Get(StreamKind.Video, i, "ScanType");
        }

        public int CheckBluRayRegulation()
        {
            if (!this.codec.Contains("MPEG-2 Video") &&
                !this.codec.Contains("AVC"))
            {
                return E_Video_Codec;
            }
            if (this.angle_of_view.Contains("1920x1080") || this.angle_of_view.Contains("1440x1080"))
            {
                if (!(this.framerate.Contains("59.94") && this.scan_type.Contains("Progressive")) &&
                    !(this.framerate.Contains("50") && this.scan_type.Contains("Progressive")) &&
                    !(this.framerate.Contains("29.97") && this.scan_type.Contains("Interlaced")) &&
                    !(this.framerate.Contains("25") && this.scan_type.Contains("Interlaced")) &&
                    !(this.framerate.Contains("24") && this.scan_type.Contains("Progressive")) &&
                    !(this.framerate.Contains("23.976") && this.scan_type.Contains("Progressive")))
                {
                    return E_Video_FrameRate;
                }
                if (!this.aspect_ratio.Contains("16:9"))
                {
                    return E_Video_AspectRatio;
                }
            }
            else
                if (this.angle_of_view.Contains("1280x720"))
                {
                    if (!(this.framerate.Contains("59.94") && this.scan_type.Contains("Progressive")) &&
                        !(this.framerate.Contains("50") && this.scan_type.Contains("Progressive")) &&
                        !(this.framerate.Contains("24") && this.scan_type.Contains("Progressive")) &&
                        !(this.framerate.Contains("23.976") && this.scan_type.Contains("Progressive")))
                    {
                        return E_Video_FrameRate;
                    }
                    if (!this.aspect_ratio.Contains("16:9"))
                    {
                        return E_Video_AspectRatio;
                    }
                }
                else
                    if (this.angle_of_view.Contains("720x480"))
                    {
                        if (!this.framerate.Contains("29.97") &&
                            !(this.framerate.Contains("24") && this.scan_type.Contains("Progressive")) &&
                            !(this.framerate.Contains("23.976") && this.scan_type.Contains("Progressive")))
                        {
                            return E_Video_FrameRate;
                        }
                        if (!this.aspect_ratio.Contains("16:9") && !this.aspect_ratio.Contains("4:3"))
                        {
                            return E_Video_AspectRatio;
                        }
                    }
                    else
                        if (this.angle_of_view.Contains("720x576"))
                        {
                            if (!this.framerate.Contains("25"))
                            {
                                return E_Video_FrameRate;
                            }
                            if (!this.aspect_ratio.Contains("16:9") && !this.aspect_ratio.Contains("4:3"))
                            {
                                return E_Video_AspectRatio;
                            }
                        }
                        else
                        {
                            return E_Video_AngleOfView;
                        }
            return 0;
        }
    }

    public class Audio
    {
        public string codec { get; set; }
        //public string profile { get; set; }
        public string bitrate_mode { get; set; }
        public string bitrate { get; set; }
        public string sampling_frequency { get; set; }
        public string channels { get; set; }
        public string delay { get; set; }

        public const int E_Audio_Codec = 0x0100;
        public const int E_Audio_SamplingFrequency = 0x0200;

        public Audio(MediaInfo MI, int i)
        {
            this.codec = MI.Get(StreamKind.Audio, i, "Codec/String");
            //this.profile = MI.Get(StreamKind.Audio, i, "Codec_Profile");
            this.bitrate_mode = MI.Get(StreamKind.Audio, i, "BitRate_Mode/String");
            this.bitrate = MI.Get(StreamKind.Audio, i, "BitRate/String");
            this.sampling_frequency = MI.Get(StreamKind.Audio, i, "SamplingRate");
            this.channels = MI.Get(StreamKind.Audio, i, "Channel(s)");
            this.delay = MI.Get(StreamKind.Audio, i, "Video_Delay");
        }

        public int CheckBluRayRegulation()
        {
            if (!this.codec.Contains("AAC") &&
                !this.codec.Contains("AC3") &&
                !this.codec.Contains("PCM") &&
                !this.codec.Contains("truehd"))
            {
                return E_Audio_Codec;
            }
            if (!this.sampling_frequency.Contains("48000") && 
                !this.sampling_frequency.Contains("96000") && 
                !this.sampling_frequency.Contains("182000"))
            {
                return E_Audio_SamplingFrequency;
            }
            return 0;
        }
    }
}
