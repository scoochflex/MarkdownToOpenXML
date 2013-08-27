using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkdownToOpenXML
{
    class Parser
    {
        public abstract class Node
        {
            public string Text { get; set; }
            public Node() { }
            public Node(string content)
            {
                Text = content;
            }
            public abstract bool CombineWith(Node node);
        }
        public class Paragraph : Node
        {
            bool IsEnded = false;
            public Paragraph(string content)
                : base(content)
            {
            }
            public override bool CombineWith(Node node)
            {
                if (node is ParagraphBreak)
                {
                    IsEnded = true;
                    return true;
                }
                if (!IsEnded && node is Paragraph)
                {
                    Text += node.Text;
                    return true;
                }
                return false;
            }
        }
        public class ParagraphBreak : Node
        {
            public override bool CombineWith(Node node)
            {
                if (node is ParagraphBreak)
                    return true;
                return false;
            }
        }
        public class Header : Node
        {
            public int Level { get; set; }
            public Header(string content)
                : base(content)
            {
            }
            public override bool CombineWith(Node node)
            {
                if (node is ParagraphBreak)
                    return true;
                return false;
            }
        }
        public void Parse(string text)
        {
            text = text.Replace("\r\n", "\n");
            List<Node> nodes = new List<Node>();
            string[] lines = text.Split('\n');
            //First parse each line into a node
            for (int i = 0; i < lines.Length; i++)
            {
                string lookahead = (i + 1 < lines.Length) ? lines[i + 1] : "";
                nodes.Add(GetNodeType(lines[i], lookahead));
                if (SkipNextLine)
                {
                    i++;
                    SkipNextLine = false;
                }
            }
            //Now combine nodes if they can be combined (for instance 2 paragraph lines can be combined)
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Node node = nodes[i];
                if (node.CombineWith(nodes[i + 1]))
                {
                    nodes.RemoveAt(i + 1);
                    i--;
                }
            }
        }
        bool SkipNextLine = false;
        Node GetNodeType(string line, string lookahead)
        {
            if (line == "")
                return new ParagraphBreak();
            int headerLevel = line.TakeWhile((x) => x == '#').Count();

            if (headerLevel > 0)
                line = line.TrimStart('#').TrimEnd('#').Trim();
            else
            {
                String sTest = Regex.Replace(lookahead, @"\w", "");
                Match isSetextHeader1 = Regex.Match(sTest, @"[=]{2,}");
                if (Regex.Match(sTest, @"[=]{2,}").Success)
                {
                    headerLevel = 1;
                    SkipNextLine = true;
                }
                if (Regex.Match(sTest, @"[-]{2,}").Success)
                {
                    headerLevel = 2;
                    SkipNextLine = true;
                }
            }
            if (headerLevel > 0)
                return new Header(line) { Level = headerLevel };
            return new Paragraph(line);
        }
    }
}
