using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using log4net;
using mojoPortal.Business;
using mojoPortal.Business.WebHelpers;
using mojoPortal.Web.Framework;
using mojoPortal.Web.UI;
using Resources;

namespace mojoPortal.Web;

public class mojoBasePage : Page
{
	private static readonly ILog log = LogManager.GetLogger(typeof(mojoBasePage));
	private ScriptManager scriptController;
	private PageSettings currentPage = null;
	private string siteRoot = null;
	private string relativeSiteRoot = null;
	private string secureSiteRoot = null;
	private string imageSiteRoot = null;
	private bool forceShowLeft = false;
	private bool forceShowRight = false;
	private MetaContent metaContentControl = null;
	private ScriptLoader scriptLoader = null;
	private StyleSheetCombiner styleCombiner = null;
	private bool scriptLoaderFoundInMaster = false;
	private int mobileOnly = (int)ContentPublishMode.MobileOnly;
	private int webOnly = (int)ContentPublishMode.WebOnly;
	private string analyticsSection = string.Empty;
	private mojoDropDownList ddlContentView = new mojoDropDownList();
	private PageViewMode viewMode = PageViewMode.WorkInProgress;

	protected SiteSettings siteSettings = null;
	protected bool IsChangePasswordPage = false;

	public string MasterPageName = "layout.Master";
	public CoreDisplaySettings DisplaySettings = new();


	public ScriptLoader ScriptConfig
	{
		get
		{
			if (scriptLoader is null)
			{
				scriptLoader = Master.FindControl("ScriptLoader1") as ScriptLoader;

				if (scriptLoader is not null)
				{
					scriptLoaderFoundInMaster = true;
				}
			}

			// older skins may not have the script loader so we can add it below in OnInit if scriptLoaderFoundInMaster is false
			//scriptLoader ??= new ScriptLoader();

			return scriptLoader ??= new ScriptLoader();
		}
	}

	public StyleSheetCombiner StyleCombiner
	{
		get
		{
			styleCombiner ??= Master.FindControl("StyleSheetCombiner") as StyleSheetCombiner;
			return styleCombiner;
		}
	}

	public void EnsureDefaultModal()
	{
		ScriptManager.RegisterStartupScript(this, typeof(Page), "mojoModalScript", $"\r\n<script data-loader=\"mojoBasePage\" src=\"{Global.SkinConfig.ModalScriptPath.ToLinkBuilder()}\"></script>", false);
		var phSiteFooter = Master.FindControl("phSiteFooter");
		if (phSiteFooter is not null && phSiteFooter.FindControlRecursive("mojoModalTemplate") is null)
		{
			FileInfo modalFile = new(Server.MapPath(Global.SkinConfig.ModalTemplatePath));
			if (modalFile.Exists)
			{
				var content = File.ReadAllText(modalFile.FullName);

				phSiteFooter.Controls.Add(new Literal
				{
					Text = content,
					ID = "mojoModalTemplate"
				});
			}
		}
	}

	public ContentPlaceHolder MPLeftPane { get; set; }
	public ContentPlaceHolder MPContent { get; set; }
	public ContentPlaceHolder MPRightPane { get; set; }
	public ContentPlaceHolder AltPane1 { get; set; }
	public ContentPlaceHolder AltPane2 { get; set; }
	public ContentPlaceHolder MPPageEdit { get; set; }
	public ContentPlaceHolder PhHead { get; set; }
	public ContentPlaceHolder PhMain { get; set; }

	public SiteSettings SiteInfo
	{
		get
		{
			EnsureSiteSettings();

			return siteSettings;
		}
	}

	public int SiteId
	{
		get
		{
			if (siteSettings is not null)
			{
				return siteSettings.SiteId;
			}

			return -1;
		}
	}

	public ScriptManager ScriptController
	{
		get
		{
			scriptController ??= (ScriptManager)Master.FindControl("ScriptManager1");

			return scriptController ??= (ScriptManager)Master.FindControl("ScriptManager1");
		}
	}

	public bool AppendQueryStringToAction { get; set; } = false;
	public bool AllowSkinOverride { get; set; } = false;
	protected bool SetMasterInBasePage { get; set; } = true;
	public bool IsMobileDevice { get; private set; } = false;
	public bool UseMobileSkin { get; private set; } = false;

	public string JQueryUIThemeName
	{
		get
		{
			if (StyleCombiner is not null)
			{
				return StyleCombiner.JQueryUIThemeName;
			}

			return string.Empty;
		}
		set
		{
			if (StyleCombiner is not null)
			{
				StyleCombiner.JQueryUIThemeName = value;
			}
		}
	}

	public bool UseIconsForAdminLinks
	{
		get
		{
			if (!WebConfigSettings.UseIconsForAdminLinks) { return false; } //previous default was true so if they changed it to false just return that

			if (StyleCombiner is not null)
			{
				return StyleCombiner.UseIconsForAdminLinks;
			}

			return true; //keeping the old default for backward compat
		}
	}

	public bool UseTextLinksForFeatureSettings
	{
		get
		{
			if (!WebConfigSettings.UseTextLinksForFeatureSettings) { return false; } //default is true so if they changed it to false just return that

			if (StyleCombiner is not null)
			{
				return StyleCombiner.UseTextLinksForFeatureSettings;
			}

			return true;
		}
	}

	/// <summary>
	/// this property is now used for the PageHeading. Title is set with "Title = SiteUtils.FormatPageTitle, siteSettings, string topicTitle);"
	/// </summary>
	public string PageTitle
	{
		get { return PageHeading.Title.Text; }
		set { PageHeading.Title.Text = value; }
	}

	public PageTitle PageHeading
	{
		get
		{
			if (Master.FindControl("PageTitle1") is not PageTitle pageTitleControl)
			{
				pageTitleControl = Master.FindControl("PageHeading1") as PageTitle;
			}

			if (pageTitleControl is not null)
			{
				return pageTitleControl;
			}

			return new PageTitle();
		}
		set
		{
			if (Master.FindControl("PageTitle1") is not PageTitle pageTitleControl)
			{
				pageTitleControl = Master.FindControl("PageHeading1") as PageTitle;
			}

			if (pageTitleControl is null)
			{
				Title = value.Title.Text;
			}
			else
			{
				pageTitleControl.Title.Text = Server.HtmlEncode(value.Title.Text);
				pageTitleControl.LiteralExtraMarkup = value.LiteralExtraMarkup;
			}
		}
	}

