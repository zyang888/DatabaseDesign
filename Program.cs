using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace H19
{
    public interface IH19Datum { };

    public interface ICountyInfo { };

    public class CaseDatum : IH19Datum
    {

        public DateTime when;
        public int cases;

        public CaseDatum(DateTime dateInput, int n)
        {
            when = dateInput;
            cases = n;
        }

    }
    public class County : ICountyInfo
    {
        public String name;
        public int population;
        public virtual List<CaseDatum> Samples { get; set; }

        public County() {
            Samples = new List<CaseDatum>();
        }

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
                Console.WriteLine("Invalid");
            }
            return inputString;
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
            return HotCountiesQuery(queryString, n);
        }

        static List<County> getHotCounties(String getDate, int n = 10)
        {
            getDate = checkDateString(getDate);
            string queryString =
                    "SELECT TOP (@topNumber) Date, County, FIPS, Cases " +
                    "FROM dbo.cases5 " +
                    $"WHERE State = 'Washington' AND Date='{getDate}' " +
                    "ORDER BY Cases DESC ";
            return HotCountiesQuery(queryString, n);
        }

        static List<County> getHotCounties(String startDate, String endDate, int n = 10)
        {
            startDate = checkDateString(startDate);
            endDate = checkDateString(endDate);
            string queryString =
                "WITH temp as " +
                "(SELECT * FROM dbo.cases5 " +
                $"WHERE (Date BETWEEN '{startDate}' AND '{endDate}') AND State = 'Washington') " +
                "SELECT TOP 5" +
                "MIN(Date) AS StartDate, " +
                "County, " +
                "AVG(FIPS), " +
                "MAX(Cases) as Cases, " +
                "MAX(Cases)-MIN(Cases) as Delta, " +
                "MAX(Date) AS LastDate " +
                "FROM temp " +
                "GROUP BY County " +
                "ORDER BY Delta DESC";
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
                "order by County, Date";

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
                        if ((int)reader[4]-temp>=delta0)
                        {
                            Console.WriteLine($"The first county where delta cases per capita is greater than {delta0} is {(string)reader[1]}, Washington");
                            return new County((string)reader[1], (int)reader[3]);
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

        static County TimeQuery(String countyName)
        {
            //Display county info including timeseries data for daterange/all time
            //return a county object with the entire timeserie of cases for use by other code(e.g.analysis engine).
            //County info includes population so that per - capita
            string queryString =
                "SELECT [Date] " +
                ",[County] " +
                ",[State] " +
                ",[FIPS] " +
                ",[Cases] " +
                ",[Deaths] " +
                "FROM[H19].[dbo].[cases5] " +
                $"WHERE State = 'Washington' AND County = '{countyName}'";
            return TimeQueryHelper(countyName, queryString);

        }

        static County TimeQuery(String countyName, String startDate, String endDate)
        {
            //Display county info including timeseries data for daterange/all time
            //return a county object with the entire timeserie of cases for use by other code(e.g.analysis engine).
            //County info includes population so that per - capita
            startDate = checkDateString(startDate);
            endDate = checkDateString(endDate);
            string queryString =
                "SELECT [Date] " +
                ",[County] " +
                ",[State] " +
                ",[FIPS] " +
                ",[Cases] " +
                ",[Deaths] " +
                "FROM[H19].[dbo].[cases5] " +
                $"WHERE State = 'Washington' AND County = '{ countyName }' AND " +
                $"Date BETWEEN '{startDate}' AND '{endDate}'";
            return TimeQueryHelper(countyName, queryString);
        }

        static County TimeQueryHelper(String countyName, String queryString)
        {
            string connectionString = "Data Source=(local);Initial Catalog=H19;"
            + "Integrated Security=true";

            County temp = new County();
            // Create and open the connection in a using block.
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);
                // Open the connection in a try/catch block.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (temp.name==null)
                        {
                            temp.name = (String)reader[1];
                            temp.population = (int)reader[3];
                        }
                        temp.Samples.Add(new CaseDatum(Convert.ToDateTime(reader[0]), (int)reader[4]));
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine($"{countyName} Time Series");
            foreach (CaseDatum element in temp.Samples)
            {
                Console.WriteLine($"Date: {element.when.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))} Cases: {element.cases}");
            }
            return temp;
        }

        

        static void Main(string[] args)
        {
            Console.WriteLine("Display hot counties on the last date");
            getHotCounties();

            Console.WriteLine("\nDisplay hot counties on a date, 2020/01/27");
            getHotCounties("2020/01/27");

            Console.WriteLine("\nDisplay hot counties within date range, 2020/01/27-2020/02/18");
            getHotCounties("2020/01/27", "2020/02/18");

            Console.WriteLine("\nReturn first county where delta cases per capita is greater than a given number");
            DeltaQuery(3);

            Console.WriteLine("\nDisplay Snohomish county info including time series data for all time");
            TimeQuery("Snohomish");

            Console.WriteLine("\nDisplay Snohomish county info including time series data for daterange, 2020 / 01 / 27-2020 / 02 / 18");
            TimeQuery("Snohomish", "2020/01/27", "2020/02/18");


        }
    }
}
