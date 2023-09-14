using Microsoft.EntityFrameworkCore;
using PTPL.FitOS.DataModels;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PTPL.FitOS.DataContext
{
    public class InsertUpdateDBContext : DbContext
    {
        /// <summary>
        /// This section is use to declare fields/properties of database tables which is used in this library module.
        /// </summary>
        /// <remarks>Code documentation and Review is pending</remarks>
        #region Fields

        public DbSet<FavouritesDTO> FavouritesDBSet { get; set; }
        public DbSet<ColoursDTO> ColoursDBSet { get; set; }               
        public DbSet<ColourDocumentDTO> ColourDocumentsDBSet { get; set; }		
        public DbSet<SequenceDTO> SequenceDBSet { get; set; }
        #endregion

        #region Constructors
        public InsertUpdateDBContext(DbContextOptions<InsertUpdateDBContext> options) : base(options)
        {
        }

        #endregion 

        #region Methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<ColoursDTO>().HasKey(c => new { c.ID });            
            modelBuilder.Entity<FavouritesDTO>().HasKey(c => new { c.ID });
            modelBuilder.Entity<ColourDocumentDTO>().HasKey(c => new { c.ColourId, c.DocumentId });
            modelBuilder.Entity<ColourDocumentDTO>().HasOne(s => s.Colour).WithMany(s => s.ColourDocuments).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ColoursDTO>().HasQueryFilter(m => EF.Property<bool>(m, "DeleteStatus") == false);
            modelBuilder.Entity<ColourDocumentDTO>().HasQueryFilter(m => EF.Property<bool>(m, "DeleteStatus") == false);
            modelBuilder.Entity<FavouritesDTO>().HasQueryFilter(m => EF.Property<bool>(m, "DeleteStatus") == false);
        }

        #endregion

        /// <summary>
        /// This method of InserUpdateDBContext is class is use to execute on SaveChange method is called from dbcontext property
        /// </summary>
        /// <returns></returns>
        /// <remarks>Code documentation and Review is pending</remarks>
        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified || e.State == EntityState.Deleted));
          

            if (entries.Count() > 0)
            {
                foreach (var entityEntry in entries)
                {
                    ((BaseEntity)entityEntry.Entity).ModifiedOn = DateTime.Now;
                    if (entityEntry.Property("CreatedOn") != null)
                    {
                        entityEntry.Property("CreatedOn").IsModified = false;
                    }
                    if (entityEntry.Property("CreatedByID") != null)
                    {
                        entityEntry.Property("CreatedByID").IsModified = false;
                    }
                    if (entityEntry.State == EntityState.Added)
                    {
                        //if (string.IsNullOrEmpty(((BaseEntity)entityEntry.Entity).ID))
                        //{
                        ((BaseEntity)entityEntry.Entity).ID = Convert.ToString(Guid.NewGuid());
                        //}
                        ((BaseEntity)entityEntry.Entity).CreatedOn = DateTime.Now;
                    }
                }
            }
            else
            {
                var entries1 = ChangeTracker
                    .Entries()
                    .Where(e => e.Entity is ReferenceEntity && (
                            e.State == EntityState.Added
                            || e.State == EntityState.Modified || e.State == EntityState.Deleted));

                foreach (var entityEntry in entries1)
                {
                    ((ReferenceEntity)entityEntry.Entity).ModifiedOn = DateTime.Now;
                    if (entityEntry.Property("CreatedOn") != null)
                    {
                        entityEntry.Property("CreatedOn").IsModified = false;
                    }
                    if (entityEntry.Property("CreatedByID") != null)
                    {
                        entityEntry.Property("CreatedByID").IsModified = false;
                    }
                    if (entityEntry.State == EntityState.Added)
                    {
                        //if (string.IsNullOrEmpty(((ReferenceEntity)entityEntry.Entity).ID))
                        if (!string.IsNullOrEmpty(entityEntry.Entity.GetType().Name) && entityEntry.Entity.GetType().Name != "FilesDTO")
                        {
                            ((ReferenceEntity)entityEntry.Entity).ID = Convert.ToString(Guid.NewGuid());
                        }
                        ((ReferenceEntity)entityEntry.Entity).CreatedOn = DateTime.Now;
                    }
                }
            }
            return base.SaveChanges();
        }        
    }
}
