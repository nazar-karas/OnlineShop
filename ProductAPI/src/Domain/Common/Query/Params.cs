using Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Query
{
    public class Params
    {
        int page;
        int number;
        public int Page
        {
            set
            {
                if (value > 0)
                {
                    page = value;
                }
                else
                {
                    page = 1;
                }
            }
            get
            {
                if (page > 0)
                {
                    return page;
                }
                else
                {
                    page = 1;
                    return page;
                }
            }
        }
        public int Number
        {
            set
            {
                if (value > 0)
                {
                    number = value;
                }
                else
                {
                    number = 5;
                }
            }
            get
            {
                if (number > 0)
                {
                    return number;
                }
                else
                {
                    number = 5;
                    return number;
                }
            }
        }
        public string? SortBy { get; set; }
        public SortingType SortType { get; set; } = SortingType.Ascending;
    }
}
