using System;
using System.Collections.Generic;
using NiconicoComment.Primitive;

namespace NiconicoComment
{
    public class CommentPresenter
    {
        public class CommentInformation
        {
            public CommentInformation(Comment content)
            {
                this.Content = content;
            }

            public Comment Content;
            public RelativeValue LastY = new RelativeValue();
            public RelativeSquare Size = new RelativeSquare();
            public RelativeValue FontSize = new RelativeValue();
        }
        public class RelativeValue
        {
            public RelativeValue(double actual,Canvas canvas)
            {
                _Relative = GetRelativeSize(actual, canvas);
                Unset = false;
            }
            public RelativeValue(double relative)
            {
                _Relative = relative;
                Unset = false;
            }
            public RelativeValue()
            {
                _Relative = 0;
                Unset = true;
            }

            public double Relative { get { return _Relative; } set { _Relative = value; Unset = false; } }
            private double _Relative;
            public bool Unset { get; private set; } 
            public double GetActual(Canvas canvas)
            {
                return GetActualSize(Relative, canvas);
            }
            public void SetActual(double actual,Canvas canvas)
            {
                this.Relative = GetRelativeSize(actual, canvas);
            }
        }
        public class RelativeSquare
        {
            public RelativeSquare() { }
            public RelativeSquare(RelativeValue width, RelativeValue height) { this.Width = width;this.Height = height; }
            public RelativeSquare(double width, double height) { this.Width = new RelativeValue(width); this.Height = new RelativeValue(height); }
            public RelativeSquare(double width, double height,Canvas canvas) { this.Width = new RelativeValue(width,canvas); this.Height = new RelativeValue(height, canvas); }

            public RelativeValue Width = new RelativeValue();
            public RelativeValue Height = new RelativeValue();
        }
        public interface Canvas
        {
            void Init();
            double Width { get; }
            double Height { get; }
            FontKind FontKind { get; set; }
            double FontSize { get; set; }
            Size MeasureText(Comment comment);
            void FillText(Comment comment, Coordinate coord, Color color,bool overFlow);
            Baseline Baseline { get; set; }
            void Done();
        }
        public interface Video
        {
            double CurrentSecond { get; }
            double Duration { get; }
        }
        public enum Baseline
        {
            Top,Bottom,Middle
        }
        public enum FontKind
        {
            Default,Mincho,Gothic
        }

        public static double GetYOffsetFromBaseline(Baseline arg)
        {
            switch (arg)
            {
                case Baseline.Top:return 0;
                case Baseline.Middle:return 0.5;
                case Baseline.Bottom:return 1;
                default:return 0;
            }
        }

        private CommentInformation[] Comments;
        private Comment[] CommentsLoaded;

        public void Load(Comment[] comments)
        {
            CommentsLoaded = comments;
            var coms = new List<CommentInformation>();
            foreach (var comment in CommentsLoaded)
            {
                coms.Add(new CommentInformation(comment));
            }
            Comments = coms.ToArray();
        }

        public void Load(string uri)
        {
            Load(Comment.ParseXml(uri));
        }

        public void Load(System.Xml.XmlReader xr)
        {
            Load(Comment.ParseXml(xr));
        }

        public void Load(System.IO.Stream sr)
        {
            Load(System.Xml.XmlReader.Create(sr));
        }

        public RelativeValue FontSizeDefault = new RelativeValue(30);
        public RelativeValue FontSizeSmall = new RelativeValue(18);
        public RelativeValue FontSizeBig = new RelativeValue(45);

        public double CommentDuration=5.0;

        public static void SetFontSizeRelative(ref Canvas canvas,RelativeValue size)
        {
            canvas.FontSize = size.GetActual(canvas);
        }

        public static double GetRelativeSize(double ActualSize,Canvas canvas)
        {
            return ActualSize * 855 / canvas.Width;
        }
        public static double GetActualSize(double RelativeSize, Canvas canvas)
        {
            return RelativeSize / 855 * canvas.Width;
        }

