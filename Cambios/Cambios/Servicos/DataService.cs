namespace Cambios.Servicos
{
    using Modelos;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;

    public class DataService // Classe que vai permitir a ligação à base de dados local
    {

        // Atributos
        private SQLiteConnection connection; //Conexão
        private SQLiteCommand command; //Comando que vai permitir as queries
        private DialogService dialogService;

        //Construtor - Vai servir para criar a pasta Data, a base de dados e a tabela, caso não existam
        public DataService()
        {
            dialogService = new DialogService();

            // Verificar se existe uma pasta com o nome "Data" --> Se não existir, a pasta é criada
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            // A nossa base de dados terá o nome de Rates (com a extensão sqlite) e será guardada na pasta Rates --> o path corresponde ao caminho completo
            var path = @"Data\Rates.sqlite";

            try
            {
                // Já temos a pasta Data criada, vamos tentar a conexão à base de dados
                connection = new SQLiteConnection("Data source=" + path);
                connection.Open(); // O método Open vai tentar abrir a base de dados. Se esta não existir, é criada pelo método

                // Neste momento já temos a pasta Data criada e a base de dados Rate, mas a temos que de seguida criar a tabela rates
                string sqlcommand = "CREATE TABLE IF NOT EXISTS rates (RateId INT, Code VARCHAR (5), TaxRate DECIMAL, Name VARCHAR(250))";
                command = new SQLiteCommand(sqlcommand, connection);
                command.ExecuteNonQuery(); // Executar o comando sql de criação da tabela rates

            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
            }          
        }

        public void SaveData(List<Rate> Rates)
        {

            try
            {
                foreach (var rate in Rates)
                {
                    //string sql = string.Format($"insert into rates (RateId, Code, TaxRate, Name) values {0}, '{1}', {2}, '{3}')", rate.RateId, rate.Code, rate.TaxRate, rate.Name);
                    command = new SQLiteCommand(connection);
                    // Tive que mudar a separação decimal do pc para ponto em vez de vírgula. Assumia a TaxRate como dois valores distintos
                    command.CommandText = $"INSERT INTO rates(RateId, Code, TaxRate, Name) VALUES({rate.RateId},'{rate.Code}',{rate.TaxRate},'{rate.Name}')";
                    command.ExecuteNonQuery();
                    
                }

                connection.Close();
            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
            }


        }

        public List<Rate> GetaData()
        {
            List<Rate> rates = new List<Rate>();

            try
            {
                string query = "SELECT RateId, Code, TaxRate, Name FROM rates";
                command = new SQLiteCommand(query, connection);

                // Lê cada registo
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read()) // reader.Read() --> Vai verificar a linha seguinte e se não existirem registos para ler devolve false
                {
                    rates.Add(new Rate
                    {
                        RateId = (int)reader["RateId"], // O reader devolve sempre uma string, é preciso fazer o cast para inteiro
                        Code = (string)reader["Code"],
                        TaxRate = (double)reader["TaxRate"],
                        Name = (string)reader["Name"]
                    });

                }

                connection.Close();
                return rates;
            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
                return null;
            }

        }


        public void DeleteData() // Apagar os dados da base de dados locais
        {
            try
            {
                string query = "DELETE FROM rates";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Erro", e.Message);
                
            }          
        }
    }
}
