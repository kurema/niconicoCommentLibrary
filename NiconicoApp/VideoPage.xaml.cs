using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Niconico.Comment;
using Niconico.Comment.Primitive;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace NiconicoApp
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        private DispatcherTimer _timer;

        public VideoPage()
        {
            this.InitializeComponent();

            mediaElement.AutoPlay = true;
            SetMovieSource();

            var cmp = new CommentPresenter();
            cmp.Load("test.nicocomment");

            var canvast = new CanvasTarget(canvas)
            {
                Fps = textBlockFps
            };
            var videot = new VideoTarget(mediaElement);

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1.0 / 30)
            };
            _timer.Tick += (f, e) =>
            {
                cmp.Draw(canvast, videot);
            };
            _timer.Start();
        }

        public async void SetMovieSource()
        {
            var installedFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var file = await installedFolder.GetFileAsync("movie.mp4");
            mediaElement.Source = new Windows.Media.Playback.MediaPlaybackItem(Windows.Media.Core.MediaSource.CreateFromStorageFile(file));

        }

        public class CanvasTarget : Niconico.Comment.CommentPresenter.ICanvas
        {
            private Dictionary<int, TextBlock> textBlocks = new Dictionary<int, TextBlock>();
            private Dictionary<int, TextBlock> textBlocksShadow = new Dictionary<int, TextBlock>();
            private List<int> textBlocksOperated = new List<int>();

            public Canvas Content { get; private set; }
            public CanvasTarget(Canvas content) { this.Content = content; }

            public double Width => Content.ActualWidth;

            public double Height => Content.ActualHeight;

            public double FontSize { get; set; }
            public CommentPresenter.Baseline Baseline { get; set; }
            public CommentPresenter.FontKind FontKind { get; set; }

            private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            public TextBlock Fps;

            public void Init()
            {
                //Content.Children.Clear();
                textBlocksOperated = new List<int>();

                Content.InvalidateArrange();
                Content.InvalidateMeasure();

                sw.Stop();
                Fps.Text = ""+ 1000.0/ sw.ElapsedMilliseconds;
                sw.Reset();
                sw.Start();
            }

            public void FillText(Comment text, Coordinate coord, Color color, bool overFlow)
            {

                double yOffset = 0;
                switch (Baseline)
                {
                    case CommentPresenter.Baseline.Bottom: yOffset = -FontSize; break;
                    case CommentPresenter.Baseline.Middle: yOffset = -FontSize / 2.0; break;
                    case CommentPresenter.Baseline.Top: yOffset = 0; break;
                }
                if (textBlocksShadow.ContainsKey(text.Count))
                {
                    var tb2 = textBlocksShadow[text.Count];
                    if (!overFlow)
                    {
                        if (!Content.Children.Contains(tb2)) { Content.Children.Insert(0,tb2); }
                        Canvas.SetLeft(tb2, coord.X - 1);
                        Canvas.SetTop(tb2, coord.Y + yOffset);
                        if (tb2.FontSize != FontSize)
                        {
                            tb2.FontSize = FontSize;
                        }
                    }
                    else
                    {
                        textBlocksShadow.Remove(text.Count);
                        Content.Children.Remove(tb2);
                    }
                }
                else if (!overFlow)
                {
                    {
                        var tb2 = GetTextBlock(text.Text);
                        tb2.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
                        Canvas.SetLeft(tb2, coord.X - 1);
                        Canvas.SetTop(tb2, coord.Y + yOffset);
                        Content.Children.Insert(0, tb2);

                        textBlocksShadow.Add(text.Count, tb2);
                    }
                }
                if (textBlocks.ContainsKey(text.Count))
                {
                    var tb2 = textBlocks[text.Count];
                    if (!Content.Children.Contains(tb2)) { Content.Children.Add(tb2); }
                    tb2.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)(overFlow ? 100 : 255), (byte)color.R, (byte)color.G, (byte)color.B));
                    Canvas.SetLeft(tb2, coord.X);
                    Canvas.SetTop(tb2, coord.Y + yOffset);
                    if (tb2.FontSize != FontSize)
                    {
                        tb2.FontSize = FontSize;
                    }

                    textBlocksOperated.Add(text.Count);
                    return;
                }

                var tb = GetTextBlock(text.Text);
                tb.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)(overFlow?100:255), (byte)color.R, (byte)color.G, (byte)color.B));
                Canvas.SetLeft(tb, coord.X);
                Canvas.SetTop(tb, coord.Y+yOffset);
                Content.Children.Add(tb);

                textBlocks.Add(text.Count, tb);
                textBlocksOperated.Add(text.Count);
            }

            public Niconico.Comment.Primitive.Size MeasureText(Comment comment)
            {
                var tb = GetTextBlock(comment.Text);
                tb.Measure(new Windows.Foundation.Size(double.MaxValue, double.MaxValue));
                if (!textBlocks.ContainsKey(comment.Count)) textBlocks.Add(comment.Count, tb);
                return new Niconico.Comment.Primitive.Size(tb.ActualWidth, tb.ActualHeight);
            }

            private TextBlock GetTextBlock(string text)
            {
                var tb = new TextBlock()
                {
                    Text = text,
                    FontSize = FontSize,
                    FontFamily = new FontFamily("Arial"),
                    FontWeight = new Windows.UI.Text.FontWeight() { Weight = (ushort)600 },
                    IsHitTestVisible = false
                };
                return tb;
            }

            public void Done()
            {
                var removeTarget = new List<int>();
                foreach(var item in textBlocks)
                {
                    if (!textBlocksOperated.Contains(item.Key))
                    {
                        removeTarget.Add(item.Key);
                    }
                }
                foreach(var item in removeTarget)
                {
                    var tb = textBlocks[item];
                    Content.Children.Remove(tb);
                    textBlocks.Remove(item);

                    if (textBlocksShadow.ContainsKey(item))
                    {
                        Content.Children.Remove(textBlocksShadow[item]);
                        textBlocksShadow.Remove(item);
                    }
                }

                Content.UpdateLayout();
            }
        }

        public class VideoTarget : CommentPresenter.IVideo
        {
            public MediaPlayerElement Content { get; private set; }

            public VideoTarget(MediaPlayerElement mediaElement)
            {
                this.Content = mediaElement;
            }

            public double CurrentSecond => Content.MediaPlayer.PlaybackSession.Position.TotalSeconds;

            public double Duration => Content.MediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
        }
    }
}
