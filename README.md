# Mini Kota Göstergesi

Windows x64 için bağımsız, Türkçe/İngilizce Codex kota göstergesi.
Sürüm: 1.0.2.0. Lisans: [MIT](LICENSE), Copyright (c) 2026 arcah.
OpenAI tarafından geliştirilmiş veya onaylanmış bir ürün değildir.

**İlk herkese açık deneysel sürüm. İmzasızdır; canlı çalışma ve kararlılık testleri tamamlanmamıştır. SignPath başvurusu henüz gönderilmemiştir.**

130×35 piksel gösterge yalnız ChatGPT masaüstü işlemi açıkken görünür.
Gösterilen veri Codex kotasıdır; ChatGPT sohbet kotası değildir. Kurulu Codex
masaüstü uygulaması ve kullanıcının mevcut oturumu gerekir. Gösterge 60 saniyede
bir sorgular; 120 saniyeden eski veriyi güncel yüzde olarak göstermez.

## Derleme ve kullanım

Windows x64 .NET Framework 4.x bileşenleriyle PowerShell'de `./Build.ps1`.
Çıktı: `build/KullanimGostergesi.exe`. NuGet veya başka paket indirilmez.
Bu işlem yalnız derler. ExecutionPolicy veya Windows güvenlik ayarını değiştirmeyin.
Engellenen dosyanın korumayı aşarak çalıştırılması desteklenmez.
İmza ve canlı test tamamlandıktan sonra taşınabilir EXE ayrı bir klasörden
kullanılabilir. Mevcut masaüstü kısayolunun hedefi henüz değiştirilmemiştir.

## Kaldırma

Sağ tık > Çıkış ile göstergeyi kapatın. Yalnız bu uygulama için oluşturduğunuz
kısayolu ve uygulama klasörünü kaldırın. Tercihleri saklayabilirsiniz; isterseniz
LocalApplicationData/MiniKotaGostergesi içindeki language.txt ve welcome-v1.txt
dosyalarını kaldırabilirsiniz. Codex, ChatGPT, hesap, sohbet veya parola
klasörlerini kaldırmayın. Uygulama otomatik başlangıç girdisi oluşturmaz;
başlangıç ayarları düğmesi yalnız Windows ayar sayfasını açar.

## Gizlilik ve imzalama

- [Privacy policy](PRIVACY.md)
- [Code signing policy](CODE-SIGNING-POLICY.md)
- [Bağımlılıklar](THIRD-PARTY-NOTICES.md)
- [Yayın taslağı](RELEASE-DRAFT.md)
- [Yayın öncesi kontrol](SIGNPATH-CHECKLIST.md)

## Bilinen sınırlar

Önceki AppHangB1 kaydı çözülmüş sayılmaz. Kabuk erişilebilirlik sorgusunda zaman
aşımı yoktur; kapanışta çocuk süreç için bekleme vardır. Uzun çalışma, kapanış,
DPI ve çoklu monitör testleri henüz yapılmadı. Codex keşfi belirli yerel kurulum
dizinine bağlıdır. Geçerli imza Windows'un bu bilgisayardaki ilkesinin dosyaya
izin vereceğini garanti etmez. SignPath başvurusu ayrıca sahibinin onayını gerektirir.

