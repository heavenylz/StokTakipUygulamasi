using System.Data;
namespace StokTakipUygulamasi;
internal static class Ui
{
    internal static void Setup(Form f,string title,int w,int h){f.Text=title;f.ClientSize=new Size(w,h);f.StartPosition=FormStartPosition.CenterScreen;f.Font=new Font("Segoe UI",10);f.BackColor=Theme.Background;f.ForeColor=Theme.Text;f.MinimumSize=new Size(Math.Min(w,1000)+16,Math.Min(h,620)+39);f.Load+=(_,_)=>Theme.Apply(f);if(f is not frmGiris){if(Theme.Motion)f.Opacity=.05;f.Shown+=(_,_)=>Theme.Fade(f);}}
    internal static T Put<T>(Control parent,T c,int x,int y,int w,int h) where T:Control {c.SetBounds(x,y,w,h);parent.Controls.Add(c);return c;}
    internal static Label Label(Control p,string text,int x,int y,int w=105,int h=24)=>Put(p,new Label{Text=text,TextAlign=ContentAlignment.MiddleRight},x,y,w,h);
    internal static TextBox Text(Control p,int x,int y,int w,int h=24,bool multi=false)=>Put(p,new TextBox{Multiline=multi,MaxLength=250},x,y,w,h);
    internal static Button Button(Control p,string text,int x,int y,int w,Action a,int h=31){var b=Put(p,new Button{Text=text},x,y,w,h);b.Click+=(_,_)=>Safe(a);return b;}
    internal static void Safe(Action a){try{a();}catch(Exception ex){var m=ex.Message;if(ex is Microsoft.Data.Sqlite.SqliteException se){m=se.SqliteExtendedErrorCode switch{2067=>"Bu bilgi zaten kayıtlı.",787=>"Bu kayıt başka kayıtlar tarafından kullanılıyor; silinemez.",_=>ex.Message};}MessageBox.Show(m,"Hata",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    internal static void Info(string s)=>MessageBox.Show(s,"Bilgi",MessageBoxButtons.OK,MessageBoxIcon.Information);
    internal static bool Confirm(string s)=>MessageBox.Show(s,"Onay",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)==DialogResult.Yes;
    internal static DataGridView Grid(Control p,int x,int y,int w,int h,bool edit=false)=>Put(p,new DataGridView{ReadOnly=!edit,AllowUserToAddRows=false,AllowUserToDeleteRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,RowHeadersVisible=true,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.None,BackgroundColor=SystemColors.AppWorkspace},x,y,w,h);
    internal static void Bind(DataGridView g,DataTable t){g.DataSource=t;g.ClearSelection();g.CurrentCell=null;foreach(DataGridViewColumn c in g.Columns){if(c.Name is "Fiyat" or "Birim Fiyat" or "Toplam")c.DefaultCellStyle.Format="N2";}}
    internal static long Selected(DataGridView g){if(g.SelectedRows.Count==0)throw new InvalidOperationException("Lütfen bir kayıt seçiniz.");return Db.Number(g.SelectedRows[0].Cells[0].Value);}
    internal static string Phone(string s){var digits=new string(s.Where(char.IsAsciiDigit).ToArray());if(digits.Length!=10)throw new InvalidOperationException("Telefon numarası 10 haneli olmalıdır (başında 0 olmadan).");return digits;}
    internal static MaskedTextBox PhoneBox(Control p,int x,int y,int w)=>Put(p,new MaskedTextBox("(000) 000-0000"){TextMaskFormat=MaskFormat.ExcludePromptAndLiterals},x,y,w,24);
}
