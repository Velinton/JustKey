using System;
using System.Data.SqlClient;
using System.Data;

namespace JustKey.Classes
{
    internal class WorkWithConnection
    {
        //строки подключения для домашнего сервера
        private const string CONNECTION_STRING = @"Server=DESKTOP-5PTDVO6\SQLEXPRESS;Database=JustKey;Trusted_Connection=True;";
        //private const string CONNECTION_STRING = @"Server=SUPER-HOMCKA\SQLEXPRESS;Database=JustKey;Trusted_Connection=True;";
        //private const string CONNECTION_STRING = @"Server=DESKTOP-5PTDVO6\SQLEXPRESS; Initial Catalog=JustKey; Persist Security Info=False;User ID=DIPLOM;Password=Aa12345";

        //Строка подключения для учебного сервера
        //private const string CONNECTION_STRING = @"Server=310-16\SQLEXPRESS; Initial Catalog=JustKey; Integrated Security=True";

        //Строка подключения для Дипломного сервера
        //private const string CONNECTION_STRING = @"Server=311-SERVER\Demoex; Persist Security Info=False;User ID=DIPLOM;Password=Aa12345;Initial Catalog=JustKey";
        //ЕСЛИ НЕ РАБОТАЕТ СТРОКА ПОДЛКЮЧЕНИЯ, ТО ИЗМЕНИ "Demoex" НА "DEMOEX"

        private readonly SqlConnection _connection;
        private readonly SqlCommand _sqlCommand;

        /// <summary>
        /// Получить последний выполненный запрос к базе данных
        /// </summary>
        public SqlCommand GetCommand { get { return _sqlCommand; } }

        public WorkWithConnection()
        {
            _connection = new SqlConnection(CONNECTION_STRING);
            _sqlCommand = new SqlCommand
            {
                Connection = _connection
            };
        }
        /// <summary>
        /// Соединяет с базой данных
        /// </summary>
        public bool Connect()
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    return true;
                }
                else
                {
                    _connection.Close();
                    _connection.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Разрывает соединение с базой данных
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
        /// <summary>
        /// Новый запрос к базе данных. В качестве аргументов: request - сам запрос к базе данных, returnValue - вернуть значение ячейки
        /// </summary>
        public object NewCommand(string request, bool returnValue)
        {
            _sqlCommand.CommandText = request;

            if (returnValue)
                return _sqlCommand.ExecuteScalar();
            else
                return _sqlCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Авторизация пользователя. login - логин пользователя, password - пароль пользователя.
        /// Возвращает ID должности сотрудника, если сотрудник с таким логином и паролем существует. Иначе -1
        /// </summary>
        public int AutorisateUser(string login, string password)
        {
            //Получение должности сотрудника для входа в аккаунт
            _sqlCommand.CommandText = $"SELECT ID_position FROM Employees WHERE ID = (SELECT ID_employee FROM Credentials WHERE Login = '{login}' AND Password = '{password}')";

            if (_sqlCommand.ExecuteScalar() != null)
                return (int)_sqlCommand.ExecuteScalar();
            else
                return -1;
        }

    }
}
