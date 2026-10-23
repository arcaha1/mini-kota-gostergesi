using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
static class L {
 static readonly string preference=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MiniKotaGostergesi","language.txt");
 public static bool English=Load();
 public static CultureInfo Culture { get { return CultureInfo.GetCultureInfo(English ? "en-US" : "tr-TR"); } }
 static bool Load() {
  try { if(File.Exists(preference)) { string value=File.ReadAllText(preference).Trim(); if(value=="en") return true; if(value=="tr") return false; } } catch {}
  return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName!="tr";
 }
 public static bool SetLanguage(bool english) {
  English=english;
  try { Directory.CreateDirectory(Path.GetDirectoryName(preference)); File.WriteAllText(preference,english ? "en" : "tr"); return true; } catch { return false; }
 }
 static readonly Dictionary<string,string> translations=new Dictionary<string,string> {
  {"Mini Kota Göstergesi","Mini Quota Indicator"},
  {"Codex kullanım göstergesi","Codex usage indicator"},
  {"Ayrıntılar","Details"},
  {"Çıkış","Exit"},
  {"Kota bilgisi yok","No quota data"},
  {"Henüz sorgulanmadı.","Not checked yet."},
  {". Ayrıntılar için tıklayın.",". Click for details."},
  {"Kaynak: resmî Codex app-server\n","Source: official Codex app-server\n"},
  {"Son başarılı kontrol: ","Last successful check: "},
  {": kullanılan %",": used %"},
  {", kalan %",", remaining %"},
  {"\nYenilenme: ","\nResets: "},
  {"Veri eskidi; yeni sorgu bekleniyor.","Data is stale; waiting for a new check."},
  {"\n60 saniyede bir otomatik kontrol. Yerel sayaç yok.\nBu veri Codex kotasıdır; ChatGPT sohbet kotası değildir.\nGösterge yalnızca ChatGPT masaüstü uygulaması açıkken görünür.","\nChecks automatically every 60 seconds. No local counter.\nThese are Codex limits, not ChatGPT chat limits.\nVisible only while the ChatGPT desktop process is running."},
  {"Kontrol ediliyor…","Checking…"},
  {"Şimdi yenile","Refresh now"},
  {"Kullanım ayrıntıları","Usage details"},
  {"Resmî Codex kullanımını aç","Open official Codex usage"},
  {"Tarayıcı açılamadı: ","Could not open browser: "},
  {"Windows başlangıç ayarlarını aç","Open Windows startup settings"},
  {"Ayarlar açılamadı: ","Could not open settings: "},
  {"Uygulamadan çık","Exit application"},
  {"Codex başlatılamadı.","Could not start Codex."},
  {"Codex kotası bulunamadı.","Codex limits were not found."},
  {"Kota yanıtı geçersiz.","Invalid quota response."},
  {"Yenilenme zamanı geçersiz.","Invalid reset time."},
  {"Kota yanıtı güncel değil.","Quota response is out of date."},
  {"Kota penceresi yok.","No quota window available."},
  {"Codex bulunamadı. Codex masaüstü uygulamasını kurup oturum açın.","Codex was not found. Install the Codex desktop app and sign in."},
  {"Codex yanıtı zaman aşımına uğradı.","Codex response timed out."},
  {"Codex bağlantısı kapandı.","Codex connection closed."},
  {"Codex kota sorgusunu reddetti. Codex'te oturum açıldığını ve bağlantıyı kontrol edin.","Codex rejected the quota request. Check your Codex sign-in and connection."},
  {"Codex yanıtı geçersiz.","Invalid Codex response."},
  {"Mini Kota Göstergesi hazır.\n\n130×35 piksellik gösterge yalnızca ChatGPT masaüstü uygulaması açıkken görünür. Codex masaüstü kurulumu ve oturumu gerektirir. Gösterilen veri Codex kotasıdır.\n\nGöstergeye tıklayınca ayrıntılar açılır. Sağ tık menüsünden çıkabilirsiniz. Windows başlangıcını ayrıntılardaki düğmeden yönetebilirsiniz.\n\nBu bağımsız uygulama OpenAI tarafından geliştirilmemiştir.","Mini Quota Indicator is ready.\n\nThe 130×35 indicator appears only while the ChatGPT desktop process is running. It requires a Codex desktop installation and signed-in session. It displays Codex limits.\n\nClick for details. Right-click to exit or choose Turkish / English. Windows startup can be managed from the details window.\n\nThis independent app is not developed by OpenAI."},
  {" gün"," days"},
  {" saat"," hours"},
  {" dk"," min"},
  {"Kalan %","Remaining %"},
  {"Dil tercihi kaydedilemedi.","Could not save your language preference."}
 };
 public static string T(string text) { string result; return English && translations.TryGetValue(text,out result) ? result : text; }
}
