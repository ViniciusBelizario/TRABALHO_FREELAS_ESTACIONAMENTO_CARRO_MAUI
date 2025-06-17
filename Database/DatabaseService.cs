using SQLite;
using JoaoCar2.Model;

namespace JoaoCar2.Database
{
    public class DatabaseService
    {
        static SQLiteAsyncConnection db;

        public static async Task Init()
        {
            if (db != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "veiculos.db");
            db = new SQLiteAsyncConnection(databasePath);
            await db.CreateTableAsync<Veiculo>();
        }

        // Retorna todos os veículos ordenados por data de entrada (mais recentes primeiro)
        public static async Task<List<Veiculo>> GetVeiculosAsync()
        {
            await Init();
            return await db.Table<Veiculo>().OrderByDescending(v => v.DataEntrada).ToListAsync();
        }

        // Retorna o último veículo cadastrado com a placa informada
        public static async Task<Veiculo> GetVeiculoPorPlacaAsync(string placa)
        {
            await Init();
            return await db.Table<Veiculo>()
                .Where(v => v.Placa == placa.ToUpper())
                .OrderByDescending(v => v.DataEntrada)
                .FirstOrDefaultAsync();
        }

        // Retorna todos os veículos já cadastrados com a mesma placa
        public static async Task<List<Veiculo>> GetVeiculosPorPlacaAsync(string placa)
        {
            await Init();
            return await db.Table<Veiculo>()
                .Where(v => v.Placa == placa.ToUpper())
                .OrderByDescending(v => v.DataEntrada)
                .ToListAsync();
        }

        // Adiciona um novo veículo
        public static async Task<int> AddVeiculoAsync(Veiculo veiculo)
        {
            await Init();
            veiculo.Placa = veiculo.Placa.ToUpper(); // Garante padrão
            return await db.InsertAsync(veiculo);
        }

        // Atualiza um veículo existente
        public static async Task<int> UpdateVeiculoAsync(Veiculo veiculo)
        {
            await Init();
            return await db.UpdateAsync(veiculo);
        }

        // Remove um veículo do banco de dados
        public static async Task<int> DeleteVeiculoAsync(Veiculo veiculo)
        {
            await Init();
            return await db.DeleteAsync(veiculo);
        }

        // Busca veículos combinando placa, proprietário, tipo de contrato, data e status de pagamento
        public static async Task<List<Veiculo>> BuscarFiltrado(
            string placa,
            string proprietario,
            string tipoContrato,
            DateTime? data,
            bool? pago)
        {
            await Init();
            var lista = await db.Table<Veiculo>().OrderByDescending(v => v.DataEntrada).ToListAsync();

            // Filtro de Placa
            if (!string.IsNullOrWhiteSpace(placa))
                lista = lista.Where(v => v.Placa.Contains(placa.ToUpper())).ToList();

            // Filtro de Proprietário
            if (!string.IsNullOrWhiteSpace(proprietario))
                lista = lista.Where(v =>
                    !string.IsNullOrEmpty(v.NomeProprietario) &&
                    v.NomeProprietario.ToUpper().Contains(proprietario.ToUpper())
                ).ToList();

            // Filtro de Tipo de Contrato (Mensalista, Diario)
            if (!string.IsNullOrWhiteSpace(tipoContrato) && tipoContrato != "Todos")
                lista = lista.Where(v =>
                    !string.IsNullOrEmpty(v.Tipo) &&
                    v.Tipo.Equals(tipoContrato, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            // Filtro de Data de Entrada
            if (data.HasValue)
                lista = lista.Where(v => v.DataEntrada.Date == data.Value.Date).ToList();

            // Filtro de Pagamento
            if (pago.HasValue)
                lista = lista.Where(v => v.Pago == pago.Value).ToList();

            return lista;
        }
    }
}
