﻿using System;
using System.Web.UI.WebControls;
using log4net;
using mojoPortal.Business;
using mojoPortal.Core.Extensions;
using mojoPortal.Web.Components;
using mojoPortal.Web.Framework;

namespace mojoPortal.Web.UI;

public partial class CustomMenu : SiteModuleControl
{
	private bool showStartingNode = false;
	private string viewName = "_CustomMenu";
	private int maxDepth = -1;
	private int startingPageId = -2;
	private static readonly ILog log = LogManager.GetLogger(typeof(CustomMenu));

	protected void Page_Load(object sender, EventArgs e)
	{
		startingPageId = WebUtils.ParseInt32FromHashtable(Settings, "CustomMenuStartingPage", startingPageId);
		maxDepth = WebUtils.ParseInt32FromHashtable(Settings, "CustomMenuMaxDepth", maxDepth);
		showStartingNode = WebUtils.ParseBoolFromHashtable(Settings, "CustomMenuShowStartingNode", showStartingNode);

		if (Settings.Contains("CustomMenuView"))
		{
			if (Settings["CustomMenuView"].ToString() != string.Empty)
			{
				viewName = Settings["CustomMenuView"].ToString();
			}
		}

		PageSettings startingPage = new(siteSettings.SiteId, startingPageId);

		SiteMapDataSource menuDataSource = new()
		{
			SiteMapProvider = "mojosite" + siteSettings.SiteId.ToInvariantString()
		};

		var startingNode = menuDataSource.Provider.RootNode;
		if (startingPageId > -1 && startingPage != null)
		{
			startingNode = menuDataSource.Provider.FindSiteMapNode(startingPage.Url);
		}

		var model = new Models.MenuModel
		{
			Id = ModuleId,
			Menu = new MenuList(startingNode, showStartingNode),
			StartingPage = startingPage == null ? null : getMenuItemFromPageSettings(startingPage),
			CurrentPage = getMenuItemFromPageSettings(currentPage),
			ShowStartingNode = showStartingNode,
			MaxDepth = maxDepth
		};

		mojoMenuItem getMenuItemFromPageSettings(PageSettings pageSettings)
		{
			return new mojoMenuItem
			{
				PageId = pageSettings.PageId,
				Name = pageSettings.PageName,
				Description = pageSettings.MenuDescription,
				URL = WebUtils.ResolveUrl(pageSettings.Url),
				CssClass = pageSettings.MenuCssClass,
				Rel = pageSettings.LinkRel,
				Clickable = pageSettings.IsClickable,
				OpenInNewTab = pageSettings.OpenInNewWindow,
				PublishMode = pageSettings.PublishMode,
				LastModDate = pageSettings.LastModifiedUtc,
				Current = currentPage.PageId == pageSettings.PageId
			};
		}

		try
		{
			lit1.Text = RazorBridge.RenderPartialToString(viewName, model, "Common");
		}
		catch (Exception ex)
		{
			lit1.Text = RazorBridge.RenderFallback(viewName, "_CustomMenu", "_CustomMenu", model, "Common", ex.ToString(), SiteUtils.DetermineSkinBaseUrl(true, false, Page));
		}
	}
}