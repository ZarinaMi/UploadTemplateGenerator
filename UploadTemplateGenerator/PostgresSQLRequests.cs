namespace LQUIAutomation
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Logging;
	using Npgsql;

	public class PostgresSQLRequests
	{
		private readonly NpgsqlConnection _connection;

		public PostgresSQLRequests( string connectionString)
		{
			_connection = new NpgsqlConnection(connectionString);
		}

		public string GetClientIdFromName(string clientName)
		{
			var queryString ="SELECT id FROM clients"
				+ $" WHERE companyname = '{clientName}';";

            try
            {
                _connection.Open();
                using var cmd = new NpgsqlCommand(queryString, _connection);
                using var dbReader = cmd.ExecuteReader();
                dbReader.Read();

                var clientId = dbReader.GetInt32("id");
                return clientId.ToString();
            }
            catch (Exception e)
            {
                _connection.Close();
                return "";
            }
            finally
            {
                _connection.Close();
            }
		}

        public bool IsGasbEnabled(string clientId)
        {
             var queryString = "select isgasbenabled from borrowingratelock"
                + $" where clientid = '{clientId}';";

            try
            {
                _connection.Open();
                using var cmd = new NpgsqlCommand(queryString, _connection);
                using var dbReader = cmd.ExecuteReader();
                dbReader.Read();

                var isGasbEnabled = dbReader.GetBoolean("isgasbenabled");
                return isGasbEnabled;
            }
            catch (Exception e)
            {                               
                _connection.Close();
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }
        public bool IsFasbEnabled(string clientId)
        {
            var queryString = "select isfasbenabled from borrowingratelock"
               + $" where clientid = '{clientId}';";

            try
            {
                _connection.Open();
                using var cmd = new NpgsqlCommand(queryString, _connection);
                using var dbReader = cmd.ExecuteReader();
                dbReader.Read();

                var isGasbEnabled = dbReader.GetBoolean("isgasbenabled");
                return isGasbEnabled;
            }
            catch (Exception e)
            {
                _connection.Close();
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }
        public int GetTransitionYear(string clientId)
        {
            var queryString = "select  effectiveyear from borrowingratelock "
               + $" where clientid = '{clientId}';";

            try
            {
                _connection.Open();
                using var cmd = new NpgsqlCommand(queryString, _connection);
                using var dbReader = cmd.ExecuteReader();
                dbReader.Read();

                var transitionYear = dbReader.GetInt16("effectiveyear");
                return transitionYear;
            }
            catch (Exception e)
            {
                _connection.Close();
                return 0;
            }
            finally
            {
                _connection.Close();
            }
        }
        public int GetTransitionMonth(string clientId)
        {
            var queryString = "select  effectivemonth from borrowingratelock "
               + $" where clientid = '{clientId}';";

            try
            {
                _connection.Open();
                using var cmd = new NpgsqlCommand(queryString, _connection);
                using var dbReader = cmd.ExecuteReader();
                dbReader.Read();

                var transitionMonth = dbReader.GetInt16("effectivemonth");
                return transitionMonth;
            }
            catch (Exception e)
            {
                _connection.Close();
                return 0;
            }
            finally
            {
                _connection.Close();
            }
        }

        public List<string> GetAllocationItems(string clientId)
        {
			var query = "select name from allocationitems where allocationlevelid = "
				+ " (select id from allocationlevels "
				+ $" where isallocatable = true and clientid = {clientId});";
            try
            {
                _connection.Open();
                using var cmd = new NpgsqlCommand(query, _connection);
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                var allocationItems = new List<string>();
                while (reader.Read())
                {
                    allocationItems.Add(reader.GetString("name"));
                }

				return allocationItems;
            }
            catch (Exception ex)
            {                              
                return null;
            }
            finally
            {
                _connection.Close();
            }
        }       
	}
}