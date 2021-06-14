using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWU_Web.Data;
using SWU_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Controllers
{
    [Authorize(Roles ="SuperAdmin")]
    public class AccountController : Controller
    {
        readonly private UserManager<User> _userManager;
        readonly private ApplicationDbContext _applicationDbContext;
        readonly private SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, ApplicationDbContext applicationDbContext, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _applicationDbContext = applicationDbContext;
            _signInManager = signInManager;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string ReturnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = ReturnUrl });
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Login, model.Password,false, false);
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "SystemMonitoring");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "SystemMonitoring");
        }

        [HttpGet]
        public IActionResult AdminPanel()
        {
            return View();
        }

        public async Task<JsonResult> GetAllUsers()
        {
            List<User> users = _applicationDbContext.Users.ToList();
            User user = await _userManager.GetUserAsync(User);
            List<AdminTableItemModel> items = new List<AdminTableItemModel>();
            foreach (var u in users)
            {
                if (!u.Id.Equals(user.Id))
                {
                    items.Add(new AdminTableItemModel()
                    {
                        Id = u.Id,
                        Login = u.UserName,
                        FullName = u.FullName,
                        Position = u.Position,
                        Role = (await _userManager.GetRolesAsync(u)).ToList()[0]
                    });
                }

            }
            return Json(items);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            User user = _applicationDbContext.Users.FirstOrDefault(u => u.Id.Equals(id));
            await _userManager.DeleteAsync(user);
            return RedirectToAction("AdminPanel");

        }
        [HttpPost]
        public async Task<IActionResult> AddUser(AdminTableItemModel model,string login, string password)
        {
            var user = new User
            {
                UserName = login,
                Email = login,
                FullName = model.FullName,
                Position = model.Position,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            await _userManager.CreateAsync(user, password);
            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction("AdminPanel");
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(AdminTableItemModel model,bool editLogin, string password)
        {
            User u = _applicationDbContext.Users.FirstOrDefault(u => u.Id.Equals(model.Id));

            
            u.FullName = model.FullName;
            u.Position = model.Position;

            string oldRole = (await _userManager.GetRolesAsync(u))[0];
            if (editLogin)
            {
                u.UserName = model.Login;
                u.Email = model.Login;
                u.PasswordHash = _userManager.PasswordHasher.HashPassword(u, password);
            }
            _applicationDbContext.SaveChanges();

            if (!oldRole.Equals(model.Role))
            {
                await _userManager.RemoveFromRoleAsync(u, oldRole);
                await _userManager.AddToRoleAsync(u, model.Role);
            }

            return RedirectToAction("AdminPanel");
        }

    }
}
