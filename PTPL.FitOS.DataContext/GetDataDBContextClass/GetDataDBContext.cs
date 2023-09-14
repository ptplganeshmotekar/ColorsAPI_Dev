using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using PTPL.FitOS.DataModels;
using System;

namespace PTPL.FitOS.DataContext
{
    /// <summary>
    /// This is an dbContext file to use get data from database tables 
    /// </summary>
    /// <remarks>Code documentation and Review is pending</remarks>
    public class GetDataDBContext : DbContext
    {
        /// <summary>
        /// This section will use to declare connection string variable and hold values as public.
        /// </summary>
        /// <remarks>Code documentation and Review is pending</remarks>
        #region Fields
        public MySqlConnection Connection { get; set; }
        public string ConnString { get; set; }    
        public DbSet<ColoursDTO> ColoursDBSet { get; set; }             
        public DbSet<ColourDocumentDTO> ColourDocumentsDBSet { get; set; }
        public DbSet<SequenceDTO> SequenceDBSet { get; set; }
        #endregion Fields

        /// <summary>
        /// This is GetDataDBContext contruction class
        /// </summary>
        /// <param name="options"></param>
        /// <remarks>Code documentation and Review is pending</remarks>
        #region Constructors
        public GetDataDBContext(DbContextOptions<GetDataDBContext> options) : base(options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetDataDbContext" /> class.
        /// </summary>
        /// <param name="connectionString">The ConnectionString<see cref="string" /></param>
        public GetDataDBContext(string connectionString)
        {
            ConnString = connectionString;
            Connection = new MySqlConnection(connectionString);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// This is an protected method which is use to execute OnConfiguring event  
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <remarks>Code documentation and Review is pending</remarks>
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseMySql(ConnString
        //        , builder => builder.EnableRetryOnFailure(
        //        maxRetryCount: 4,
        //        maxRetryDelay: TimeSpan.FromSeconds(20),
        //        errorNumbersToAdd: null)
        //        );
        //}
        #endregion
    }
}
