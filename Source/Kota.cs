using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

class KotaWindow {
 public double Used;
 public int Minutes;
 public DateTime Reset;
 public string Name { get { return Minutes % 1440==0 ? (Minutes/1440)+L.T(" gün") : Minutes % 60==0 ? (Minutes/60)+L.T(" saat") : Minutes+L.T(" dk"); } }
}
class Kota {
 public DateTime Checked;
 public List<KotaWindow> Windows=new List<KotaWindow>();
 public bool Fresh(DateTime now) { return Windows.Count>0 && now>=Checked && (now-Checked).TotalSeconds<=120 && Windows.All(w=>w.Reset>now); }
 public KotaWindow Limiting { get { return Windows.OrderByDescending(w=>w.Used).First(); } }
 public string Summary { get { string number=(100-Limiting.Used).ToString("0.#",L.Culture); return L.English ? number+"% left" : "Kalan %"+number; } }
 static Dictionary<string,object> Obj(object v) { return v as Dictionary<string,object>; }
 static object Get(Dictionary<string,object> d,string key) { object v; return d!=null && d.TryGetValue(key,out v) ? v : null; }
 static bool Number(object v,out double n) { n=0; return v!=null && !(v is string) && !(v is bool) && double.TryParse(Convert.ToString(v,CultureInfo.InvariantCulture),NumberStyles.Float,CultureInfo.InvariantCulture,out n) && !double.IsNaN(n) && !double.IsInfinity(n); }
 public static Kota Parse(string json,DateTime now) {
  var root=Obj(new JavaScriptSerializer().DeserializeObject(json));
  var map=Obj(Get(root,"rateLimitsByLimitId"));
  var bucket=map!=null ? Obj(Get(map,"codex")) : Obj(Get(root,"rateLimits"));
  if(bucket==null || (Get(bucket,"limitId")!=null && Convert.ToString(Get(bucket,"limitId"))!="codex")) throw new Exception("Codex kotası bulunamadı.");
  var result=new Kota { Checked=now };
  foreach(string key in new[]{"primary","secondary"}) {
   object raw=Get(bucket,key); if(raw==null) continue;
   var win=Obj(raw); double used,mins,reset;
   if(!Number(Get(win,"usedPercent"),out used) || used<0 || used>100 || !Number(Get(win,"windowDurationMins"),out mins) || mins<=0 || mins>int.MaxValue || mins!=Math.Floor(mins) || !Number(Get(win,"resetsAt"),out reset)) throw new Exception("Kota yanıtı geçersiz.");
   DateTime date; try { date=new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc).AddSeconds(reset); } catch { throw new Exception("Yenilenme zamanı geçersiz."); }
   if(date<=now) throw new Exception("Kota yanıtı güncel değil.");
   result.Windows.Add(new KotaWindow { Used=used,Minutes=(int)mins,Reset=date });
  }
  if(result.Windows.Count==0) throw new Exception("Kota penceresi yok.");
  return result;
 }
}
class KotaReader : IDisposable {
 readonly object gate=new object(); Process active; bool disposed;
 public static string FindCodex() {
  string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"OpenAI","Codex","bin");
  if(Directory.Exists(folder)) {
   var paths=Directory.GetDirectories(folder).Select(d=>Path.Combine(d,"codex.exe")).Where(File.Exists).OrderByDescending(File.GetLastWriteTimeUtc).ToArray();
   if(paths.Length>0) return paths[0];
  }
  throw new Exception("Codex bulunamadı. Codex masaüstü uygulamasını kurup oturum açın.");
 }
 static async Task<Dictionary<string,object>> Request(Process p,string request,int id) {
  p.StandardInput.WriteLine(request); p.StandardInput.Flush();
  var until=DateTime.UtcNow.AddSeconds(25);
  while(DateTime.UtcNow<until) {
   var read=p.StandardOutput.ReadLineAsync();
   if(await Task.WhenAny(read,Task.Delay(TimeSpan.FromSeconds(Math.Max(0.01,(until-DateTime.UtcNow).TotalSeconds))))!=read) throw new Exception("Codex yanıtı zaman aşımına uğradı.");
   string line=await read; if(line==null) throw new Exception("Codex bağlantısı kapandı.");
   var item=new JavaScriptSerializer().DeserializeObject(line) as Dictionary<string,object>;
   object value; if(item==null || !item.TryGetValue("id",out value) || Convert.ToString(value,CultureInfo.InvariantCulture)!=id.ToString(CultureInfo.InvariantCulture)) continue;
   if(item.ContainsKey("error")) throw new Exception("Codex kota sorgusunu reddetti. Codex'te oturum açıldığını ve bağlantıyı kontrol edin.");
   if(!item.ContainsKey("result")) throw new Exception("Codex yanıtı geçersiz.");
   return item;
  }
  throw new Exception("Codex yanıtı zaman aşımına uğradı.");
 }
 public async Task<Kota> Read() {
  Process p=null;
  try {
   string exe=FindCodex();
   var info=new ProcessStartInfo(exe,"app-server --stdio -c analytics.enabled=false") { UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,WorkingDirectory=Path.GetDirectoryName(exe) };
   lock(gate) { if(disposed) throw new ObjectDisposedException("KotaReader"); active=p=Process.Start(info); }
   p.ErrorDataReceived+=delegate {}; p.BeginErrorReadLine();
   await Request(p,"{\"id\":1,\"method\":\"initialize\",\"params\":{\"clientInfo\":{\"name\":\"turkce_kota_gostergesi\",\"version\":\"2.0.0\"}}}",1);
   p.StandardInput.WriteLine("{\"method\":\"initialized\",\"params\":{}}");
   var response=await Request(p,"{\"id\":2,\"method\":\"account/rateLimits/read\"}",2);
   return Kota.Parse(new JavaScriptSerializer().Serialize(response["result"]),DateTime.UtcNow);
  } finally {
   lock(gate) {
    if(p!=null) { try { p.StandardInput.Close(); if(!p.WaitForExit(1000)) p.Kill(); } catch {} p.Dispose(); }
    active=null;
   }
  }
 }
 public void Dispose() { lock(gate) { disposed=true; if(active!=null) try { if(!active.HasExited) active.Kill(); } catch {} } }
}

