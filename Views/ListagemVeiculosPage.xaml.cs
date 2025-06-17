using JoaoCar2.Database;
using JoaoCar2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace JoaoCar2.Views;

public partial class ListagemVeiculosPage : ContentPage
{
    private List<Veiculo> listaAtual = new();

    public ListagemVeiculosPage()
    {
        InitializeComponent();

        // Escuta mensagem de filtro vindo da página de filtros
        MessagingCenter.Subscribe<FiltrosVeiculosPage, (string placa, DateTime? data, bool? pago)>(this, "FiltrosAplicados", async (sender, filtros) =>
        {
            await AplicarFiltroAsync(filtros.placa, filtros.data, filtros.pago);
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CarregarVeiculos();
    }

    private async void CarregarVeiculos()
    {
        var lista = await DatabaseService.GetVeiculosAsync();
        var ativos = lista.Where(v => v.DataSaida == null).ToList();
        veiculosCollectionView.ItemsSource = ativos;
        listaAtual = ativos;
    }

    // ATENÇÃO: agora usamos todos os parâmetros do filtro!
    private async Task AplicarFiltroAsync(string placa, DateTime? data, bool? pago)
    {
        var filtrados = await DatabaseService.BuscarFiltrado(
            placa,
            null,   // proprietário
            null,   // tipoContrato
            data,
            pago
        );
        filtrados = filtrados.Where(v => v.DataSaida == null).ToList(); // só ativos
        veiculosCollectionView.ItemsSource = filtrados;
    }

    private async void OnAbrirFiltrosClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FiltrosVeiculosPage());
    }

    // Botão pagamento: alterna status pago/não pago
    private async void OnPagamentoClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is Veiculo veiculo)
        {
            veiculo.Pago = !veiculo.Pago;
            await DatabaseService.UpdateVeiculoAsync(veiculo);
            CarregarVeiculos();
        }
    }

    // Botão saída: confirma, marca saída e remove da tela
    private async void OnSaidaClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Veiculo veiculo)
        {
            if (!veiculo.Pago)
            {
                await DisplayAlert("Saída não permitida", "O veículo só pode sair após o pagamento.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Saída", $"Confirmar saída do veículo {veiculo.Placa}?", "Sim", "Não");
            if (confirm)
            {
                veiculo.DataSaida = DateTime.Now;
                await DatabaseService.UpdateVeiculoAsync(veiculo);
                await DisplayAlert("Sucesso", $"Veículo {veiculo.Placa} saiu do estacionamento.", "OK");
                CarregarVeiculos();
            }
        }
    }

    // Botão excluir: remove veículo após confirmação
    private async void OnExcluirClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is Veiculo veiculo)
        {
            bool confirm = await DisplayAlert("Excluir", $"Deseja excluir o veículo {veiculo.Placa}?", "Sim", "Não");
            if (confirm)
            {
                await DatabaseService.DeleteVeiculoAsync(veiculo);
                CarregarVeiculos();
            }
        }
    }

    private void OnImageClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is string fotoPath && !string.IsNullOrEmpty(fotoPath))
        {
            ampliarImagemOverlay.IsVisible = true;
            imagemAmpliada.Source = fotoPath;
        }
    }

    private void OnFecharImagemClicked(object sender, EventArgs e)
    {
        ampliarImagemOverlay.IsVisible = false;
        imagemAmpliada.Source = null;
    }

    // Mostra/oculta StackLayout de ações
    private void OnAcoesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.Parent is Layout parentLayout)
        {
            var index = parentLayout.Children.IndexOf(btn);
            if (index >= 0 && index + 1 < parentLayout.Children.Count)
            {
                if (parentLayout.Children[index + 1] is StackLayout acoesStack)
                {
                    acoesStack.IsVisible = !acoesStack.IsVisible;
                }
            }
        }
    }
}
