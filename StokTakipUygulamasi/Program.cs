namespace StokTakipUygulamasi;
static class Program
{
    [STAThread] static void Main(){ApplicationConfiguration.Initialize();try{Db.Init();while(true){using var login=new frmGiris();if(login.ShowDialog()!=DialogResult.OK || login.User is not Session user)return;using var dashboard=new BentoDashboard(user);dashboard.ShowDialog();if(!dashboard.SignOut)return;}}catch(Exception ex){MessageBox.Show(ex.Message,"Başlatma hatası",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
}
