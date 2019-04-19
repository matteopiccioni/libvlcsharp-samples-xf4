﻿using LibVLCSharp.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gestures
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThreeSixty : ContentPage
	{
        LibVLC _libVLC;
        Media _media;
        MediaPlayer _mediaPlayer;

		public ThreeSixty ()
		{
			InitializeComponent();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _libVLC = new LibVLC();
            _media = new Media(_libVLC, "https://streams.videolan.org/streams/360/eagle_360.mp4", FromType.FromLocation);
            if (Device.RuntimePlatform == Device.Android)
            {
                var mc = new MediaConfiguration();
                mc.EnableHardwareDecoding();
                _media.AddOption(mc);
            }

            _mediaPlayer = new MediaPlayer(_media);
            videoView.MediaPlayer = _mediaPlayer;
            videoView.MediaPlayer.Play();            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            videoView.MediaPlayer.Stop();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            ScreenWidth = width;
            ScreenHeight = height;
        }

        MediaPlayer MediaPlayer => videoView.MediaPlayer;
        
        bool Is360Video => _media.Tracks[_mediaPlayer.VideoTrack].Data.Video.Projection == VideoProjection.Equirectangular;

        void PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    if (ScreenWidth > 0 && ScreenHeight > 0)
                    {
                        double range = Math.Max(ScreenWidth, ScreenHeight);
                        float yaw = (float)(Fov * -e.TotalX / range);// up/down
                        float pitch = (float)(Fov * -e.TotalY / range);// left/right
                        MediaPlayer.UpdateViewpoint(Yaw + yaw, Pitch + pitch, Roll, Fov);
                    }
                    break;
                case GestureStatus.Started:
                case GestureStatus.Canceled:
                    break;
                case GestureStatus.Completed:
                    Fov = MediaPlayer.Viewpoint.Fov;
                    Pitch = MediaPlayer.Viewpoint.Pitch;
                    Roll = MediaPlayer.Viewpoint.Roll;
                    Yaw = MediaPlayer.Viewpoint.Yaw;
                    break;
            }
        }

        /// <summary>
        /// Use Xamarin.Forms.Page OnSizeAllocated to set this
        /// </summary>
        public double ScreenHeight { get; internal set; }

        /// <summary>
        /// Use Xamarin.Forms.Page OnSizeAllocated to set this
        /// </summary>
        public double ScreenWidth { get; internal set; }

        /// <summary>
        /// view point yaw in degrees  ]-180;180]
        /// </summary>
        public float Yaw { get; internal set; }

        /// <summary>
        /// view point pitch in degrees  ]-90;90]
        /// </summary>
        public float Pitch { get; internal set; }

        /// <summary>
        /// view point roll in degrees ]-180;180]
        /// </summary>
        public float Roll { get; internal set; } = 0.0f;

        /// <summary>
        /// field of view in degrees ]0;180[ (default 80.)
        /// </summary>
        public float Fov { get; internal set; } = 80.0f;
    }
}