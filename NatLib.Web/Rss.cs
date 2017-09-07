using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NatLib.Web
{
    public class Rss
    {
        public RssStyle Style { get; set; }
        public Rss(RssStyle style = null)
        {
            Style = new RssStyle();
            if (style == null) LoadDefaultStyle(Style);
        }

        public void LoadDefaultStyle(RssStyle style)
        {
            style.OverFlow = "hidden";
            style.Width = 500;
            style.Height = 500;
            style.LineSeparatorBorder = "1px dashed #6e6969";
            style.Transition = "visibility .5s, opacity 0.5s linear";
            style.DescriptionColor = "#101010";
            style.LinkColor = "#101010";
            style.TitleFontSize = "1.1em";
            style.TitleFontWeight = "bold";
            style.IsFlowVertical = true;
            style.HasLineSeparatorBorder = true;
            style.LinkDisable = true;
            style.DescriptionLines = 10;
            style.DescriptionWidth = 200;
            style.DescriptionHeight = 300;
        }

        public string ParseFeed(string feed)
        {
            XmlDocument rssXmlDoc = new XmlDocument();

            // Load the RSS file from the RSS URL
            rssXmlDoc.Load(feed);

            // Parse the Items in the RSS file
            XmlNodeList rssNodes = rssXmlDoc.SelectNodes("rss/channel/item");

            StringBuilder rssContent = new StringBuilder();
            rssContent.Append("<div class='rss-item'>");
            rssContent.Append("<div class='item-body'>");
            rssContent.Append("<div class='item-list'>");

            // Iterate through the items in the RSS file
            if (rssNodes != null)
                foreach (XmlNode rssNode in rssNodes)
                {
                    XmlNode rssSubNode = rssNode.SelectSingleNode("title");
                    string title = rssSubNode?.InnerText ?? "";

                    rssSubNode = rssNode.SelectSingleNode("link");
                    string link = rssSubNode?.InnerText ?? "";

                    rssSubNode = rssNode.SelectSingleNode("description");

                    var description = rssSubNode?.InnerText ?? "";

                    //var filterTitle = Style.LinkDisable ? title : $"<a href='{link}'>{title}</a>";
                    var filterTitle = $"<a href='{link}'>{title}</a>";

                    var item = $"<div class='item'><div class='title'>{filterTitle}</div><div class='description'>{description}</div></div>";

                    rssContent.Append(item);
                }

            rssContent.Append("</div>");
            rssContent.Append("</div>");
            rssContent.Append("</div>");

            // Return the string that contain the RSS items
            return StyleString() + rssContent.ToString();

        }

        public string StyleString()
        {
            var str = new StringBuilder();
            str.Append("<style>");
            str.Append("div.rss-item { width: " + Style.Width + "px; height: " + Style.Height + "px;}");
            str.Append("div.item-body { overflow: " + Style.OverFlow + "; width: 100%; height: 100% }");

            var itemListHeight = Style.IsFlowVertical ? "200vh" : Style.DescriptionLines.ToString() + "em";
            var itemListWidth = (Style.IsFlowVertical ? Style.Width.ToString() + "px" : "200vw");
            var style = $"overflow: hidden; width: {itemListWidth}; " +
                        $"height: {itemListHeight};";            
            str.Append("div.item-list {" + style + "}");

            style = $"transition: {Style.Transition}; color: {Style.DescriptionColor}; width: {Style.DescriptionWidth}px; overflow: hidden; " +
                    $"height: {Style.DescriptionHeight}px; display: -webkit-box; " +
                    $"-webkit-line-clamp: 10; -webkit-box-orient: vertical; margin-bottom: 10px; padding: 10px; ";
            if (Style.HasLineSeparatorBorder)
            {
                var border = $"border-bottom: {Style.LineSeparatorBorder};";
                if (!Style.IsFlowVertical) border = $"border-right: {Style.LineSeparatorBorder};";
                style = style + border;
            }

            if (!Style.IsFlowVertical) style = style + "float: left;";

            str.Append("div.item {" + style + "}");
            str.Append("div.item a {text-decoration: none; color: " + Style.LinkColor + ";" + (Style.LinkDisable ? "pointer-events: none;" : "") + "}");
            str.Append("div.item .title {font-size: " + Style.TitleFontSize + ";font-weight: " + Style.TitleFontWeight + "; margin-bottom: 5px;}");
            str.Append("</style>");

            return str.ToString();
        }
    }

    public class RssStyle
    {
        public string OverFlow { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }        
        public string Transition { get; set; }
        public string DescriptionColor { get; set; }
        public string LinkColor { get; set; }
        public bool LinkDisable { get; set; }
        public int DescriptionWidth { get; set; }
        public int DescriptionHeight { get; set; }
        public int DescriptionLines { get; set; }
        public string TitleFontSize { get; set; }
        public string TitleFontWeight { get; set; }
        public bool IsFlowVertical { get; set; }
        public bool HasLineSeparatorBorder { get; set; }
        public string LineSeparatorBorder { get; set; }
    }
}