	/// <summary>
	/// This appends css class to the body, requires the body be declared like this in layout.master:
	/// <body class="pagebody" id="Body" runat="server">
	/// </summary>
	/// <param name="cssClass"></param>
	public void AddClassToBody(string cssClass)
	{
		if (Master.FindControl("Body") is HtmlGenericControl body)
		{
			body.Attributes["class"] = body.Attributes["class"].Union(cssClass, ' ');
		}
	}

	private void EnsureMetaContentControl()
	{
		metaContentControl ??= Master.FindControl("MetaContent") as MetaContent;
	}

	public string MetaKeywordCsv
	{
		get
		{
			EnsureMetaContentControl();

			if (metaContentControl is not null)
			{
				return metaContentControl.KeywordCsv;
			}

			return string.Empty;
		}
		set
		{
			EnsureMetaContentControl();

			if (metaContentControl is not null)
			{
				metaContentControl.KeywordCsv = value;
			}
		}
	}

	public new string MetaDescription
	{
		get
		{
			EnsureMetaContentControl();

			if (metaContentControl is not null)
			{
				return metaContentControl.Description;
			}

			return string.Empty;
		}
		set
		{
			EnsureMetaContentControl();

			if (metaContentControl is not null)
			{
				metaContentControl.Description = value;
			}
		}
	}

	public void AddMetaKeword(string keyword)
	{
		EnsureMetaContentControl();

		metaContentControl?.AddKeword(keyword);
	}

	public string AdditionalMetaMarkup
	{
		get
		{
			EnsureMetaContentControl();

			if (metaContentControl is not null)
			{
				return metaContentControl.AdditionalMetaMarkup;
			}

			return string.Empty;
		}
		set
		{
			EnsureMetaContentControl();

			if (metaContentControl is not null)
			{
				metaContentControl.AdditionalMetaMarkup = value;
			}
		}
	}

	/// <summary>
	/// used by the google analytics tracking
	/// if you specify a section it will be tracked as a custom variable scoped at page level in slot 2
	/// </summary>
	public string AnalyticsSection
	{
		get { return analyticsSection; }
		set { analyticsSection = value; }
	}

	public PageSettings CurrentPage
	{
		get
		{
			currentPage ??= CacheHelper.GetCurrentPage();

			return currentPage;
		}
	}

