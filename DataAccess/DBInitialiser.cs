using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public static class DBInitialiser
    {
        public static void Initialise(DataExtractionContext context)
        {
            context.Database.EnsureCreated();
            context.Database.Migrate();

            // Seed data if necessary

        }
    }
}
