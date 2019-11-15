// Source: https://stackoverflow.com/questions/1464883/how-can-i-easily-convert-datareader-to-listt

using (IDataReader reader = ...)
{
    List<Customer> customers = reader.Select(r => new Customer {
        CustomerId = r["id"] is DBNull ? null : r["id"].ToString(),
        CustomerName = r["name"] is DBNull ? null : r["name"].ToString() 
    }).ToList();
}