﻿using System;
using System.Collections.Generic;
using System.Text;
using NicoNico.Comment.Primitive;

namespace NicoNico.Comment
{
    public class CommentBuffer : CommentPresenter.ICanvas
    {
        public interface ICanvasBuffered
        {
            double Width { get; }
            double Height { get; }

            FillCommand[] Commands { get; set; }

            void Updated();
            Size MeasureText(Comment comment,double fontSize, CommentPresenter.FontKind fontKind);
        }

        public struct FillCommand
        {
            public CommentPresenter.FontKind FontKind;
            public double FontSize;
            public CommentPresenter.Baseline Baseline;

            public Comment Comment;
            public Coordinate Coordinate;
            public Color Color;
            public bool OverFlow;
        }

        public ICanvasBuffered Content { get; set; }

        public double CanvasWidth => Content.Width;

        public double CanvasHeight => Content.Height;

        public CommentPresenter.FontKind FontKind { get; set; }
        public double FontSize { get; set; }
        public CommentPresenter.Baseline Baseline { get; set; }

        private List<FillCommand> CommandOperating;

        public void Done()
        {
            Content.Commands = CommandOperating.ToArray();
            Content.Updated();
        }

        public void FillText(Comment comment, Coordinate coord, Color color, bool overFlow)
        {
            CommandOperating.Add(
                new FillCommand()
                {
                    FontKind = this.FontKind,
                    FontSize = this.FontSize,
                    Baseline = this.Baseline,
                    Comment = comment,
                    Coordinate = coord,
                    Color = color,
                    OverFlow = overFlow
                }
                );
        }

        public void Init()
        {
            CommandOperating = new List<FillCommand>();
        }

        public Size MeasureText(Comment comment)
        {
            return Content.MeasureText(comment,this.FontSize,this.FontKind);
        }
    }
}
