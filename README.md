# Stok Takip Uygulaması — v4.0.0

C# ve Windows Forms ile hazırlanmış çevrimdışı stok ve satış takip uygulaması. MEB Yazılım Projesi 11–12 ders materyalinin 59–105. sayfalarındaki işlevsel gereksinimlerden esinlenilerek sıfırdan uygulanmıştır. Kaynak kitaptaki kod, resim veya ikonlar paketlenmemiştir. Bu sürüm MagicBento şablonundan esinlenen yerel Windows Forms kontrol paneli, mor spotlight ve etkileşimli kartlar sunar. Veritabanı ve işlem mantığı v1.0 ile aynıdır.

Geliştirici: [heavenylz](https://github.com/heavenylz) · © 2026 heavenylz

## Başlatma

1. Windows'ta Visual Studio 2026 ve **.NET masaüstü geliştirme** iş yükünü, .NET 10 SDK ile birlikte kurun.
2. `StokTakipUygulamasi.sln` dosyasını açın. NuGet paketlerinin geri yüklenmesine izin verin.
3. F5 tuşuna basın (veya terminalde `dotnet run --project StokTakipUygulamasi/StokTakipUygulamasi.csproj`). İlk kullanımda paketlerin indirilmesi için internet gerekebilir.
4. İlk açılışta yönetici hesabı oluşturulur. Kullanıcı adı varsayılan olarak `admin` görünür; en az 8 karakterli kendi şifrenizi belirleyin. Sabit bir varsayılan parola yoktur.
5. İlk yönetici ad ve telefon alanları geçici olarak `Sistem Yöneticisi` ve `0000000000` değerlerine sahiptir. Personel İşlemleri ekranından güncelleyin. Telefon alanına başında 0 olmadan 10 rakam girilir.

## İlk deneme sırası

Tedarikçi ekleyin → Ürün İşlemleri bölümünde kategori oluşturun (`+` butonu) → ürün ekleyin (örneğin stok 25) → müşteri ekleyin → Yeni satış kartını açıp telefonla müşteriyi seçin → ürünü satış listesine ekleyin, miktar 7 girin → SATIŞ YAP. Stok 18 olur. Satış Detayları ekranında satışın ayrıntılarını seçip iptal ederseniz stok 25 olmalıdır. Stok miktarı 20'nin altına düştüğünde düşük stok ekranında görünür.

## Veritabanı

İlk açılışta otomatik yaratılan dosya:

`%LOCALAPPDATA%\StokTakipUygulamasi\stok_takip_db.sqlite3`

Sekiz tablo: kategoriler, tedarikciler, urunler, musteriler, personeller, yetkiler, satislar, satis_detaylari. İki yetki vardır: Yönetici ve Personel. Personel işlemleri yalnızca yöneticiye açıktır. Veri tabanı dosyasını yedeklemek için uygulamayı kapatıp dosyayı güvenli bir konuma kopyalayın. Kişisel bilgiler içerdiği için GitHub'a yüklemeyin. `.gitignore` yerel veri dosyalarını dışlar.

Kitaptaki MySQL / Entity Framework Database First yerine Microsoft.Data.Sqlite kullanıldı. Parolalar MD5 yerine tuzlu PBKDF2-SHA256 ile saklanır. Parasal alanlar kuruş cinsinden tamsayıdır. Satış ve iptal atomik transaction ile çalışır; iptal edilen satış kaydı silinir (kitaptaki davranış). İlişkili kayıtların silinmesi engellenir. Arayüz C# ile programatik olarak kurulur; görsel tasarımcıya özgü `.Designer.cs` dosyaları yoktur. Kitaptaki özgün simgeler paketlenmemiştir. Windows tarafından çizilen kontroller (örn. takvim, pencere çerçevesi) kendi yerel davranışlarını korur.

## Derleme / dağıtım

```powershell
dotnet restore
dotnet build -c Release
dotnet publish StokTakipUygulamasi/StokTakipUygulamasi.csproj -c Release -r win-x64 --self-contained true -o artifacts/win-x64
```

Yayımlanan klasördeki tüm dosyaları birlikte dağıtın. Uygulamanın yerel verileri bu klasörde tutulmaz. GitHub Actions iş akışı Windows üzerinde derleme ve taşınabilir klasör çıktısını hazırlar. Derleme durumunu [Actions](https://github.com/heavenylz/StokTakipUygulamasi/actions) sayfasından kontrol edin.

## Test durumu

`python tests/test_schema.py` komutu, gerçek `schema.sql` dosyasını kullanarak SQLite kısıtlarını ve satış/iptal işlemlerinin SQL düzeyindeki eşdeğer davranışlarını test eder. Bu testler C# kodunu veya WinForms ekranlarını çalıştırmaz. v2.0 için 8 SQL testi tekrar çalıştırıldı. Bu paketin oluşturulduğu ortamda .NET SDK ve Windows GUI bulunmadığından v2.0 C# derlemesi, ekran yerleşimi ve gerçek uygulama uçtan uca testleri henüz doğrulanmadı. `tests/MANUAL-TESTS.md` listesini ilk çalıştırmada kullanın.

Kaynak: MEB, *Yazılım Projesi 11–12 Ders Materyali*, 2. öğrenme birimi, s. 59–105 (ISBN 978-975-11-7115-3).

## v2.0 arayüz notları

- `Theme.cs`: ortak renk paleti, zaman tabanlı fade, hover, focus, glow çizimi. Animasyonlar Windows menü animasyonu kapalıysa veya yüksek karşıtlık etkinse atlanır.
- `UiLayout.cs`: ortak tablo yerleşimleri, 20 px dış boşluk, 16 px kart iç boşluğu ve 12 px kart aralığı.
- `frmGiris.cs`: dekoratif yükleme animasyonu, ilerleme çizgisi, ardından giriş formu ve başlık geçişleri. Gerçek yükleme süresi göstergesi değildir.
- Eski veriler aynı LocalAppData yolundan otomatik okunur; veritabanı göçü veya parola sıfırlaması gerekmez. Yükseltmeden önce uygulamayı kapatıp veritabanını yedekleyin.
- `tests/UI-V2-TESTS.md`: Windows üzerinde görsel/etkileşim kontrolü.

## v3.0 — MagicBento uyarlaması

- React/GSAP/Tailwind şablonunun görsel konsepti GDI+ ve C# ile özgün biçimde yeniden uygulandı; web bileşeni veya yeni NuGet paketi eklenmedi.
- `BentoDashboard.cs`: giriş sonrası dört sütunlu iki satırlı kontrol paneli. Satış işlemleri mevcut ayrı formda; diğer kartlar mevcut yönetim formlarını açar. Personel kartı yöneticiye özeldir.
- `BentoCard.cs`: 24 px köşeler, 300 px fare takipli spotlight, dinamik kenar glow, 12 hareketli parçacık, tıklama ripple, hafif manyetik kayma ve iki boyutlu affine eğim (gerçek CSS 3D perspektifi değil).
- `Theme.cs`: #060010 / #0a041a ve #8400ff teması tüm formlara uygulanır. V2 açılış animasyonu korunur.
- Native pencere çerçevesi, takvim ve diğer WinForms kontrolleri tarayıcıdaki CSS görünümüyle birebir aynı değildir.
- v3.0 SQL testleri tekrar çalıştırıldı; C# derlemesi ve Windows GUI doğrulaması bu ortamda yapılamadı. `tests/UI-V3-TESTS.md` kontrol listesini kullanın.
- Veritabanı yolu ve iş kuralları değişmedi.

## v4.0 — Kullanım kolaylığı

- Geliştirici imzası: © 2026 heavenylz. Kontrol panelinde GitHub bağlantılı yuvarlak profil alanı. Profil resmi GitHub'dan (yalnızca `heavenylz.png?size=128` isteği) arka planda alınır ve LocalAppData altında 7 gün önbelleğe alınır. İnternet yoksa H simgesi veya önbellekteki resim gösterilir. Profil resmi bu pakete gömülmemiştir; gerçek görüntü burada doğrulanamadı.
- Teknik veritabanı ibareleri giriş ve kontrol panelinden kaldırıldı. Ana panelde ürün, müşteri, azalan stok ve günlük satış sayısı görünür; modül kapandıktan sonra yenilenir.
- F1 kısayol listesi; F2 satış, F3 ürünler, F4 tedarikçiler, F5 yenile, F6 müşteriler, F7 satış detayları, F8 personel (yalnızca yönetici), F9 azalan stok. Kayıt ve satış formlarında Ctrl+F arama.
- Satışta ürün arama, çift tıklamayla sepete ekleme, tekrar eklemede miktar artırma, müşteri seçim penceresi, satış öncesi onay ve taslak silme uyarısı.
- Girişte şifre göster/gizle, panelden oturum kapatma ve yeniden giriş.
- Veritabanı dosyası ve şema değişmedi. Yeni NuGet bağımlılığı yok. SQL testleri Windows GUI testi değildir; Windows üzerinde derleme ve kullanıcı akışı kontrolü gerekir.
