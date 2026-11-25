namespace StokTakipUygulamasi;
internal sealed class CustomerPicker:Form
{
    readonly TextBox search;
    readonly DataGridView grid;
    internal string PhoneNumber{get;private set;}="";
    internal CustomerPicker()
    {
        Ui.Setup(this,"Müşteri seç",740,540);KeyPreview=true;
        var root=UiLayout.Root(this,4);UiLayout.Row(root,0,48);UiLayout.Row(root,1,64);UiLayout.Row(root,2,100,false);UiLayout.Row(root,3,52);
        UiLayout.Add(root,UiLayout.Heading("Müşteri seç",20),0,0);
        search=UiLayout.Text();search.PlaceholderText="Ad, telefon veya firma…";UiLayout.Add(root,UiLayout.Field("Müşteri ara",search),0,1,margin:new Padding(0,0,0,8));
        grid=UiLayout.Grid();UiLayout.Add(root,grid,0,2);var actions=UiLayout.Table(2,1);UiLayout.Add(root,actions,0,3,margin:new Padding(0,8,0,0));
        UiLayout.Add(actions,UiLayout.Button("Vazgeç",Close),0,0,margin:new Padding(0,0,6,0));var select=UiLayout.Button("Müşteriyi seç",Choose,true);UiLayout.Add(actions,select,1,0,margin:new Padding(6,0,0,0));AcceptButton=select;
        search.TextChanged+=(_,_)=>Ui.Safe(Reload);grid.CellDoubleClick+=(_,e)=>{if(e.RowIndex>=0)Ui.Safe(Choose);};KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Escape){e.SuppressKeyPress=true;Close();}};Shown+=(_,_)=>search.Focus();Reload();
    }
    void Reload()=>Ui.Bind(grid,Db.Query("SELECT id AS Kod,ad_soyad AS Müşteri,telefon AS Telefon,firma_adi AS Firma FROM musteriler WHERE @p0='' OR tr_contains(ad_soyad,@p0) OR tr_contains(telefon,@p0) OR tr_contains(firma_adi,@p0) ORDER BY ad_soyad",search.Text.Trim()));
    void Choose(){if(grid.SelectedRows.Count==0)throw new InvalidOperationException("Bir müşteri seçiniz.");PhoneNumber=Db.Str(grid.SelectedRows[0].Cells["Telefon"].Value);DialogResult=DialogResult.OK;}
}