	public bool UserCanViewPage()
	{
		if (CurrentPage is null)
		{
			return false;
		}

		if (WebUser.IsAdmin)
		{
			return true;
		}

		if (WebUser.IsContentAdmin)
		{
			if (CurrentPage.AuthorizedRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (SiteUtils.UserIsSiteEditor())
		{
			if (CurrentPage.AuthorizedRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (WebUser.IsInRoles(CurrentPage.AuthorizedRoles))
		{
			return true;
		}

		return false;
	}

	public bool UserCanViewPage(int moduleId)
	{
		if (CurrentPage is null)
		{
			return false;
		}

		Module m = GetModule(moduleId);

		if (m is null)
		{
			return false;
		}

		if (WebUser.IsAdmin)
		{
			return true;
		}

		if (WebUser.IsContentAdmin)
		{
			if (CurrentPage.AuthorizedRoles == "Admins;")
			{
				return false;
			}

			if (m.ViewRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (SiteUtils.UserIsSiteEditor())
		{
			if (CurrentPage.AuthorizedRoles == "Admins;")
			{
				return false;
			}

			if (m.ViewRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (!WebUser.IsInRoles(m.ViewRoles))
		{
			return false;
		}

		if (WebUser.IsInRoles(CurrentPage.AuthorizedRoles))
		{
			return true;
		}

		return false;
	}

	public bool UserCanViewPage(int moduleId, Guid featureGuid)
	{
		if (CurrentPage is null)
		{
			return false;
		}

		Module m = GetModule(moduleId, featureGuid);

		if (m is null)
		{
			return false;
		}

		if (WebUser.IsAdmin)
		{
			return true;
		}

		if (WebUser.IsContentAdmin)
		{
			if (CurrentPage.AuthorizedRoles == "Admins;")
			{
				return false;
			}

			if (m.ViewRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (SiteUtils.UserIsSiteEditor())
		{
			if (CurrentPage.AuthorizedRoles == "Admins;")
			{
				return false;
			}

			if (m.ViewRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (!WebUser.IsInRoles(m.ViewRoles))
		{
			return false;
		}

		if (WebUser.IsInRoles(CurrentPage.AuthorizedRoles))
		{
			return true;
		}

		return false;
	}

	public bool UserCanEditPage(int pageId)
	{
		if (pageId == -1) { return false; }
		if (CurrentPage is null) { return false; }
		if (CurrentPage.PageId != pageId) { return false; }

		if (WebUser.IsAdmin) { return true; }

		if (WebUser.IsContentAdmin)
		{
			if (CurrentPage.EditRoles == "Admins;") { return false; }
			return true;
		}

		if (SiteUtils.UserIsSiteEditor())
		{
			if (CurrentPage.EditRoles == "Admins;") { return false; }
			return true;
		}

		return WebUser.IsInRoles(CurrentPage.EditRoles);
	}

	public bool UserCanEditPage()
	{
		if (CurrentPage is null)
		{
			return false;
		}

		if (WebUser.IsAdmin)
		{
			return true;
		}

		if (WebUser.IsContentAdmin)
		{
			if (CurrentPage.EditRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (SiteUtils.UserIsSiteEditor())
		{
			if (CurrentPage.EditRoles == "Admins;")
			{
				return false;
			}

			return true;
		}

		if (WebUser.IsInRoles(CurrentPage.EditRoles))
		{
			return true;
		}

		return false;
	}

	public Module GetModule(int moduleId)
	{
		return GetModule(moduleId, Guid.Empty);
	}

	/// <summary>
	/// this overload is preferred because  it verifies that the module represents an instance of the correct feature
	/// </summary>
	/// <param name="moduleId"></param>
	/// <param name="featureGuid"></param>
	/// <returns></returns>
	public Module GetModule(int moduleId, Guid featureGuid)
	{
		if (CurrentPage is null)
		{
			return null;
		}

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId && (featureGuid == Guid.Empty || m.FeatureGuid == featureGuid))
			{
				return m;
			}
		}

		return null;
	}

	/// <summary>
	/// Returns true if the module exists on the page and the user has permission to edit the page or the module.
	/// </summary>
	/// <param name="moduleId"></param>
	/// <returns></returns>
	public bool UserCanEditModule(int moduleId)
	{
		return UserCanEditModule(moduleId, Guid.Empty);
	}

	/// <summary>
	/// this overload is preferred because it also checks that the module is an instance of the correct feature 
	/// </summary>
	/// <param name="moduleId"></param>
	/// <param name="featureGuid"></param>
	/// <returns></returns>
	public bool UserCanEditModule(int moduleId, Guid featureGuid)
	{
		if (!Request.IsAuthenticated)
		{
			return false;
		}

		if (CurrentPage is null)
		{
			return false;
		}

		if (WebUser.IsAdmin)
		{
			return true;
		}

		bool isSiteEditor = SiteUtils.UserIsSiteEditor();

		bool moduleFoundOnPage = false;
		bool moduleIsGlobal = false; //global modules can only be edited by admins, content admins or users in module edit roles

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId && (featureGuid == Guid.Empty || m.FeatureGuid == featureGuid))
			{
				moduleFoundOnPage = true;
				moduleIsGlobal = m.IsGlobal;

				if (WebUser.IsContentAdmin && m.AuthorizedEditRoles == "Admins;")
				{
					return false;
				}

				if (isSiteEditor && m.AuthorizedEditRoles == "Admins;")
				{
					return false;
				}
			}
		}

		if (WebUser.IsContentAdmin && CurrentPage.EditRoles != "Admins;")
		{
			return true;
		}

		if (isSiteEditor && CurrentPage.EditRoles != "Admins;")
		{
			return true;
		}

		// the above are allowed even if not on page to support modulewrapper

		if (!moduleFoundOnPage)
		{
			return false;
		}

		if (!moduleIsGlobal && WebUser.IsInRoles(CurrentPage.EditRoles))
		{
			return true;
		}

		SiteUser currentUser = SiteUtils.GetCurrentSiteUser();

		if (currentUser is null)
		{
			return false;
		}

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId)
			{
				if (m.EditUserId == currentUser.UserId)
				{
					return true;
				}

				if (WebUser.IsInRoles(m.AuthorizedEditRoles))
				{
					return true;
				}
			}
		}

		return false;
	}


	public bool UserCanApproveDraftModule(int moduleId, Guid featureGuid)
	{
		if (!Request.IsAuthenticated)
		{
			return false;
		}

		if (CurrentPage is null)
		{
			return false;
		}

		if (WebUser.IsAdminOrContentAdmin || SiteUtils.UserIsSiteEditor())
		{
			return true;
		}

		bool moduleFoundOnPage = false;
		bool moduleIsGlobal = false;
		SiteUser currentUser = SiteUtils.GetCurrentSiteUser();

		if (currentUser is null)
		{
			return false;
		}

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId && (featureGuid == Guid.Empty || m.FeatureGuid == featureGuid))
			{
				moduleFoundOnPage = true;
				moduleIsGlobal = m.IsGlobal;

				if (WebUser.IsInRoles(m.DraftApprovalRoles) || WebUser.IsInRoles(m.AuthorizedEditRoles) || m.EditUserId == currentUser.UserId)
				{
					return true;
				}
			}
		}

		if (!moduleFoundOnPage)
		{
			return false;
		}

		if (!moduleIsGlobal && (WebUser.IsInRoles(CurrentPage.DraftApprovalRoles)))
		{
			return true;
		}

		if (siteSettings is null)
		{
			return false;
		}

		return false;
	}

	public bool UserCanOnlyEditModuleAsDraft(int moduleId)
	{
		return UserCanOnlyEditModuleAsDraft(moduleId, Guid.Empty);
	}

	public bool UserCanOnlyEditModuleAsDraft(int moduleId, Guid featureGuid)

	{
		if (!Request.IsAuthenticated)
		{
			return false;
		}

		if (WebUser.IsAdminOrContentAdmin)
		{
			return false;
		}

		if (SiteUtils.UserIsSiteEditor())
		{
			return false;
		}

		if (!WebConfigSettings.EnableContentWorkflow)
		{
			return false;
		}

		if (siteSettings is null)
		{
			return false;
		}

		if (!siteSettings.EnableContentWorkflow)
		{
			return false;
		}

		if (CurrentPage is null)
		{
			return false;
		}

		bool moduleFoundOnPage = false;
		bool moduleIsGlobal = false; //global modules can only be edited by admins, content admins or users in module edit roles

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId && (featureGuid == Guid.Empty || m.FeatureGuid == featureGuid))
			{
				moduleFoundOnPage = true;
				moduleIsGlobal = m.IsGlobal;
			}
		}

		if (!moduleFoundOnPage || moduleIsGlobal)
		{
			return false;
		}

		if (WebUser.IsInRoles(CurrentPage.DraftEditOnlyRoles))
		{
			return true;
		}

		SiteUser currentUser = SiteUtils.GetCurrentSiteUser();

		if (currentUser is null)
		{
			return false;
		}

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId)
			{
				if (WebUser.IsInRoles(m.DraftEditRoles)) { return true; }
			}
		}

		return false;
	}

	public string GetModuleTitle(int moduleId)
	{
		string title = string.Empty;

		if (CurrentPage is null)
		{
			return title;
		}

		foreach (Module m in CurrentPage.Modules)
		{
			if (m.ModuleId == moduleId)
			{
				title = m.ModuleTitle;
			}
		}

		return title;
	}

	public bool ContainsPlaceHolder(string placeHoderId) => Master.FindControl(placeHoderId) is not null;

	public string SiteRoot
	{
		get
		{
			siteRoot ??= SiteUtils.GetNavigationSiteRoot();

			return siteRoot;
		}
	}

	public string RelativeSiteRoot
	{
		get
		{
			relativeSiteRoot ??= SiteUtils.GetRelativeNavigationSiteRoot();

			return relativeSiteRoot;
		}
	}

	public string SecureSiteRoot
	{
		get
		{
			if (!SiteUtils.SslIsAvailable())
			{
				return SiteRoot;
			}

			secureSiteRoot ??= WebUtils.GetSecureSiteRoot();

			return secureSiteRoot;
		}
	}

	public string ImageSiteRoot
	{
		get
		{
			imageSiteRoot ??= SiteUtils.GetImageSiteRoot(this);

			return imageSiteRoot;
		}
	}

	// implement support for closing sites so tha only admins can access the content
	private bool DidRedirectForClosedSite()
	{
		// allow accessing the login page
		if (this is UI.Pages.LoginPage)
		{
			return false;
		}

		// allow accessing the closed message
		if (this is UI.Pages.ClosedPage)
		{
			return false;
		}

		EnsureSiteSettings();

		if (siteSettings is null)
		{
			return false;
		}

		if (siteSettings.SiteIsClosed && WebConfigSettings.AllowClosingSites)
		{
			if (!WebUser.IsInRoles(WebConfigSettings.RolesThatCanAccessClosedSites))
			{
				string redirectUrl = SiteUtils.GetNavigationSiteRoot() + WebConfigSettings.ClosedPageUrl;

				Response.Redirect(redirectUrl, true);

				return true;
			}
		}

		return false;
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		if (DidRedirectForClosedSite())
		{
			return;
		}

		if (Request.IsAuthenticated && WebConfigSettings.ForceSingleSessionPerUser)
		{
			ForceSingleSession();
		}

		if (Request.IsAuthenticated && WebConfigSettings.EnforceRequirePasswordChanges)
		{
			if (!IsChangePasswordPage)
			{
				SiteUser currentUser = SiteUtils.GetCurrentSiteUser();

				if (currentUser is null)
				{
					// user has authentication cookie but user came back null usually means they changed their email/loginname from a different browser
					// but they still have a persistent auth cookie in this browser with the old username
					// so redirect to logoff page to clear their cookies
					Response.Redirect(SiteRoot + "/Logoff.aspx", true);
				}
				else
				{
					if (currentUser.RolesChanged)
					{
						if (Roles.Provider is mojoRoleProvider)
						{
							mojoRoleProvider.ResetCurrentUserRolesCookie();
						}

						currentUser.RolesChanged = false; //reset the flag that was set to true when roles were changed
						currentUser.Save();

						// we need to redirect because the updated cookie won't be detected until the next request
						Response.Redirect(Request.RawUrl, true);
					}

					if (currentUser.MustChangePwd)
					{
						Response.Redirect(SiteRoot + "/Secure/PasswordReset.aspx?returnurl=" + Server.UrlEncode(Request.RawUrl), true);
					}
				}
			}
		}

		AddHtmlAttributes();
	}

	private void AddHtmlAttributes()
	{
		// requires having <html id="Html" runat="server" in layout.master of the skin

		if (Master.FindControl("Html") is HtmlControl html)
		{
			html.ClientIDMode = ClientIDMode.Static;

			if (WebConfigSettings.AdaptHtmlDirectionToCulture) //false by default
			{
				html.Attributes.Remove("dir");

				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				{
					html.Attributes.Add("dir", "rtl");
				}
				else
				{
					html.Attributes.Add("dir", "ltr");
				}
			}

			if (WebConfigSettings.AddLangToHtmlElement) //true by default
			{
				var langAttr = html.Attributes["lang"];

				if (langAttr is null)
				{
					html.Attributes.Add("lang", SiteUtils.GetDefaultUICulture().Name);
				}
			}
		}
		else
		{
			string logMessage = $"{string.Format(Resource.ElementNotFound, "Html")} {Resource.AttributeNotApplied} {Resource.HowToFixHtmlElement}";
			log.Warn(logMessage);
		}
	}

	override protected void OnPreInit(EventArgs e)
	{
		base.OnPreInit(e);

		if (HttpContext.Current is null)
		{
			return;
		}

		IsMobileDevice = SiteUtils.IsMobileDevice();
		UseMobileSkin = SiteUtils.UseMobileSkin();

		try
		{
			if (WebConfigSettings.AllowForcingPreferredHostName)
			{
				EnsureSiteSettings();
				if (siteSettings is not null && siteSettings.PreferredHostName.Length > 0)
				{
					string requestedHostName = WebUtils.GetHostName();

					if (siteSettings.PreferredHostName != requestedHostName)
					{
						string redirectUrl;
						string protocol = SiteUtils.SslIsAvailable() ? "https://" : "http://";
						string serverPort = HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
						if (!string.IsNullOrWhiteSpace(serverPort) && (serverPort == "80" || serverPort == "443"))
						{
							serverPort = string.Empty;
						}
						else
						{
							serverPort = $":{serverPort}";
						}

						if (WebConfigSettings.RedirectToRootWhenEnforcingPreferredHostName)
						{
							redirectUrl = protocol + siteSettings.PreferredHostName + serverPort;
						}
						else
						{
							redirectUrl = protocol + siteSettings.PreferredHostName + serverPort + Request.RawUrl;
						}

						if (WebConfigSettings.LogRedirectsToPreferredHostName)
						{
							log.Info($"received a request for hostname {requestedHostName}{serverPort}{Request.RawUrl}, redirecting to preferred host name {redirectUrl}");
						}

						if (WebConfigSettings.Use301RedirectWhenEnforcingPreferredHostName)
						{
							Response.RedirectPermanent(redirectUrl, true);
							return;
						}

						Response.Redirect(redirectUrl, true);

						return;
					}
				}
			}

			SetupMasterPage();
		}
		catch (HttpException ex)
		{
			log.Error($"Error setting master page. This is likely because the master page file listed in the error below doesn't exist in the current skin.  mojoPortal will use the master page file from the default skin.\r\n{CultureInfo.CurrentCulture} - {SiteUtils.GetIP4Address()}", ex);

			try
			{
				SetupFailsafeMasterPage();
			}
			catch (HttpException ex2)
			{
				log.Error($"Error setting failsafe master page. This is likely because the master page file listed in the error below doesn't exist in the current skin.\r\n{CultureInfo.CurrentCulture} - {SiteUtils.GetIP4Address()}", ex2);
			}
		}

		if (SetMasterInBasePage)
		{
			if (Master.FindControl("StyleSheetCombiner") is StyleSheetCombiner styleCombiner)
			{
				styleCombiner.AllowPageOverride = AllowSkinOverride;
			}
		}

		EnsureSiteSettings();

		if (Master.FindControl("Body") is HtmlControl body)
		{
			body.ClientIDMode = (ClientIDMode)Enum.Parse(typeof(ClientIDMode), WebConfigSettings.BodyElementClientIDMode);
		}

		var head = Master.FindControl("Head1") ?? Master.FindControl("Head");
		if (head is not null && siteSettings is not null)
		{
			head.ClientIDMode = (ClientIDMode)Enum.Parse(typeof(ClientIDMode), WebConfigSettings.HeadElementClientIDMode);

			if (Page is not NonCmsBasePage && !string.IsNullOrWhiteSpace(siteSettings.SiteWideHeaderContent))
			{
				//add to head
				head.Controls.Add(new Literal
				{
					Text = siteSettings.SiteWideHeaderContent
				});
			}

			if (Page is not NonCmsBasePage && !string.IsNullOrWhiteSpace(siteSettings.SiteWideFooterContent))
			{
				//add to bottom of body
				Controls.AddAt(Controls.Count, new Literal
				{
					Text = siteSettings.SiteWideFooterContent
				});
			}

			if (Page is NonCmsBasePage && !string.IsNullOrWhiteSpace(siteSettings.SiteWideHeaderAdminContent))
			{
				//add to head
				head.Controls.Add(new Literal
				{
					Text = siteSettings.SiteWideHeaderAdminContent
				});
			}

			if (Page is NonCmsBasePage && !string.IsNullOrWhiteSpace(siteSettings.SiteWideFooterAdminContent))
			{
				//add to bottom body
				Controls.AddAt(Controls.Count, new Literal
				{
					Text = siteSettings.SiteWideFooterAdminContent
				});
			}
		}
	}

	protected void EnsureSiteSettings()
	{
		siteSettings ??= CacheHelper.GetCurrentSiteSettings();
	}

	private void SetupMasterPage()
	{
		// default to not allow page override so the non content system pages
		// will still use the site default skin
		// even if the default content page has an alternate skin
		// allowSkinOverride is overridden in Default.aspx.cs for content system
		// pages

		EnsureSiteSettings();

		if (siteSettings is not null && siteSettings.SiteGuid != Guid.Empty)
		{
			string skinName = SiteUtils.GetSkinName(AllowSkinOverride, this);

			if (SetMasterInBasePage)
			{
				MasterPageFile = SiteUtils.GetMasterPage(this, skinName, siteSettings, AllowSkinOverride, MasterPageName);
			}

			SetupTheme(skinName);
		}

		if (!string.IsNullOrEmpty(MasterPageFile))
		{
			MPLeftPane = (ContentPlaceHolder)Master.FindControl("leftContent");
			MPContent = (ContentPlaceHolder)Master.FindControl("mainContent");
			MPRightPane = (ContentPlaceHolder)Master.FindControl("rightContent");
			MPPageEdit = (ContentPlaceHolder)Master.FindControl("pageEditContent");
			AltPane1 = (ContentPlaceHolder)Master.FindControl("altContent1");
			AltPane2 = (ContentPlaceHolder)Master.FindControl("altContent2");
		}
	}

	protected void SetupTheme(string skinName)
	{
		if (WebConfigSettings.DisableASPThemes)
		{
			Theme = string.Empty;

			return;
		}

		bool registeredVirtualThemes = Global.RegisteredVirtualThemes; //should always be true under .NET 4

		if (!registeredVirtualThemes && !WebConfigSettings.UseSiteIdAppThemesInMediumTrust)
		{
			Theme = "default";

			return;
		}

		if (!registeredVirtualThemes && WebConfigSettings.UseSiteIdAppThemesInMediumTrust)
		{
			Theme = "default" + siteSettings.SiteId.ToInvariantString();

			return;
		}

		// default to site skin
		Theme = "default" + siteSettings.SiteId.ToInvariantString() + skinName;

		//use page skin if allowed
		if (AllowSkinOverride)
		{
			if (siteSettings.AllowPageSkins && CurrentPage is not null && CurrentPage.Skin.Length > 0)
			{
				Theme = "pageskin-" + siteSettings.SiteId.ToInvariantString() + CurrentPage.Skin;
				//Global.SkinConfigManager.GetConfig(CurrentPage.Skin);
			}
		}

		//use user skin if allowed
		if (siteSettings.AllowUserSkins || WebConfigSettings.AllowEditingSkins && WebUser.IsInRoles(siteSettings.RolesThatCanManageSkins))
		{
			SiteUser currentUser = SiteUtils.GetCurrentSiteUser();

			if (currentUser is not null && currentUser.Skin.Length > 0)
			{
				Theme = "userpersonal" + siteSettings.SiteId.ToInvariantString() + currentUser.Skin;
			}
		}

		//use preview if skin param exists
		string previewSkin = SiteUtils.GetSkinPreviewParam(siteSettings);

		if (previewSkin.Length > 0)
		{
			Theme = "preview_" + siteSettings.SiteId.ToInvariantString() + previewSkin;
		}

		//use mobile if mobile device
		if (UseMobileSkin)
		{
			Theme = "mobile" + siteSettings.SiteId.ToInvariantString() + siteSettings.MobileSkin + WebConfigSettings.MobilePhoneSkin;
		}
	}

	protected void SetupFailsafeMasterPage()
	{
		if (!WebConfigSettings.UseFailSafeMasterPageOnError)
		{
			return;
		}

		MasterPageFile = $"~/App_MasterPages/{MasterPageName}";

		switch (MasterPageName.ToLower())
		{
			case "layout.master":
			default:
				MPLeftPane = (ContentPlaceHolder)Master.FindControl("leftContent");
				MPContent = (ContentPlaceHolder)Master.FindControl("mainContent");
				MPRightPane = (ContentPlaceHolder)Master.FindControl("rightContent");
				AltPane1 = (ContentPlaceHolder)Master.FindControl("altContent1");
				AltPane2 = (ContentPlaceHolder)Master.FindControl("altContent2");
				MPPageEdit = (ContentPlaceHolder)Master.FindControl("pageEditContent");

				break;

			case "dialogmaster.master":
				PhHead = (ContentPlaceHolder)Master.FindControl("phHead");
				PhMain = (ContentPlaceHolder)Master.FindControl("phMain");
				break;
		}
	}

	protected bool ShouldShowModule(Module m)
	{
		if (UseMobileSkin && m.PublishMode == webOnly)
		{
			return false;
		}

		if (!UseMobileSkin && m.PublishMode == mobileOnly)
		{
			if (WebConfigSettings.RolesThatAlwaysViewMobileContent.Length > 0)
			{
				if (WebUser.IsInRoles(WebConfigSettings.RolesThatAlwaysViewMobileContent))
				{
					return true;
				}
			}

			return false;
		}

		return true;
	}

	public void LoadSideContent(bool includeLeft, bool includeRight)
	{
		if (CurrentPage is null)
		{
			return;
		}

		if (!includeLeft && !includeRight)
		{
			return;
		}

		int leftModulesAdded = 0;
		int rightModulesAdded = 0;

		if (CurrentPage.Modules.Count > 0)
		{
			foreach (Module module in CurrentPage.Modules)
			{
				if (!ShouldShowModule(module)) //mobile skin publishing
				{
					continue;
				}

				if (ModuleIsVisible(module)) //hide from authenticated/unauthenticated
				{
					if (includeLeft && module.PaneName.IsCaseInsensitiveMatch("leftpane"))
					{
						if (LoadContentControl(module) is not Control c)
						{
							continue;
						}

						MPLeftPane.Controls.Add(c);
						MPLeftPane.Visible = true;
						MPLeftPane.Parent.Visible = true;

						leftModulesAdded += 1;
					}

					if (includeRight && module.PaneName.IsCaseInsensitiveMatch("rightpane"))
					{
						if (LoadContentControl(module) is not Control c)
						{
							continue;
						}

						MPRightPane.Controls.Add(c);
						MPRightPane.Visible = true;
						MPRightPane.Parent.Visible = true;

						rightModulesAdded += 1;
					}
				}
			}
		}

		forceShowLeft = leftModulesAdded > 0;
		forceShowRight = rightModulesAdded > 0;
	}

	private void SetupColumnCss(bool showLeft, bool showRight)
	{
		if (!showLeft && !showRight)
		{
			return;
		}

		if (Master.FindControl("divLeft") is not Panel divLeft)
		{
			return;
		}

		if (Master.FindControl("divRight") is not Panel divRight)
		{
			return;
		}

		if (Master.FindControl("divCenter") is not Panel divCenter)
		{
			return;
		}

		if (showLeft)
		{
			divLeft.Visible = showLeft;
		}

		if (showRight)
		{
			divRight.Visible = showRight;
		}

		string leftSideNoRightSideCss = "art-layout-cell art-sidebar1 leftside left2column";
		string rightSideNoLeftSideCss = "art-layout-cell art-sidebar2 rightside right2column";
		string centerNoLeftSideCss = "art-layout-cell art-content center-rightmargin cmszone";
		string centerNoRightSideCss = "art-layout-cell art-content center-leftmargin cmszone";
		string centerNoLeftOrRightSideCss = "art-layout-cell art-content-wide center-nomargins cmszone";
		string centerWithLeftAndRightSideCss = "art-layout-cell  art-content-narrow center-rightandleftmargins cmszone";


		if (Master.FindControl("LayoutDisplaySettings1") is LayoutDisplaySettings layoutSettings)
		{
			leftSideNoRightSideCss = layoutSettings.LeftSideNoRightSideCss;
			rightSideNoLeftSideCss = layoutSettings.RightSideNoLeftSideCss;
			centerNoLeftSideCss = layoutSettings.CenterNoLeftSideCss;
			centerNoRightSideCss = layoutSettings.CenterNoRightSideCss;
			centerNoLeftOrRightSideCss = layoutSettings.CenterNoLeftOrRightSideCss;
			centerWithLeftAndRightSideCss = layoutSettings.CenterWithLeftAndRightSideCss;
		}

		if (divLeft.Visible && !divRight.Visible)
		{
			divLeft.CssClass = leftSideNoRightSideCss;
		}

		if (divRight.Visible && !divLeft.Visible)
		{
			divRight.CssClass = rightSideNoLeftSideCss;
		}

		divCenter.CssClass =
			divLeft.Visible
				? (divRight.Visible ? centerWithLeftAndRightSideCss : centerNoRightSideCss)
				: (divRight.Visible ? centerNoLeftSideCss : centerNoLeftOrRightSideCss);
	}

	public void LoadAltContent(bool includeTop, bool includeBottom)
	{
		if (CurrentPage is null)
		{
			return;
		}

		if (!includeTop && !includeBottom)
		{
			return;
		}

		if (CurrentPage.Modules.Count > 0)
		{
			foreach (Module module in CurrentPage.Modules)
			{
				if (!ShouldShowModule(module))
				{
					continue;
				}

				if (ModuleIsVisible(module))
				{
					if (AltPane1 is not null && includeTop && module.PaneName.IsCaseInsensitiveMatch("altcontent1"))
					{
						if (LoadContentControl(module) is not Control c)
						{
							continue;
						}

						if ((Master.FindControl("divAlt1") ?? Master.FindControl("divAltContent1")) is Control a1)
						{
							a1.Visible = true;
						}

						AltPane1.Controls.Add(c);
					}

					if (AltPane2 is not null && includeBottom && module.PaneName.IsCaseInsensitiveMatch("altcontent2"))
					{
						if (LoadContentControl(module) is not Control c)
						{
							continue;
						}

						if ((Master.FindControl("divAlt2") ?? Master.FindControl("divAltContent2")) is Control a2)
						{
							a2.Visible = true;
						}

						AltPane2.Controls.Add(c);
					}
				}
			}
		}
	}

	protected Control LoadContentControl(Module module)
	{
		if (Page.LoadControl($"~/{module.ControlSource}") is not Control c)
		{
			return null;
		}

		if (c is SiteModuleControl siteModule)
		{
			siteModule.SiteId = siteSettings.SiteId;
			siteModule.ModuleConfiguration = module;
		}
		return c;
	}

	protected bool ModuleIsVisible(Module module)
	{
		if (module.HideFromAuthenticated && Request.IsAuthenticated)
		{
			return false;
		}

		if (module.HideFromUnauthenticated && !Request.IsAuthenticated)
		{
			return false;
		}

		return true;
	}

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		// a litle extra protection against CSRF attacks in forms
		// try catch added because of this post http://www.mojoportal.com/Forums/Thread.aspx?thread=2819&mid=34&pageid=5&ItemID=2&pagenumber=1#post11883

		try
		{
			if (Session is not null && !string.IsNullOrEmpty(Session.SessionID))
			{
				ViewStateUserKey = Session.SessionID;
			}
		}
		catch (HttpException)
		{ }

		if (siteSettings is not null && siteSettings.MetaProfile.Length > 0 && (Page.Header is not null))
		{
			Page.Header.Attributes.Add("profile", siteSettings.MetaProfile);
		}

		SetupAdminLinks();

		// older skins may not have it included in layout.master so we load it here
		if (ScriptConfig is not null && !scriptLoaderFoundInMaster)
		{
			Controls.Add(scriptLoader);
		}

		if (WebConfigSettings.UseAjaxFormActionUpdateScript) //possibly may want to remove this in future versions of .NET
		{
			//this keeps the form action correct during ajax postbacks
			string formActionScript = @"<script data-loader=""mojoBasepage"">
Sys.Application.add_load(function() {
	var form = Sys.WebForms.PageRequestManager.getInstance()._form;
	form._initialAction = form.action = window.location.href;
});
</script>";

			Page.ClientScript.RegisterStartupScript(GetType(), "formactionset", formActionScript);
		}
	}

	private void SetupAdminLinks()
	{
		if (string.IsNullOrEmpty(MasterPageFile))
		{
			return;
		}

		// 2010-01-04 made it possible to add these links directly in layout.master so they can be arranged and styled easier
		if (MPPageEdit is not null)
		{
			if (Page.Master.FindControl("lnkAdminMenu") is null)
			{
				MPPageEdit.Controls.Add(new AdminMenuLink());
			}

			if (Page.Master.FindControl("lnkFileManager") is null)
			{
				MPPageEdit.Controls.Add(new FileManagerLink());
			}

			if (Page.Master.FindControl("lnkNewPage") is null)
			{
				MPPageEdit.Controls.Add(new NewPageLink());
			}
		}
	}

	protected void HideViewSelector()
	{
		ddlContentView.Visible = false;
	}

	protected bool UserCanSeeWorkflowControls()
	{
		if (
			WebUser.IsContentAdmin ||
			WebUser.IsInRoles(CurrentPage.DraftEditOnlyRoles) ||
			WebUser.IsInRoles(CurrentPage.EditRoles) ||
			WebUser.IsInRoles(CurrentPage.DraftApprovalRoles) ||
			SiteUtils.UserIsSiteEditor()
		)
		{
			return true;
		}

		if (CurrentPage.Modules.Count > 0)
		{
			foreach (Module module in CurrentPage.Modules)
			{
				if (
					UserCanEditModule(module.ModuleId, module.FeatureGuid) ||
					UserCanOnlyEditModuleAsDraft(module.ModuleId, module.FeatureGuid) ||
					UserCanApproveDraftModule(module.ModuleId, module.FeatureGuid)
				)
				{
					return true;
				}
			}
		}

		return false;
	}

	protected void SetupWorkflowControls(bool forceShowWorkflow)
	{
		if (WebConfigSettings.EnableContentWorkflow && siteSettings.EnableContentWorkflow && (forceShowWorkflow || UserCanSeeWorkflowControls()))
		{
			viewMode = GetUserViewMode();

			ddlContentView.SkinID = "workflow";
			ddlContentView.CssClass = "ddworkflow";
			ddlContentView.Items.Add(new ListItem(Resource.ViewWorkInProgress, PageViewMode.WorkInProgress.ToString()));
			ddlContentView.Items.Add(new ListItem(Resource.ViewLive, PageViewMode.Live.ToString()));

			ListItem item = ddlContentView.Items.FindByValue(viewMode.ToString());

			if (item is not null)
			{
				ddlContentView.ClearSelection();
				item.Selected = true;
			}

			ddlContentView.SelectedIndexChanged += new EventHandler(ddlContentView_SelectedIndexChanged);
			ddlContentView.AutoPostBack = true;

			MPPageEdit?.Controls.Add(ddlContentView);
		}
		else
		{
			viewMode = PageViewMode.Live;
		}

	}

	protected void ddlContentView_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (ddlContentView.SelectedIndex == -1)
		{
			//WebUtils.SetupRedirect(this, SiteUtils.GetCurrentPageUrl());
			Response.Redirect(SiteUtils.GetCurrentPageUrl());

			return;
		}

		CookieHelper.SetCookie(GetViewModeCookieName(), ddlContentView.SelectedItem.Value, false);

		Response.Redirect(Request.RawUrl);
	}

