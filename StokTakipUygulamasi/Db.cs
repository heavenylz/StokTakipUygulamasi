using System.Data;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Globalization;

namespace StokTakipUygulamasi;
internal static class Db
{
    internal static string PathName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StokTakipUygulamasi", "stok_takip_db.sqlite3");
    internal static SqliteConnection Open()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
        var c = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=PathName, ForeignKeys=true }.ToString());
        c.Open();
        c.CreateFunction<string?, string?, bool>("tr_contains", (value, term) => CultureInfo.GetCultureInfo("tr-TR").CompareInfo.IndexOf(value ?? "", term ?? "", CompareOptions.IgnoreCase) >= 0);
        return c;
    }
    internal static void Init()
    {
        using var c=Open(); using var s=typeof(Db).Assembly.GetManifestResourceStream("StokTakipUygulamasi.schema.sql")!;
        using var reader=new StreamReader(s); Run(c,null,reader.ReadToEnd());
    }
    internal static SqliteCommand Cmd(SqliteConnection c, SqliteTransaction? t, string sql, params object?[] args)
    {
        var cmd=c.CreateCommand(); cmd.Transaction=t; cmd.CommandText=sql;
        for(int i=0;i<args.Length;i++) cmd.Parameters.AddWithValue("@p"+i,args[i]??DBNull.Value);
        return cmd;
    }
    internal static int Run(SqliteConnection c,SqliteTransaction? t,string sql,params object?[] args) {using var cmd=Cmd(c,t,sql,args);return cmd.ExecuteNonQuery();}
    internal static object? Scalar(SqliteConnection c,SqliteTransaction? t,string sql,params object?[] args){using var cmd=Cmd(c,t,sql,args);return cmd.ExecuteScalar();}
    internal static int Exec(string sql,params object?[] args){using var c=Open();return Run(c,null,sql,args);}
    internal static DataTable Query(string sql,params object?[] args){using var c=Open();return Table(c,null,sql,args);}
    internal static DataTable Table(SqliteConnection c,SqliteTransaction? t,string sql,params object?[] args){using var cmd=Cmd(c,t,sql,args);using var r=cmd.ExecuteReader();var d=new DataTable();d.Load(r);return d;}
    internal static long Number(object? v)=>Convert.ToInt64(v,CultureInfo.InvariantCulture);
    internal static string Str(object? v)=>Convert.ToString(v,CultureInfo.CurrentCulture)??"";
    internal static long Cents(decimal v)=>checked((long)(v*100m));
    internal static string Money(object? cents)=>(Number(cents)/100m).ToString("N2",CultureInfo.CurrentCulture);
    internal static string Password(string s){var salt=RandomNumberGenerator.GetBytes(16);var hash=Rfc2898DeriveBytes.Pbkdf2(s,salt,600000,HashAlgorithmName.SHA256,32);return Convert.ToBase64String(salt)+":"+Convert.ToBase64String(hash);}
    internal static bool Verify(string s,string stored){try{var p=stored.Split(':');var hash=Rfc2898DeriveBytes.Pbkdf2(s,Convert.FromBase64String(p[0]),600000,HashAlgorithmName.SHA256,32);return CryptographicOperations.FixedTimeEquals(hash,Convert.FromBase64String(p[1]));}catch{return false;}}
    internal static string Products = "SELECT u.urun_kodu AS Kod,u.urun_adi AS [Ürün Adı],u.stok_adedi AS Stok,u.fiyat/100.0 AS Fiyat,k.kategori_adi AS Kategori,t.firma_adi AS Tedarikçi,u.aciklama AS Açıklama FROM urunler u JOIN kategoriler k ON k.kategori_id=u.kategori_id JOIN tedarikciler t ON t.id=u.tedarikci_id";
    internal static void Sale(long customer,long person,string note,IEnumerable<(long Code,int Quantity,long Price)> items)
    {
        var list=items.ToList(); if(list.Count==0||customer<=0)throw new InvalidOperationException("Müşteri ve ürün seçiniz.");
        using var c=Open();using var t=c.BeginTransaction();
        Run(c,t,"INSERT INTO satislar(musteri_id,personel_id,islemTarihi,notlar) VALUES(@p0,@p1,@p2,@p3)",customer,person,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture),note);
        var id=Number(Scalar(c,t,"SELECT last_insert_rowid()"));
        foreach(var item in list){if(item.Quantity<=0||Run(c,t,"UPDATE urunler SET stok_adedi=stok_adedi-@p0 WHERE urun_kodu=@p1 AND stok_adedi>=@p0 AND fiyat=@p2",item.Quantity,item.Code,item.Price)!=1)throw new InvalidOperationException("Stok yetersiz veya ürün/fiyat değişti. Listeyi yenileyiniz.");Run(c,t,"INSERT INTO satis_detaylari(satis_id,urun_kodu,miktar,fiyat) VALUES(@p0,@p1,@p2,@p3)",id,item.Code,item.Quantity,item.Price);}
        t.Commit();
    }
    internal static void Cancel(long id)
    {
        using var c=Open();using var t=c.BeginTransaction();var rows=Table(c,t,"SELECT urun_kodu,miktar FROM satis_detaylari WHERE satis_id=@p0",id);
        foreach(DataRow r in rows.Rows)Run(c,t,"UPDATE urunler SET stok_adedi=stok_adedi+@p0 WHERE urun_kodu=@p1",r[1],r[0]);
        if(Run(c,t,"DELETE FROM satislar WHERE satis_id=@p0",id)!=1)throw new InvalidOperationException("Satış bulunamadı.");t.Commit();
    }
}
internal record Session(long Id,string Name,long Role);
