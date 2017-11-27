using foip.CLI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace foip
{
    public class ResultList : List<Result>
    {
        private string orderBy;

        public ResultList(string OrderBy)
        {
            orderBy = OrderBy;
        }

        public IEnumerable<Result> OrderAsRequested()
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return this;
            }

            string[] orderStatementsRaw = orderBy.Split(',');

            var orderStatements = orderStatementsRaw
                .Select(o => o.Trim())
                .Select(o =>
            {
                string[] tokens = o.Split(' ');
                string field = tokens[0];
                ListSortDirection direction = ListSortDirection.Ascending;
                if (tokens.Length > 1)
                {
                    if (tokens[1].Equals("desc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        direction = ListSortDirection.Descending;
                    }
                }

                return new
                {
                    Field = field,
                    Direction = direction
                };
            })
            .ToList();


            Object sortedList = this;
            orderStatements
                .ForEach(os =>
                {
                    //TODO: Think of a nicer way of doing this

                    if (os.Field.Equals("{" + Fields.IP.ToString() + "}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortedList = Extensions.PerformSort<Result, UInt32>(sortedList, result => result.Endpoint.Address.ToUInt32(), os.Direction);
                    }

                    if (os.Field.Equals("{" + Fields.Port.ToString() + "}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortedList = Extensions.PerformSort<Result, int>(sortedList, result => result.Endpoint.Port, os.Direction);
                    }

                    if (os.Field.Equals("{" + Fields.Date.ToString() + "}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortedList = Extensions.PerformSort<Result, DateTime>(sortedList, result => result.Date, os.Direction);
                    }

                    if (os.Field.Equals("{" + Fields.FQDN.ToString() + "}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortedList = Extensions.PerformSort<Result, string>(sortedList, result => result.FQDN, os.Direction);
                    }

                    if (os.Field.Equals("{" + Fields.Hostname.ToString() + "}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortedList = Extensions.PerformSort<Result, string>(sortedList, result => result.Hostname, os.Direction);
                    }

                    if (os.Field.Equals("{" + Fields.Scheme.ToString() + "}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sortedList = Extensions.PerformSort<Result, string>(sortedList, result => result.Scheme, os.Direction);
                    }
                });

            var resultList = sortedList as IEnumerable<Result>;
            return resultList;
        }
    }
}
