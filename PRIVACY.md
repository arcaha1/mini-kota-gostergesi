# Privacy policy

Mini Kota Göstergesi bağımsız telemetri, reklam, güncelleme servisi veya uzak
sunucu içermez. Parola, token, kimlik doğrulama dosyası veya sohbet okumaz.
Yerel kurulu Codex app-server ile stdio üzerinden initialize ve
account/rateLimits/read çağrılarını yapar. Kota yanıtı bellekte tutulur;
uygulama tarafından dosyaya kaydedilmez. Kullanım sırasında 60 saniyede bir
sorgulanır. Durdurmak için göstergeyi kapatın.

Codex kendi mevcut oturumunu ve ağ bağlantısını yönetir; kota sorgusunun
karşılanması OpenAI hizmetleriyle iletişim gerektirebilir. Gösterge bu oturumu
kopyalamaz veya değiştirmez. Alt süreç için analytics.enabled=false geçici
başlatma parametresi kullanılır; yerleşik ayarlar dosyasına yazılmaz.

Etkileyen üçüncü taraf hizmet: OpenAI Codex / ChatGPT.
[OpenAI gizlilik politikası](https://openai.com/policies/privacy-policy/)
(bölgesel sürümlere bağlantı içerir).

Dil ve ilk karşılama tercihleri LocalApplicationData/MiniKotaGostergesi altında
saklanır. Görev çubuğu yerleşimi için Windows erişilebilirlik ağacındaki düğme
adları/konumları yerel olarak sorgulanır; kaydedilmez veya gönderilmez.
Kullanıcı tıklarsa resmi kullanım sayfası ya da Windows başlangıç ayarları açılır.
Kaldırma ve isteğe bağlı tercih temizliği README'de açıklanmıştır.
