using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Automation;

static class Yerlesim {
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr FindWindow(string cls,string title);
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [StructLayout(LayoutKind.Sequential)] struct Rect { public int Left,Top,Right,Bottom; }
 public static Rectangle? Locate() {
  IntPtr bar=FindWindow("Shell_TrayWnd",null); Rect r;
  if(bar==IntPtr.Zero || !GetWindowRect(bar,out r) || r.Bottom-r.Top<35) return null;
  try {
   var root=AutomationElement.FromHandle(bar);
   var buttons=root.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button));
   foreach(AutomationElement button in buttons) {
    string id=button.Current.AutomationId, name=button.Current.Name;
    if(id!="StartButton" && name!="Start" && name!="Başlat" && name!="Başlangıç") continue;
    var b=button.Current.BoundingRectangle;
    if(b.IsEmpty) continue;
    var position=new Rectangle((int)b.Left-138,r.Top+(r.Bottom-r.Top-35)/2,130,35);
    if(position.Left<r.Left+4 || position.Right>r.Right) return null;
    foreach(AutomationElement other in buttons) {
     var box=other.Current.BoundingRectangle;
     if(box.IsEmpty || box.Width>300 || box.Height>r.Bottom-r.Top+4) continue;
     if(position.IntersectsWith(new Rectangle((int)box.Left,(int)box.Top,(int)box.Width,(int)box.Height))) return null;
    }
    return position;
   }
  } catch {}
  return null;
 }
}
