using System;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Teste
{
    // =========================================================================
    // CLASSES DE MODELO (MODELOS PARA DESSERIALIZAÇÃO)
    // =========================================================================

    public class Reserva
    {
        public int Id { get; set; }
        public int AgendaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataReserva { get; set; }
        public int Quantidade { get; set; }
        public int NPEntrada { get; set; }
        public int MeiaEntrada { get; set; }
        public int InteiraEntrada { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
    }

    public class Agenda
    {
        public int Id { get; set; }
        public int AtividadeId { get; set; }
        public string DataHora { get; set; }
        // Adicione outras propriedades da Agenda que você precisa
    }
    
    // =========================================================================
    // CLASSE DA PÁGINA (CORRIGIDA)
    // * Não contém a declaração redundante de ListaReservasAtivas *
    // =========================================================================

    public partial class ReservasAtivasPage : ContentPage
    {
        // URLs das APIs
        private readonly string apiUrlReserva = "http://tiijeferson.runasp.net/api/Reserva";
        private readonly string apiUrlAgenda = "http://tiijeferson.runasp.net/api/Agenda";
        
        // O campo 'ListaReservasAtivas' será automaticamente criado pelo InitializeComponent
        // a partir da declaração 'x:Name' no seu XAML.

        public ReservasAtivasPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarReservasAtivasAsync();
        }

        private async Task CarregarReservasAtivasAsync()
        {
            // O componente ListaReservasAtivas é inicializado via XAML/InitializeComponent.
            if (ListaReservasAtivas == null) 
            {
                await DisplayAlert("Erro de Componente", "O componente ListaReservasAtivas não foi inicializado. Verifique seu XAML.", "OK");
                return;
            }
            ListaReservasAtivas.Children.Clear();

            try
            {
                // Tenta obter UsuarioId, se não existir, tenta ClienteId, com default 1
                int usuarioId = Preferences.Get("UsuarioId", Preferences.Get("ClienteId", 1));

                using var client = new HttpClient();
                // Opções para desserialização (ignora case)
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1. Tenta padrões de endpoint para buscar reservas do usuário
                HttpResponseMessage resp = await client.GetAsync($"{apiUrlReserva}/usuario/{usuarioId}");
                if (!resp.IsSuccessStatusCode)
                {
                    resp = await client.GetAsync($"{apiUrlReserva}?usuarioId={usuarioId}");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    await DisplayAlert("Aviso", "Não foi possível carregar reservas ativas do servidor.", "OK");
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                
                // Desserialização direta para a lista de objetos Reserva
                var reservas = JsonSerializer.Deserialize<List<Reserva>>(json, opts);
                
                if (reservas == null || reservas.Count == 0)
                {
                    ListaReservasAtivas.Children.Add(new Label { Text = "Nenhuma reserva ativa encontrada.", HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 20) });
                    return;
                }

                // 2. Itera sobre cada reserva carregada
                foreach (var reserva in reservas)
                {
                    // Determina o status e filtra para manter apenas ativas (PENDENTE e PAGO)
                    string statusTexto = reserva.Status?.ToUpperInvariant() ?? "PENDENTE";
                    
                    if (statusTexto != "PENDENTE" && statusTexto != "PAGO")
                        continue; // Pula reservas canceladas, concluídas, etc.

                    string atividadeNome = $"Reserva #{reserva.Id}";
                    string dataTexto = "";

                    // 3. Tenta buscar detalhes de Agenda/Atividade (se o AgendaId for válido)
                    if (reserva.AgendaId > 0)
                    {
                        try
                        {
                            var rAgenda = await client.GetAsync($"{apiUrlAgenda}/{reserva.AgendaId}");
                            if (rAgenda.IsSuccessStatusCode)
                            {
                                var jAg = await rAgenda.Content.ReadAsStringAsync();
                                var agenda = JsonSerializer.Deserialize<Agenda>(jAg, opts);
                                
                                if (agenda != null)
                                {
                                    // Formata DataHora
                                    if (!string.IsNullOrWhiteSpace(agenda.DataHora) && DateTime.TryParse(agenda.DataHora, out var dh))
                                    {
                                        dataTexto = dh.ToString("dd/MM/yyyy - HH:mm");
                                    }
                                    
                                    if (agenda.AtividadeId > 0)
                                    {
                                        // Substituir esta linha pela busca do nome da atividade, se necessário
                                        atividadeNome = $"Atividade #{agenda.AtividadeId}"; 
                                    }
                                }
                            }
                        }
                        catch (Exception ex) 
                        { 
                            System.Diagnostics.Debug.WriteLine($"Erro ao buscar Agenda {reserva.AgendaId}: {ex.Message}");
                        }
                    }

                    // 4. Monta e adiciona o card
                    var card = MontarCardReserva(
                        atividadeNome, 
                        dataTexto,
                        reserva.InteiraEntrada, 
                        reserva.MeiaEntrada, 
                        reserva.NPEntrada, 
                        reserva.ValorTotal, 
                        statusTexto
                    );
                    ListaReservasAtivas.Children.Add(card);
                }
            }
            catch (JsonException jEx)
            {
                await DisplayAlert("Erro de Dados", $"Erro ao processar dados de reservas. Verifique a estrutura do JSON. Detalhes: {jEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro geral ao carregar reservas: {ex.Message}", "OK");
            }
        }

        // =========================================================================
        // MÉTODOS AUXILIARES E EVENTOS
        // =========================================================================

        private static View MontarCardReserva(string atividade, string dataTexto,
            int inteira, int meia, int npen, decimal valorTotal, string status)
        {
            var ptBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            var border = new Border
            {
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                //StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = 16
            };

            var stack = new VerticalStackLayout { Spacing = 8 };
            stack.Children.Add(new Label { Text = atividade, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#A42D45") });
            if (!string.IsNullOrWhiteSpace(dataTexto))
            {
                stack.Children.Add(new Label { Text = $"Data: {dataTexto}", FontSize = 13, TextColor = Color.FromArgb("#A42D45") });
            }

            stack.Children.Add(new Label
            {
                Text = $"Participantes: {inteira}x Adultos, {meia}x Meia, {npen}x 0–5",
                FontSize = 13,
                TextColor = Color.FromArgb("#A42D45")
            });

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                Margin = new Thickness(0, 8, 0, 0)
            };

            grid.Add(new Label
            {
                Text = $"Valor: R$ {valorTotal.ToString("N2", ptBr)}",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#A42D45"),
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);

            var statusUpper = status.ToUpperInvariant();
            var statusBorder = new Border
            {
                BackgroundColor = statusUpper == "PAGO" ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F68F55"),
                //StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(16, 6)
            };
            statusBorder.Content = new Label
            {
                Text = statusUpper == "PAGO" ? "Pago" : "Pendente",
                TextColor = Colors.White,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold
            };
            grid.Add(statusBorder, 1, 0);

            stack.Children.Add(grid);
            border.Content = stack;
            return border;
        }

        private async void OnAnterioresClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReservasAnterioresPage());
        }

        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Menu",
                "Cancelar",
                null,
                "Configurações", 
                "Ajuda",
                "Sobre"
            );

            if (action == "Configurações")
            {
                await DisplayAlert("Configurações", "Abrindo configurações...", "OK");
            }
            else if (action == "Ajuda")
            {
                await DisplayAlert("Ajuda", "Abrindo central de ajuda...", "OK");
            }
            else if (action == "Sobre")
            {
                await DisplayAlert("Sobre", "App de Reservas - Versão 1.0", "OK");
            }
        }

        private void OnAtivasClicked(object sender, EventArgs e)
        {
            // Já está na página ativa
        }

        private async void OnExcluirClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Exclusão", 
                "Deseja realmente excluir esta reserva?",
                "Sim",
                "Não"
            );

            if (answer)
            {
                // Adicione a lógica de DELETE/CANCELAMENTO de reserva no backend aqui
                await DisplayAlert("Sucesso", "Reserva excluída com sucesso!", "OK");
                // Após a exclusão, recarregue a lista:
                // await CarregarReservasAtivasAsync(); 
            }
        }

        private async void OnSairClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Confirmar Saída",
                "Deseja realmente sair?",
                "Sim",
                "Não"
            );

            if (answer)
            {
                await Navigation.PopToRootAsync();
            }
        }

        private async void OnVerTodasClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Todas as Reservas", "Carregando todas as reservas...", "OK");
        }

        private async void OnVerDetalhesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Detalhes", "Abrindo detalhes da reserva...", "OK");
        }

        private async void OnCardClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Reserva", "Abrindo detalhes da reserva...", "OK");
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue;
            // Lógica de filtro local com base no texto
        }
    }
}