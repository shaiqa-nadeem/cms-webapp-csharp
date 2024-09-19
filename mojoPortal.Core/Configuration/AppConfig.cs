﻿using System.Configuration;
using System.Web.Configuration;

namespace mojoPortal.Core.Configuration;

public static class AppConfig
{
	public static bool Debug
	{
		get
		{
			var compilationSection = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
			return compilationSection is not null && compilationSection.Debug;
		}
	}

	/// <summary>
	/// this can be used to detect a secure request in a proxied environment when the mere presence of a specific server variable indicates a secure connection
	/// for example this can be used with IIS AAR (Application Request Routing Module) where the presence of a server variable named HTTP_X_ARR_SSL indicates a secure request
	/// So you would add this to user.config  <add key="SecureConnectionServerVariableForPresenceCheck" value="HTTP_X_ARR_SSL"/>
	/// This setting is checked in WebHelper.IsSecureRequest();
	/// </summary>
	public static string SecureConnectionServerVariableForPresenceCheck => ConfigHelper.GetStringProperty("SecureConnectionServerVariableForPresenceCheck", string.Empty);

	/// <summary>
	/// use this if you need to check a custom server variable for a specific value to determine a secure request
	/// you must also set the value for SecureConnectionServerVariableSecureValue that corresponds to a secure request
	/// </summary>
	public static string SecureConnectionServerVariableForValueCheck => ConfigHelper.GetStringProperty("SecureConnectionServerVariableForValueCheck", string.Empty);

	public static string SecureConnectionServerVariableSecureValue => ConfigHelper.GetStringProperty("SecureConnectionServerVariableSecureValue", string.Empty);

	public static string StaticFileExtensions => ConfigHelper.GetStringProperty("StaticFileExtensions", ".asf|.asx|.avi|.css|.csv|.doc|.docx|.gif|.htm|.html|.ico|.jpeg|.jpg|.js" +
		"|.json|.less|.m4a|.m4v|.mov|.mp3|.mp4|.mpeg|.mpg|.oga|.ogg|.ogv|.pdf|.png|.pps|.ppt|.pptx|.svg|.tif|.ttf|.txt|.wav|.webm|.webma|.webmv|.webp|.wmv|.woff|.xls|.xlsx|.xml|.zip").ToLower();

	public static string EditorTemplatesOrder => ConfigHelper.GetStringProperty("EditorTemplatesOrder", "site,skin,system");

	public static string JQueryVersion => ConfigHelper.GetStringProperty("JQueryVersion", "3.6.0");

	public static string JQueryUIVersion => ConfigHelper.GetStringProperty("JQueryUIVersion", "~/Scripts/");

	public static string JQueryBasePath => ConfigHelper.GetStringProperty("JQueryPath", "~/Scripts/");

	public static string GoogleAnalyticsInitScript => ConfigHelper.GetStringProperty("GoogleAnalyticsInitScript", "~/ClientScript/GA4-gtag.js");

	public static string GoogleAnalyticsScript => ConfigHelper.GetStringProperty("GoogleAnalyticsScript", "https://www.googletagmanager.com/gtag/js?id=");

	public static bool EnableUploads => ConfigHelper.GetBoolProperty("EnableUploads", true);

	public static string DefaultAdminUserEmailFormat => ConfigHelper.GetStringProperty("DefaultAdminUserEmailFormat", "admin{0}@admin.com");

	public static string DefaultAdminUsernameFormat => ConfigHelper.GetStringProperty("DefaultAdminUsernameFormat", "admin{0}");

	public static string DefaultAdminPassword => ConfigHelper.GetStringProperty("DefaultAdminPassword", "admin");

	public static string DefaultAdminSecurityQuestion => ConfigHelper.GetStringProperty("DefaultAdminSecurityQuestion", "What is your username?");

	public static string DefaultAdminSecurityAnswer => ConfigHelper.GetStringProperty("DefaultAdminSecurityPassword", "admin");

	public static bool RelatedSiteModeEnabled => ConfigHelper.GetBoolProperty("UseRelatedSiteMode", false);

	public static bool RelatedSiteModeShareContentFolder => ConfigHelper.GetBoolProperty("UseSameContentFolderForRelatedSiteMode", false);

	public static int RelatedSiteID => ConfigHelper.GetIntProperty("RelatedSiteID", 1);

	public static bool RelatedSiteModeHideRoleManagerInChildSites => ConfigHelper.GetBoolProperty("RelatedSiteModeHideRoleManagerInChildSites", true);

	public static bool SanitizeQueryStrings => ConfigHelper.GetBoolProperty("SanitizeQueryStrings", true);
}