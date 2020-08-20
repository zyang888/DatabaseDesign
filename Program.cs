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
        static String checkDateString(String inputString)
        {
            DateTime dDate;
            if (DateTime.TryParse(inputString, out dDate))
            {
                inputString = String.Format("{0:yyyy-MM-d}", dDate);
            }
            else
            {
                Console.WriteLine("Invalid"); // <-- Control flow goes here
            }
            return inputString;
        }

        static List<County> getHotCounties(String startDate, String endDate, int n = 10)
        {
            startDate = checkDateString(startDate);
            endDate = checkDateString(endDate);
            string queryString =
                "WITH temp as " +
                "(SELECT * FROM dbo.cases5 " +
                $"WHERE Date = {startDate} or Date = {endDate}) " +
                "SELECT TOP 5" +
                "MIN(Date) AS StartDate, " +
                "MAX(Date) AS LastDate, " +
                "County, " +
                "MAX(Cases)-MIN(Cases) as Delta " +
                "FROM temp " +
                "WHERE State = 'Washington' " +
                "GROUP BY County " +
                "ORDER BY Delta DESC";
            return HotCountiesQuery(queryString,n);
        }

        static List<County> getHotCounties(String getDate, int n = 10)
        {
            getDate = checkDateString(getDate);
            string queryString =
                    "SELECT TOP (@topNumber), County, AVG(FIPS), Cases " +
                    "FROM dbo.cases5 " +
                    "WHERE State = 'Washington') AS Temp " +
                    "ORDER BY Cases DESC ";
            return HotCountiesQuery(queryString,n);
        }

        static List<County> getHotCounties(int n = 10)
        {
            string queryString =
                    $"SELECT TOP (@topNumber) MAX(Date) AS LastDay, County, AVG(FIPS), (MAX(Cases)-Min(Cases)) AS Delta " +
                    "FROM (SELECT *, ROW_NUMBER() OVER(PARTITION BY County ORDER BY Date DESC) AS RowNumber " +
                    "FROM dbo.cases5 " +
                    "WHERE State = 'Washington') AS Temp " +
                    "WHERE Temp.RowNumber <= 2 " +
                    "GROUP BY County " +
                    "ORDER BY Delta DESC ";
            return HotCountiesQuery(queryString,n);
        }
        static List<County> HotCountiesQuery(String queryString,int n)
        {
            string connectionString =
            "Data Source=(local);Initial Catalog=H19;"
            + "Integrated Security=true";

            List<County> hotCounties = new List<County>();
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
            int count = 0;
            foreach (County element in hotCounties)
            {
                count++;
                Console.WriteLine($"No.#{count} hottest county: {element.name}");
            }
            return hotCounties;
        }


        static County DeltaQuery(int delta0)
        {
            //return first county(object) where delta cases per capita is greater than a given number
            string connectionString =
                "Data Source=(local); Initial Catalog=H19; " + "Integrated Security=true";

            // Provide the query string with a parameter placeholder.
            string queryString =
                "SELECT * " +
                "FROM dbo.cases5 " +
                "WHERE state = 'Washington' " +
                "order by county, date";

            // Create and open the connection in a using block.
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    int temp = 0;
                    while (reader.Read())
                    {
                        Console.WriteLine(reader[4]);
                        if ((int)reader[4]-temp>=delta0)
                        {
                            return new County((string)reader[1], (int)reader[3]);
                            Console.WriteLine("loop not broken");
                        }
                        temp = (int) reader[4];
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        static void TimeQuery()
        {
            //Display county info including timeseries data for daterange/all time
            //return a county object with the entire timeserie of cases for use by other code(e.g.analysis engine).
            //County info includes population so that per - capita

        }

        static void Main(string[] args)
        {
            List<County> Counties = getHotCounties();
            DeltaQuery(3);
            //TimeQuery();

        }
    }
}
