using System;
using System.Diagnostics;
using System.Reflection;
using Krypton.Toolkit;

namespace Org.Edgerunner.Moo.Udditor;
partial class AboutBox : KryptonForm
{
   public AboutBox()
   {
      InitializeComponent();
      this.Text = $"About {AssemblyTitle}";
      this.labelProductName.Text = AssemblyProduct;
      this.lblProject.Text = ProjectUrl;
      this.labelVersion.Text = $"Version {AssemblyVersion}";
      this.labelCopyright.Text = AssemblyCopyright;
      this.labelCompanyName.Text = AssemblyCompany;
      this.lblDescription.Text = AssemblyDescription;
   }

   public static AboutBox Instance { get; set; } = new();

   #region Assembly Attribute Accessors

   public string AssemblyTitle
   {
      get
      {
         object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
         if (attributes.Length > 0)
         {
            AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
            if (titleAttribute.Title != "")
            {
               return titleAttribute.Title;
            }
         }
         return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
      }
   }

   public string AssemblyVersion
   {
      get
      {
         return $"{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? new Version(0,0).ToString()} Beta";
      }
   }

   public string ProjectUrl => "https://github.com/wiredwiz/Moo-Developer-Tools";

   public string AssemblyDescription
   {
      get
      {
         object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
         if (attributes.Length == 0)
         {
            return "";
         }
         return ((AssemblyDescriptionAttribute)attributes[0]).Description;
      }
   }

   public string AssemblyProduct
   {
      get
      {
         object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
         if (attributes.Length == 0)
         {
            return "";
         }
         return ((AssemblyProductAttribute)attributes[0]).Product;
      }
   }

   public string AssemblyCopyright
   {
      get
      {
         object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
         if (attributes.Length == 0)
         {
            return "";
         }
         return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
      }
   }

   public string AssemblyCompany
   {
      get
      {
         object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
         if (attributes.Length == 0)
         {
            return "";
         }
         return ((AssemblyCompanyAttribute)attributes[0]).Company;
      }
   }
   #endregion

   private void lnkProject_LinkClicked(object sender, EventArgs e)
   {
      Process.Start(new ProcessStartInfo
      {
         FileName = ProjectUrl,
         UseShellExecute = true
      });
   }
}
