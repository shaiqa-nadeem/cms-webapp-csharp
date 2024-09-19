﻿using System;
using System.Globalization;
using mojoPortal.Business;
using mojoPortal.Business.WebHelpers;
using mojoPortal.Web.Framework;
using Resources;

namespace mojoPortal.Web.AdminUI;

public partial class PermissionsMenuPage : NonCmsBasePage
{
	private SiteSettings selectedSite = null;
	private int siteId = -1;

	protected void Page_Load(object sender, EventArgs e)
	{
		LoadParams();
		if (!Request.IsAuthenticated)
		{
			SiteUtils.RedirectToLoginPage(this);
			return;
		}
		if (!WebUser.IsAdmin)
		{
			SiteUtils.RedirectToAccessDeniedPage(this);
			return;
		}

		if ((siteId > -1) && (!siteSettings.IsServerAdminSite))
		{
			SiteUtils.RedirectToAccessDeniedPage(this);
			return;
		}


		LoadSettings();
		PopulateLabels();
	}


	private void PopulateLabels()
	{

		Title = SiteUtils.FormatPageTitle(siteSettings, Resource.SiteSettingsPermissionsTab);

		if (selectedSite.SiteId != siteSettings.SiteId)
		{
			heading.Text = string.Format(CultureInfo.InvariantCulture, Resource.SitePermissionFormat, selectedSite.SiteName, Resource.SiteSettingsPermissionsTab);
		}
		else
		{
			heading.Text = Resource.SiteSettingsPermissionsTab;
		}

		liSiteEditorRoles.Visible = WebConfigSettings.UseRelatedSiteMode && (selectedSite.SiteId != siteSettings.SiteId);
		lnkSiteEditorRoles.Text = Resource.SiteEditRolesLabel;
		lnkSiteEditorRoles.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.SiteEditor).ToString();

		lnkRolesThatCanViewCommerceReports.Text = Resource.RolesThatCanViewCommerceReportsLabel;
		lnkRolesThatCanViewCommerceReports.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.ViewCommerceReports).ToString();

		lnkRolesThatCanBrowseFileSystem.Text = Resource.GeneralBrowseRoles;
		lnkRolesThatCanBrowseFileSystem.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.GeneralBrowse).ToString();

		lnkRolesThatCanUploadAndBrowse.Text = Resource.GeneralBrowseAndUploadRoles;
		lnkRolesThatCanUploadAndBrowse.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.GeneralBrowseAndUpload).ToString();

		lnkRolesThatCanUploadAndBrowseUserOnly.Text = Resource.UserFilesBrowseAndUploadRoles;
		lnkRolesThatCanUploadAndBrowseUserOnly.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.UserBrowseAndUpload).ToString();

		lnkRolesThatCanDeleteFiles.Text = Resource.RolesThatCanDeleteFilesInEditor;
		lnkRolesThatCanDeleteFiles.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.DeleteFiles).ToString();

		lnkRolesThatCanManageSkins.Text = Resource.RolesThatCanManageSkins;
		lnkRolesThatCanManageSkins.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.ManageSkins).ToString();

		lnkRolesThatCanAssignSkinsToPages.Text = Resource.RolesThatCanAssignSkinsToPages;
		lnkRolesThatCanAssignSkinsToPages.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.AssignPageSkins).ToString();

		lnkRolesThatCanEditContentTemplates.Text = Resource.RolesThatCanEditContentTemplates;
		lnkRolesThatCanEditContentTemplates.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.EditContentTemplates).ToString();

		lnkRolesNOTAllowedInstanceSettings.Text = Resource.RolesNotAllowedToEditModuleSettings;
		lnkRolesNOTAllowedInstanceSettings.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.NoInstanceSettings).ToString();

		lnkRolesThatCanLookupUsers.Text = Resource.RolesThatCanLookupUsers;
		lnkRolesThatCanLookupUsers.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.LookupUsers).ToString();

		lnkRolesThatCanCreateUsers.Text = Resource.RolesThatCanCreateUsers;
		lnkRolesThatCanCreateUsers.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.CreateUsers).ToString();

		lnkRolesThatCanManageUsers.Text = Resource.RolesThatCanManageUsers;
		lnkRolesThatCanManageUsers.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.ManageUsers).ToString();

		lnkRolesThatCanViewMemberList.Text = Resource.RolesThatCanViewMemberList;
		lnkRolesThatCanViewMemberList.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.ViewMemberList).ToString();

		lnkRolesThatCanCreateRootLevelPages.Text = Resource.RolesThatCanCreateRootPages;
		lnkRolesThatCanCreateRootLevelPages.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.CreateRootPages).ToString();

		lnkDefaultRootLevelViewRoles.Text = Resource.DefaultRootPageViewRoles;
		lnkDefaultRootLevelViewRoles.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.ViewRootPages).ToString();

		lnkDefaultRootLevelEditRoles.Text = Resource.DefaultRootPageEditRoles;
		lnkDefaultRootLevelEditRoles.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.EditRootPages).ToString();

		lnkDefaultRootLevelCreateChildPageRoles.Text = Resource.DefaultRootPageCreateChildPageRoles;
		lnkDefaultRootLevelCreateChildPageRoles.NavigateUrl = "Admin/PermissionEdit.aspx".ToLinkBuilder().SiteId(selectedSite.SiteId).AddParam("p", CorePermission.CreateChildBelowRootPages).ToString();


		lnkAdminMenu.Text = Resource.AdminMenuLink;
		lnkAdminMenu.ToolTip = Resource.AdminMenuLink;
		lnkAdminMenu.NavigateUrl = "Admin/AdminMenu.aspx".ToLinkBuilder().ToString();

		lnkSiteList.Visible = WebConfigSettings.AllowMultipleSites && (siteSettings.IsServerAdminSite);
		lnkSiteList.Text = Resource.SiteList;
		lnkSiteList.NavigateUrl = "Admin/SiteList.aspx".ToLinkBuilder().ToString();
		litSiteListLinkSeparator.Visible = lnkSiteList.Visible;

		lnkPermissionsMenu.Text = Resource.SiteSettingsPermissionsTab;
		lnkPermissionsMenu.ToolTip = Resource.SiteSettingsPermissionsTab;
		lnkPermissionsMenu.NavigateUrl = "Admin/PermissionsMenu.aspx".ToLinkBuilder().SiteId(siteId).ToString();
	}

	private void LoadSettings()
	{
		if (siteId > -1 && siteSettings.IsServerAdminSite)
		{
			selectedSite = new SiteSettings(siteId);
			if (selectedSite.SiteId == -1)
			{
				selectedSite = siteSettings; // not found so use current site
			}
		}
		else
		{
			selectedSite = siteSettings; //currentsite
		}

		AddClassToBody("administration");
	}

	private void LoadParams()
	{
		siteId = WebUtils.ParseInt32FromQueryString("SiteID", siteId);
	}


	#region OnInit

	override protected void OnInit(EventArgs e)
	{
		base.OnInit(e);
		this.Load += new EventHandler(this.Page_Load);

		SuppressMenuSelection();
		SuppressPageMenu();


	}

	#endregion
}
