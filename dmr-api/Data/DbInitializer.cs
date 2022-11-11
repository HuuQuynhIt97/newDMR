using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API.Data;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;

namespace dmr_api.Data
{
    public class DbInitializer
    {
        private readonly DataContext _context;
        private readonly string SuperAdminCode = "SUPPER_ADMIN";

        public DbInitializer(DataContext context)
        {
            _context = context;
        }
        public async Task Seed() {
            #region Quyền
            if (!(await _context.Roles.AnyAsync()))
            {
                await _context.Roles.AddAsync(new Role
                {
                    Name = "Supper Admin",
                    Code = SuperAdminCode,
                });
                await _context.SaveChangesAsync();
            }

            #endregion

            #region Người dùng
            #endregion

            #region Module

            if (!(await _context.Modules.AnyAsync()))
            {
                
                await _context.Modules.AddAsync(new Module
                {
                    Name = "System",
                    Code = "SYSTEM",
                    Sequence = 1
                });
                await _context.SaveChangesAsync();
            }
            #endregion
            #region Thao tac

            if (!(await _context.Actions.AnyAsync()))
            {
                await _context.Actions.AddRangeAsync(new List<Action> {
                    new Action {Name = "Create", Code = "CREATE"},
                    new Action {Name = "Edit", Code = "EDIT"},
                    new Action {Name = "Delete", Code = "DELETE"},
                    new Action {Name = "Read", Code = "VIEW"}
                });
                await _context.SaveChangesAsync();
            }
            #endregion

            #region Chuc nang

            if (!(await _context.FunctionSystem.AnyAsync()))
            {
                var module = await _context.Modules.FirstOrDefaultAsync();

                await _context.FunctionSystem.AddRangeAsync(new List<FunctionSystem> {
                    new FunctionSystem { ModuleID = module.ID, Name = "Module", Code = "SYSTEM", Sequence = 1, Url = "/ec/system/module"},
                    new FunctionSystem {ModuleID = module.ID, Name = "Action", Code = "SYSTEM", Sequence = 2, Url = "/ec/system/action"},
                    new FunctionSystem {ModuleID = module.ID, Name = "Function", Code = "SYSTEM", Sequence = 3, Url = "/ec/system/function"},
                    new FunctionSystem {ModuleID = module.ID, Name = "Action In Function", Code = "SYSTEM", Sequence = 4, Url = "/ec/system/action-in-function"}
                });
                await _context.SaveChangesAsync();


                var functions =  _context.FunctionSystem;
                var actions = _context.Actions;

                if (!_context.ActionInFunctionSystem.Any())
                {
                    var actionInfunctionlist = new List<ActionInFunctionSystem>();
                    foreach (var function in functions)
                    {
                        foreach (var action in actions)
                        {
                            var createAction = new ActionInFunctionSystem()
                            {
                                ActionID = action.ID,
                                FunctionSystemID = function.ID
                            };
                            actionInfunctionlist.Add(createAction);
                        }
                    }
                    _context.ActionInFunctionSystem.AddRange(actionInfunctionlist);
                    await _context.SaveChangesAsync();

                }

                if (!_context.Permisions.Any())
                {
                    var adminRole = await _context.Roles.FirstOrDefaultAsync(x => x.Code == SuperAdminCode);
                    var permissionlist = new List<Permission>();
                    var actionInFunction = _context.ActionInFunctionSystem;
                    foreach (var item in actionInFunction)
                    {
                        permissionlist.Add(new Permission {RoleID = adminRole.ID, FunctionSystemID = item.FunctionSystemID , ActionID = item.ActionID});
                    }
                    _context.Permisions.AddRange(permissionlist);
                    await _context.SaveChangesAsync();

                }
            }
            #endregion
        }
    }
}