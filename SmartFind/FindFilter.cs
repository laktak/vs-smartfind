using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace ChristianZangl.SmartFind
{
  [Export(typeof(IVsTextViewCreationListener))]
  [ContentType("text")]
  [TextViewRole(PredefinedTextViewRoles.Interactive)]
  class VsTextViewCreationListener : IVsTextViewCreationListener
  {
    public void VsTextViewCreated(IVsTextView textViewAdapter)
    {
      var filter=new Filter();

      IOleCommandTarget next;
      if (ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out next)))
        filter.Next=next;
    }
  }

  class Filter : IOleCommandTarget
  {
    internal IOleCommandTarget Next { get; set; }

    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
      //Debug.WriteLine("cmd: "+nCmdID+"-"+((VSConstants.VSStd97CmdID)nCmdID)+" "+(pguidCmdGroup==VSConstants.GUID_VSStandardCommandSet97));

      if (pguidCmdGroup==VSConstants.GUID_VSStandardCommandSet97 &&
        (nCmdID==(uint)VSConstants.VSStd97CmdID.Find ||
        nCmdID==(uint)VSConstants.VSStd97CmdID.FindInFiles ||
        nCmdID==(uint)VSConstants.VSStd97CmdID.ReplaceInFiles ||
        nCmdID==(uint)VSConstants.VSStd97CmdID.Replace))
      {
        var dte=Package.GetGlobalService(typeof(_DTE)) as _DTE;

        if (!File.Exists(SmartFindPackage.SettingsFile))
          SmartFindPackage.Instance.ResetOptions(true);

        dte.ExecuteCommand("Tools.ImportandExportSettings", "/import:\""+SmartFindPackage.SettingsFile+"\"");
      }

      int hresult=Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
      return hresult;
    }

    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
    {
      return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
    }
  }

}
