﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NicoNico.Comment
{
    public class Comment
    {
        public int Count;
        public int Number;
        public string Text;
        public int Vpos;
        public string Mail;

        public static async System.Threading.Tasks.Task<Comment[]> ParseXmlHttp(string uri)
        {
            var client = new System.Net.Http.HttpClient();
            var stream = await client.GetStreamAsync(uri);
            return ParseXml(stream);
        }

        public static Comment[] ParseXml(System.IO.Stream sr)
        {
            return ParseXml(System.Xml.XmlReader.Create(sr));
        }

        public static Comment[] ParseXml(string uri)
        {
            return ParseXml(System.Xml.XmlReader.Create(uri));
        }

        public static Comment[] ParseXml(System.Xml.XmlReader xr)
        {
            List<Comment> result = new List<Comment>();
            Comment currentComment = null;
            int count = 0;
            while (xr.Read())
            {
                switch (xr.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        if (xr.Name != "chat" || xr.GetAttribute("deleted") != null) continue;
                        currentComment = new Comment()
                        {
                            Count = count,
                            Number = int.Parse(xr.GetAttribute("no")),
                            Vpos = int.Parse(xr.GetAttribute("vpos")),
                            Mail = xr.GetAttribute("mail") ?? ""
                        };
                        break;
                    case System.Xml.XmlNodeType.Text:
                        currentComment.Text = xr.Value;
                        break;
                    case System.Xml.XmlNodeType.EndElement:
                        result.Add(currentComment);
                        count++;
                        break;
                }
            }
            result.Sort((a, b) =>
            {
                if (a.Vpos == b.Vpos) { return a.Number > b.Number ? 1 : -1; }
                return a.Vpos > b.Vpos ? 1 : -1;
            });
            return result.ToArray();
        }
    }
}
