﻿namespace mojoPortal.Web.UI;

public class CoreDisplaySettings : BaseDisplaySettings
{
    public CoreDisplaySettings() : base() { }
	public override string FeatureName => "Core";

	public string DefaultPageHeaderMarkup { get; set; } = "<h2>{0}</h2>";
	public string AlertSuccessMarkup { get; set; } = "<div class='alert alert-success'>{0}</div>";
	public string AlertNoticeMarkup { get; set; } = "<div class='alert alert-info'>{0}</div>";
	public string AlertWarningMarkup { get; set; } = "<div class='alert alert-warning'>{0}</div>";
	public string AlertErrorMarkup { get; set; } = "<div class='alert alert-danger'>{0}</div>";
	public bool HideAllMenusOnSiteClosedPage { get; set; } = WebConfigSettings.HideAllMenusOnSiteClosedPage;
	public bool HideMenusOnLoginPage { get; set; } = WebConfigSettings.HideMenusOnLoginPage;
	public bool HideMenusOnSiteMap { get; set; } = WebConfigSettings.HideMenusOnSiteMap;
	public bool HidePageMenusOnSiteMap { get; set; } = WebConfigSettings.HidePageMenusOnSiteMap;
	public bool HideMenusOnRegisterPage { get; set; } = WebConfigSettings.HideMenusOnRegisterPage;
	public bool HideMenusOnPasswordRecoveryPage { get; set; } = WebConfigSettings.HideMenusOnPasswordRecoveryPage;
	public bool HideMenusOnChangePasswordPage { get; set; } = WebConfigSettings.HideMenusOnChangePasswordPage;
	public bool HideAllMenusOnProfilePage { get; set; } = WebConfigSettings.HideAllMenusOnProfilePage;
	public bool HidePageMenuOnProfilePage { get; set; } = WebConfigSettings.HidePageMenuOnProfilePage;
	public bool HidePageMenuOnMemberListPage { get; set; } = WebConfigSettings.HidePageMenuOnMemberListPage;
}