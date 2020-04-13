using Cambios.Modelos;
using Cambios.Servicos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Cambios
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region atributos

        private NetworkService _networkService;
        private ApiService _apiService;
        private List<Rate> Rates;
        private DialogService dialogService;
        private DataService dataService;
        #endregion



        public MainWindow()
        {
            InitializeComponent();
            _networkService = new NetworkService();
            _apiService = new ApiService();
            Rates = new List<Rate>();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            lb_resultado.Text = "A actualizar taxas...";

            var connection = _networkService.CheckConnection(); // Verificar se tenho conexão à internet
            bool load;

            // Conexão não foi bem sucedida
            if (!connection.IsSuccess)
            {                
                LoadLocalRates();
                load = false;
            }

            else // Se tiver conexão irei chamar a apiService (só depois de saber que tenho acesso à internet é que vou tentar aceder à api)
            {                
                await LoadApiRates();
                load = true;
            }

            // Supondo que me ligo e a minha base de dados não está preenchida 
            // (Por exemplo, a primeira vez que me ligo e não tenho internet. Logo não consegui carregar a minha base de dados local)
            if (Rates.Count == 0)
            {

                lb_resultado.Text = "Não há ligação à internet e não foram previamente carregadas as taxas na base de dados local! Tente mais tarde!";
                lb_status.Text = "Primeira Iniciliazação deverá ter ligação à internet";
                return;
            }
            
            //Carregar as comboBoxes com a lista de cambios
            cb_origem.ItemsSource = Rates;
            cb_origem.DisplayMemberPath = "Name"; // Para visualizarmos o nome e não como um objecto - Tmabém poderiamos fazer um override na classe ToString()
            cb_destino.ItemsSource = Rates;
            cb_destino.DisplayMemberPath = "Name"; // Para visualizarmos o nome e não como um objecto
            

            // Depois das taxas carregadas, podemos disponiblizar o botão para o cálculo da conversão
            btn_converter.IsEnabled = true;
            btn_troca.IsEnabled = true;
            lb_resultado.Text = "Taxas actualizadas...";

            //Vou passar a informação para a label do status
            if (load) //Taxas carregadas pela Api - Colocar a data de carregamento
            {
                lb_status.Text = $"Taxas carregadas da internet em {DateTime.Now.ToString()}";
            }
            else //Taxas Carregadas a partir da base de dados local
            {
                lb_status.Text = string.Format("Taxas carregadas da base de dados local");
            }

            ProgressBar.Value = 100;
        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetaData();
        }

        private async Task LoadApiRates()
        {
            ProgressBar.Value = 0;
            var response = await _apiService.GetRates("https://cambiosrafa.azurewebsites.net", "api/rates");

            Rates = (List<Rate>) response.Result; // É necessário fazer o cast
            dataService.DeleteData(); // Apagam-se os dados da base de dados local para se reescrever com os actualizados. Se não os apagasse iriam ser adicionadas as novas taxas às antigas
            dataService.SaveData(Rates);
        }

        private void Btn_converter_Click(object sender, RoutedEventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(tb_valor.Text)) // TextBox valor deve estar preenchido
            {
                dialogService.ShowMessage("Erro", "Insira um valor a converter");
                return;
            }

            // Declarar uma variável decimal para receber o valor da textBox. E ver se se consegue converter o valor através de um tryParse
            decimal valor;

            if (!decimal.TryParse(tb_valor.Text, out valor)) // Não se consegui converter o texto da text box num decimal
            {                
                dialogService.ShowMessage("Erro de conversão", "Valor terá que ser numérico");
                return;
            }

            // Comboxs terão que ter uma moeda seleccionada
            if (cb_origem.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda a converter");
                return;
            }

            // Comboxs terão que ter uma moeda de destino
            if (cb_destino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda de destino para converter");
                return;
            }


            // Guardar as moedas seleccionadas em duas variáveis;
            var taxaOrigem = (Rate)cb_origem.SelectedItem;
            var taxaDestino = (Rate)cb_destino.SelectedItem;

            // Calcular o valor a apresentar
            var ValorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            lb_resultado.Text = $"{valor:N2} {taxaOrigem.Code} = {ValorConvertido:N2} {taxaDestino.Code}";

            

        }

        private void Btn_troca_Click(object sender, RoutedEventArgs e)
        {
            // No botão troca vamos ter apenas o método Trocar()
            Trocar();
        }

        private void Trocar()
        {
            // Criar uma variiável auxiliar para fazer a troca

            var aux = cb_origem.SelectedItem;
            cb_origem.SelectedItem = cb_destino.SelectedItem;
            cb_destino.SelectedItem = aux;

            Converter();
        }
    }
}
