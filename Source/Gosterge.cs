using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;

static class Program {
 [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
 [STAThread] static void Main() {
  SetProcessDPIAware(); Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  bool first; using(var mutex=new Mutex(true,"Local\\TurkceKullanimGostergesi_v1",out first)) {
   if(!first) return;
   string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MiniKotaGostergesi");
   string welcome=Path.Combine(folder,"welcome-v1.txt");
   if(!File.Exists(welcome)) {
    MessageBox.Show(L.T("Mini Kota Göstergesi hazır.\n\n130×35 piksellik gösterge yalnızca ChatGPT masaüstü uygulaması açıkken görünür. Codex masaüstü kurulumu ve oturumu gerektirir. Gösterilen veri Codex kotasıdır.\n\nGöstergeye tıklayınca ayrıntılar açılır. Sağ tık menüsünden çıkabilirsiniz. Windows başlangıcını ayrıntılardaki düğmeden yönetebilirsiniz.\n\nBu bağımsız uygulama OpenAI tarafından geliştirilmemiştir."),L.T("Mini Kota Göstergesi"),MessageBoxButtons.OK,MessageBoxIcon.Information);
    try { Directory.CreateDirectory(folder); File.WriteAllText(welcome,"1"); } catch {}
   }
   Application.Run(new Widget());
  }
 }
}
class Widget : Form {
 public static string Status { get { return L.T("Kota bilgisi yok"); } }
 public const string Dashboard="https://chatgpt.com/codex/settings/usage";
 readonly System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer();
 readonly KotaReader reader=new KotaReader();
 Kota kota; string problem="Henüz sorgulanmadı."; DateTime nextPoll=DateTime.MinValue;
 bool busy,closing; Label detailText; Button refresh;
 public bool TestMode;
 public bool TaskbarMode=false;
 string iconState;
 Icon customIcon;
 public bool PollingEnabled=true;
 readonly Func<bool> hostDetector;
 bool hostOpen;
 Form details;
 Rectangle? cachedPlacement;
 bool placementBusy;
 DateTime nextPlacement=DateTime.MinValue;
 protected override bool ShowWithoutActivation { get { return true; } }
 public Widget() : this(IsChatGptOpen) {}
 public Widget(Func<bool> detector) {
  hostDetector=detector;
  Text=L.T("Codex kullanım göstergesi"); AutoScaleMode=AutoScaleMode.None;
  FormBorderStyle=FormBorderStyle.None; ClientSize=new Size(130,35); ShowInTaskbar=false; TopMost=true;
  BackColor=Color.FromArgb(32,34,37); ForeColor=Color.White; DoubleBuffered=true;
  AccessibleName="Codex: "+Status+L.T(". Ayrıntılar için tıklayın."); Cursor=Cursors.Hand;
  BuildMenu();
  MouseClick+=delegate(object s,MouseEventArgs e) { if(e.Button==MouseButtons.Left) OpenDetails(); };
  Shown+=delegate { Tick(); }; timer.Interval=2000; timer.Tick+=delegate { Tick(); }; timer.Start();
  Resize+=delegate { if(TaskbarMode && !TestMode && WindowState==FormWindowState.Normal && Visible) { WindowState=FormWindowState.Minimized; OpenDetails(); } };
 }
 protected override void SetVisibleCore(bool value) {
  bool show=value && (TestMode || hostDetector());
  if(show && TaskbarMode && !TestMode) { WindowState=FormWindowState.Minimized; ShowInTaskbar=true; }
  base.SetVisibleCore(show);
 }
 protected override void WndProc(ref Message m) {
  if(TaskbarMode && !TestMode && m.Msg==0x112 && ((long)m.WParam & 0xFFF0)==0xF120) { OpenDetails(); return; }
  base.WndProc(ref m);
 }
 public static bool IsChatGptOpen() {
  Process[] processes; try { processes=Process.GetProcessesByName("ChatGPT"); } catch(System.ComponentModel.Win32Exception) { return false; } catch(InvalidOperationException) { return false; }
  try { return processes.Length > 0; } finally { foreach(var p in processes) p.Dispose(); }
 }
 internal void Tick() {
  if(TestMode) { Place(); return; }
  bool open=hostDetector();
  if(open && !hostOpen) { nextPoll=DateTime.MinValue; kota=null; }
  hostOpen=open;
  if(!open) { if(Visible) Hide(); if(details!=null && !details.IsDisposed) details.Hide(); kota=null; return; }
  if(!Visible) Show(); if(!TaskbarMode) Place(); RenderState();
  if(PollingEnabled && !busy && DateTime.UtcNow>=nextPoll) Poll();
 }
 async void Poll() {
  if(busy || closing) return;
  busy=true; RenderState();
  try { var value=await Task.Run(()=>reader.Read()); if(!closing) { kota=value; problem=""; } }
  catch(Exception ex) { if(!closing) { kota=null; problem=ex is System.ComponentModel.Win32Exception ? "Codex başlatılamadı." : ex.Message; } }
  finally { busy=false; nextPoll=DateTime.UtcNow.AddSeconds(60); if(!closing) RenderState(); }
 }
 public string Caption { get { return kota!=null && kota.Fresh(DateTime.UtcNow) ? kota.Summary : Status; } }
 internal void SetKota(Kota value) { kota=value; RenderState(); }
 void BuildMenu() {
  var old=ContextMenuStrip;
  var menu=new ContextMenuStrip();
  menu.Items.Add(L.T("Ayrıntılar"),null,delegate { OpenDetails(); });
  var languages=new ToolStripMenuItem("Dil / Language");
  var tr=new ToolStripMenuItem("Türkçe") { Checked=!L.English };
  var en=new ToolStripMenuItem("English") { Checked=L.English };
  tr.Click+=delegate { ChangeLanguage(false); }; en.Click+=delegate { ChangeLanguage(true); };
  languages.DropDownItems.Add(tr); languages.DropDownItems.Add(en); menu.Items.Add(languages);
  menu.Items.Add(L.T("Çıkış"),null,delegate { Close(); }); ContextMenuStrip=menu;
  if(old!=null) old.Dispose();
 }
 void ChangeLanguage(bool english) {
  if(!L.SetLanguage(english)) MessageBox.Show(L.T("Dil tercihi kaydedilemedi."),L.T("Mini Kota Göstergesi"));
  bool reopen=details!=null && !details.IsDisposed && details.Visible;
  if(details!=null) { details.Dispose(); details=null; detailText=null; refresh=null; }
  Text=L.T("Codex kullanım göstergesi"); BuildMenu(); RenderState(); if(reopen) OpenDetails();
 }
 string DetailMessage() {
  string result=L.T("Kaynak: resmî Codex app-server\n");
  if(kota!=null && kota.Fresh(DateTime.UtcNow)) {
   result+=L.T("Son başarılı kontrol: ")+kota.Checked.ToLocalTime().ToString("HH:mm:ss")+"\n\n";
   foreach(var w in kota.Windows) { string used=w.Used.ToString("0.#",L.Culture), left=(100-w.Used).ToString("0.#",L.Culture); result+=w.Name+(L.English ? ": used "+used+"%, remaining "+left+"%" : ": kullanılan %"+used+", kalan %"+left)+L.T("\nYenilenme: ")+w.Reset.ToLocalTime().ToString(L.English ? "yyyy-MM-dd HH:mm" : "dd.MM.yyyy HH:mm",L.Culture)+"\n"; }
  } else result+=Status+"\n"+(kota!=null ? L.T("Veri eskidi; yeni sorgu bekleniyor.") : L.T(problem))+"\n";
  return result+L.T("\n60 saniyede bir otomatik kontrol. Yerel sayaç yok.\nBu veri Codex kotasıdır; ChatGPT sohbet kotası değildir.\nGösterge yalnızca ChatGPT masaüstü uygulaması açıkken görünür.");
 }
 void RenderState() {
  AccessibleName="Codex: "+Caption+L.T(". Ayrıntılar için tıklayın."); Invalidate();
  if(TaskbarMode && !TestMode) {
   Text="Codex · "+Caption+(kota!=null && kota.Fresh(DateTime.UtcNow) ? " · "+kota.Limiting.Name : "");
   if(iconState!=Text) {
    iconState=Text; string number=kota!=null && kota.Fresh(DateTime.UtcNow) ? Math.Floor(100-kota.Limiting.Used).ToString() : "?";
    using(var bmp=new Bitmap(32,32)) {
     using(var g=Graphics.FromImage(bmp)) {
      g.Clear(Color.FromArgb(32,34,37));
      using(var f=new Font("Segoe UI",number.Length>2 ? 12f : 15f,FontStyle.Bold)) TextRenderer.DrawText(g,number,f,new Rectangle(0,0,32,32),number=="?" ? Color.Silver : Color.FromArgb(110,231,183),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
     }
     IntPtr handle=bmp.GetHicon(); try { using(var borrowed=System.Drawing.Icon.FromHandle(handle)) { var old=customIcon; customIcon=(Icon)borrowed.Clone(); Icon=customIcon; if(old!=null) old.Dispose(); } } finally { DestroyIcon(handle); }
    }
   }
  }
  if(detailText!=null && !detailText.IsDisposed) detailText.Text=DetailMessage();
  if(refresh!=null && !refresh.IsDisposed) { refresh.Enabled=!busy; refresh.Text=busy ? L.T("Kontrol ediliyor…") : L.T("Şimdi yenile"); }
 }
 [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
 public static Rectangle Position(Rectangle area) { return new Rectangle(area.Right-138,area.Bottom-43,130,35); }
 public async void Place() {
  Bounds=TestMode ? Position(Screen.PrimaryScreen.WorkingArea) : (cachedPlacement ?? Position(Screen.PrimaryScreen.WorkingArea)); TopMost=true;
  if(TestMode || placementBusy || closing || DateTime.UtcNow<nextPlacement) return;
  placementBusy=true; nextPlacement=DateTime.UtcNow.AddSeconds(30);
  try {
   // Windows shell accessibility can stall; it must never block the widget UI.
   var found=await Task.Run(()=>Yerlesim.Locate());
   if(!closing && !IsDisposed) { cachedPlacement=found; Bounds=found ?? Position(Screen.PrimaryScreen.WorkingArea); }
  } catch {} finally { placementBusy=false; }
 }
 protected override void OnPaint(PaintEventArgs e) {
  base.OnPaint(e); using(var pen=new Pen(Color.FromArgb(73,78,83))) e.Graphics.DrawRectangle(pen,0,0,129,34);
  string title="Codex"+(kota!=null && kota.Fresh(DateTime.UtcNow) ? " · "+kota.Limiting.Name : "");
  using(var f=new Font("Segoe UI",11f,FontStyle.Regular,GraphicsUnit.Pixel)) TextRenderer.DrawText(e.Graphics,title,f,new Rectangle(6,1,120,15),Color.FromArgb(163,173,181),TextFormatFlags.NoPadding);
  using(var f=new Font("Segoe UI",13f,FontStyle.Regular,GraphicsUnit.Pixel)) TextRenderer.DrawText(e.Graphics,Caption,f,new Rectangle(6,16,120,18),Color.White,TextFormatFlags.NoPadding);
 }
 public void OpenDetails() {
  if(details!=null && !details.IsDisposed) { if(!details.Visible) details.Show(this); details.Activate(); return; }
  details=new Form { Text=L.T("Kullanım ayrıntıları"),ClientSize=new Size(440,470),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,TopMost=true,StartPosition=FormStartPosition.CenterScreen,Font=new Font("Segoe UI",9f) };
  detailText=new Label { Location=new Point(18,16),Size=new Size(404,290),AutoEllipsis=true,Text=DetailMessage() };
  details.Controls.Add(detailText);
  var open=new Button { Text=L.T("Resmî Codex kullanımını aç"),Location=new Point(18,324),Size=new Size(250,30) };
  open.Click+=delegate { try { Process.Start(new ProcessStartInfo(Dashboard){UseShellExecute=true}); } catch(Exception ex) { MessageBox.Show(details,L.T("Tarayıcı açılamadı: ")+ex.Message); } }; details.Controls.Add(open);
  refresh=new Button { Text=L.T("Şimdi yenile"),Location=new Point(278,324),Size=new Size(144,30) };
  refresh.Click+=delegate { Poll(); }; details.Controls.Add(refresh);
  var start=new Button { Text=L.T("Windows başlangıç ayarlarını aç"),Location=new Point(18,366),Size=new Size(404,30) };
  start.Click+=delegate { try { Process.Start(new ProcessStartInfo("ms-settings:startupapps"){UseShellExecute=true}); } catch(Exception ex) { MessageBox.Show(details,L.T("Ayarlar açılamadı: ")+ex.Message); } }; details.Controls.Add(start);
  var exit=new Button { Text=L.T("Uygulamadan çık"),Location=new Point(284,425),Size=new Size(138,28) }; exit.Click+=delegate { Close(); }; details.Controls.Add(exit);
  details.AutoScaleDimensions=new SizeF(96,96); details.AutoScaleMode=AutoScaleMode.Dpi;
  details.Show(this);
 }
 protected override void Dispose(bool disposing) { if(disposing) { closing=true; timer.Dispose(); reader.Dispose(); if(details!=null) details.Dispose(); } base.Dispose(disposing); if(disposing && customIcon!=null) { customIcon.Dispose(); customIcon=null; } }
}





