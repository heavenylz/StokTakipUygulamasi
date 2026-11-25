namespace StokTakipUygulamasi;
internal static class UiLayout
{
    internal static TableLayoutPanel Table(int cols,int rows,Color? color=null){var t=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=cols,RowCount=rows,Margin=Padding.Empty,Padding=Padding.Empty,BackColor=color??Theme.Background,GrowStyle=TableLayoutPanelGrowStyle.FixedSize};for(int i=0;i<cols;i++)t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/cols));for(int i=0;i<rows;i++)t.RowStyles.Add(new RowStyle(SizeType.Percent,100f/rows));return t;}
    internal static void Row(TableLayoutPanel t,int i,float size,bool absolute=true)=>t.RowStyles[i]=new RowStyle(absolute?SizeType.Absolute:SizeType.Percent,size);
    internal static void Col(TableLayoutPanel t,int i,float size,bool absolute=true)=>t.ColumnStyles[i]=new ColumnStyle(absolute?SizeType.Absolute:SizeType.Percent,size);
    internal static T Add<T>(TableLayoutPanel t,T c,int x,int y,int span=1,Padding? margin=null) where T:Control{c.Dock=DockStyle.Fill;c.Margin=margin??Padding.Empty;t.Controls.Add(c,x,y);if(span>1)t.SetColumnSpan(c,span);return c;}
    internal static TableLayoutPanel Root(Form f,int rows){var t=Table(1,rows);t.Padding=new Padding(20);f.Controls.Add(t);return t;}
    internal static TableLayoutPanel Card(int cols,int rows){var t=Table(cols,rows,Theme.Surface);t.Padding=new Padding(16);return t;}
    internal static Label Caption(string text)=>new(){Text=text,TextAlign=ContentAlignment.MiddleLeft,ForeColor=Theme.Muted,AutoEllipsis=true};
    internal static GlowLabel Heading(string text,float size=16)=>new(text,size);
    internal static MotionButton Button(string text,Action click,bool primary=false,bool danger=false){var b=new MotionButton(text,primary,danger);b.Click+=(_,_)=>Ui.Safe(click);return b;}
    internal static TextBox Text(bool multi=false,bool readOnly=false)=>new(){Multiline=multi,ReadOnly=readOnly,MaxLength=250};
    internal static MaskedTextBox Phone()=>new("(000) 000-0000"){TextMaskFormat=MaskFormat.ExcludePromptAndLiterals};
    internal static DataGridView Grid(bool edit=false)=>new(){ReadOnly=!edit,AllowUserToAddRows=false,AllowUserToDeleteRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.None};
    internal static TableLayoutPanel Field(string text,Control input){var t=Table(1,2,Theme.Surface);Row(t,0,24);Row(t,1,100,false);Add(t,Caption(text),0,0);Add(t,input,0,1);return t;}
}