	protected PageViewMode GetUserViewMode()
	{
		PageViewMode viewMode = PageViewMode.WorkInProgress;

		// we let the query string value trump because we pass it in an url from the Admin workflow pages
		if (!string.IsNullOrEmpty(Request.QueryString["vm"]))
		{
			try
			{
				viewMode = (PageViewMode)Enum.Parse(typeof(PageViewMode), Request.QueryString["vm"]);
			}
			catch (ArgumentException)
			{ }
		}
		else
		{
			// if query string is not passed use the cookie
			string viewCookieValue = CookieHelper.GetCookieValue(GetViewModeCookieName());

			if (!string.IsNullOrEmpty(viewCookieValue))
			{
				try
				{
					viewMode = (PageViewMode)Enum.Parse(typeof(PageViewMode), viewCookieValue);
				}
				catch (ArgumentException)
				{ }
			}
		}

		return viewMode;
	}

	private string GetViewModeCookieName()
	{
		string cookieName = "viewmode";

		if (siteSettings is not null)
		{
			cookieName += siteSettings.SiteId.ToString(CultureInfo.InvariantCulture);
		}

		return cookieName;
	}

	/// <summary>
	/// Identifies whether to show Work in progress or live content. This value is switched via the dropdown at the top of the page
	/// when the site is in edit mode.
	/// Your Feature should read this and display content accordingly:
	/// </summary>
	public PageViewMode ViewMode
	{
		get { return viewMode; }
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);

