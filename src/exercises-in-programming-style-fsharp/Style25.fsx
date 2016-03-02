#r "../../packages/SQLProvider/lib/FSharp.Data.SqlProvider.dll"

open FSharp.Data.Sql
open System.IO

#time

let ``stop words`` = __SOURCE_DIRECTORY__ + "../stop_words.txt"
let ``p & p`` = __SOURCE_DIRECTORY__ + "../pride-and-prejudice.txt"

let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + @"..\..\files\sqlite" 
let [<Literal>] connectionString = "Data Source=" + __SOURCE_DIRECTORY__ + @"\northwindEF.db;Version=3"
// create a type alias with the connection string and database vendor settings
type sql = SqlDataProvider< 
              ConnectionString = connectionString,
              DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
              ResolutionPath = resolutionPath,
              IndividualsAmount = 1000,
              UseOptionTypes = true >
let ctx = sql.GetDataContext()

let createDbSchema (conn : SQLiteConnection) =
    let query = """
CREATE TABLE documents (id INTEGER PRIMARY KEY AUTOINCREMENT, name);
CREATE TABLE words (id, doc_id, value);
CREATE TABLE characters (id, word_id, value);
"""
    use cmd = new SQLiteCommand(query, conn)
    cmd.ExecuteNonQuery()

SQLiteConnection.CreateFile("p&p.sqlite")
let conn = new SQLiteConnection("Data Source=p&p.sqlite")

