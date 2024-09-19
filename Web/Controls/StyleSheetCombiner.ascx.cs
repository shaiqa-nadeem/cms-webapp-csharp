﻿using System;
using System.Configuration;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using mojoPortal.Business;
using mojoPortal.Business.WebHelpers;

namespace mojoPortal.Web.UI;

/// <summary>
/// This control combines the css files in the order they are listed in
/// the style.config file located in the skin folder
/// </summary>
public partial class StyleSheetCombiner : UserControl
{
	private string skinBaseUrl = string.Empty;
	private string protocol = "http";
	/// <summary>
	/// this is just a flag used within specific features to determine if schema.org attributes can be added
	/// it should always be fine in HTML 5 doctype but
	/// may cause validation errors for XHTML doctypes
	/// so, you have the option to disable it to fix w3c validation if using xhtml
	/// but at the expense of SEO.
	/// </summary>
	public bool UseSchemaDotOrgFormats { get; set; } = true;

	public bool AddBodyClassForLiveWriter { get; set; } = true;

	public bool AddBodyClassForiPad { get; set; } = false;

	/// <summary>
	/// this property is not used directly by this control but the base page and cms page detect ths setting and use it
	/// so it allows configuring this per skin
	/// </summary>
	public bool UseIconsForAdminLinks { get; set; } = true;

	/// <summary>
	/// set this to true if you are including the css from /Data/style/formvalidation in your style.config
	/// this flag is chacked by code in the register.aspx page and if true then it wires up the script for jquery validation
	/// </summary>
	public bool UsingjQueryHintsOnRegisterPage { get; set; } = false;

	/// <summary>
	/// this property is not used directly by this control but the base page and cms page detect ths setting and use it
	/// so it allows configuring this per skin
	/// </summary>
	public bool UseTextLinksForFeatureSettings { get; set; } = true;

	/// <summary>
	/// this property is not used directly by this control but the base page and cms page detect ths setting and use it
	/// so it allows configuring this per skin
	/// </summary>
	public bool SiteMapPopulateOnDemand { get; set; } = false;

	/// <summary>
	/// this property is not used directly by this control but the base page and cms page detect ths setting and use it
	/// so it allows configuring this per skin
	/// the default -1 means fully expanded, when using SiteMapPopulateOnDemand=true you would typically set this to 0
	/// </summary>
	public int SiteMapExpandDepth { get; set; } = -1;

	/// <summary>
	/// valid options are: base, black-tie, blitzer, cupertino, dot-luv, excite-bike, hot-sneaks, humanity, mint-choc,
	/// redmond, smoothness, south-street, start, swanky-purse, trontastic, ui-darkness, ui-lightness, vader
	/// </summary>
	public string JQueryUIThemeName { get; set; } = "smoothness";

	public bool IncludejQueryUI { get; set; } = true;

	public bool IncludejCrop { get; set; } = false;

	public bool AllowPageOverride { get; set; } = false;

	public bool LoadSkinCss { get; set; } = true;

	public string OverrideSkinName { get; set; } = string.Empty;

	public bool IncludeColorPickerCss { get; set; } = false;

	/// <summary>
	/// valid options are empty for default, "folders", and "menu"
	/// </summary>
	//public string TreeViewStyle { get; set; } = string.Empty;

	public bool IncludeTwitterCss { get; set; } = false;

	public bool AlwaysShowLeftColumn { get; set; } = false;

	public bool AlwaysShowRightColumn { get; set; } = false;

	public bool IncludeGoogleCustomSearchCss { get; set; } = false;

	public bool DisableCssHandler { get; set; } = false;

	#region Property Bag settings stored here but not used in this control

