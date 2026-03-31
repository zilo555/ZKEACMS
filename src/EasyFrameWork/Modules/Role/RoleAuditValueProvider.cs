using Easy.AuditTrail;
using System;
using System.Reflection;

namespace Easy.Modules.Role
{
    public class RoleAuditValueProvider : IAuditValueProvider
    {
        private readonly IRoleService _roleService;

        public RoleAuditValueProvider(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public int Priority => 10;

        public bool CanHandle(PropertyInfo property, Type entityType)
        {
            return property.Name == nameof(UserRoleRelation.RoleID) && entityType == typeof(UserRoleRelation);
        }

        public string GetDisplayValue(PropertyInfo property, object rawValue)
        {
            if (rawValue is int roleId)
            {
                var role = _roleService.Get(roleId);
                if (role != null)
                {
                    return role.Title;
                }
                return rawValue.ToString();
            }

            return rawValue?.ToString() ?? string.Empty;
        }
    }
}
