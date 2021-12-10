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
            await SeedDemoUsersAsync(userManagerSvc);
            await SeedProjectPrioritiesAsync(dbContextSvc);
            await SeedTicketStatusesAsync(dbContextSvc);
            await SeedTicketPrioritiesAsync(dbContextSvc);
            await SeedTicketTypesAsync(dbContextSvc);
            await SeedNotificationTypes(dbContextSvc);
            await SeedProjectsAsync(dbContextSvc);
            await SeedTicketsAsync(dbContextSvc);

        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManagerSvc)
        {
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.Admin.ToString())); //Used for enums
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.ProjectManager.ToString()));
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.Developer.ToString()));
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.Submitter.ToString()));
            await roleManagerSvc.CreateAsync(new IdentityRole(BTRoles.DemoUser.ToString()));
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
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Companies");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
                throw;
            };
        }

        private static async Task SeedUsersAsync(UserManager<BTUser> userManagerSvc)
        {
            var defaultUser = new BTUser
            {
                UserName = "AdeleCotopaxi1@bugtracker.com",
                Email = "AdeleCotopaxi1@bugtracker.com",
                FirstName = "Adele",
                LastName = "Cotopaxi",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user is null)
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
                UserName = "AdamCotopaxi2@bugtracker.com",
                Email = "AdamCotopaxi2@bugtracker.com",
                FirstName = "Adam",
                LastName = "Cotopaxi",
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
                UserName = "PoppyCotopaxi1@bugtracker.com",
                Email = "PoppyCotopaxi1@bugtracker.com",
                FirstName = "Poppy",
                LastName = "Cotopaxi",
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
                UserName = "PrattCotopaxi2@bugtracker.com",
                Email = "PrattCotopaxi2@bugtracker.com",
                FirstName = "Pratt",
                LastName = "Cotopaxi",
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
                UserName = "DevinCotopaxi1@bugtracker.com",
                Email = "DevinCotopaxi1@bugtracker.com",
                FirstName = "Devin",
                LastName = "Cotopaxi",
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
                UserName = "DevCotopaxi2@bugtracker.com",
                Email = "DevCotopaxi2@bugtracker.com",
                FirstName = "Dev",
                LastName = "Cotopaxi",
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
                UserName = "DevoCotopaxi3@bugtracker.com",
                Email = "DevoCotopaxi3@bugtracker.com",
                FirstName = "Devo",
                LastName = "Cotopaxi",
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
                UserName = "DrakeCotopaxi4@bugtracker.com",
                Email = "DrakeCotopaxi4@bugtracker.com",
                FirstName = "Drake",
                LastName = "Cotopaxi",
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
                UserName = "DreyfusCotopaxi5@bugtracker.com",
                Email = "DreyfusCotopaxi5@bugtracker.com",
                FirstName = "Dreyfus",
                LastName = "Cotopaxi",
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
                UserName = "DrenCotopaxi6@bugtracker.com",
                Email = "DrenCotopaxi6@bugtracker.com",
                FirstName = "Dren",
                LastName = "Cotopaxi",
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
                UserName = "SabrinaCotopaxi1@bugtracker.com",
                Email = "SabrinaCotopaxi1@bugtracker.com",
                FirstName = "Sabrina",
                LastName = "Cotopaxi",
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
                UserName = "SameCotopaxi2@bugtracker.com",
                Email = "SamCotopaxi2@bugtracker.com",
                FirstName = "Sam",
                LastName = "Cotopaxi",
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
                UserName = "SpinnerCotopaxi3@bugtracker.com",
                Email = "SpinnerCotopaxi3@bugtracker.com",
                FirstName = "Spinner",
                LastName = "Cotopaxi",
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
                UserName = "ShellyCotopaxi4@bugtracker.com",
                Email = "ShellyCotopaxi4@bugtracker.com",
                FirstName = "Shelly",
                LastName = "Cotopaxi",
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
                UserName = "SeanCotopaxi5@bugtracker.com",
                Email = "SeanCotopaxi5@bugtracker.com",
                FirstName = "Sean",
                LastName = "Cotopaxi",
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
                UserName = "SherryCotopaxi6@bugtracker.com",
                Email = "SherryCotopaxi6@bugtracker.com",
                FirstName = "Sherry",
                LastName = "Cotopaxi",
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

        private static async Task SeedDemoUsersAsync(UserManager<BTUser> userManagerSvc)
        {
            //Seed Demo Admin User
            var defaultUser = new BTUser
            {
                UserName = "demoadmin@bugtracker.com",
                Email = "demoadmin@bugtracker.com",
                FirstName = "Demo",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Admin.ToString());
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.DemoUser.ToString());

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Demo ProjectManager User
            defaultUser = new BTUser
            {
                UserName = "demopm@bugtracker.com",
                Email = "demopm@bugtracker.com",
                FirstName = "Demo",
                LastName = "ProjectManager",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.ProjectManager.ToString());
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo ProjectManager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Demo Developer User
            defaultUser = new BTUser
            {
                UserName = "demodev@bugtracker.com",
                Email = "demodev@bugtracker.com",
                FirstName = "Demo",
                LastName = "Developer",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Developer.ToString());
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Demo Submitter User
            defaultUser = new BTUser
            {
                UserName = "demosub@bugtracker.com",
                Email = "demosub@bugtracker.com",
                FirstName = "Demo",
                LastName = "Submitter",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Submitter User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Demo New User
            defaultUser = new BTUser
            {
                UserName = "demonew@bugtracker.com",
                Email = "demonew@bugtracker.com",
                FirstName = "Demo",
                LastName = "NewUser",
                EmailConfirmed = true,
                CompanyId = company2Id
            };
            try
            {
                var user = await userManagerSvc.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManagerSvc.CreateAsync(defaultUser, "Abc@123!");
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.Submitter.ToString());
                    await userManagerSvc.AddToRoleAsync(defaultUser, BTRoles.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo New User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

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
                throw;
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

                var dbTicketPriorities = dbContextSvc.TicketPriorities.Select(c => c.Name).ToList();

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
                throw;
            }
        }

        private static async Task SeedNotificationTypes(ApplicationDbContext dbContextSvc)
        {
            try
            {
                IList<NotificationType> notificationTypes = new List<NotificationType>()
                {
                    new NotificationType() { Name = BTNotificationType.Project.ToString() },
                    new NotificationType() { Name = BTNotificationType.Ticket.ToString() },
                };

                var dbNotificationTypes = dbContextSvc.NotificationTypes.Select(c => c.Name).ToList();

                await dbContextSvc.NotificationTypes.AddRangeAsync(notificationTypes.Where(c => !dbNotificationTypes.Contains(c.Name)));

                await dbContextSvc.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Notification Types");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
                throw;
            }
        }

        private static async Task SeedProjectsAsync(ApplicationDbContext dbContextSvc)
        {
            //Get project priority Ids
            int priorityLow = dbContextSvc.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Low.ToString()).Id;
            int priorityMedium = dbContextSvc.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Medium.ToString()).Id;
            int priorityHigh = dbContextSvc.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.High.ToString()).Id;
            int priorityUrgent = dbContextSvc.ProjectPriorities.FirstOrDefault(p => p.Name == BTProjectPriority.Urgent.ToString()).Id;

            try
            {
                IList<Project> projects = new List<Project>() {
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build a Personal Porfolio",
                         Description="Single page html, css & javascript page.  Serves as a landing page for candidates and contains a bio and links to all applications and challenges." ,
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(1),
                         ProjectPriorityId = priorityLow
                     },
                     new Project()
                     {
                         CompanyId = company2Id,
                         Name = "Build a supplemental Blog Web Application",
                         Description="Candidate's custom built web application using .Net Core with MVC, a postgres database and hosted in a heroku container.  The app is designed for the candidate to create, update and maintain a live blog site.",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(4),
                         ProjectPriorityId = priorityMedium
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build an Issue Tracking Web Application",
                         Description="A custom designed .Net Core application with postgres database.  The application is a multi tennent application designed to track issue tickets' progress.  Implemented with identity and user roles, Tickets are maintained in projects which are maintained by users in the role of projectmanager.  Each project has a team and team members.",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(6),
                         ProjectPriorityId = priorityHigh
                     },
                     new Project()
                     {
                         CompanyId = company2Id,
                         Name = "Build an Address Book Web Application",
                         Description="A custom designed .Net Core application with postgres database.  This is an application to serve as a rolodex of contacts for a given user..",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(2),
                         ProjectPriorityId = priorityLow
                     },
                    new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build a Movie Information Web Application",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2021,8,20),
                         EndDate = new DateTime(2021,8,20).AddMonths(3),
                         ProjectPriorityId = priorityHigh
                     }
                };

                var dbProjects = dbContextSvc.Projects.Select(c => c.Name).ToList();
                await dbContextSvc.Projects.AddRangeAsync(projects.Where(c => !dbProjects.Contains(c.Name)));
                await dbContextSvc.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Projects.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        private static async Task SeedTicketsAsync(ApplicationDbContext dbContextSvc)
        {
            //Get project Ids
            int portfolioId = dbContextSvc.Projects.FirstOrDefault(p => p.Name == "Build a Personal Porfolio").Id;
            int blogId = dbContextSvc.Projects.FirstOrDefault(p => p.Name == "Build a supplemental Blog Web Application").Id;
            int bugtrackerId = dbContextSvc.Projects.FirstOrDefault(p => p.Name == "Build an Issue Tracking Web Application").Id;
            int movieId = dbContextSvc.Projects.FirstOrDefault(p => p.Name == "Build a Movie Information Web Application").Id;

            //Get ticket type Ids
            int typeNewDev = dbContextSvc.TicketTypes.FirstOrDefault(p => p.Name == BTTicketType.NewDevelopment.ToString()).Id;
            int typeWorkTask = dbContextSvc.TicketTypes.FirstOrDefault(p => p.Name == BTTicketType.WorkTask.ToString()).Id;
            int typeDefect = dbContextSvc.TicketTypes.FirstOrDefault(p => p.Name == BTTicketType.Defect.ToString()).Id;
            int typeEnhancement = dbContextSvc.TicketTypes.FirstOrDefault(p => p.Name == BTTicketType.Enhancement.ToString()).Id;
            int typeChangeRequest = dbContextSvc.TicketTypes.FirstOrDefault(p => p.Name == BTTicketType.ChangeRequest.ToString()).Id;

            //Get ticket priority Ids
            int priorityLow = dbContextSvc.TicketPriorities.FirstOrDefault(p => p.Name == BTTicketPriority.Low.ToString()).Id;
            int priorityMedium = dbContextSvc.TicketPriorities.FirstOrDefault(p => p.Name == BTTicketPriority.Medium.ToString()).Id;
            int priorityHigh = dbContextSvc.TicketPriorities.FirstOrDefault(p => p.Name == BTTicketPriority.High.ToString()).Id;
            int priorityUrgent = dbContextSvc.TicketPriorities.FirstOrDefault(p => p.Name == BTTicketPriority.Urgent.ToString()).Id;

            //Get ticket status Ids
            int statusNew = dbContextSvc.TicketStatuses.FirstOrDefault(p => p.Name == BTTicketStatus.New.ToString()).Id;
            int statusDev = dbContextSvc.TicketStatuses.FirstOrDefault(p => p.Name == BTTicketStatus.Development.ToString()).Id;
            int statusTest = dbContextSvc.TicketStatuses.FirstOrDefault(p => p.Name == BTTicketStatus.Testing.ToString()).Id;
            int statusResolved = dbContextSvc.TicketStatuses.FirstOrDefault(p => p.Name == BTTicketStatus.Resolved.ToString()).Id;


            try
            {
                IList<Ticket> tickets = new List<Ticket>() {
                                //PORTFOLIO
                                new Ticket() {Title = "Portfolio Ticket 1", Description = "Ticket details for portfolio ticket 1", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Portfolio Ticket 2", Description = "Ticket details for portfolio ticket 2", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Portfolio Ticket 3", Description = "Ticket details for portfolio ticket 3", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Portfolio Ticket 4", Description = "Ticket details for portfolio ticket 4", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Portfolio Ticket 5", Description = "Ticket details for portfolio ticket 5", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Portfolio Ticket 6", Description = "Ticket details for portfolio ticket 6", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Portfolio Ticket 7", Description = "Ticket details for portfolio ticket 7", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Portfolio Ticket 8", Description = "Ticket details for portfolio ticket 8", Created = DateTimeOffset.Now, ProjectId = portfolioId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                //BLOG
                                new Ticket() {Title = "Blog Ticket 1", Description = "Ticket details for blog ticket 1", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Blog Ticket 2", Description = "Ticket details for blog ticket 2", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Blog Ticket 3", Description = "Ticket details for blog ticket 3", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Blog Ticket 4", Description = "Ticket details for blog ticket 4", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Blog Ticket 5", Description = "Ticket details for blog ticket 5", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Blog Ticket 6", Description = "Ticket details for blog ticket 6", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Blog Ticket 7", Description = "Ticket details for blog ticket 7", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Blog Ticket 8", Description = "Ticket details for blog ticket 8", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Blog Ticket 9", Description = "Ticket details for blog ticket 9", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Blog Ticket 10", Description = "Ticket details for blog ticket 10", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Blog Ticket 11", Description = "Ticket details for blog ticket 11", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Blog Ticket 12", Description = "Ticket details for blog ticket 12", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Blog Ticket 13", Description = "Ticket details for blog ticket 13", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Blog Ticket 14", Description = "Ticket details for blog ticket 14", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Blog Ticket 15", Description = "Ticket details for blog ticket 15", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Blog Ticket 16", Description = "Ticket details for blog ticket 16", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Blog Ticket 17", Description = "Ticket details for blog ticket 17", Created = DateTimeOffset.Now, ProjectId = blogId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                //BUGTRACKER                                                                                                                         
                                new Ticket() {Title = "Bug Tracker Ticket 1", Description = "Ticket details for bug tracker ticket 1", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 2", Description = "Ticket details for bug tracker ticket 2", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 3", Description = "Ticket details for bug tracker ticket 3", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 4", Description = "Ticket details for bug tracker ticket 4", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 5", Description = "Ticket details for bug tracker ticket 5", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 6", Description = "Ticket details for bug tracker ticket 6", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 7", Description = "Ticket details for bug tracker ticket 7", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 8", Description = "Ticket details for bug tracker ticket 8", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 9", Description = "Ticket details for bug tracker ticket 9", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 10", Description = "Ticket details for bug tracker 10", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 11", Description = "Ticket details for bug tracker 11", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 12", Description = "Ticket details for bug tracker 12", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 13", Description = "Ticket details for bug tracker 13", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 14", Description = "Ticket details for bug tracker 14", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 15", Description = "Ticket details for bug tracker 15", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 16", Description = "Ticket details for bug tracker 16", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 17", Description = "Ticket details for bug tracker 17", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 18", Description = "Ticket details for bug tracker 18", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 19", Description = "Ticket details for bug tracker 19", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 20", Description = "Ticket details for bug tracker 20", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 21", Description = "Ticket details for bug tracker 21", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 22", Description = "Ticket details for bug tracker 22", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 23", Description = "Ticket details for bug tracker 23", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 24", Description = "Ticket details for bug tracker 24", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 25", Description = "Ticket details for bug tracker 25", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 26", Description = "Ticket details for bug tracker 26", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 27", Description = "Ticket details for bug tracker 27", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 28", Description = "Ticket details for bug tracker 28", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 29", Description = "Ticket details for bug tracker 29", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 30", Description = "Ticket details for bug tracker 30", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                //MOVIE
                                new Ticket() {Title = "Movie Ticket 1", Description = "Ticket details for movie ticket 1", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 2", Description = "Ticket details for movie ticket 2", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 3", Description = "Ticket details for movie ticket 3", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 4", Description = "Ticket details for movie ticket 4", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 5", Description = "Ticket details for movie ticket 5", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 6", Description = "Ticket details for movie ticket 6", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 7", Description = "Ticket details for movie ticket 7", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 8", Description = "Ticket details for movie ticket 8", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 9", Description = "Ticket details for movie ticket 9", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 10", Description = "Ticket details for movie ticket 10", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 11", Description = "Ticket details for movie ticket 11", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 12", Description = "Ticket details for movie ticket 12", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 13", Description = "Ticket details for movie ticket 13", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 14", Description = "Ticket details for movie ticket 14", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 15", Description = "Ticket details for movie ticket 15", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 16", Description = "Ticket details for movie ticket 16", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 17", Description = "Ticket details for movie ticket 17", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 18", Description = "Ticket details for movie ticket 18", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 19", Description = "Ticket details for movie ticket 19", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 20", Description = "Ticket details for movie ticket 20", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},

                };


                var dbTickets = dbContextSvc.Tickets.Select(c => c.Title).ToList();
                await dbContextSvc.Tickets.AddRangeAsync(tickets.Where(c => !dbTickets.Contains(c.Title)));
                await dbContextSvc.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Tickets.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

    }
}