	/// <summary>
	/// it is possible for dynamic menus like jQuery superfish to make unclickable menu links
	/// the menu controls will check this setting to determine whether to enable it
	/// the property is stored here just to make it controlled by the skin since 
	/// other menus don't makesense with unclickable links
	/// doesn't work for TreeView
	/// 2014-01-08 changed the default from false to to true because all newer skins use FlexMenu
	/// which does support it, TreeView is now legacy
	/// 2023-12-22 changed default to false because this is not good for SEO or UX unless you're building an
	/// application, most mojo sites are websites, not applications
	/// </summary>
	public bool EnableNonClickablePageLinks { get; set; } = false;

	/// <summary>
	/// there are no good ways to expand MenuItem with additional properties so we are using a property for something other than its intended purposes
	/// the MenuAdapterArtisteer is used to override the rendering and there we can use the tooltip property as a way to add a custom css class to soecific menu items.
	/// Admittedly an ugly solution but no other solutions seem feasible
	/// </summary>
	public bool UseMenuTooltipForCustomCss { get; set; } = true;

	public bool UseArtisteer3 { get; set; } = false;

	public bool HideEmptyAlt1 { get; set; } = true;

	public bool HideEmptyAlt2 { get; set; } = true;

	public string Media { get; set; } = string.Empty;

	#endregion

	private string FormatMedia()
	{
		if (Media.Length > 0)
		{
			return $" media=\"{Media}\" ";
		}
		return " ";
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);

		if (Core.Helpers.WebHelper.IsSecureRequest()) { protocol = "https"; }

		if (IncludejQueryUI)
		{
			SetupjQueryUICss();
		}
		if (IncludejCrop) { SetupjCropCss(); }
		if (IncludeTwitterCss) { SetupTwitter(); }
		if (IncludeGoogleCustomSearchCss) { SetupGoogleSearch(); }

		if (!LoadSkinCss) { return; }

		skinBaseUrl = SiteUtils.DetermineSkinBaseUrl(AllowPageOverride, false, Page);

		if (OverrideSkinName.Length > 0)
		{
			skinBaseUrl = SiteUtils.DetermineSkinBaseUrl(OverrideSkinName);
		}