        public void Draw(Canvas canvas,Video video)
        {
            canvas.Init();
            SetFontSizeRelative(ref canvas, FontSizeDefault);
            var time = video.CurrentSecond;
            canvas.FontKind = FontKind.Default;

            var RangesTop = new List<Range>();
            var RangesBottom = new List<Range>();

            double commentDuration = this.CommentDuration;
            var operatedComments = new List<CommentInformation>();

            foreach (var comment in Comments)
            {
                var vpos = GetActualVpos(comment.Content.Vpos, video.Duration, commentDuration);
                if (time - commentDuration >= vpos / 100.0 || vpos / 100.0 >= time) { continue; }
                var mails = comment.Content.Mail.Split(' ');
                if (ContainsCommand(mails, "invisible")) { continue; }

                if (!comment.FontSize.Unset){ }
                else if (ContainsCommand(mails, "small")) comment.FontSize = FontSizeSmall;
                else if (ContainsCommand(mails, "big")) comment.FontSize = FontSizeBig;
                else comment.FontSize = FontSizeDefault;
                SetFontSizeRelative(ref canvas, comment.FontSize);
                var fontSizeActual = comment.FontSize.GetActual(canvas);

                if (ContainsCommand(mails, "mincho"))
                {
                    canvas.FontKind = FontKind.Mincho;
                }
                else if (ContainsCommand(mails, "gothic"))
                {
                    canvas.FontKind = FontKind.Gothic;
                }
                else
                {
                    canvas.FontKind = FontKind.Default;
                }

                var r = new Coordinate();

                var commentSize = new Size();
                if (! comment.Size.Width.Unset)
                {
                    commentSize = new Size(comment.Size.Width.GetActual( canvas), comment.Size.Height.GetActual(canvas));
                }
                else
                {
                    commentSize = canvas.MeasureText(comment.Content);
                    comment.Size = new RelativeSquare(commentSize.Width, commentSize.Height, canvas);
                }

                if (ContainsCommand(mails, "ue") || ContainsCommand(mails, "shita"))
                {
                    if (commentSize.Width > canvas.Width)
                    {
                        comment.FontSize.Relative *= canvas.Width / commentSize.Width;
                        comment.FontSize.Relative = Math.Max(comment.FontSize.Relative, GetRelativeSize(1, canvas));
                        SetFontSizeRelative(ref canvas, comment.FontSize);

                        commentSize = canvas.MeasureText(comment.Content);
                        comment.Size = new RelativeSquare(commentSize.Width, commentSize.Height, canvas);
                    }
                }

                bool overFlow = false;
                if (ContainsCommand(mails, "ue"))
                {
                    r = new Coordinate(canvas.Width / 2.0 - commentSize.Width / 2.0, Math.Max(0, comment.LastY.GetActual(canvas)));
                    var changed = false;
                    while(! CheckRanges(RangesTop,new Range(r.Y, r.Y + fontSizeActual)))
                    {
                        r.Y += fontSizeActual;
                        changed = true;
                    }
                    if (changed)
                    {
                        foreach(var range in RangesTop)
                        {
                            if(range.B<r.Y && CheckRanges(RangesTop,new Range(range.B, range.B+fontSizeActual)))
                                r.Y = range.B;
                        }
                    }
                    canvas.Baseline = Baseline.Top;
                    comment.LastY = new RelativeValue(r.Y, canvas);
                    RangesTop.Add(new Range(r.Y, r.Y + fontSizeActual));

                    overFlow = FixOverflow(ref r.Y, canvas.Height,canvas);
                }
                else if (ContainsCommand(mails, "shita"))
                {
                    r = new Coordinate(canvas.Width / 2.0 - commentSize.Width / 2.0, Math.Max(0, comment.LastY.GetActual(canvas)));
                    var changed = false;
                    while (!CheckRanges(RangesBottom, new Range(r.Y, r.Y + fontSizeActual)))
                    {
                        r.Y += fontSizeActual;
                        changed = true;
                    }
                    if (changed)
                    {
                        foreach (var range in RangesBottom)
                        {
                            if (range.B < r.Y && CheckRanges(RangesBottom, new Range(range.B, range.B+fontSizeActual)))
                                r.Y = range.B;
                        }
                    }
                    canvas.Baseline = Baseline.Bottom;
                    comment.LastY = new RelativeValue(r.Y, canvas);
                    RangesBottom.Add(new Range(r.Y, r.Y + fontSizeActual));

                    overFlow = FixOverflow(ref r.Y, canvas.Height,canvas);

                    r.Y = canvas.Height - r.Y;
                }
                else
                {
                    r = GetFromOperated(operatedComments, comment, video.Duration, commentDuration, time, canvas, FontSizeDefault);
                    comment.LastY = new RelativeValue(r.Y, canvas);
                    canvas.Baseline = Baseline.Middle;

                    overFlow = FixOverflow(ref r.Y, canvas.Height,canvas);

                    r.Y += fontSizeActual / 2.0;
                    operatedComments.Add(comment);
                }
                var color = GetColorFromMail(comment.Content.Mail);
                canvas.FillText(comment.Content, r, color, overFlow);
            }
            canvas.Done();
        }

        public bool FixOverflow(ref double y,double canvasHeight,Canvas canvas)
        {
            bool result = false;
            var modnum = canvasHeight - FontSizeDefault.GetActual(canvas);
            if (y > modnum) { result = true; }
            y %= modnum;
            return result;
        }

        public bool ContainsCommand(string mails, string key)
        {
            return ContainsCommand(mails.Split(' '), key);
        }

        public bool ContainsCommand(string[] mails,string key)
        {
            var keyLow = key.ToLower();
            foreach(var mail in mails)
            {
                if (mail == keyLow) return true;
            }
            return false;
        }

