using Microsoft.AspNetCore.Identity;
using SWU_Web.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Data
{
    public class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Employee.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Quest.ToString()));
        }

        public static void SeedTypesDetectorsAsync(ApplicationDbContext context)
        {
            List<TypeDetector> typeDetectors = context.TypeDetectors.ToList();
            string[] names = Enum.GetNames(typeof(TypeDetectors));
            for(int i =0; i < Math.Min(typeDetectors.Count,names.Length); i++)
            {

                TypeDetector type = typeDetectors.FirstOrDefault(d => d.Id == i+1);
                type.Name = names[i];
            }
            for(int i = Math.Min(typeDetectors.Count, names.Length); i < names.Length; i++)
            {
                TypeDetector type = new TypeDetector() { Name = names[i] };
                context.TypeDetectors.Add(type);
            }
            for(int i = names.Length; i < typeDetectors.Count; i++)
            {
                TypeDetector type = typeDetectors.FirstOrDefault(d => d.Id == i+1);
                context.Entry(type).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
            }
            context.SaveChanges();
        }

        public static async Task SeedSuperAdminAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new User
            {
                UserName = "superadmin",
                Email = "superadmin",
                FullName= "Superadmin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "P@ssw0rd.");
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.SuperAdmin.ToString());
                }

            }
        }
    }
}
