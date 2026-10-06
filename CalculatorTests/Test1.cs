using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using AnalaizerClassLibrary;

namespace CalculatorTests
{
    [TestClass]
    [DoNotParallelize] // Забороняє паралельний запуск, захищаючи статичне поле
    public class FormatTests
    {
        private static readonly object _syncLock = new object();

        public static IEnumerable<object[]> GetTestDataFromDb()
        {
            string connectionString = "Server=localhost;Database=CalculatorTestsDB;Trusted_Connection=True;TrustServerCertificate=True;";
            var testCases = new List<object[]>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT InputExpression, ExpectedExpression FROM FormatTestData";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string input = reader["InputExpression"]?.ToString() ?? string.Empty;
                        string expected = reader["ExpectedExpression"]?.ToString() ?? string.Empty;
                        testCases.Add(new object[] { input, expected });
                    }
                }
            }

            return testCases;
        }

        [TestMethod]
        [DynamicData(nameof(GetTestDataFromDb), DynamicDataSourceType.Method)]
        public void Format_ExpressionsFromDatabase_ReturnsExpectedResult(string input, string expected)
        {
            lock (_syncLock)
            {
                // Записуємо вхідний вираз у статичне поле
                AnalaizerClass.expression = input;

                // Викликаємо метод Format
                string actual = AnalaizerClass.Format();

                // Звіряємо результат
                Assert.AreEqual(expected, actual, $"Помилка для виразу: '{input}'");
            }
        }
    }
}