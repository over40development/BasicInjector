﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Launcher.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.6.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("RETAIL")]
        public string GTAClient {
            get {
                return ((string)(this["GTAClient"]));
            }
            set {
                this["GTAClient"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoClose {
            get {
                return ((bool)(this["AutoClose"]));
            }
            set {
                this["AutoClose"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://192.168.0.1")]
        public string Host {
            get {
                return ((string)(this["Host"]));
            }
            set {
                this["Host"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string License {
            get {
                return ((string)(this["License"]));
            }
            set {
                this["License"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("/login.php")]
        public string Login {
            get {
                return ((string)(this["Login"]));
            }
            set {
                this["Login"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("/path_to/current_release/menu.mnu")]
        public string URL {
            get {
                return ((string)(this["URL"]));
            }
            set {
                this["URL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://website.url")]
        public string HomepageURL {
            get {
                return ((string)(this["HomepageURL"]));
            }
            set {
                this["HomepageURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Custom User Agent")]
        public string UserAgent {
            get {
                return ((string)(this["UserAgent"]));
            }
            set {
                this["UserAgent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://website-base.url")]
        public string BaseURL {
            get {
                return ((string)(this["BaseURL"]));
            }
            set {
                this["BaseURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("/path_to/endpoint.php")]
        public string BaseAuth {
            get {
                return ((string)(this["BaseAuth"]));
            }
            set {
                this["BaseAuth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("/path_to/menu_release/")]
        public string UpdateURL {
            get {
                return ((string)(this["UpdateURL"]));
            }
            set {
                this["UpdateURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int ManifestVersion {
            get {
                return ((int)(this["ManifestVersion"]));
            }
            set {
                this["ManifestVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<Manifest version=""1"">
  <!-- Your application will check for updates every (seconds) -->
  <CheckInterval>900</CheckInterval>
  <!-- The URI to the remote manifest -->
  <RemoteConfigUri>http://192.168.0.1/path_to/current_release/Update.xml</RemoteConfigUri>
  <!-- This token must be the same at both ends to avoid tampering -->
  <SecurityToken>00000000-0000-0000-0000-000000000000</SecurityToken>
  <!-- All payload files are assumed to have this URI prefix. -->
  <BaseUri>https://website.url/path_to/current_release/</BaseUri>
  <!-- One or more files containing updates. -->
  <Payload>UpdatePackage.zip</Payload>
</Manifest>")]
        public global::System.Xml.XmlDocument Manifest {
            get {
                return ((global::System.Xml.XmlDocument)(this["Manifest"]));
            }
            set {
                this["Manifest"] = value;
            }
        }
    }
}