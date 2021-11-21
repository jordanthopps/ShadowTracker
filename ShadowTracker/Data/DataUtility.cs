using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ShadowTracker.Models;
using ShadowTracker.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Data
{
    public static class DataUtility
    {
        private static int company1Id;
        private static int company2Id;
        private static int company3Id;
        private static int company4Id;
        private static int company5Id;

        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL"); //connection to Heroku

            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        private static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);

            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };

            return builder.ToString();

        }

        public static async Task ManageDataAsync(IHost host)
        {
            using var svcScope = host.Services.CreateScope();

            var svcProvider = svcScope.ServiceProvider;

            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BTUser>>();

            await dbContextSvc.Database.MigrateAsync(); //Runs the database migrations automatically. Programmatic equivalent of update-database.

            //These need to be in order:
            await SeedRolesAsync(roleManagerSvc);
            await SeedCompaniesAsync(dbContextSvc);
            await SeedUsersAsync(userManagerSvc);
            //await SeedDemoUsersAsync(userManagerSvc);
            await SeedProjectPrioritiesAsync(dbContextSvc);
            await SeedTicketStatusesAsync(dbContextSvc);
            await SeedTicketPrioritiesAsync(dbContextSvc);
            await SeedTicketTypesAsync(dbContextSvc);
            //await SeedNotificationTypes(dbContextSvc);
            //await SeedProjectsAsync(dbContextSvc);
            //await SeedTicketsAsync(dbContextSvc);

        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManagerSvc)
        {
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.Admin.ToString())); //Used for enums
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.ProjectManager.ToString()));
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.Developer.ToString()));
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.Submitter.ToString()));
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.ProjectManager.ToString()));
        }

        private static async Task SeedCompaniesAsync(ApplicationDbContext dbContextSvc)
        {
            try
            {
                IList<Company> defaultCompanies = new List<Company>()
                {
                    new Company() {Name = "Company 1", Description = "This is default Company 1"}, //reference this for creating a set and to make sure they don't exist in the db
                    new Company() {Name = "Company 2", Description = "This is default Company 2"},
                    new Company() {Name = "Company 3", Description = "This is default Company 3"},
                    new Company() {Name = "Company 4", Description = "This is default Company 4"},
                    new Company() {Name = "Company 5", Description = "This is default Company 5"},
                };

                var dbCompanies = dbContextSvc.Companies.Select(c => c.Name).ToList();

                await dbContextSvc.Companies.AddRangeAsync(defaultCompanies.Where(c => !dbCompanies.Contains(c.Name)));

                await dbContextSvc.SaveChangesAsync();

                company1Id = (await dbContextSvc.Companies.FirstOrDefaultAsync(c => c.Name == "Company 1")).Id;
                company2Id = (await dbContextSvc.Companies.FirstOrDefaultAsync(c => c.Name == "Company 2")).Id;
                company3Id = (await dbContextSvc.Companies.FirstOrDefaultAsync(c => c.Name == "Company 3")).Id;
                company4Id = (await dbContextSvc.Companies.FirstOrDefaultAsync(c => c.Name == "Company 4")).Id;
                company5Id = (await dbContextSvc.Companies.FirstOrDefaultAsync(c => c.Name == "Company 5")).Id;
            }
            catch(Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Companies");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            };
        }

        private static async Task SeedUsersAsync(UserManager<BTUser> userManagerSvc)
        {
            var defaultUser = new BTUser
            {
                UserName = "TotoCotoadmin1@bugtracker.com",
                Email = "TotoCotoadmin1@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if(user is null)
                {
                    /*When seeding users, you MUST meet password complexity requirements
                     6 char minimum, 1 upper, 1 lower, 1 number, 1 special
                    If you do not meet the requirement, the user will not be created
                    but no error will be thrown either*/

                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Admin User");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }

            defaultUser = new BTUser
            {
                UserName = "TotoCotoadmin2@bugtracker.com",
                Email = "TotoCotoadmin2@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Admin User 2");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }

            defaultUser = new BTUser
            {
                UserName = "TotoCotoPM1@bugtracker.com",
                Email = "TotoCotoPM1@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default PM User 1");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoPM2@bugtracker.com",
                Email = "TotoCotoPM2@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default PM User 2");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }

            defaultUser = new BTUser
            {
                UserName = "TotoCotoDev1@bugtracker.com",
                Email = "TotoCotoDev1@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Dev User 1");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }

            defaultUser = new BTUser
            {
                UserName = "TotoCotoDev2@bugtracker.com",
                Email = "TotoCotoDev2@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Dev User 2");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoDev3@bugtracker.com",
                Email = "TotoCotoDev3@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Dev User 3");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }

            defaultUser = new BTUser
            {
                UserName = "TotoCotoDev4@bugtracker.com",
                Email = "TotoCotoDev4@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Dev User 4");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoDev5@bugtracker.com",
                Email = "TotoCotoDev5@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Dev User 5");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoDev6@bugtracker.com",
                Email = "TotoCotoDev6@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Dev User 6");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoSub1@bugtracker.com",
                Email = "TotoCotoSub1@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Sub User 1");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoSub2@bugtracker.com",
                Email = "TotoCotoSub2@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Sub User 2");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoSub3@bugtracker.com",
                Email = "TotoCotoSub3@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Sub User 3");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoSub4@bugtracker.com",
                Email = "TotoCotoSub4@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Sub User 4");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoSub5@bugtracker.com",
                Email = "TotoCotoSub5@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Sub User 5");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            
            defaultUser = new BTUser
            {
                UserName = "TotoCotoSub6@bugtracker.com",
                Email = "TotoCotoSub6@bugtracker.com",
                FirstName = "Toto",
                LastName = "Coto",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Default Sub User 6");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
            

        }

        //private static async Task SeedDemoUsersAsync(UserManager<BTUser> userManagerSvc)
        //{
        //    throw new NotImplementedException();
        //}

        private static async Task SeedProjectPrioritiesAsync(ApplicationDbContext dbContextSvc)
        {
            try
            {
                IList<ProjectPriority> projectPriorities = new List<ProjectPriority>()
                {
                    new ProjectPriority() { Name = BTProjectPriority.Low.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.Medium.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.High.ToString() },
                    new ProjectPriority() { Name = BTProjectPriority.Urgent.ToString() },
                };

                var dbProjectPriorities = dbContextSvc.ProjectPriorities.Select(c => c.Name).ToList();

                await dbContextSvc.ProjectPriorities.AddRangeAsync(projectPriorities.Where(c => !dbProjectPriorities.Contains(c.Name)));

                await dbContextSvc.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Project Priorities");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
        }


        private static async Task SeedTicketStatusesAsync(ApplicationDbContext dbContextSvc)
        {
            try
            {
                IList<TicketStatus> ticketStatuses = new List<TicketStatus>()
                {
                    new TicketStatus() { Name = BTTicketStatus.New.ToString() },
                    new TicketStatus() { Name = BTTicketStatus.Development.ToString() },
                    new TicketStatus() { Name = BTTicketStatus.Testing.ToString() },
                    new TicketStatus() { Name = BTTicketStatus.Resolved.ToString() },
                };

                var dbTicketStatuses = dbContextSvc.TicketStatuses.Select(c => c.Name).ToList();

                await dbContextSvc.TicketStatuses.AddRangeAsync(ticketStatuses.Where(c => !dbTicketStatuses.Contains(c.Name)));

                await dbContextSvc.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Ticket Statuses");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
        }


        private static async Task SeedTicketPrioritiesAsync(ApplicationDbContext dbContextSvc)
        {
            try
            {
                IList<TicketPriority> ticketPriorities = new List<TicketPriority>()
                {
                    new TicketPriority() { Name = BTTicketPriority.Low.ToString() },
                    new TicketPriority() { Name = BTTicketPriority.Medium.ToString() },
                    new TicketPriority() { Name = BTTicketPriority.High.ToString() },
                    new TicketPriority() { Name = BTTicketPriority.Urgent.ToString() },
                };

                var dbTicketPriorities = dbContextSvc.ProjectPriorities.Select(c => c.Name).ToList();

                await dbContextSvc.TicketPriorities.AddRangeAsync(ticketPriorities.Where(c => !dbTicketPriorities.Contains(c.Name)));

                await dbContextSvc.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Ticket Priorities");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
        }

        private static async Task SeedTicketTypesAsync(ApplicationDbContext dbContextSvc)
        {
            try
            {
                IList<TicketType> ticketTypes = new List<TicketType>()
                {
                    new TicketType() { Name = BTTicketType.ChangeRequest.ToString() },
                    new TicketType() { Name = BTTicketType.Defect.ToString() },
                    new TicketType() { Name = BTTicketType.Enhancement.ToString() },
                    new TicketType() { Name = BTTicketType.GeneralTask.ToString() },
                    new TicketType() { Name = BTTicketType.NewDevelopment.ToString() },
                    new TicketType() { Name = BTTicketType.WorkTask.ToString() }
                };

                var dbTicketTypes = dbContextSvc.TicketTypes.Select(c => c.Name).ToList();

                await dbContextSvc.TicketTypes.AddRangeAsync(ticketTypes.Where(c => !dbTicketTypes.Contains(c.Name)));

                await dbContextSvc.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Ticket Types");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
            }
        }

        //private static async Task SeedNotificationTypes(ApplicationDbContext dbContextSvc)
        //{
        //    throw new NotImplementedException();
        //}

        //private static async Task SeedProjectsAsync(ApplicationDbContext dbContextSvc)
        //{
        //    throw new NotImplementedException();
        //}

        //private static async Task SeedTicketsAsync(ApplicationDbContext dbContextSvc)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
