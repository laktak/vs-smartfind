using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;

namespace ChristianZangl.SmartFind
{
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  ///
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the
  /// IVsPackage interface and uses the registration attributes defined in the framework to
  /// register itself and its components with the shell.
  /// </summary>
  // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
  // a package.
  [PackageRegistration(UseManagedResourcesOnly = true)]
  // This attribute is used to register the information needed to show this package
  // in the Help/About dialog of Visual Studio.
  [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
  // This attribute is needed to let the shell know that this package exposes some menus.
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [Guid(GuidList.guidSmartFindPkgString)]
  public sealed class SmartFindPackage : Package
  {
    public static readonly string SettingsFile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Visual Studio 2012\Settings\SmartFind.vssettings");
    public static SmartFindPackage Instance { get; private set; }

    /// <summary>
    /// Default constructor of the package.
    /// Inside this method you can place any initialization code that does not require
    /// any Visual Studio service because at this point the package object is created but
    /// not sited yet inside Visual Studio environment. The place to do all the other
    /// initialization is the Initialize method.
    /// </summary>
    public SmartFindPackage()
    {
      Instance=this;
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
    }

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize()
    {
      Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
      base.Initialize();

      // Add our command handlers for menu (commands must exist in the .vsct file)
      OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (null != mcs)
      {
        // Create the command for the menu item.
        CommandID menuCommandID = new CommandID(GuidList.guidSmartFindCmdSet, (int)PkgCmdIDList.cmdidResetSmartFind);
        MenuCommand menuItem = new MenuCommand(menuResetOptions, menuCommandID);
        mcs.AddCommand(menuItem);
      }
    }

    /// <summary>
    /// This function is the callback used to execute a command when the a menu item is clicked.
    /// See the Initialize method to see how the menu item is associated to this function using
    /// the OleMenuCommandService service and the MenuCommand class.
    /// </summary>
    private void menuResetOptions(object sender, EventArgs e)
    {
      ResetOptions(false);
    }

    public void ResetOptions(bool initial)
    {
      var dte=Package.GetGlobalService(typeof(_DTE)) as _DTE;
      dte.ExecuteCommand("Tools.ImportandExportSettings", "/export:\""+SmartFindPackage.SettingsFile+"\"");

      var doc=XElement.Load(SmartFindPackage.SettingsFile);
      foreach (var item in doc.Elements().ToArray())
      {
        if (item.Name=="ApplicationIdentity") { }
        else if (item.Name=="Category")
        {
          if (item.Attribute("name").Value=="Environment_Group")
          {
            foreach (var sub in item.Elements().ToArray())
            {
              if (sub.Name=="Category" && sub.Attribute("name").Value=="Environment_UnifiedFind")
              {
              }
              else sub.Remove();
            }
          }
          else item.Remove();
        }
        else item.Remove();
      }
      doc.Save(SmartFindPackage.SettingsFile);

      string text="SmartFind will set the currently active options every time you open the find/replace dialogs.";
      if (initial) text+="\r\n\r\nYou can update the options using the Tools/Reset SmartFind menu.";
      MessageBox(text);
    }

    public void MessageBox(string message)
    {
      System.Windows.Forms.MessageBox.Show(message, "SmartFind", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
      //IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
      //Guid clsid = Guid.Empty;
      //int result;
      //Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
      //  uiShell.ShowMessageBox(0, ref clsid, "SmartFind", message, string.Empty, 0,
      //  OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO, 0, out result));
    }
  }
}