		SetupColumnCss(forceShowLeft, forceShowRight);
	}

	private void EnsureFormAction()
	{
		if (Form.Action.Length == 0)
		{
			Form.Action = "/";
		}
	}

	protected override void Render(HtmlTextWriter writer)
	{
		if (!SiteUtils.UrlWasReWritten())
		{
			try
			{
				// form action can't be empty string
				EnsureFormAction();
			}
			catch (MissingMethodException)
			{
				//this method was introduced in .NET 3.5 SP1
			}

			base.Render(writer);

			return;
		}

		/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		* 
		* Custom HtmlTextWriter to fix Form Action
		* Based on Jesse Ezell's "Fixing Microsoft's Bugs: URL Rewriting"
		* http://weblogs.asp.net/jezell/archive/2004/03/15/90045.aspx
		* 
		* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
		// this removes the action attribute from the form
		// so that it posts back correctly when using url re-writing

		string action = Request.RawUrl;

		// we need to append the query string to the form action
		// otherwise the params won't be available when the form posts back
		// example if editing an event ~/EventCalendarEdit.aspx??mid=4&pageid=3
		// if the form action is merely "EventCalendarEdit.aspx" we won't have the
		// mid and pageid params on postback. querystring is only available in get requests
		// unless the form action includes it, then its also available in post requests

		if (AppendQueryStringToAction && action.IndexOf("?") == -1 && Request.QueryString.Count > 0)
		{
			action += "?" + Request.QueryString.ToString();
		}

		if (action.Length == 0)
		{
			action = "/";
		}

		if (writer.GetType() == typeof(HtmlTextWriter))
		{
			writer = new MojoHtmlTextWriter(writer, action);
		}
		else if (writer.GetType() == typeof(Html32TextWriter))
		{
			writer = new MojoHtml32TextWriter(writer, action);
		}

		base.Render(writer);
	}

	private void ForceSingleSession()
	{

		if (Context.User.Identity.AuthenticationType != "Forms")
		{
			return;
		}

		lock (HttpRuntime.Cache)
		{
			string requestUserSessionGUID;

			if (Session["requestGuid"] is null)
			{
				requestUserSessionGUID = Context.User.Identity.Name + Guid.NewGuid().ToString();
				Session["requestGuid"] = requestUserSessionGUID;
				HttpRuntime.Cache.Insert("UserSessionID", requestUserSessionGUID);

				return;
			}
			else
			{
				requestUserSessionGUID = (string)Session["requestGuid"];

				if ((string)HttpRuntime.Cache["UserSessionID"] is null)
				{
					HttpRuntime.Cache.Insert("UserSessionID", requestUserSessionGUID);
				}

				string cachedUserSessionID = (string)HttpRuntime.Cache["UserSessionID"];

				if (cachedUserSessionID == requestUserSessionGUID)
				{
					return;
				}
				else
				{
					// same user is logged in with another session, force this session to logout
					FormsAuthentication.SignOut();
					Session.Abandon();

					return;
				}
			}
		}
	}

	protected override void OnError(EventArgs e)
	{
		Exception lastError = Server.GetLastError();

		if (lastError is not null && lastError is PathTooLongException)
		{
			//let it be handled elsewhere (global.asax.cs)
			return;
		}

		string exceptionUrl;
		string exceptionIpAddress;

		if (HttpContext.Current is not null)
		{
			if (HttpContext.Current.Request is not null)
			{
				exceptionUrl = $"{CultureInfo.CurrentCulture} - {HttpContext.Current.Request.RawUrl}";
				exceptionIpAddress = SiteUtils.GetIP4Address();
			}
		}

		int siteCount = DatabaseHelper.ExistingSiteCount();

		if (siteCount == 0)
		{
			Server.ClearError();

			log.Info("no sites or no database found in application error so try to redirect to Setup Page");

			try
			{
				HttpContext.Current.Response.Clear();
				HttpContext.Current.Response.Redirect(WebUtils.GetSiteRoot() + "/Setup/Default.aspx");
			}
			catch (HttpException) { }
		}

		bool upgradeNeeded = mojoSetup.UpgradeIsNeeded();

		if (upgradeNeeded)
		{
			try
			{
				log.Info("detected need for upgrade so redirecting to setup");

				Server.ClearError();
				HttpContext.Current.Response.Clear();
				HttpContext.Current.Response.Redirect(WebUtils.GetSiteRoot() + "/Setup/Default.aspx");
			}
			catch (HttpException) { }
		}
	}

	public void SuppressAllMenus()
	{
		Control menu = Master.FindControl("SiteMenu1");

		if (menu is not null)
		{
			menu.Visible = false;
		}

		SuppressPageMenu();
	}

	public void SuppressMenuSelection()
	{
		SiteMenu siteMenu = (SiteMenu)Master.FindControl("SiteMenu1");

		if (siteMenu is not null)
		{
			siteMenu.SuppressPageSelection = true;
		}
	}

	public void SuppressPageMenu()
	{
		PageMenuControl menu = (PageMenuControl)Master.FindControl("PageMenu1");

		if (menu is not null && menu.Direction == "Vertical")
		{
			menu.Visible = false;
		}

		menu = (PageMenuControl)Master.FindControl("PageMenu2");

		if (menu is not null && menu.Direction == "Vertical")
		{
			menu.Visible = false;
		}

		menu = (PageMenuControl)Master.FindControl("PageMenu3");

		if (menu is not null && menu.Direction == "Vertical")
		{
			menu.Visible = false;
		}
	}

	public void SuppressGoogleAds()
	{
		Control googleAdControl = Master.FindControl("pnlGoogle");

		if (googleAdControl is not null)
		{
			googleAdControl.Visible = false;
		}

		googleAdControl = Master.FindControl("pnlGoogle1");

		if (googleAdControl is not null)
		{
			googleAdControl.Visible = false;
		}

		googleAdControl = Master.FindControl("pnlGoogle2");

		if (googleAdControl is not null)
		{
			googleAdControl.Visible = false;
		}
	}
}