		if (WebConfigSettings.CombineCSS)
		{
			SetupCombinedCssUrl();
		}
		else
		{
			AddCssLinks();
		}
	}

	private void SetupCombinedCssUrl()
	{
		if (DisableCssHandler) { return; }
		var cssLink = new Literal();

		string siteRoot = SiteUtils.GetRelativeNavigationSiteRoot();

		if (WebConfigSettings.UseFullUrlsForSkins)
		{
			siteRoot = SiteUtils.GetNavigationSiteRoot();
		}

		string siteParam = "&amp;s=-1";
		var siteSettings = CacheHelper.GetCurrentSiteSettings();

		if (WebConfigSettings.IncludeVersionInCssUrl && siteSettings != null)
		{
			siteParam = $"&amp;s={siteSettings.SiteId.ToInvariantString()}&amp;v={Server.UrlEncode(DatabaseHelper.AppCodeVersion().ToString())}&amp;sv={siteSettings.SkinVersion}";
		}
		else if (siteSettings != null)
		{
			siteParam = $"&amp;s={siteSettings.SiteId.ToInvariantString()}&amp;sv={siteSettings.SkinVersion}";
		}


		if (SiteUtils.UseMobileSkin())
		{
			if (siteSettings.MobileSkin.Length > 0)
			{
				OverrideSkinName = siteSettings.MobileSkin;
			}
			//web.config setting trumps site setting
			if (WebConfigSettings.MobilePhoneSkin.Length > 0)
			{
				OverrideSkinName = WebConfigSettings.MobilePhoneSkin;
			}
		}

		if (OverrideSkinName.Length > 0)
		{
			cssLink.Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" {FormatMedia()}href=\"{siteRoot}/csshandler.ashx?skin={OverrideSkinName + siteParam}\"/>\n";
		}
		else
		{
			cssLink.Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" {FormatMedia()}href=\"{siteRoot}/csshandler.ashx?skin={SiteUtils.GetSkinName(AllowPageOverride, Page) + siteParam}\"/>\n";
		}

		this.Controls.Add(cssLink);
	}

	private void SetupTwitter()
	{
		var cssLink = new Literal
		{
			ID = "twittercsss",
			Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" href=\"{protocol}://widgets.twimg.com/j/1/widget.css\" />"
		};
		Controls.Add(cssLink);
	}

	private void SetupGoogleSearch()
	{
		var cssLink = new Literal
		{
			ID = "googlesearchcsss",
			Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" href=\"{protocol}://www.google.com/cse/static/style/look/v4/default.css\" />"
		};
		Controls.Add(cssLink);
	}

	private void SetupjQueryUICss()
	{
		if (WebConfigSettings.DisablejQueryUI) { return; }

		string jQueryUIBasePath;
		string jQueryUIVersion = WebConfigSettings.GoogleCDNjQueryUIVersion;
		string jQueryCssAllName = "jquery-ui.css"; //the jquery.ui.all.css uses @imports so it loads 15 separate style sheets, whereas jquery-ui.css is all in one file

		if (WebConfigSettings.UseGoogleCDN)
		{
			jQueryUIBasePath = $"{protocol}:{WebConfigSettings.GoogleCDNJQueryUIBaseUrl + jQueryUIVersion}/";
		}
		else
		{
			jQueryUIBasePath = Page.ResolveUrl(WebConfigSettings.jQueryUIBasePath);
		}

		var cssLink = new Literal
		{
			ID = "jqueryui-css",
			Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" href=\"{jQueryUIBasePath}themes/{JQueryUIThemeName}/{jQueryCssAllName}\" />"
		};
		this.Controls.Add(cssLink);
	}

	private void SetupjCropCss()
	{
		var cssLink = new Literal
		{
			ID = "jcrop-css",
			Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" href=\"{Page.ResolveUrl("~/ClientScript/jcrop0912/jquery.Jcrop.css")}\" />"
		};
		this.Controls.Add(cssLink);
	}

	private void AddCssLinks()
	{

		string configFilePath;
		if (OverrideSkinName.Length > 0)
		{
			configFilePath = Server.MapPath(SiteUtils.DetermineSkinBaseUrl(SiteUtils.SanitizeSkinParam(OverrideSkinName)) + "style.config");
		}
		else
		{
			configFilePath = Server.MapPath(SiteUtils.DetermineSkinBaseUrl(AllowPageOverride, false, Page) + "style.config");
		}

		if (File.Exists(configFilePath)) //if no file, no style is added
		{
			using (XmlReader reader = new XmlTextReader(new StreamReader(configFilePath)))
			{
				reader.MoveToContent();
				while (reader.Read())
				{
					if (("file" == reader.Name) && (reader.NodeType != XmlNodeType.EndElement))
					{
						string csswebconfigkey = reader.GetAttribute("csswebconfigkey");
						string cssVPath = reader.GetAttribute("cssvpath");

						if ((!string.IsNullOrEmpty(csswebconfigkey)))
						{
							if ((ConfigurationManager.AppSettings[csswebconfigkey] != null))
							{
								AddCssLink(Page.ResolveUrl(ConfigurationManager.AppSettings[csswebconfigkey]));
							}
						}
						else if ((!string.IsNullOrEmpty(cssVPath)))
						{
							AddCssLink(Page.ResolveUrl("~" + cssVPath));
						}
						else
						{
							string cssFile = reader.ReadElementContentAsString();
							AddCssLink(skinBaseUrl + cssFile);
						}
					}
				}
			}
		}
	}

	private void AddCssLink(string cssUrl)
	{
		// don't add .less files
		if (!cssUrl.EndsWith(".css")) { return; }

		var cssLink = new Literal
		{
			Text = $"\n<link rel=\"stylesheet\" data-loader=\"StyleSheetCombiner\" href=\"{cssUrl}\" />"
		};

		this.Controls.Add(cssLink);
	}
}