using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DMR_API._Services.Services
{
    public class PermissionService : IPermissionService
    {

        private readonly IPermissionRepository _repoPermission;

        private readonly IMapper _mapper;
        private readonly IActionRepository _repoAction;
        private readonly IModuleRepository _repoModule;
        private readonly IRoleRepository _repoRole;
        private readonly IFunctionSystemRepository _repoFunctionSystem;
        private readonly IActionInFunctionSystemRepository _repoActionInFunctionSystem;
        private readonly IUserRoleRepository _repoUserRole;
        private readonly MapperConfiguration _configMapper;
        private readonly string[] Permissions = new string [] {"Action", "Action In Function", "Module", "Function"};
        public PermissionService(
            IPermissionRepository repoPermission,
            IMapper mapper,
            IActionRepository repoAction,
            IModuleRepository repoModule,
            IRoleRepository repoRole,
            IFunctionSystemRepository repoFunctionSystem,
            IActionInFunctionSystemRepository repoActionInFunctionSystem,
            IUserRoleRepository repoUserRole,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoAction = repoAction;
            _repoModule = repoModule;
            _repoRole = repoRole;
            _repoFunctionSystem = repoFunctionSystem;
            _repoActionInFunctionSystem = repoActionInFunctionSystem;
            _repoUserRole = repoUserRole;
            _repoPermission = repoPermission;
        }

        public async Task<bool> Add(Permission model)
        {
            var Permission = _mapper.Map<Permission>(model);

            _repoPermission.Add(Permission);
            return await _repoPermission.SaveAll();
        }



        public async Task<bool> Delete(object id)
        {
            var Permission = _repoPermission.FindById(id);
            _repoPermission.Remove(Permission);
            return await _repoPermission.SaveAll();
        }

        public async Task<bool> Update(Permission model)
        {
            var Permission = _mapper.Map<Permission>(model);
            _repoPermission.Update(Permission);
            return await _repoPermission.SaveAll();
        }


        public Permission GetById(object id) => _repoPermission.FindById(id);

        public async Task<List<FunctionSystem>> GetAllFunction()
        {
            var functions = await _repoFunctionSystem.FindAll().Include(x => x.Module).ToListAsync();
            return functions;
        }

        public async Task<List<Module>> GetAllModule()
        => await _repoModule.FindAll().OrderBy(x => x.Sequence).ToListAsync();

        public Task<ResponseDetail<object>> GetAllFunctionByPermision()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Permission>> GetAllAsync() => await _repoPermission.FindAll().ToListAsync();

        public Task<PagedList<Permission>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<Permission>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDetail<object>> UpdateModule(Module module)
        {
            _repoModule.Update(module);
            try
            {
                var result = await _repoModule.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }

        }

        public async Task<ResponseDetail<object>> DeleteModule(int moduleID)
        {
            var module = await _repoModule.FindAll(x => x.ID == moduleID).FirstOrDefaultAsync();
            _repoModule.Remove(module);
            try
            {
                var result = await _repoModule.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<ResponseDetail<object>> AddModule(Module module)
        {
            module.CreatedTime = DateTime.Now;
            _repoModule.Add(module);
            try
            {
                var result = await _repoModule.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<ResponseDetail<object>> UpdateFunction(FunctionSystem module)
        {
            _repoFunctionSystem.Update(module);
            try
            {
                var result = await _repoFunctionSystem.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<ResponseDetail<object>> DeleteFunction(int functionID)
        {
            var module = await _repoFunctionSystem.FindAll(x => x.ID == functionID).FirstOrDefaultAsync();
            _repoFunctionSystem.Remove(module);
            try
            {
                var result = await _repoFunctionSystem.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }

        }

        public async Task<ResponseDetail<object>> AddFunction(FunctionSystem module)
        {
            _repoFunctionSystem.Add(module);
            try
            {
                var result = await _repoFunctionSystem.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<ResponseDetail<object>> UpdateAction(Models.Action action)
        {
            _repoAction.Update(action);
            try
            {
                var result = await _repoAction.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<ResponseDetail<object>> DeleteAction(int actionID)
        {
            var action = await _repoAction.FindAll(x => x.ID == actionID).FirstOrDefaultAsync();
            _repoAction.Remove(action);
            try
            {
                var result = await _repoAction.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<ResponseDetail<object>> AddAction(Models.Action action)
        {
            _repoAction.Add(action);
            try
            {
                var result = await _repoAction.SaveAll();
                return new ResponseDetail<object> { Status = result };
            }
            catch (System.Exception ex)
            {
                return new ResponseDetail<object> { Message = ex.Message };
            }
        }

        public async Task<List<Models.Action>> GetAllAction() => await _repoAction.FindAll().ToListAsync();

        public async Task<IEnumerable<HierarchyNode<FunctionSystem>>> GetFunctionsAsTreeView()
        {
            var model = (await _repoFunctionSystem.FindAll().Include(x => x.Module).Include(x => x.Function).OrderBy(x => x.ID).OrderBy(x => x.ModuleID).ThenBy(x => x.Sequence).ToListAsync()).AsHierarchy(x => x.ID, y => y.ParentID);
            return model;
        }

        public async Task<object> GetMenuByUserPermission(int userId)
        {
            var roles = await _repoUserRole.FindAll(x => x.UserID == userId).Select(x => x.RoleID).ToArrayAsync();
            var query = from f in _repoFunctionSystem.FindAll().Include(x => x.Module)
                        join p in _repoPermission.FindAll()
                            on f.ID equals p.FunctionSystemID
                        join r in _repoRole.FindAll() on p.RoleID equals r.ID
                        join a in _repoAction.FindAll()
                            on p.ActionID equals a.ID
                        where roles.Contains(r.ID) && a.Code == "VIEW"
                        select new
                        {
                            Id = f.ID,
                            Name = f.Name,
                            Code = f.Code,
                            Url = f.Url,
                            Icon = f.Icon,
                            ParentId = f.ParentID,
                            SortOrder = f.Sequence,
                            Module = f.Module,
                            ModuleId = f.ModuleID
                        };
            var data = await query.Distinct()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToListAsync();
            return data.GroupBy(x => x.Module).Select(x => new
            {
                Module = x.Key.Name,
                Icon = x.Key.Icon,
                Url = x.Key.Url,
                Sequence = x.Key.Sequence,
                Children = x,
                HasChildren = x.Any()
            }).OrderBy(x => x.Sequence).ToList();
        }
        public async Task<ResponseDetail<object>> PostActionToFunction(int functionID, ActionAssignRequest request)
        {
            foreach (var actionId in request.ActionIds)
            {
                if (await _repoActionInFunctionSystem.FindAll(x => x.FunctionSystemID == functionID && x.ActionID == actionId).AnyAsync() != false)
                    return new ResponseDetail<object> { Status = false, Message = "This action has been existed in function" };
                var entity = new ActionInFunctionSystem
                {
                    ActionID = actionId,
                    FunctionSystemID = functionID
                };

                _repoActionInFunctionSystem.Add(entity);
            }

            try
            {
                var result = await _repoActionInFunctionSystem.SaveAll();
                return new ResponseDetail<object> { Status = true };
            }
            catch (System.Exception ex)
            {
                // TODO
                return new ResponseDetail<object> { Status = false, Message = ex.Message };
            }

            // tao role moi
        }
        public async Task<ResponseDetail<object>> PutPermissionByRoleId(int roleID, UpdatePermissionRequest request)
        {

            try
            {
                //create new permission list from user changed
                var newPermissions = new List<Permission>();
                foreach (var p in request.Permissions)
                {
                    newPermissions.Add(new Permission(roleID, p.ActionID, p.FunctionID));
                }
                var existingPermissions = await _repoPermission.FindAll(x => x.RoleID == roleID).ToListAsync();

                _repoPermission.RemoveMultiple(existingPermissions);
                await _repoPermission.SaveAll();

                _repoPermission.AddRange(newPermissions.DistinctBy(x => new { x.RoleID, x.ActionID, x.FunctionSystemID }).ToList());

                await _repoPermission.SaveAll();
                return new ResponseDetail<object> { Status = true };
            }
            catch (System.Exception ex)
            {
                // TODO
                return new ResponseDetail<object> { Status = false, Message = ex.Message };
            }

            // tao role moi
        }
        public async Task<ResponseDetail<object>> DeleteActionToFunction(int functionID, ActionAssignRequest request)
        {
            try
            {
                foreach (var actionId in request.ActionIds)
                {
                    var entity = await _repoActionInFunctionSystem.FindAll(x => x.FunctionSystemID == functionID && x.ActionID == actionId).FirstOrDefaultAsync();
                    if (entity == null)
                        return new ResponseDetail<object> { Status = false, Message = "This action is not existed in function" };

                    _repoActionInFunctionSystem.Remove(entity);
                }
                var result = await _repoPermission.SaveAll();
                return new ResponseDetail<object> { Status = true };
            }
            catch (System.Exception ex)
            {
                // TODO
                return new ResponseDetail<object> { Status = false, Message = ex.Message };
            }

            // tao role moi
        }

        public async Task<object> GetScreenAction(int functionID)
        {
            var query = from a in _repoAction.FindAll()
                        join f in _repoActionInFunctionSystem.FindAll(x => x.FunctionSystemID == functionID)
                                    .Include(x => x.FunctionSystem)
                            on a.ID equals f.ActionID
                        into af
                        from c in af.DefaultIfEmpty()
                        select new
                        {
                            Id = a.ID,
                            Name = a.Name,
                            FuncName = c != null ? c.FunctionSystem.Name : "N/A",
                            Status = c != null ? true : false,
                        };
            var data = await query.ToListAsync();
            return data;
        }
        public async Task<object> GetActionInFunctionByRoleID(int roleID) {
            var query = _repoPermission.FindAll(x => x.RoleID == roleID)
                .Include(x=> x.Functions)
                .Include(x=> x.Action)
                .Select(x => new {
                    x.Functions.Name,
                    FunctionCode = x.Functions.Code,
                    x.Functions.Url,
                    x.Action.Code,
                    x.ActionID
                });
                var data = (await query.ToListAsync()).GroupBy(x => new {x.Name, x.FunctionCode, x.Url})
                        .Select(x => new {
                            x.Key.Name,
                            x.Key.FunctionCode,
                            x.Key.Url,
                            Childrens = x
                        });
                return data;
        }
        public async Task<object> GetScreenFunctionAndAction(ScreenFunctionAndActionRequest request)
        {

            var roleID = request.RoleIDs;
            var permission = _repoPermission.FindAll();
            var query = _repoActionInFunctionSystem.FindAll()
                .Include(x => x.Action)
                .Include(x => x.FunctionSystem)
                .ThenInclude(x => x.Module)
                .Select(x => new
                {
                    Id = x.FunctionSystem.ID,
                    FunctionCode = x.FunctionSystem.Code,
                    Name = x.FunctionSystem.Name,
                    ActionName = x.Action.Name,
                    ActionID = x.Action.ID,
                    Module = x.FunctionSystem.Module,
                    ModuleCode = x.FunctionSystem.Module.Code,
                    ModuleNameID = x.FunctionSystem.Module.ID,
                    Code = x.Action.Code,
                })
                .Where(x => !Permissions.Contains(x.FunctionCode)); // Dieu kien nay de khong load nhung chuc nang he thong
          var model =  from t1 in query
                        from t2 in permission.Where(x => roleID.Contains(x.RoleID) && t1.Id == x.FunctionSystemID && x.ActionID== t1.ActionID)
                            .DefaultIfEmpty()
                        select new {
                            t1.Id,
                            t1.Name ,
                            t1.ActionName,
                            t1.ActionID,
                            t1.Code,
                            t1.Module,
                            Permission = t2
                        };
            var data = (await model.ToListAsync())
                        .GroupBy(x => x.Module)
                        .Select(x => new
                        {
                            ModuleName = x.Key.Name,
                            Sequence = x.Key.Sequence,
                            Fields = new
                            {
                                DataSource = x.GroupBy(s => new { s.Id, s.Name })
                                .Select(g => new
                                {
                                    Id = g.Key.Id,
                                    Name = g.Key.Name,
                                    Childrens = g
                                    .Select(a => new
                                    {
                                        ParentID = g.Key.Id,
                                        ID = $"{a.ActionID}_{g.Key.Id}_{roleID.FirstOrDefault()}",
                                        Name = a.ActionName,
                                        a.ActionID,
                                        FunctionID = g.Key.Id,
                                        a.ActionName,
                                        Status = a.Permission != null,
                                        // Status = permission.Any(p => roleID.Contains(p.RoleID) && a.ActionID == p.ActionID && p.FunctionSystemID == g.Key.Id) 
                                        // IsChecked = permission.Any(p => roleID.Contains(p.RoleID) && a.ActionID == p.ActionID && p.FunctionSystemID == g.Key.Id)

                                    }).ToList()
                                }),
                                Id = "id",
                                Text = "name",
                                Child = "childrens"
                            }
                        });
            return data.OrderBy(x=>x.Sequence).ToList();
        }
    }
}
