using JoaoCar2.Model;
using JoaoCar2.Database;
using System.Text.RegularExpressions;

namespace JoaoCar2.Views;

public partial class CadastroVeiculoPage : ContentPage
{
    private string fotoPath = null;
    private DateTime dataEntrada = DateTime.Now;

    public CadastroVeiculoPage()
    {
        InitializeComponent();
        dataEntradaLabel.Text = $"Data/Hora de entrada: {dataEntrada:dd/MM/yyyy HH:mm}";
    }

    // Preenche os campos se digitar uma placa j� cadastrada
    private async void PlacaEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry?.Text != null)
        {
            string upper = entry.Text.ToUpper();

            // Limita a 7 caracteres
            if (upper.Length > 7)
                upper = upper.Substring(0, 7);

            if (entry.Text != upper)
                entry.Text = upper;

            if (upper.Length == 7)
            {
                // Busca o �ltimo ve�culo com essa placa (se j� saiu, permite novo cadastro)
                var veiculos = await DatabaseService.GetVeiculosPorPlacaAsync(upper);
                var ultimo = veiculos.OrderByDescending(v => v.DataEntrada).FirstOrDefault();
                if (ultimo != null)
                {
                    marcaEntry.Text = ultimo.Marca;
                    modeloEntry.Text = ultimo.Modelo;
                    corEntry.Text = ultimo.Cor;
                    nomeProprietarioEntry.Text = ultimo.NomeProprietario;
                    tipoPicker.SelectedItem = ultimo.Tipo;
                    fotoPath = ultimo.FotoPath;
                    if (!string.IsNullOrEmpty(fotoPath))
                        fotoImage.Source = fotoPath;
                }
            }
        }
    }

    // Selecionar foto do ve�culo
    private async void OnSelecionarFotoClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione uma foto do ve�culo"
            });
            if (result != null)
            {
                fotoPath = result.FullPath;
                fotoImage.Source = fotoPath;
            }
        }
        catch { /* Trate erros se quiser */ }
    }

    // Bot�o Salvar
    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        // Valida��o dos campos obrigat�rios
        if (string.IsNullOrWhiteSpace(placaEntry.Text) ||
            string.IsNullOrWhiteSpace(marcaEntry.Text) ||
            string.IsNullOrWhiteSpace(modeloEntry.Text) ||
            string.IsNullOrWhiteSpace(corEntry.Text) ||
            string.IsNullOrWhiteSpace(nomeProprietarioEntry.Text) ||
            tipoPicker.SelectedItem == null)
        {
            await DisplayAlert("Erro", "Preencha todos os campos obrigat�rios.", "OK");
            return;
        }

        // Valida��o da placa (padr�o Brasil)
        string placa = placaEntry.Text.ToUpper().Replace("-", "").Trim();
        if (!PlacaBrasileiraValida(placa))
        {
            await DisplayAlert("Erro", "Informe uma placa v�lida no padr�o brasileiro (ABC1234 ou BRA2E19).", "OK");
            return;
        }

        // Permite cadastrar novamente s� se n�o houver ve�culo ATIVO com essa placa
        var veiculosMesmoPlaca = await DatabaseService.GetVeiculosPorPlacaAsync(placa);
        var veiculoAtivo = veiculosMesmoPlaca.FirstOrDefault(v => v.DataSaida == null);
        if (veiculoAtivo != null)
        {
            await DisplayAlert("Erro", $"J� existe um ve�culo com a placa {placa} no estacionamento.\nS� � permitido cadastrar novamente ap�s a sa�da.", "OK");
            return;
        }

        var veiculo = new Veiculo
        {
            Placa = placa,
            Marca = marcaEntry.Text,
            Modelo = modeloEntry.Text,
            Cor = corEntry.Text,
            NomeProprietario = nomeProprietarioEntry.Text,
            Tipo = tipoPicker.SelectedItem.ToString(),
            FotoPath = fotoPath,
            DataEntrada = dataEntrada,
            Pago = false,
            DataSaida = null
        };

        try
        {
            await DatabaseService.AddVeiculoAsync(veiculo);
            await DisplayAlert("Sucesso", "Ve�culo cadastrado com sucesso!", "OK");
            LimparCampos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
    }

    // Limpa os campos ap�s salvar
    private void LimparCampos()
    {
        placaEntry.Text = "";
        marcaEntry.Text = "";
        modeloEntry.Text = "";
        corEntry.Text = "";
        nomeProprietarioEntry.Text = "";
        tipoPicker.SelectedItem = null;
        fotoImage.Source = null;
        fotoPath = null;
        dataEntrada = DateTime.Now;
        dataEntradaLabel.Text = $"Data/Hora de entrada: {dataEntrada:dd/MM/yyyy HH:mm}";
    }

    // Fun��o de valida��o de placas brasileiras
    public static bool PlacaBrasileiraValida(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return false;

        placa = placa.ToUpper().Replace("-", "").Trim();

        // Padr�o antigo: 3 letras + 4 n�meros (ABC1234)
        var padraoAntigo = new Regex("^[A-Z]{3}[0-9]{4}$");

        // Padr�o Mercosul: 3 letras + 1 n�mero + 1 letra + 2 n�meros (BRA2E19)
        var padraoMercosul = new Regex("^[A-Z]{3}[0-9][A-Z][0-9]{2}$");

        return padraoAntigo.IsMatch(placa) || padraoMercosul.IsMatch(placa);
    }
}
