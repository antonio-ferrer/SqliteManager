using System;
using System.Collections.Generic;
using System.Data;

namespace CommonAccessDataObjectHelper
{
    public interface ICommonDbManager
    {
        void ExecuteCommand(StringSqlCommand stringSqlCommand);
        DataTable GetData(StringSqlCommand stringSqlCommand);
        IEnumerable<T> GetEntity<T>(StringSqlCommand stringSqlCommand, Func<IDataRecord, T> parse);
        ICommonDbManager Initialize(params KeyValuePair<string, string>[] config);
    }
}
