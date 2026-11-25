using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net.Http;
namespace StokTakipUygulamasi;

internal sealed class DeveloperBadge : Button
{
    static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };
    static readonly string Cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StokTakipUygulamasi", "heavenylz-avatar.png");
    Bitmap? avatar;
    bool hovered;
    internal DeveloperBadge()
    {
        Text="© 2026 heavenylz"; AccessibleName="heavenylz GitHub profili";
        FlatStyle=FlatStyle.Flat; FlatAppearance.BorderSize=0; Cursor=Cursors.Hand;
        SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        Click+=(_,_)=>Ui.Safe(()=>Process.Start(new ProcessStartInfo("https://github.com/heavenylz"){UseShellExecute=true}));
        MouseEnter+=(_,_)=>{hovered=true;Invalidate();}; MouseLeave+=(_,_)=>{hovered=false;Invalidate();};
        HandleCreated+=async (_,_)=>await LoadAvatar();
    }
    async Task LoadAvatar()
    {
        try
        {
            if(File.Exists(Cache)) SetAvatar(await File.ReadAllBytesAsync(Cache));
            if(IsDisposed || (avatar!=null && DateTime.UtcNow-File.GetLastWriteTimeUtc(Cache)<TimeSpan.FromDays(7))) return;
            using var response=await Client.GetAsync("https://github.com/heavenylz.png?size=128",HttpCompletionOption.ResponseHeadersRead);
            if(!response.IsSuccessStatusCode || response.Content.Headers.ContentLength>1_000_000) return;
            using var stream=await response.Content.ReadAsStreamAsync(); using var memory=new MemoryStream();
            var buffer=new byte[8192]; int n;
            while((n=await stream.ReadAsync(buffer.AsMemory()))>0){if(memory.Length+n>1_000_000)return;memory.Write(buffer,0,n);}
            var bytes=memory.ToArray(); if(IsDisposed)return; SetAvatar(bytes);
            Directory.CreateDirectory(Path.GetDirectoryName(Cache)!); await File.WriteAllBytesAsync(Cache,bytes);
        }
        catch { /* The profile is optional; offline use remains available. */ }
    }
    void SetAvatar(byte[] bytes)
    {
        if(IsDisposed)return; using var stream=new MemoryStream(bytes);using var source=Image.FromStream(stream);
        var next=new Bitmap(source,64,64);avatar?.Dispose();avatar=next;Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(Parent?.BackColor??Theme.Background);g.SmoothingMode=SmoothingMode.AntiAlias;
        int size=Math.Min(36,Math.Max(1,Height-8)), y=(Height-size)/2;
        using var path=new GraphicsPath();path.AddEllipse(3,y,size,size);
        using var fill=new SolidBrush(Theme.Input);g.FillPath(fill,path);
        if(avatar!=null){var state=g.Save();g.SetClip(path);g.DrawImage(avatar,new Rectangle(3,y,size,size));g.Restore(state);}
        else TextRenderer.DrawText(g,"H",Font,new Rectangle(3,y,size,size),Theme.Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
        using var pen=new Pen(hovered?Theme.Accent:Theme.Border,1.5f);g.DrawPath(pen,path);
        int x=size+12;using var bold=new Font(Font.FontFamily,9,FontStyle.Bold);using var small=new Font(Font.FontFamily,8);
        TextRenderer.DrawText(g,"© 2026 heavenylz",bold,new Rectangle(x,y,Math.Max(1,Width-x),19),hovered?Theme.Accent:Theme.Text,TextFormatFlags.EndEllipsis|TextFormatFlags.SingleLine);
        TextRenderer.DrawText(g,"Geliştirici · GitHub ↗",small,new Rectangle(x,y+19,Math.Max(1,Width-x),17),Theme.Muted,TextFormatFlags.EndEllipsis|TextFormatFlags.SingleLine);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,ClientRectangle);
    }
    protected override void Dispose(bool disposing){if(disposing)avatar?.Dispose();base.Dispose(disposing);}
}
