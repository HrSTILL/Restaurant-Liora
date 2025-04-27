using Restaurant_Manager.Utils;
using System;
using System.Collections.Generic;

namespace Restaurant_Manager.Models.ViewModels
{
    public class AdminIncomeViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal IncomeToday { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public DailyIncome HighestIncomeDay { get; set; }
        public PaginatedList<DailyIncome> DailyIncomes { get; set; }

        public List<DailyIncome> Last10DaysIncome { get; set; }   
        public List<int> AvailableYears { get; set; }            
        public int SelectedYear { get; set; }                    
        public List<MonthlyIncome> MonthlyIncomes { get; set; }    
    }

    public class DailyIncome
    {
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
    }

    public class MonthlyIncome
    {
        public int Month { get; set; }         // Example: 1=Jan, 2=Feb, 3=Mar
        public decimal TotalIncome { get; set; }
    }
}
