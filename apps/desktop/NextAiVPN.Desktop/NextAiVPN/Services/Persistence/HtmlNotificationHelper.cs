using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using HtmlAgilityPack;

namespace NextAiVPN.Services.Persistence;

internal class HtmlNotificationHelper(IBrowserLinksOpener browserLinksOpener) : IHtmlNotificationHelper
{
	public void ConvertToTextBlock(string htmlString, ref TextBlock textBlock)
	{
		HtmlDocument htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(htmlString);
		ConvertTo(htmlDocument.DocumentNode, textBlock);
	}

	private void ConvertTo(HtmlNode node, TextBlock textBlock)
	{
		switch (node.NodeType)
		{
		case HtmlNodeType.Document:
			ConvertContentTo(node, textBlock);
			break;
		case HtmlNodeType.Text:
		{
			string name2 = node.ParentNode.Name;
			if (!(name2 == "script") && !(name2 == "style"))
			{
				string text2 = ((HtmlTextNode)node).Text;
				if (!HtmlNode.IsOverlappedClosingElement(text2) && text2.Trim().Length > 0)
				{
					textBlock.Inlines.Add(HtmlEntity.DeEntitize(text2));
				}
			}
			break;
		}
		case HtmlNodeType.Element:
		{
			bool flag = node.HasChildNodes;
			string name = node.Name;
			if (!(name == "p"))
			{
				if (name == "a")
				{
					MatchCollection matchCollection = UrlFinder.FindUrl(node.OuterHtml);
					if (matchCollection != null)
					{
						string text = string.Empty;
						{
							IEnumerator enumerator = matchCollection.GetEnumerator();
							try
							{
								if (enumerator.MoveNext())
								{
									text = enumerator.Current.ToString();
								}
							}
							finally
							{
								IDisposable disposable = enumerator as IDisposable;
								if (disposable != null)
								{
									disposable.Dispose();
								}
							}
						}
						if (!string.IsNullOrEmpty(text))
						{
							Hyperlink hyperlink = new Hyperlink
							{
								NavigateUri = new Uri(text, UriKind.Absolute)
							};
							hyperlink.RequestNavigate += HyperLink_RequestNavigate;
							hyperlink.Inlines.Add(node.InnerText);
							textBlock.Inlines.Add(hyperlink);
							flag = false;
						}
					}
				}
			}
			else
			{
				textBlock.Inlines.Add("\r\n");
			}
			if (flag)
			{
				ConvertContentTo(node, textBlock);
			}
			break;
		}
		case HtmlNodeType.Comment:
			break;
		}
	}

	private void ConvertContentTo(HtmlNode node, TextBlock textBlock)
	{
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)node.ChildNodes)
		{
			ConvertTo(item, textBlock);
		}
	}

	private void HyperLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		if (sender is Hyperlink hyperlink && hyperlink.NavigateUri != null)
		{
			browserLinksOpener.OpenBrowserLink(hyperlink.NavigateUri.AbsoluteUri);
		}
	}
}
