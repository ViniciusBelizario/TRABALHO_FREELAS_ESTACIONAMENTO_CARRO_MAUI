using JoaoCar2.Database;
using JoaoCar2.Model;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace JoaoCar2.Views
{
    public partial class FiltrosVeiculosPage : ContentPage
    {
        public FiltrosVeiculosPage()
        {
            InitializeComponent();
            statusPagamentoPicker.SelectedIndex = 0; // "Todos"
            tipoContratoPicker.SelectedIndex = 0;    // "Todos"
            dataPicker.Date = DateTime.Now;
        }

        // Botão Voltar
        private async void OnVoltarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Botão Limpar
        private void OnLimparClicked(object sender, EventArgs e)
        {
            placaEntry.Text = "";
            proprietarioEntry.Text = "";
            tipoContratoPicker.SelectedIndex = 0;
            dataPicker.Date = DateTime.Now;
            statusPagamentoPicker.SelectedIndex = 0;
            resultadosCollectionView.IsVisible = false;
        }

        // Botão Aplicar Filtros
        private async void OnAplicarClicked(object sender, EventArgs e)
        {
            string placa = placaEntry.Text?.ToUpper().Trim();
            string proprietario = proprietarioEntry.Text?.Trim();
            string tipoContrato = tipoContratoPicker.SelectedIndex > 0
                ? tipoContratoPicker.SelectedItem.ToString()
                : null; // null para "Todos"
            DateTime? data = dataPicker.Date;
            bool? pago = null;

            if (statusPagamentoPicker.SelectedIndex == 1)
                pago = true;
            else if (statusPagamentoPicker.SelectedIndex == 2)
                pago = false;

            // Busca filtrada (todos os veículos)
            var resultados = await DatabaseService.BuscarFiltrado(
                placa,
                proprietario,
                tipoContrato,
                data,
                pago
            );

            resultadosCollectionView.ItemsSource = resultados;
            resultadosCollectionView.IsVisible = true;
        }
    }
}
