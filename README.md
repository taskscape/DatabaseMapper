DatabaseMapper
==============

Maps stored procedure to any object, with no use of relection
Field names of returned columns and property names of classes have to be identical.

usage:
```
long postID = 123;
List<DbParameter> parameters = new List<DbParameter>(); 
parameters.Add(new DbParameter("postID", System.Data.ParameterDirection.Input, postID));
List<Category> categories = DatabaseMapper.ExecuteList<Category>("GetCategories", parameters);
```
