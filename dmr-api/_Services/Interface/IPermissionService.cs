using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Action = DMR_API.Models.Action;

namespace DMR_API._Services.Interface
{
   public interface IPermissionService : IECService<Permission>
    {
        Task<List<FunctionSystem>> GetAllFunction();
        Task<IEnumerable<HierarchyNode<FunctionSystem>>> GetFunctionsAsTreeView();
        Task<List<Module>> GetAllModule();

        Task<List<Action>> GetAllAction();

        Task<ResponseDetail<object>> GetAllFunctionByPermision();
        Task<ResponseDetail<object>> UpdateModule(Module module);
        Task<ResponseDetail<object>> DeleteModule(int moduleID);
        Task<ResponseDetail<object>> AddModule(Module module);

        Task<ResponseDetail<object>> UpdateFunction(FunctionSystem module);
        Task<ResponseDetail<object>> DeleteFunction(int functionID);
        Task<ResponseDetail<object>> AddFunction(FunctionSystem module);

        Task<ResponseDetail<object>> UpdateAction(Action module);
        Task<ResponseDetail<object>> DeleteAction(int actionID);
        Task<ResponseDetail<object>> AddAction(Action module);
        Task<ResponseDetail<object>> PutPermissionByRoleId(int roleID, UpdatePermissionRequest request);
        Task<ResponseDetail<object>> PostActionToFunction(int functionID, ActionAssignRequest request);
        Task<ResponseDetail<object>> DeleteActionToFunction(int functionID, ActionAssignRequest request);
        Task<object> GetMenuByUserPermission(int userId);
        Task<object> GetScreenAction(int functionID);
        Task<object> GetActionInFunctionByRoleID(int roleID);
        Task<object> GetScreenFunctionAndAction(ScreenFunctionAndActionRequest request);

    }
}
