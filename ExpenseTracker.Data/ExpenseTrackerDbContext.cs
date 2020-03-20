using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data
{
    public class ExpenseTrackerDbContext : IdentityDbContext<AppUser>
    {
        public ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Expense>()
                .Property(x => x.CostOfExpense)
                .HasColumnType("decimal(13,4)");
            modelBuilder.Entity<Budget>()
               .Property(x => x.Amount)
               .HasColumnType("decimal(13,4)");
            modelBuilder.Entity<Income>()
               .Property(x => x.Amount)
               .HasColumnType("decimal(13,4)");
            modelBuilder.Entity<BankTransaction>()
               .Property(x => x.TransactionAmount)
               .HasColumnType("decimal(13,4)");
        }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<IncomeCategory> IncomeCategories { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<AdminExpenseCategory> AdminExpenseCategories { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<GoogleAuth> GoogleAuths { get; set; }

    }
}
