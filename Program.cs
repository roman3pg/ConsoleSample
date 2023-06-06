using System.Data;
using Microsoft.Data.SqlClient;

namespace sqltest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {            
                int userInput = 0;
                do
                {
                    userInput = DisplayMenu();

                    switch (userInput)
                    {
                        case 1:
                            ConnectionTest();
                            break;
                        case 2:
                            CreateDatabase(); 
                            break;
                        case 3:
                            CreateMemberCase();
                            break;
                        case 4:
                            UpdateMemberCase();
                            break;
                        case 5:
                            DeleteMemberCase();
                            break;
                        case 6:
                            SelectMemberCase();
                            break;
                        default:
                        break;
                    }
                    
                }
                while(userInput!=7);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void CreateMemberCase()
        {
            Console.WriteLine("\nPlease, enter the name and started year using comma as a delimiter then press enter.");
            string input = Console.ReadLine();
            string[] split = input.Split(',');
            string name = split[0];
            int year = Convert.ToInt32(split[1]);
            CreateMember(name, year);
        }

        private static void UpdateMemberCase()
        {
            Console.WriteLine("\nPlease, enter the ID, name and started year using comma as a delimiter then press enter.");
            string input = Console.ReadLine();
            string[] split = input.Split(',');
            int memberId = Convert.ToInt32(split[0]);
            string name = split[1];
            int year = Convert.ToInt32(split[2]);
            UpdateMember(memberId, name, year);
        }

        private static void DeleteMemberCase()
        {
            Console.WriteLine("\nPlease, enter the ID and then press enter.");
            int memberId = Convert.ToInt32(Console.ReadLine());
            DeleteMember(memberId);
        }

        private static void SelectMemberCase()
        {
            Console.WriteLine("\nPlease, enter the ID and then press enter.");
            int memberId = Convert.ToInt32(Console.ReadLine());
            SelectMember(memberId);
        }

        private static int DisplayMenu()
        {   
            Console.WriteLine("\nADO .Net Sample");
            Console.WriteLine();
            Console.WriteLine("1. TEST Connection");
            Console.WriteLine("2. CREATE DATABASE");
            Console.WriteLine("3. CREATE member team");
            Console.WriteLine("4. UPDATE member team");
            Console.WriteLine("5. DELETE member team");
            Console.WriteLine("6. SELECT member team");
            Console.WriteLine("7. Exit");

            var result = Convert.ToInt32(Console.ReadLine());

            return result;
        }