        public Color GetColorFromMail(string text)
        {
            var mails = text.Split(' ');
            var commandDic = CommandColorDictionary;
            foreach (var mail in mails)
            {
                if (commandDic.ContainsKey(mail.ToLower()))
                    return commandDic[mail.ToLower()];
                var col = Color.Parse(mail);
                if (col.HasValue)
                    return col.Value;
            }
            return new Color(255, 255, 255);
        }

        public static Dictionary<string, Color> CommandColorDictionary
        {
            get
            {
                return new Dictionary<string, Color>()
                {
                    { "black", new Color("#000000")},
                    { "red", new Color("#ff0000") },
                    { "pink", new Color("#FF8080")},
                    { "orange", new Color("#FFC000")},
                    { "yellow", new Color("#FFFF00")},
                    { "green", new Color("#00FF00")},
                    { "cyan", new Color("#00FFFF")},
                    { "blue", new Color("#0000ff")},
                    { "purple", new Color("#c000ff")},
                    { "white", new Color("#ffffff")},
                    { "white2", new Color("#cccc99")},
                    { "niconicowhite", new Color("#cccc99")},
                    { "red2", new Color("#cc0033")},
                    { "truered", new Color("#cc0033")},
                    { "pink2", new Color("#ff33cc")},
                    { "orange2", new Color("#ff6600")},
                    { "passionorange", new Color("#ff6600")},
                    { "yellow2", new Color("#999900")},
                    { "madyellow", new Color("#999900")},
                    { "green2", new Color("#00cc66")},
                    { "elementalgreen", new Color("#00cc66")},
                    { "cyan2", new Color("#00cccc")},
                    { "blue2", new Color("#3399ff")},
                    { "marineblue", new Color("#3399ff")},
                    { "purple2", new Color("#6633cc")},
                    { "nobleviolet", new Color("#6633cc")},
                    { "black2", new Color("#666666")},
                };
            }
        }

        public static double GetPositionX(int vpos,double second,double width,double canvasWidth,double duration)
        {
            var vposSec = vpos / 100.0;
            return (1 - (second - vposSec) / duration) * (canvasWidth + width) - width;
        }

        public bool CheckY(double y1, double y2, double fontHeight)
        {
            var result= y1 - fontHeight * 0.9 < y2 && y2 < y1 + fontHeight * 0.9;
            return result;
        }

        public Coordinate GetFromOperated(List<CommentInformation> operatedComments, CommentInformation currentComment,
            double videoDuration,double commentDuration,double time,Canvas canvas,RelativeValue fontHeight)
        {
            var canvasWidth = canvas.Width;
            var vpos1 = GetActualVpos(currentComment.Content.Vpos,videoDuration,commentDuration);
            var y = Math.Max(0, currentComment.LastY.GetActual(canvas));
            var firstCollision = true;
            var currentWidth = currentComment.Size.Width.GetActual(canvas);
            for(int i=0;i< operatedComments.Count;i++)
            {
                var operatedComment = operatedComments[i];
                var vpos2 = GetActualVpos(operatedComment.Content.Vpos, videoDuration, commentDuration);
                var operatedWidth = operatedComment.Size.Width.GetActual(canvas);

                var a = GetPositionX(vpos1, vpos2/100.0 + commentDuration, currentWidth, canvasWidth, commentDuration);
                var b = GetPositionX(vpos2, vpos1 / 100.0, operatedWidth, canvasWidth, commentDuration)+operatedComment.Size.Width.GetActual(canvas);
                var c = GetPositionX(vpos2, vpos1 / 100.0 + commentDuration, operatedWidth, canvasWidth, commentDuration);
                var d = GetPositionX(vpos1, vpos2 / 100.0, currentWidth, canvasWidth, commentDuration) + currentComment.Size.Width.GetActual(canvas);

                if(
                    CheckY(y,operatedComment.LastY.GetActual( canvas),fontHeight.GetActual(canvas))&&
                    (!(((b<canvasWidth)&&(0<a)&&(vpos2<=vpos1))||((c<canvasWidth)&&(0<d)&&(vpos1<vpos2))))
                    )
                {
                    i = -1;
                    if (firstCollision)
                    {
                        firstCollision = false;
                        y = 0;
                    }
                    else
                    {
                        y += fontHeight.GetActual(canvas);
                    }
                }
            }
            var x = GetPositionX(vpos1, time, currentWidth, canvasWidth, commentDuration);
            return new Coordinate(x, y);
        }

        private bool CheckRanges(List<Range> ranges,Range range)
        {
            if (ranges == null || ranges.Count == 0) return true;
            foreach(var item in ranges)
            {
                if (!range.Check(item,false,true)) { return false; }
            }
            return true;
        }

        private int GetActualVpos(int vpos,double Duration,double CommentDuration)
        {
            return (int)Math.Min(vpos, Duration * 100 - CommentDuration * 100 * 0.8);
        }

    }
}
