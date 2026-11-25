"""SQLite schema and transaction tests; does not execute C# or Windows Forms."""
import pathlib, sqlite3, unittest
SCHEMA = pathlib.Path(__file__).resolve().parents[1] / 'StokTakipUygulamasi' / 'schema.sql'
class TestSchema(unittest.TestCase):
    def setUp(self):
        self.db=sqlite3.connect(':memory:'); self.db.execute('PRAGMA foreign_keys=ON'); self.db.executescript(SCHEMA.read_text(encoding='utf-8')); self.db.executescript("""
        INSERT INTO kategoriler VALUES(1,'Donanım');
        INSERT INTO tedarikciler VALUES(1,'Firma','Yetkili','5551111111',NULL,'Adres');
        INSERT INTO musteriler VALUES(1,'Müşteri',NULL,'5552222222',NULL,NULL);
        INSERT INTO personeller VALUES(1,'Test','Yönetici','5553333333',NULL,NULL,'test','dummy-test-only',1);
        INSERT INTO urunler VALUES(1,'Mouse',1,1,25,15999,NULL);
        """); self.db.commit()
    def tearDown(self): self.db.close()
    def sale(self, quantity):
        with self.db:
            sid=self.db.execute("INSERT INTO satislar(musteri_id,personel_id,islemTarihi) VALUES(1,1,'2026-11-25 12:00:00')").lastrowid
            if self.db.execute('UPDATE urunler SET stok_adedi=stok_adedi-? WHERE urun_kodu=1 AND stok_adedi>=?',(quantity,quantity)).rowcount!=1: raise ValueError('stock')
            self.db.execute('INSERT INTO satis_detaylari(satis_id,urun_kodu,miktar,fiyat) VALUES(?,1,?,15999)',(sid,quantity))
        return sid
    def stock(self): return self.db.execute('SELECT stok_adedi FROM urunler').fetchone()[0]
    def test_sale_cancel(self):
        sid=self.sale(7); self.assertEqual(self.stock(),18)
        with self.db:
            for code,qty in self.db.execute('SELECT urun_kodu,miktar FROM satis_detaylari WHERE satis_id=?',(sid,)).fetchall(): self.db.execute('UPDATE urunler SET stok_adedi=stok_adedi+? WHERE urun_kodu=?',(qty,code))
            self.db.execute('DELETE FROM satislar WHERE satis_id=?',(sid,))
        self.assertEqual(self.stock(),25); self.assertEqual(self.db.execute('SELECT COUNT(*) FROM satis_detaylari').fetchone()[0],0)
    def test_insufficient(self):
        with self.assertRaises(ValueError): self.sale(26)
        self.assertEqual(self.stock(),25); self.assertEqual(self.db.execute('SELECT COUNT(*) FROM satislar').fetchone()[0],0)
    def test_unique(self):
        with self.assertRaises(sqlite3.IntegrityError): self.db.execute("INSERT INTO kategoriler(kategori_adi) VALUES('Donanım')")
    def test_fk(self):
        with self.assertRaises(sqlite3.IntegrityError): self.db.execute('DELETE FROM kategoriler WHERE kategori_id=1')
    def test_nonnegative(self):
        with self.assertRaises(sqlite3.IntegrityError): self.db.execute('UPDATE urunler SET stok_adedi=-1')
    def test_roles(self): self.assertEqual(self.db.execute('SELECT COUNT(*) FROM yetkiler').fetchone()[0],2)
    def test_low_stock(self):
        self.sale(6); self.assertEqual(self.db.execute('SELECT COUNT(*) FROM urunler WHERE stok_adedi<20').fetchone()[0],1)
        self.db.execute('UPDATE urunler SET stok_adedi=20'); self.assertEqual(self.db.execute('SELECT COUNT(*) FROM urunler WHERE stok_adedi<20').fetchone()[0],0)
    def test_price_exact(self):
        self.sale(3); self.assertEqual(self.db.execute('SELECT miktar*fiyat FROM satis_detaylari').fetchone()[0],47997)
if __name__=='__main__': unittest.main(verbosity=2)
