using Microsoft.Maui.Controls;

namespace Teste
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Registrar rotas programaticamente
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("CadastroFamilia", typeof(CadastroFamiliaPage));
            Routing.RegisterRoute("CadastroAgencia", typeof(CadastroAgenciaPage));
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Agendamento", typeof(AgendamentoPage));
            Routing.RegisterRoute("Atividades", typeof(AtividadesPage));
            Routing.RegisterRoute("Pagamento", typeof(PagamentoPage));
            Routing.RegisterRoute("Reservas", typeof(ReservasPage));
            Routing.RegisterRoute("Perfil", typeof(PerfilPage));
        }
    }
}
