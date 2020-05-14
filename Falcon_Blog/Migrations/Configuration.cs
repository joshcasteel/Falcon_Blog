namespace Falcon_Blog.Migrations
{
    using Falcon_Blog.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Falcon_Blog.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Falcon_Blog.Models.ApplicationDbContext context)
        {

            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Moderator"))
            {
                roleManager.Create(new IdentityRole { Name = "Moderator" });
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (!context.Users.Any(u => u.Email == "joshcasteelz@gmail.com"))
            {
                var user = new ApplicationUser
                {
                    UserName = "joshcasteelz@gmail.com",
                    Email = "joshcasteelz@gmail.com",
                    FirstName = "Josh",
                    LastName = "Casteel",
                    DisplayName = "Josh Casteel"
                };

                userManager.Create(user, "Abc&123!");

                userManager.AddToRoles(user.Id, "Admin");
            }

            if (!context.Users.Any(u => u.Email == "ARussell@coderfoundry.com"))
            {
                var user = new ApplicationUser
                {
                    UserName = "ARussell@coderfoundry.com",
                    Email = "ARussell@coderfoundry.com",
                    FirstName = "Andrew",
                    LastName = "Russell",
                    DisplayName = "Andrew Russell"
                };

                userManager.Create(user, "Abc&123!");

                userManager.AddToRoles(user.Id, "Moderator");
            }

        }
    }
}
