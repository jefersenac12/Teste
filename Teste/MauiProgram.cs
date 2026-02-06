using Microsoft.Extensions.Logging;
using Teste.Services;
using Teste.ViewModels;

namespace Teste
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("SourceSansPro-Regular.ttf", "SourceSansPro");
                    fonts.AddFont("fonnts.com-Canvas_Inline_Reg.otf", "Canvas_Inline_Reg");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registrar serviços
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<ShellNavigationService>(); // Serviço de navegação moderno

            // Registrar ViewModels
            builder.Services.AddSingleton<InicioViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<CadastroFamiliaViewModel>();
            builder.Services.AddTransient<CadastroAgenciaViewModel>();
            builder.Services.AddTransient<AgendamentoViewModel>();
            builder.Services.AddTransient<AtividadesViewModel>();
            builder.Services.AddTransient<PagamentoViewModel>();
            builder.Services.AddTransient<ReservasViewModel>();

            // Registrar Pages
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CadastroFamiliaPage>();
            builder.Services.AddTransient<CadastroAgenciaPage>();
            builder.Services.AddTransient<AgendamentoPage>();
            builder.Services.AddTransient<AtividadesPage>();
            builder.Services.AddTransient<PagamentoPage>();
            builder.Services.AddTransient<ReservasPage>();
            builder.Services.AddTransient<PerfilPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
