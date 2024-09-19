//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option or rebuild the Visual Studio project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Web.Application.StronglyTypedResourceProxyBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class PageManagerResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PageManagerResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.PageManagerResources", global::System.Reflection.Assembly.Load("App_GlobalResources"));
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Administration.
        /// </summary>
        internal static string Administration {
            get {
                return ResourceManager.GetString("Administration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deleting this page would make its child pages become root level pages but you do not have permission to create root level pages. Please either delete or move the child pages before deleting this page..
        /// </summary>
        internal static string CantOrphanPagesWarning {
            get {
                return ResourceManager.GetString("CantOrphanPagesWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delete Page.
        /// </summary>
        internal static string DeletePage {
            get {
                return ResourceManager.GetString("DeletePage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request denied: user {0} tried to delete page {1} {2} but did not heve permission..
        /// </summary>
        internal static string DeletePageDeniedLogFormat {
            get {
                return ResourceManager.GetString("DeletePageDeniedLogFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You do not have permission to delete the page: {0}.
        /// </summary>
        internal static string DeletePageNotAllowedFormat {
            get {
                return ResourceManager.GetString("DeletePageNotAllowedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to user {0} deleted page {1} {2}.
        /// </summary>
        internal static string DeletePageSuccessLogFormat {
            get {
                return ResourceManager.GetString("DeletePageSuccessLogFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to delete this page?.
        /// </summary>
        internal static string DeletePageWarning {
            get {
                return ResourceManager.GetString("DeletePageWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to delete this page? Child pages of this page will be orphaned and will become root level pages..
        /// </summary>
        internal static string DeletePageWithChildrenWarning {
            get {
                return ResourceManager.GetString("DeletePageWithChildrenWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Edit Page.
        /// </summary>
        internal static string EditPage {
            get {
                return ResourceManager.GetString("EditPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Edit Permissions.
        /// </summary>
        internal static string EditPermissions {
            get {
                return ResourceManager.GetString("EditPermissions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Edit Settings.
        /// </summary>
        internal static string EditSettings {
            get {
                return ResourceManager.GetString("EditSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Request.
        /// </summary>
        internal static string InvalidRequest {
            get {
                return ResourceManager.GetString("InvalidRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request denied: {0} tried to move page node {1} {2} {3} page node {4} {5}.
        /// </summary>
        internal static string MoveNodeRequestDeniedFormat {
            get {
                return ResourceManager.GetString("MoveNodeRequestDeniedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} moved page node {1} {2} {3} page node {4} {5}.
        /// </summary>
        internal static string MoveNodeRequestLogFormat {
            get {
                return ResourceManager.GetString("MoveNodeRequestLogFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to move the page?.
        /// </summary>
        internal static string MovePageConfirmPrompt {
            get {
                return ResourceManager.GetString("MovePageConfirmPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You do not have permission to edit or move the page: {0}.
        /// </summary>
        internal static string MovePageNotAllowedFormat {
            get {
                return ResourceManager.GetString("MovePageNotAllowedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You do not have permission to create child pages below {0}  therefore you cannnot move a page to this location..
        /// </summary>
        internal static string MoveToNewParentNotAllowedFormat {
            get {
                return ResourceManager.GetString("MoveToNewParentNotAllowedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, you are not allowed to create root level pages or move existing pages to the root level..
        /// </summary>
        internal static string MoveToRootNotAllowed {
            get {
                return ResourceManager.GetString("MoveToRootNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New Child Page.
        /// </summary>
        internal static string NewChildPage {
            get {
                return ResourceManager.GetString("NewChildPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New Root Level Page.
        /// </summary>
        internal static string NewRootPage {
            get {
                return ResourceManager.GetString("NewRootPage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Page Manager.
        /// </summary>
        internal static string PageManager {
            get {
                return ResourceManager.GetString("PageManager", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can drag and drop page nodes in the treeview below to move them around in the site hierarchy or to change their sort position. Click on a page to show a menu of commands related to that page..
        /// </summary>
        internal static string PageManagerInstructions {
            get {
                return ResourceManager.GetString("PageManagerInstructions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Protected.
        /// </summary>
        internal static string Protected {
            get {
                return ResourceManager.GetString("Protected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Public.
        /// </summary>
        internal static string Public {
            get {
                return ResourceManager.GetString("Public", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to sort the child pages of the selected page alphabetically?.
        /// </summary>
        internal static string SortAlphaPrompt {
            get {
                return ResourceManager.GetString("SortAlphaPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request denied: user {0} tried to sort child pages for page {1} {2} but did not heve permission..
        /// </summary>
        internal static string SortChildPagesDeniedLogFromat {
            get {
                return ResourceManager.GetString("SortChildPagesDeniedLogFromat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to user {0} sorted child pages below page {1} {2} alphabetically.
        /// </summary>
        internal static string SortChildPagesSuccessLogFormat {
            get {
                return ResourceManager.GetString("SortChildPagesSuccessLogFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sort Child Pages Alphabetically.
        /// </summary>
        internal static string SortPagesAlpha {
            get {
                return ResourceManager.GetString("SortPagesAlpha", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alternate Page Manager.
        /// </summary>
        internal static string StandardPageManager {
            get {
                return ResourceManager.GetString("StandardPageManager", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to View Page.
        /// </summary>
        internal static string ViewPage {
            get {
                return ResourceManager.GetString("ViewPage", resourceCulture);
            }
        }
    }
}
