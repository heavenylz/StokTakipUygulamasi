namespace StokTakipUygulamasi;

internal sealed class BentoDashboard : Form
{
    readonly Session user;
    readonly List<BentoCard> cards = new();
    bool opening;
    readonly Label summary;
    internal bool SignOut { get; private set; }
    readonly Dictionary<Keys,Func<Form>> shortcuts = new();
    public BentoDashboard(Session user)
    {
        this.user=user;
        Ui.Setup(this,"Stok Takip • Kontrol Paneli",1160,740);
        MinimumSize=new Size(1030,650);
        KeyPreview=true;
        var root=UiLayout.Root(this,5); root.Padding=new Padding(27,22,27,18);
        UiLayout.Row(root,0,68);UiLayout.Row(root,1,54);UiLayout.Row(root,2,48);UiLayout.Row(root,3,100,false);UiLayout.Row(root,4,54);
        var header=UiLayout.Table(2,1);UiLayout.Col(header,0,65,false);UiLayout.Col(header,1,35,false);
        UiLayout.Add(header,UiLayout.Heading("STOK TAKİP",25),0,0);
        var identity=UiLayout.Caption(user.Name+"  /  "+(user.Role==1?"Yönetici":"Personel"));identity.TextAlign=ContentAlignment.MiddleRight;UiLayout.Add(header,identity,1,0);UiLayout.Add(root,header,0,0);
        var subtitle=UiLayout.Table(1,2);UiLayout.Row(subtitle,0,28);UiLayout.Row(subtitle,1,26);UiLayout.Add(subtitle,UiLayout.Heading("Kontrol paneli",15),0,0);UiLayout.Add(subtitle,UiLayout.Caption("İşlem yapmak istediğiniz modülü seçin."),0,1);UiLayout.Add(root,subtitle,0,1);
        summary=UiLayout.Caption("");
        UiLayout.Add(root,summary,0,2);
        var grid=UiLayout.Table(4,2);UiLayout.Add(root,grid,0,3);
        void Add(string tag,string title,string description,string symbol,int x,int y,int span,Func<Form> make,Keys shortcut)
        {
            var card=new BentoCard(tag,title,description,symbol);cards.Add(card);
            UiLayout.Add(grid,card,x,y,span,Padding.Empty);
            card.Click+=(_,_)=>Open(make);shortcuts[shortcut]=make;
        }
        Add("İşlemler","Yeni satış","Müşteri seçin, ürünleri sepete ekleyin ve satış işlemini tamamlayın.","↗",0,0,2,()=>new frmAnaSayfa(user),Keys.F2);
        Add("Envanter","Ürünler","Ürün ve kategori kayıtlarını yönetin.","▦",2,0,1,()=>new frmUrunIslemleri(),Keys.F3);
        Add("Tedarik","Tedarikçiler","Firma ve iletişim bilgilerini düzenleyin.","▤",3,0,1,()=>new frmTedarikciIslemleri(),Keys.F4);
        Add("Kayıtlar","Müşteriler","Müşteri bilgilerini yönetin.","◎",0,1,1,()=>new frmMusteriIslemleri(),Keys.F6);
        Add("Geçmiş","Satış detayları","Satışları inceleyin ve iptal edin.","≡",1,1,1,()=>new frmSatisDetaylari(),Keys.F7);
        if(user.Role==1){Add("Yönetim","Personeller","Personel hesapları ve yetkileri.","◇",2,1,1,()=>new frmPersonelIslemleri(user),Keys.F8);Add("Kontrol","Azalan stok","Düşük stoklu ürünleri görüntüleyin.","↓",3,1,1,()=>new frmStokuAzalanUrunler(),Keys.F9);}
        else Add("Kontrol","Azalan stok","Düşük stoklu ürünleri görüntüleyin.","↓",2,1,2,()=>new frmStokuAzalanUrunler(),Keys.F9);
        var footer=UiLayout.Table(4,1);UiLayout.Col(footer,0,100,false);UiLayout.Col(footer,1,130);UiLayout.Col(footer,2,130);UiLayout.Col(footer,3,110);
        UiLayout.Add(footer,new DeveloperBadge(),0,0);
        UiLayout.Add(footer,UiLayout.Button("Kısayollar",ShowShortcuts),1,0,margin:new Padding(6,7,6,7));
        UiLayout.Add(footer,UiLayout.Button("Oturumu kapat",()=>{if(Ui.Confirm("Oturum kapatılsın mı?")){SignOut=true;Close();}}),2,0,margin:new Padding(6,7,6,7));
        UiLayout.Add(footer,UiLayout.Button("Çıkış",Close),3,0,margin:new Padding(6,7,0,7));UiLayout.Add(root,footer,0,4);
        Shown+=(_,_)=>Ui.Safe(RefreshSummary);Activated+=(_,_)=>Ui.Safe(RefreshSummary);
        KeyDown+=(_,e)=>{if(shortcuts.TryGetValue(e.KeyCode,out var action)&&e.Modifiers==Keys.None){e.SuppressKeyPress=true;Open(action);}else if(e.KeyCode==Keys.F5){e.SuppressKeyPress=true;Ui.Safe(RefreshSummary);}else if(e.KeyCode==Keys.F1){e.SuppressKeyPress=true;ShowShortcuts();}};
        if(Theme.Motion){foreach(var card in cards)card.RevealFrom(0);Shown+=(_,_)=>Theme.Animate(this,850,t=>{for(int i=0;i<cards.Count;i++)cards[i].RevealFrom(Math.Clamp(t*1.65-i*.085,0,1));});}
    }
    void RefreshSummary()
    {
        var r=Db.Query("SELECT (SELECT COUNT(*) FROM urunler), (SELECT COUNT(*) FROM musteriler), (SELECT COUNT(*) FROM urunler WHERE stok_adedi<20), (SELECT COUNT(*) FROM satislar WHERE substr(islemTarihi,1,10)=@p0)",DateTime.Now.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)).Rows[0];
        summary.Text=$"{r[0]} ürün     ·     {r[1]} müşteri     ·     {r[2]} azalan stok     ·     Bugün {r[3]} satış";
    }
    void ShowShortcuts()=>Ui.Info("F1  Kısayollar\nF2  Yeni satış\nF3  Ürünler\nF4  Tedarikçiler\nF5  Yenile\nF6  Müşteriler\nF7  Satış detayları\n"+(user.Role==1?"F8  Personeller\n":"")+"F9  Azalan stok");
    void Open(Func<Form> make)
    {
        if(opening)return;opening=true;
        void Run(){if(IsDisposed)return;try{Ui.Safe(()=>{using var form=make();form.ShowDialog(this);});}finally{opening=false; if(!IsDisposed)Ui.Safe(RefreshSummary);}}
        if(!Theme.Motion){Run();return;}
        // Leave a short visible click ripple before opening the modal.
        var timer=new System.Windows.Forms.Timer{Interval=220};
        EventHandler? disposed=null;void Stop(){timer.Stop();timer.Dispose();Disposed-=disposed;}disposed=(_,_)=>Stop();Disposed+=disposed;
        timer.Tick+=(_,_)=>{Stop();Run();};timer.Start();
    }
}
