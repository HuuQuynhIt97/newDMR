using dmr_api.Data.Interface;
using dmr_api.Models;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DMR_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<GlueIngredient> GlueIngredient { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<GlueType> GlueTypes { get; set; }

        public DbSet<Glue> Glues { get; set; }
        public DbSet<Supplier> Supplier { get; set; }

        public DbSet<Line> Line { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Stir> Stirs { get; set; }
        public DbSet<ModelName> ModelNames { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanDetail> PlanDetails { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<ArticleNo> ArticleNos { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<BuildingUser> BuildingUser { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<ArtProcess> ArtProcesses { get; set; }
        public DbSet<ModelNo> ModelNos { get; set; }
        public DbSet<BPFCEstablish> BPFCEstablishes { get; set; }

        public DbSet<Kind> Kinds { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MixingInfo> MixingInfos { get; set; }
        public DbSet<MixingInfoDetail> MixingInfoDetails { get; set; }
        public DbSet<IngredientInfo> IngredientsInfos { get; set; }
        public DbSet<IngredientInfoReport> IngredientInfoReports { get; set; }
        public DbSet<BPFCHistory> BPFCHistories { get; set; }
        public DbSet<Abnormal> Abnormals { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ScaleMachine> ScaleMachines { get; set; }
        public DbSet<LunchTime> LunchTime { get; set; }
        public DbSet<Dispatch> Dispatches { get; set; }
        public DbSet<ToDoList> ToDoList { get; set; }
        public DbSet<GlueName> GlueName { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Mailing> Mailings { get; set; }
        public DbSet<PeriodDispatch> PeriodDispatch { get; set; }
        public DbSet<PeriodMixing> PeriodMixing { get; set; }
        public DbSet<Subpackage> Subpackages { get; set; }
        public DbSet<DispatchList> DispatchList { get; set; }
        public DbSet<DispatchListDetail> DispatchListDetail { get; set; }
        public DbSet<Shake> Shakes { get; set; }
        public DbSet<BuildingType> BuildingType { get; set; }
        public DbSet<KindType> KindType { get; set; }

        public DbSet<Models.Permission> Permisions { get; set; }

        public DbSet<Models.Action> Actions { get; set; }
        public DbSet<Models.Version> Versions { get; set; }
        public DbSet<Models.Module> Modules { get; set; }
        public DbSet<Models.StirRawData> StirRawData { get; set; }

        public DbSet<ActionInFunctionSystem> ActionInFunctionSystem { get; set; }

        public DbSet<FunctionSystem> FunctionSystem { get; set; }
        public DbSet<SubpackageCapacity> SubpackageCapacity { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ct = DateTime.Now;
            modelBuilder.Entity<BPFCEstablish>().HasKey(x => x.ID);

            modelBuilder.Entity<Permission>()
            .HasKey(a => new { a.ActionID, a.FunctionSystemID, a.RoleID });

            modelBuilder.Entity<ActionInFunctionSystem>()
            .HasKey(a => new { a.ActionID, a.FunctionSystemID });
            //var periodList = new List<Period>()
            //{
            //    {new Period
            //    {
            //        LunchTimeID = 7,
            //        Sequence = 1,
            //        StartTime = new DateTime(ct.Year,ct.Month,ct.Day, 7, 00,00),
            //        EndTime = new DateTime(ct.Year,ct.Month,ct.Day, 9, 00,00),

            //    }},
            //     {new Period
            //    {
            //          LunchTimeID = 7,
            //        Sequence = 2,
            //        StartTime = new DateTime(ct.Year,ct.Month,ct.Day, 9, 00,00),
            //        EndTime = new DateTime(ct.Year,ct.Month,ct.Day, 11, 00,00),

            //    }},
            //      {new Period
            //    {
            //           LunchTimeID = 7,
            //        Sequence = 3,
            //        StartTime = new DateTime(ct.Year,ct.Month,ct.Day, 11, 00,00),
            //        EndTime = new DateTime(ct.Year,ct.Month,ct.Day, 12, 30,00),

            //    }},
            //       {new Period
            //    {
            //            LunchTimeID = 7,
            //        Sequence = 4,
            //        StartTime = new DateTime(ct.Year,ct.Month,ct.Day, 13, 30,00),
            //        EndTime = new DateTime(ct.Year,ct.Month,ct.Day,15, 30,00),

            //    }},
            //        {new Period
            //    {
            //             LunchTimeID = 7,
            //        Sequence = 5,
            //        StartTime = new DateTime(ct.Year,ct.Month,ct.Day, 15, 30,00),
            //        EndTime = new DateTime(ct.Year,ct.Month,ct.Day, 16, 30,00),

            //    }}
            //};
            //modelBuilder.Entity<Period>().HasData(periodList);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EntityEntry> modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (EntityEntry item in modified)
            {
                if (item.Entity is IDateTracking changedOrAddedItem)
                {
                    if (item.State == EntityState.Added)
                    {
                        changedOrAddedItem.CreatedTime= DateTime.Now;
                    }
                    else
                    {
                        changedOrAddedItem.UpdatedTime = DateTime.Now;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}