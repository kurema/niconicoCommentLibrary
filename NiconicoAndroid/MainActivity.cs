using Android.App;
using Android.Widget;
using Android.OS;
using System;

namespace NiconicoAndroid
{
    [Activity(Label = "NiconicoAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private System.Timers.Timer timer;
        private Handler mHandler = new Handler();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            VideoView videoView = (VideoView)this.FindViewById(Resource.Id.videoView);
            videoView.SetVideoURI(Android.Net.Uri.Parse(""));
            videoView.SetMediaController(new Android.Widget.MediaController(this));
            videoView.Start();
            var video = new VideoHandler(videoView);
            
            CommentView commentView=(CommentView)this.FindViewById(Resource.Id.commentView);

            var commentAsset = Assets.Open("test.nicocomment");

            var presenter = new NiconicoComment.CommentPresenter();
            presenter.Load(commentAsset);

            var canvas = new NiconicoComment.CommentBuffer();
            canvas.Content = commentView;


            var timer = new System.Threading.Timer((o) => 
            {
                presenter.Draw(canvas, video);
                RunOnUiThread(() =>
                commentView.Invalidate());
            },null,0,100);
        }

        public class VideoHandler : NiconicoComment.CommentPresenter.Video
        {
            public VideoView Content;
            public VideoHandler(VideoView v)
            {
                this.Content = v;
            }

            public double CurrentSecond => Content.CurrentPosition / 1000.0;

            public double Duration => Content.Duration/1000.0;
        }
    }
}

