using GASH.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace GASH
{
    public static class Db
    {

        //static string constr = "server = sql11.freemysqlhosting.net; uid = sql11682148; pwd = PCfugnr5sC; database = sql11682148";
        static string constr = "server=10.10.1.24;uid=user_01;pwd=user01pro;database=pro1_6";

        static Db()
        {
        }

        public static MySqlConnection CreateConnection()
        {
            return new MySqlConnection(constr);
        }

        #region Login

        public  static string GetAccountfio(string login, string password)
        {
            MySqlConnection connection = CreateConnection();

            string fio = null;

             connection.Open();

            MySqlCommand command = new MySqlCommand("select * from Account where login = @l and password = @p", connection);
            command.Parameters.AddWithValue("@l", login);
            command.Parameters.AddWithValue("@p", password);

            MySqlDataReader reader = command.ExecuteReader();

            while ( reader.Read())
            {
                fio = reader.GetString(4);
            }

            connection.Close();

            return fio;
        }
        
        public async static Task<bool> CheckLogin(string login, string password)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("select * from Account where login = @l and password = @p", connection);
            command.Parameters.AddWithValue("@l", login);
            command.Parameters.AddWithValue("@p", password);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (reader.HasRows)
                {
                    connection.Close();

                    return true;
                }

                connection.Close();

                return false;
            }

            await connection.CloseAsync();

            return false;
        }
        #endregion

        #region Good

        public async static Task<List<Good>> GetAllGood()
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            DataTable dt = new DataTable();
            List<Good> goods = new List<Good>();
            MySqlCommand command = new MySqlCommand("select * from Good", connection);
            MySqlDataAdapter reader = new MySqlDataAdapter(command);
            await reader.FillAsync(dt);
            foreach (DataRow item in dt.Rows)
            {
                Good good = new Good();
                good.id = item.Field<int>("id");
                good.imgPath = item.Field<string>("image");
                good.name = item.Field<string>("name");
                good.description = item.Field<string>("description");
                good.price = item.Field<float>("price");
                good.count = item.Field<int>("count");
                good.unit = await GetUnitAt(item.Field<int>("unit"));
                good.manufacturer = await GetManufacturerAt(item.Field<int>("manufacturer"));
                good.category = await GetCategoryAt(item.Field<int>("category"));

                goods.Add(good);
            }

            await connection.CloseAsync();

            return goods;
        }

        public async static Task AddGood(Good c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("insert into Good (name, image, description, manufacturer, category, price, count, unit) values (@n, @im, @d, @m, @ca, @p, @co, @u)", connection);
            command.Parameters.AddWithValue("@n", c.name);
            command.Parameters.AddWithValue("@im", c.imgPath);
            command.Parameters.AddWithValue("@d", c.description);
            command.Parameters.AddWithValue("@m", c.manufacturer.id);
            command.Parameters.AddWithValue("@ca", c.category.id);
            command.Parameters.AddWithValue("@p", c.price);
            command.Parameters.AddWithValue("@co", c.count);
            command.Parameters.AddWithValue("@u", c.unit.id);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task ChangeGood(Good c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("update Good set name = @n, image = @im, description = @d, manufacturer = @m, category = @ca, price = @p, count = @co, unit = @u where id = @i", connection);
            command.Parameters.AddWithValue("@i", c.id);
            command.Parameters.AddWithValue("@im", c.imgPath);
            command.Parameters.AddWithValue("@n", c.name);
            command.Parameters.AddWithValue("@d", c.description);
            command.Parameters.AddWithValue("@m", c.manufacturer.id);
            command.Parameters.AddWithValue("@ca", c.category.id);
            command.Parameters.AddWithValue("@p", c.price);
            command.Parameters.AddWithValue("@co", c.count);
            command.Parameters.AddWithValue("@u", c.unit.id);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task DeleteGood(Good c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("delete from Good where id = @i", connection);
            command.Parameters.AddWithValue("@i", c.id);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        #endregion

        #region Manufacturer

        public async static Task<List<Manufacturer>> GetAllManufacturer()
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            List<Manufacturer> ms = new List<Manufacturer>();

            MySqlCommand command = new MySqlCommand("Select * from Manufacturer", connection);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ms.Add(new Manufacturer(reader.GetInt32(0), reader.GetString(1)));
            }

            await connection.CloseAsync();

            return ms;
        }

        public async static Task<Manufacturer> GetManufacturerAt(int id)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("select * from Manufacturer where id = @i", connection);
            command.Parameters.AddWithValue("@i", id);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Manufacturer man = new Manufacturer(reader.GetInt32(0), reader.GetString(1));

            await connection.CloseAsync();

            return man;
        }

        public async static Task AddManufacturer(Manufacturer m)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("insert into Manufacturer (name) values (@n)", connection);
            command.Parameters.AddWithValue("@n", m.name);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task ChangeManufacturer(Manufacturer m)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("update Manufacturer set name = @n where id = @i", connection);
            command.Parameters.AddWithValue("@i", m.id);
            command.Parameters.AddWithValue("@n", m.name);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task DeleteManufacturer(Manufacturer m)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("delete from Manufacturer where id = @i", connection);
            command.Parameters.AddWithValue("@i", m.id);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        #endregion

        #region Category

        public async static Task<List<Category>> GetAllCategory()
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            List<Category> cs = new List<Category>();

            MySqlCommand command = new MySqlCommand("Select * from Category", connection);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cs.Add(new Category(reader.GetInt32(0), reader.GetString(1)));
            }

            await connection.CloseAsync();

            return cs;
        }

        public async static Task<Category> GetCategoryAt(int id)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("select * from Category where id = @i", connection);
            command.Parameters.AddWithValue("@i", id);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Category cat = new Category(reader.GetInt32(0), reader.GetString(1));

            await connection.CloseAsync();

            return cat;
        }

        public async static Task AddCategory(Category c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("insert into Category (name) values (@n)", connection);
            command.Parameters.AddWithValue("@n", c.name);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task ChangeCategory(Category c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("update Category set name = @n where id = @i", connection);
            command.Parameters.AddWithValue("@i", c.id);
            command.Parameters.AddWithValue("@n", c.name);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task DeleteCategory(Category c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("delete from Category where id = @i", connection);
            command.Parameters.AddWithValue("@i", c.id);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        #endregion

        #region Unit

        public async static Task<List<Unit>> GetAllUnit()
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            List<Unit> us = new List<Unit>();

            MySqlCommand command = new MySqlCommand("Select * from Unit", connection);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                us.Add(new Unit(reader.GetInt32(0), reader.GetString(1)));
            }

            await connection.CloseAsync();

            return us;
        }

        public async static Task<Unit> GetUnitAt(int id)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("select * from Unit where id = @i", connection);
            command.Parameters.AddWithValue("@i", id);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Unit unit = new Unit(reader.GetInt32(0), reader.GetString(1));         

            await connection.CloseAsync();

            return unit;
        }

        public async static Task AddUnit(Unit c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("insert into Unit (name) values (@n)", connection);
            command.Parameters.AddWithValue("@n", c.name);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task ChangeUnit(Unit c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("update Unit set name = @n where id = @i", connection);
            command.Parameters.AddWithValue("@i", c.id);
            command.Parameters.AddWithValue("@n", c.name);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async static Task DeleteUnit(Unit c)
        {
            MySqlConnection connection = CreateConnection();

            await connection.OpenAsync();

            MySqlCommand command = new MySqlCommand("delete from Unit where id = @i", connection);
            command.Parameters.AddWithValue("@i", c.id);

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        #endregion
    }
}
