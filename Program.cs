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
        }

    }

    class Program
    {
        static List<County> HotDay(string iDate, int n)
        {
            string connectionString =
            "Data Source=(local);Initial Catalog=H19;"
            + "Integrated Security=true";

            // Provide the query string with a parameter placeholder.
            string queryString =
                "SELECT * from dbo.cases5 "
                    + "WHERE State='Washington' AND Date=@date ";

            DateTime tempDate = Convert.ToDateTime(iDate);

            List<County> hotDays = new List<County>();
            // Create and open the connection in a using block.
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@date", tempDate);

                // Open the connection in a try/catch block.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine("\t{0}\t{1}\t{2}",
                            reader[0], reader[1], reader[2]);
                        hotDays.Add(new County((string)reader[1], (int)reader[3]));
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.ReadLine();
            }
            return hotDays;
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
            HotDay("2020/3/2", 5);
            //HotQuery();
            //DeltaQuery();
            //TimeQuery();

        }
    }
}
