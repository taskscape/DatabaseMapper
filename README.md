# DatabaseMapper

DatabaseMapper is a lightweight .NET library that maps the results of SQL Server stored procedures to objects. No external mapping framework is involved: every returned column is assigned to the class property with the identical name.

## How It Works

1. Create a `DatabaseMapper` instance with a SQL Server connection string.
2. Call one of the `Execute*` methods with the stored procedure name and an optional list of parameters.
3. The stored procedure is executed through `System.Data.SqlClient`, and each returned column is copied to the matching property of the target class.
4. After the call, stored procedure output parameters are available in the `Parameters` collection.

## Features

- Maps the first result row to a single object (`ExecuteSingle<T>`).
- Maps all result rows to a `List<T>` (`ExecuteList<T>`).
- Executes non-query procedures and returns the number of affected rows (`ExecuteNonQuery`).
- Supports input and output parameters.
- `NULL` column values are skipped and leave the property at its default value.
- Single source file, only `System.Data.SqlClient` as the dependency.

## Requirements

- .NET Framework 4.x
- SQL Server database reachable through `System.Data.SqlClient`

## Usage

```csharp
var mapper = new DatabaseMapper(connectionString);

// get a list of objects
var categories = mapper.ExecuteList<Category>("GetCategories");

// pass parameters
var parameters = new List<DatabaseMapper.DbParameter>();
parameters.Add(new DatabaseMapper.DbParameter("postID", System.Data.ParameterDirection.Input, postID));
var posts = mapper.ExecuteList<Post>("GetPosts", parameters);

// get a single object
var post = mapper.ExecuteSingle<Post>("GetPost", parameters);

// execute an insert/update/delete procedure
var affected = mapper.ExecuteNonQuery("DeletePost", parameters);

// read output parameters after the call
var outputValue = mapper.Parameters.Single(p => p.Name == "@result").Value;
```

Each mapped class needs a parameterless constructor, and the field names of the returned columns have to be identical to the property names of the class:

```csharp
public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
}
```

## License

This project is licensed under the GNU General Public License v2.0 - see the [LICENSE](LICENSE) file for details.
