using SQLite;

namespace JoaoCar2.Model
{
    public class Veiculo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Placa { get; set; }

        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Cor { get; set; }
        public string NomeProprietario { get; set; }
        public string Tipo { get; set; } // Mensalista ou Diario
        public string FotoPath { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public bool Pago { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
