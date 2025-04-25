using Npgsql;

namespace API.Utils
{
    public class DbBuilderExample // change this to DbBuilder
    {
        public static void Main(string[] args)
        {
            string host = "localhost";
            string port = "YOUR_DB_LISTEN_PORT";
            string user = "YOUR_DB_USERNAME";
            string password = "YOUR_DB_PASSWORD_HERE";
            string databaseName = "ICT-Backend";

            string connectionString = $"Host={host};Port={port};Username={user};Password={password};";

            try
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                command.ExecuteNonQuery();
                Console.WriteLine($"Database '{databaseName}' created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}