﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TSPlotGenerator.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\DEV\\Swaner\\TSPlotGenerator\\TSPlotGenerator\\bin\\Debug\\images")]
        public string imagePath {
            get {
                return ((string)(this["imagePath"]));
            }
            set {
                this["imagePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>pH_Avg, 0, 14</string>
  <string>Cond_Avg, 0, 3000</string>
  <string>WLevel_ft_Avg, 0, 10 </string>
  <string>Turb_Median, -5, 1650</string>
  <string>LDO_mgl_Avg, 0, 20</string>
  <string>MS_WTemp_C_Avg, -5, 40</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection VariableCode_Bounds {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["VariableCode_Bounds"]));
            }
            set {
                this["VariableCode_Bounds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>DO, Temp, -5, 40</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection MultVarVariable_Bounds {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["MultVarVariable_Bounds"]));
            }
            set {
                this["MultVarVariable_Bounds"] = value;
            }
        }
    }
}
