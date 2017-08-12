using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using NiconicoComment;
using NiconicoComment.Primitive;

namespace NiconicoAndroid
{
    public class CommentView : View,CommentBuffer.CanvasBuffered
    {
        public CommentPresenter.FontKind FontKind { get; set; }

        double CommentBuffer.CanvasBuffered.Width => this.Width;

        double CommentBuffer.CanvasBuffered.Height => this.Height;

        public CommentBuffer.FillCommand[] Commands { get; set; }

        Paint _Paint = new Paint();

        public CommentView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public CommentView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
        }

        private int i = 1;

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (Commands != null)
            {
                foreach (var item in Commands)
                {
                    _Paint.TextSize = (float)item.FontSize;
                    double yoffset = item.FontSize * CommentPresenter.GetYOffsetFromBaseline(item.Baseline);
                    _Paint.Color = new Android.Graphics.Color((byte)item.Color.R, (byte)item.Color.G, (byte)item.Color.B, item.OverFlow ? 200 : (byte)(Math.Min(item.Color.Alpha * 256, 255)));
                    canvas.Save();
                    canvas.DrawText(item.Comment.Text, (float)item.Coordinate.X, (float)(item.Coordinate.Y + yoffset), _Paint);
                    canvas.Restore();
                }
            }
        }

        public void Updated()
        {
        }

        public NiconicoComment.Primitive.Size MeasureText(Comment comment, double fontSize, CommentPresenter.FontKind fontKind)
        {
            _Paint.TextSize = (float)fontSize;
            var w = _Paint.MeasureText(comment.Text);
            return new NiconicoComment.Primitive.Size(w, fontSize);
        }
    }
}