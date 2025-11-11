using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Teste
{
    public class Atividade : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }

        private bool _isSelecionada;
        public bool IsSelecionada
        {
            get => _isSelecionada;
            set
            {
                if (_isSelecionada != value)
                {
                    _isSelecionada = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelecionada)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class AtividadesPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string apiUrlAtividade = "http://tiijeferson.runasp.net/api/Atividade";

        private ObservableCollection<Atividade> _atividades = new();
        private DateTime dataSelecionada;
        private TimeSpan? horarioSelecionado;

        // Contadores globais - iniciando com 1 adulto como padrão
        private int adultos = 1;
        private int criancas0a5 = 0;
        private int criancas6a12 = 0;

        // Preço padrão para meia entrada (crianças 6-12)
        private readonly double precoCrianca6a12 = 12.50;

        public AtividadesPage()
        {
            InitializeComponent();

            // Inicializa Labels de contagem na tela
            LblAdultos.Text = adultos.ToString();
            LblCrianca0a5.Text = criancas0a5.ToString();
            LblCrianca6a12.Text = criancas6a12.ToString();

            if (Preferences.ContainsKey("DataAgendamento"))
            {
                var dataStr = Preferences.Get("DataAgendamento", "");
                if (DateTime.TryParse(dataStr, out var data))
                    dataSelecionada = data;
                else
                    dataSelecionada = DateTime.Now;
            }
            else
            {
                dataSelecionada = DateTime.Now;
            }

            AtualizarDataNaTela();
            TimePickerHorario.PropertyChanged += TimePickerHorario_PropertyChanged;
            _ = CarregarAtividades();
        }

        public void SetData(DateTime data)
        {
            dataSelecionada = data;
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            AtualizarDataNaTela();
        }

        private void AtualizarDataNaTela()
        {
            if (LblDataSelecionada != null)
                LblDataSelecionada.Text = dataSelecionada.ToString("dd/MM/yyyy");
        }

        private async Task CarregarAtividades()
        {
            try
            {
                var response = await client.GetAsync(apiUrlAtividade);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var atividades = JsonSerializer.Deserialize<List<Atividade>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Atividade>();

                    _atividades = new ObservableCollection<Atividade>(
                        atividades.Select(a =>
                        {
                            a.IsSelecionada = false;
                            return a;
                        }).Take(3)
                    );
                }
                else
                {
                    CriarAtividadesFicticias();
                }
            }
            catch
            {
                CriarAtividadesFicticias();
            }

            AtividadesCollectionView.ItemsSource = _atividades;

            // Adiciona listener para quando o checkbox mudar
            foreach (var atividade in _atividades)
            {
                atividade.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Atividade.IsSelecionada))
                        AtualizarTotal();
                };
            }

            AtualizarTotal();
        }

        private void CriarAtividadesFicticias()
        {
            var lista = new List<Atividade>
            {
                new() { Id = 1, Nome = "Café da manhã Básicaaa", Valor = 25.00m, IsSelecionada = false },
                new() { Id = 2, Nome = "Café da manhã Completo", Valor = 65.00m, IsSelecionada = false },
                new() { Id = 3, Nome = "Trezinho / Colha e Pague", Valor = 15.00m, IsSelecionada = false }
            };

            _atividades = new ObservableCollection<Atividade>(lista);
        }

        private void AtualizarTotal()
        {
            if (_atividades == null || _atividades.Count == 0)
            {
                if (LblTotalEstimado != null)
                    LblTotalEstimado.Text = "Total estimado: R$ 0,00";
                return;
            }

            double total = 0;

            // Calcula o custo das atividades selecionadas (por adulto)
            foreach (var atividade in _atividades.Where(a => a.IsSelecionada))
            {
                total += adultos * (double)atividade.Valor;
            }

            // Custo das crianças (6 a 12 anos) - meia entrada
            total += criancas6a12 * precoCrianca6a12;

            if (LblTotalEstimado != null)
                LblTotalEstimado.Text = $"Total estimado: R$ {total:F2}";
        }

        private void BtnVoltar_Clicked(object sender, EventArgs e) => Navigation.PopAsync();

        private void TimePickerHorario_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimePicker.Time))
                horarioSelecionado = TimePickerHorario.Time;
        }

        // ======= BOTÕES DE ADULTOS =======
        private void BtnAdultoMais_Clicked(object sender, EventArgs e)
        {
            adultos++;
            LblAdultos.Text = adultos.ToString();
            AtualizarTotal();
        }

        private void BtnAdultoMenos_Clicked(object sender, EventArgs e)
        {
            if (adultos > 1) // Mantém no mínimo 1 adulto
            {
                adultos--;
                LblAdultos.Text = adultos.ToString();
                AtualizarTotal();
            }
        }

        // ======= BOTÕES DE CRIANÇAS 0 A 5 (GRÁTIS) =======
        private void BtnCrianca0a5Mais_Clicked(object sender, EventArgs e)
        {
            criancas0a5++;
            LblCrianca0a5.Text = criancas0a5.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca0a5Menos_Clicked(object sender, EventArgs e)
        {
            if (criancas0a5 > 0)
            {
                criancas0a5--;
                LblCrianca0a5.Text = criancas0a5.ToString();
                AtualizarTotal();
            }
        }

        // ======= BOTÕES DE CRIANÇAS 6 A 12 (MEIA ENTRADA) =======
        private void BtnCrianca6a12Mais_Clicked(object sender, EventArgs e)
        {
            criancas6a12++;
            LblCrianca6a12.Text = criancas6a12.ToString();
            AtualizarTotal();
        }

        private void BtnCrianca6a12Menos_Clicked(object sender, EventArgs e)
        {
            if (criancas6a12 > 0)
            {
                criancas6a12--;
                LblCrianca6a12.Text = criancas6a12.ToString();
                AtualizarTotal();
            }
        }

        private async void BtnContinuar_Clicked(object sender, EventArgs e)
        {
            var horarioCapturado = TimePickerHorario.Time;

            if (horarioCapturado == new TimeSpan(0, 0, 0) && horarioSelecionado == null)
            {
                await DisplayAlert("Aviso", "Por favor, selecione um horário.", "OK");
                return;
            }

            if (horarioSelecionado == null)
                horarioSelecionado = horarioCapturado;

            var selecionadas = _atividades.Where(a => a.IsSelecionada).ToList();

            if (selecionadas.Count == 0)
            {
                await DisplayAlert("Aviso", "Selecione pelo menos uma atividade.", "OK");
                return;
            }

            // Calcula o total CORRETAMENTE
            double total = 0;
            foreach (var atividade in selecionadas)
            {
                total += adultos * (double)atividade.Valor;
            }
            total += criancas6a12 * precoCrianca6a12;

            // Salva no Preferences - ADICIONE ESTA LINHA PARA SALVAR O VALOR DAS ATIVIDADES
            var atividadesValores = selecionadas.Select(a => a.Valor.ToString(CultureInfo.InvariantCulture));
            Preferences.Set("AtividadesValores", string.Join(",", atividadesValores));

            var atividadesSelecionadas = selecionadas.Select(a => a.Nome);
            var atividadeIds = selecionadas.Select(a => a.Id);

            Preferences.Set("AtividadesSelecionadas", string.Join(", ", atividadesSelecionadas));
            Preferences.Set("AtividadesIds", string.Join(",", atividadeIds));
            Preferences.Set("QtdAdultos", adultos);
            Preferences.Set("QtdCriancas0a5", criancas0a5);
            Preferences.Set("QtdCriancas6a12", criancas6a12);
            Preferences.Set("ValorTotalEstimado", total.ToString("F2", CultureInfo.InvariantCulture)); // Use cultura invariável
            Preferences.Set("DataAgendamento", dataSelecionada.ToString("yyyy-MM-dd"));
            Preferences.Set("HorarioSelecionado", horarioSelecionado?.ToString(@"hh\:mm") ?? string.Empty);

            var jsonSelecionadas = JsonSerializer.Serialize(selecionadas.Select(a => new
            {
                a.Id,
                a.Nome,
                a.Valor
            }));
            Preferences.Set("AtividadesSelecionadasDetalhe", jsonSelecionadas);

            await DisplayAlert("Resumo da Reserva",
                $"Atividades: {string.Join(", ", atividadesSelecionadas)}\n\n" +
                $"Data: {dataSelecionada:dd/MM/yyyy}\n" +
                $"Horário: {horarioSelecionado:hh\\:mm}\n" +
                $"Adultos: {adultos}\n" +
                $"Crianças (0–5): {criancas0a5}\n" +
                $"Crianças (6–12): {criancas6a12}\n\n" +
                $"Total estimado: R$ {total:F2}",
                "OK");

            await Navigation.PushAsync(new PagamentoPage());
        }
    }
}