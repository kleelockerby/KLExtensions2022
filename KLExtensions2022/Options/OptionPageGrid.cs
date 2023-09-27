using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace KLExtensions2022 
{
    public class OptionPageGrid : DialogPage
    {
        [Category("Add New Class")]
        [DisplayName("Implicit Usings")]
        [Description("Use Implicit Using Statements")]
        [DefaultValue(false)]
        public bool UsingsOption { get; set; } = false;

        [Category("Add New Class")]
        [DisplayName("Namespace Option")]
        [Description("Select the value you want from the list.")]
        [DefaultValue(NamespaceOptions.Project)]
        [TypeConverter(typeof(EnumConverter))]
        public NamespaceOptions KLNamespaceOptions { get; set; } = NamespaceOptions.Project;
    }
}
