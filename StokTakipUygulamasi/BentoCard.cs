using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace StokTakipUygulamasi;

// Native, keyboard-accessible interpretation of the supplied MagicBento design.
internal sealed class BentoCard : Button
{
    readonly System.Windows.Forms.Timer clock = new() { Interval = 16 };
    readonly Stopwatch watch = Stopwatch.StartNew();
    readonly (float X, float Y, float Phase)[] stars;
    readonly string tag, description, symbol;
    readonly Font tagFont = new("Segoe UI", 9, FontStyle.Bold);
    readonly Font titleFont = new("Segoe UI", 15, FontStyle.Bold);
    readonly Font descFont = new("Segoe UI", 10);
    readonly Font symbolFont = new("Segoe UI Symbol", 20);
    PointF cursor, ripple;
    double hover, reveal = 1, rippleAt = -100;
    float shiftX, shiftY;
    bool inside, down;
    double last;
    internal BentoCard(string tag, string title, string description, string symbol)
    {
        this.tag = tag; this.description = description; this.symbol = symbol;
        Text = title; AccessibleName = title; AccessibleDescription = description;
        BackColor = Theme.Background; ForeColor = Theme.Text; FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0; Cursor = Cursors.Hand; TabStop = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        var rng = new Random(); stars = Enumerable.Range(0, 12).Select(_ => ((float)rng.NextDouble(), (float)rng.NextDouble(), (float)(rng.NextDouble()*6.283))).ToArray();
        clock.Tick += (_, _) => Tick();
    }
    internal void RevealFrom(double value) { reveal = value; Invalidate(); }
    void Wake() { if (Theme.Motion) { if (!clock.Enabled) { last = watch.Elapsed.TotalSeconds; clock.Start(); } } else { hover = inside || Focused ? 1 : 0; shiftX = shiftY = 0; Invalidate(); } }
    void Tick()
    {
        double now = watch.Elapsed.TotalSeconds, dt = Math.Clamp(now-last, .001, .05); last=now;
        double target=inside||Focused?1:0, k=1-Math.Exp(-dt*16);
        hover+=(target-hover)*k; if(Math.Abs(hover-target)<.005)hover=target;
        float tx=inside?Math.Clamp((cursor.X-Width/2f)*.035f,-6,6):0, ty=inside?Math.Clamp((cursor.Y-Height/2f)*.035f,-5,5):0;
        shiftX+=(tx-shiftX)*(float)k; shiftY+=(ty-shiftY)*(float)k;
        Invalidate();
        if(!inside && hover==target && Math.Abs(shiftX)+Math.Abs(shiftY)<.1 && now-rippleAt>.8) { shiftX=shiftY=0; clock.Stop(); }
    }
    protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);inside=true;cursor=PointToClient(MousePosition);Wake();}
    protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);cursor=e.Location;Wake();}
    protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);inside=down=false;Wake();}
    protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Wake();}
    protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);down=false;Wake();}
    protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){down=true;ripple=e.Location;if(Theme.Motion)rippleAt=watch.Elapsed.TotalSeconds;Wake();}}
    protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);down=false;Invalidate();}
    static Color Alpha(Color c,double a)=>Color.FromArgb((int)Math.Clamp(a,0,255),c);
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics; g.Clear(Parent?.BackColor??Theme.Background); if(Width<32||Height<32)return;
        g.SmoothingMode=SmoothingMode.AntiAlias; g.TextRenderingHint=TextRenderingHint.AntiAliasGridFit;
        var r=new RectangleF(9,9,Width-18,Height-18); var state=g.Save();
        // Slight affine tilt and magnetic translation; all drawn inside the reserved gutter.
        if(Theme.Motion && (inside || Math.Abs(shiftX)+Math.Abs(shiftY)>.1)){using var m=new Matrix(1,(cursor.Y-Height/2f)/Math.Max(Height,1)*.025f*(float)hover,(cursor.X-Width/2f)/Math.Max(Width,1)*.025f*(float)hover,1,shiftX,shiftY);g.Transform=m;}
        using var path=Theme.Rounded(r,24);
        var surface=Theme.Mix(Theme.Background,Theme.Surface,reveal);
        if(down)surface=Theme.Mix(surface,Theme.Background,.2);
        using(var b=new SolidBrush(surface))g.FillPath(b,path);
        var clip=g.Save();g.SetClip(path);
        var pointer=inside?cursor:new PointF(r.X+r.Width/2,r.Y+r.Height/2);
        if(hover>0)
        {
            using var haloPath=new GraphicsPath(); haloPath.AddEllipse(pointer.X-300,pointer.Y-300,600,600);
            using var halo=new PathGradientBrush(haloPath){CenterPoint=pointer,CenterColor=Alpha(Theme.Glow,65*hover*reveal),SurroundColors=new[]{Alpha(Theme.Glow,0)}};g.FillPath(halo,haloPath);
            if(inside&&Theme.Motion)foreach(var s in stars){double t=watch.Elapsed.TotalSeconds;float x=r.X+10+s.X*(r.Width-20)+(float)Math.Sin(t*.9+s.Phase)*13,y=r.Y+10+s.Y*(r.Height-20)+(float)Math.Cos(t*.7+s.Phase)*10;double pulse=.5+.5*Math.Sin(t*2+s.Phase);using var aura=new SolidBrush(Alpha(Theme.Accent,28*hover*reveal*pulse));g.FillEllipse(aura,x-4,y-4,8,8);using var dot=new SolidBrush(Alpha(Theme.Accent,165*hover*reveal*pulse));g.FillEllipse(dot,x-1.5f,y-1.5f,3,3);}
        }
        double age=watch.Elapsed.TotalSeconds-rippleAt;if(Theme.Motion&&age>=0&&age<.8){float radius=(float)(Theme.Ease(age/.8)*Math.Sqrt(Width*(double)Width+Height*(double)Height));using var rb=new SolidBrush(Alpha(Theme.Glow,65*(1-age/.8)*reveal));g.FillEllipse(rb,ripple.X-radius,ripple.Y-radius,2*radius,2*radius);}
        g.Restore(clip);
        using(var border=new Pen(Theme.Mix(Theme.Background,Theme.Mix(Theme.Border,Theme.Glow,hover*.65),reveal),1.2f))g.DrawPath(border,path);
        if(hover>.01){using var borderPath=new GraphicsPath();borderPath.AddEllipse(pointer.X-180,pointer.Y-180,360,360);using var grad=new PathGradientBrush(borderPath){CenterPoint=pointer,CenterColor=Alpha(Theme.Accent,230*hover*reveal),SurroundColors=new[]{Alpha(Theme.Glow,0)}};using var pen=new Pen(grad,2);g.DrawPath(pen,path);}
        float pad=18, x0=r.X+pad, w=Math.Max(1,r.Width-2*pad);
        using var fmt=new StringFormat{Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap};
        using var wrap=new StringFormat{Trimming=StringTrimming.EllipsisWord,FormatFlags=StringFormatFlags.LineLimit};
        using var accent=new SolidBrush(Theme.Mix(surface,Theme.Accent,reveal));using var fg=new SolidBrush(Theme.Mix(surface,Theme.Text,reveal));using var muted=new SolidBrush(Theme.Mix(surface,Theme.Muted,reveal));
        g.DrawString(tag.ToUpperInvariant(),tagFont,accent,new RectangleF(x0,r.Y+17,Math.Max(1,w-38),20),fmt);
        g.DrawString(symbol,symbolFont,accent,new RectangleF(r.Right-54,r.Y+11,38,37),fmt);
        var title=new RectangleF(x0,r.Y+44,w,43);
        if(hover>0){using var glow=new SolidBrush(Alpha(Theme.Glow,65*hover*reveal));for(int i=-2;i<=2;i++){var shadow=title;shadow.Offset(i,1);g.DrawString(Text,titleFont,glow,shadow,fmt);}}
        g.DrawString(Text,titleFont,fg,title,fmt);
        float descY=Math.Max(r.Y+82,r.Bottom-65);g.DrawString(description,descFont,muted,new RectangleF(x0,descY,w,Math.Max(1,r.Bottom-descY-13)),wrap);
        if(Focused&&ShowFocusCues){using var focus=new Pen(Theme.Accent,1){DashStyle=DashStyle.Dot};using var fp=Theme.Rounded(RectangleF.Inflate(r,-5,-5),19);g.DrawPath(focus,fp);}
        g.Restore(state);
    }
    protected override void Dispose(bool disposing){if(disposing){clock.Dispose();tagFont.Dispose();titleFont.Dispose();descFont.Dispose();symbolFont.Dispose();}base.Dispose(disposing);}
}
