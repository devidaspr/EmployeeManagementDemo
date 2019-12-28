using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    //For an extension method the class has to be a static class
    public static class ModelBuilderExtensions
    {
        //we are creating extension method for ModelBuilder class
        //Use 'this' and ModelBuilder as first parameter for adding an extension method to ModelBuilder
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                    new Employee
                    {
                        Id = 1,
                        Name = "Mary",
                        Email = "Mary@email.com",
                        Department = Dept.IT
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "John",
                        Email = "John@email.com",
                        Department = Dept.HR
                    }
                );
        }
    }
}