        private static void ConnectionTest()
        {
            // SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            // builder.DataSource = "localhost,1433";
            // builder.UserID = "sa";
            // builder.Password = "P@ssw0rd";
            // builder.InitialCatalog = "master";
            // builder.Encrypt = false;

            using (SqlConnection connection = new SqlConnection(GetStringConnection()))
            {
                Console.WriteLine("\nQuery data example:");
                Console.WriteLine("=========================================\n");

                connection.Open();

                String sql = "SELECT name, collation_name FROM sys.databases";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                        }
                    }
                }
            }
        }

        private static void CreateDatabase()
        {

            String sqlDatabase = @" --Check if the database exists
                            IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'ApplicationDB')
                            BEGIN
                                CREATE DATABASE ApplicationDB;
                            END
                            USE ApplicationDB;
                            --Check if the table exists
                            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Members' and xtype='U')
                            BEGIN                                
                                CREATE TABLE Members (
                                 Id INT PRIMARY KEY IDENTITY (1, 1),
                                 Name VARCHAR(100),
                                 StartYear SMALLINT
                                )
                            END";

            string spNewMember = @"DECLARE @SqlCommand nvarchar(max) = '
                                    CREATE OR ALTER PROCEDURE [dbo].[uspNewMember]  
                                                                @MemberName NVARCHAR (40),  
                                                                @StartYear SMALLINT,
                                                                @MemberID INT OUTPUT  
                                                                AS  
                                                                BEGIN  
                                                                INSERT INTO [dbo].[Members] (Name, StartYear) VALUES (@MemberName, @StartYear);  
                                                                SET @MemberID = SCOPE_IDENTITY();  
                                                                RETURN @@ERROR  
                                                                END';

                                    DECLARE @spexecute nvarchar(1000) = QUOTENAME('ApplicationDB') + '.sys.sp_executesql';

                                    EXEC @spexecute @SqlCommand;
                                    ";
            string spUpdateMember = @"DECLARE @SqlCommand nvarchar(max) = '
                                    CREATE OR ALTER PROCEDURE [dbo].[uspUpdateMember]  
                                    @MemberID INT, @MemberName NVARCHAR (40),  
                                    @StartYear SMALLINT  
                                    AS  
                                    BEGIN  

                                    BEGIN TRANSACTION  
                                    UPDATE [dbo].[Members]  
                                    SET  
                                    Name = @MemberName,
                                    StartYear = @StartYear
                                        WHERE [Id] = @MemberID  
                                    COMMIT TRANSACTION  
                                    END ';

                                    DECLARE @spexecute nvarchar(1000) = QUOTENAME('ApplicationDB') + '.sys.sp_executesql';

                                    EXEC @spexecute @SqlCommand;
                                    ";
            
            string spDeleteMember = @"DECLARE @SqlCommand nvarchar(max) = '
                                    CREATE OR ALTER PROCEDURE [dbo].[uspDeleteMember]  
                                    @MemberID INT
                                    AS  
                                    BEGIN  

                                    DELETE FROM [dbo].[Members] WHERE Id = @MemberID
                                    END ';

                                    DECLARE @spexecute nvarchar(1000) = QUOTENAME('ApplicationDB') + '.sys.sp_executesql';

                                    EXEC @spexecute @SqlCommand;
                                    ";

            string spSelectMember = @"DECLARE @SqlCommand nvarchar(max) = '
                                    CREATE OR ALTER PROCEDURE [dbo].[uspSelectMember]  
                                    @MemberID INT
                                    AS  
                                    BEGIN  

                                    SELECT * FROM [dbo].[Members] WHERE Id = @MemberID
                                    END ';

                                    DECLARE @spexecute nvarchar(1000) = QUOTENAME('ApplicationDB') + '.sys.sp_executesql';

                                    EXEC @spexecute @SqlCommand;
                                    ";

            using (SqlConnection connection = new SqlConnection(GetStringConnection()))
            {
                Console.WriteLine("\nCreating DB and Table...");
                //Console.WriteLine("=========================================\n");

                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sqlDatabase, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        Console.WriteLine("Database and Table created...");
                    }

                    using (SqlCommand command = new SqlCommand(spNewMember, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        Console.WriteLine("SP new member created...");
                    }

                    using (SqlCommand command = new SqlCommand(spUpdateMember, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        Console.WriteLine("SP update member created...");
                    } 

                    using (SqlCommand command = new SqlCommand(spDeleteMember, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        Console.WriteLine("SP delete member created...");
                    }    

                    using (SqlCommand command = new SqlCommand(spSelectMember, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        Console.WriteLine("SP select member created...");
                    }                    
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString(), "Error");
                }
                finally
                {
                    connection.Close();
                }
            }
            //Console.WriteLine("DataBase is Created Successfully");

        }

        private static void CreateMember(string memberName, int startYear)
        {
            Console.WriteLine("parameters: {0}, {1}", memberName, startYear);
            using (SqlConnection connection = new SqlConnection(GetStringConnection()))
            {
                // Create a SqlCommand, and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand("ApplicationDB.dbo.uspNewMember", connection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    // Add input parameter for the stored procedure and specify what to use as its value.
                    sqlCommand.Parameters.Add(new SqlParameter("@MemberName", SqlDbType.NVarChar, 40));
                    sqlCommand.Parameters["@MemberName"].Value = memberName;

                    // Add the output parameter.
                    sqlCommand.Parameters.Add(new SqlParameter("@StartYear", SqlDbType.SmallInt));
                    sqlCommand.Parameters["@StartYear"].Value = startYear;

                    // Add the output parameter.
                    sqlCommand.Parameters.Add(new SqlParameter("@MemberID", SqlDbType.Int));
                    sqlCommand.Parameters["@MemberID"].Direction = ParameterDirection.Output;

                    try
                    {
                        connection.Open();

                        // Run the stored procedure.
                        sqlCommand.ExecuteNonQuery();

                        // Member ID IDENTITY value from the database.
                        int memberID = (int)sqlCommand.Parameters["@MemberID"].Value;

                        Console.WriteLine("Member with ID: {0} created!", memberID);
                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString(), "Error");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        
        private static void UpdateMember(int memberId, string memberName, int startYear)
        {
            Console.WriteLine("parameters: {0}, {1}, {2}", memberId, memberName, startYear);
            using (SqlConnection connection = new SqlConnection(GetStringConnection()))
            {
                // Create a SqlCommand, and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand("ApplicationDB.dbo.uspUpdateMember", connection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    // Add input parameters for the stored procedure and specify what to use as its value.
                    sqlCommand.Parameters.Add(new SqlParameter("@MemberID", SqlDbType.Int));
                    sqlCommand.Parameters["@MemberID"].Value = memberId;

                    sqlCommand.Parameters.Add(new SqlParameter("@MemberName", SqlDbType.NVarChar, 40));
                    sqlCommand.Parameters["@MemberName"].Value = memberName;

                    sqlCommand.Parameters.Add(new SqlParameter("@StartYear", SqlDbType.SmallInt));
                    sqlCommand.Parameters["@StartYear"].Value = startYear;                    
                   
                    try
                    {
                        connection.Open();

                        // Run the stored procedure.
                        var result = sqlCommand.ExecuteNonQuery();
                        Console.WriteLine("Changes applied to member ID: {0}!", memberId);
                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString(), "Error");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private static void DeleteMember(int memberId)
        {
            Console.WriteLine("parameters: {0}", memberId);
            using (SqlConnection connection = new SqlConnection(GetStringConnection()))
            {
                // Create a SqlCommand, and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand("ApplicationDB.dbo.uspDeleteMember", connection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    // Add input parameters for the stored procedure and specify what to use as its value.
                    sqlCommand.Parameters.Add(new SqlParameter("@MemberID", SqlDbType.Int));
                    sqlCommand.Parameters["@MemberID"].Value = memberId;               
                   
                    try
                    {
                        connection.Open();

                        // Run the stored procedure.
                        var result = sqlCommand.ExecuteNonQuery();
                        Console.WriteLine("Member ID: {0} deleted!", memberId);
                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString(), "Error");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private static void SelectMember(int memberId)
        {
            Console.WriteLine("parameters: {0}", memberId);
            using (SqlConnection connection = new SqlConnection(GetStringConnection()))
            {
                // Create a SqlCommand, and identify it as a stored procedure.
                using (SqlCommand sqlCommand = new SqlCommand("ApplicationDB.dbo.uspSelectMember", connection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    // Add input parameters for the stored procedure and specify what to use as its value.
                    sqlCommand.Parameters.Add(new SqlParameter("@MemberID", SqlDbType.Int));
                    sqlCommand.Parameters["@MemberID"].Value = memberId;               
                   
                    try
                    {
                        connection.Open();

                        using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                        {
                            // Create a data table to hold the retrieved data.
                            DataTable dataTable = new DataTable();

                            // Load the data from SqlDataReader into the data table.
                            dataTable.Load(dataReader);                            

                            // Close the SqlDataReader.
                            dataReader.Close();
                            
                            Console.WriteLine("Member with ID: {0} belongs to {1} and starts on {2}!", 
                            dataTable.Rows[0].ItemArray[0],
                            dataTable.Rows[0].ItemArray[1],
                            dataTable.Rows[0].ItemArray[2]);
                        }                                                
                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString(), "Error");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private static string GetStringConnection()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = "localhost,1433";
            builder.UserID = "sa";
            builder.Password = "P@ssw0rd";
            builder.InitialCatalog = "master";
            builder.Encrypt = false;

            return builder.ConnectionString;
        }
    }
}