using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace H19
{

    public class CaseDatum
    {

        public DateTime when;
        public int cases;

        public CaseDatum(DateTime dateInput, int n)
        {
            when = dateInput;
            cases = n;
        }

    }
    public class County
    {
        public String name;
        public int population;
        public virtual List<CaseDatum> Samples { get; set; }

        public County(string countyName, int n)
        {
            name = countyName;
            population = n;
            Samples = new List<CaseDatum>();
        }
    }

    class Program
    {
        static List<County> getHotCounties(int n=10)
        {
            string connectionString =
            "Data Source=(local);Initial Catalog=H19;"
            + "Integrated Security=true";

            // Provide the query string with a parameter placeholder.
            //string queryString = "SELECT * from dbo.cases5 "+"WHERE State='Washington' AND Date=@date";
            string queryString =
                "SELECT TOP (@topNumber) MAX(Date) AS LastDay, County, AVG(FIPS), (MAX(Cases)-Min(Cases)) AS Delta " +
                "FROM (SELECT *, ROW_NUMBER() OVER(PARTITION BY County ORDER BY Date DESC) AS RowNumber " +
                "FROM dbo.cases5 " +
                "WHERE State = 'Washington') AS Temp " +
                "WHERE Temp.RowNumber <= 2 " +
                "GROUP BY County " +
                "ORDER BY Delta DESC ";

            //DateTime tempDate = Convert.ToDateTime(iDate);

            List <County> hotCounties = new List<County>();
            // Create and open the connection in a using block.
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);
                //command.Parameters.AddWithValue("@date", tempDate);
                command.Parameters.Add("@topNumber", SqlDbType.Int);
                command.Parameters["@topNumber"].Value = n;
                // Open the connection in a try/catch block.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        County temp = new County((string)reader[1], (int)reader[2]);
                        temp.Samples.Add(new CaseDatum(Convert.ToDateTime(reader[0]), (int)reader[3]));
                        hotCounties.Add(temp);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return hotCounties;
        }

        static void HotQuery()
        {
            //Display "hot" counties at a date or date range

        }

        static void DeltaQuery()
        {
            //return first county(object) where delta cases per capita is greater than a given number

        }

        static void TimeQuery()
        {
            //Display county info including timeseries data for daterange/all time
            //return a county object with the entire timeserie of cases for use by other code(e.g.analysis engine).
            //County info includes population so that per - capita

        }

        static void Main(string[] args)
        {
            List<County> Counties = getHotCounties(5);
            Console.WriteLine(Counties[0]);
            //HotQuery();
            //DeltaQuery();
            //TimeQuery();

        }
    }
}
