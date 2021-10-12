using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Utils
{
    public class ErrorMessages
    {
        public static readonly string RBAC_ALREADY_INITIALIZED = "There's already one or more admins in the service. Please manage user permissions using the admin AAD account.";
        public static readonly string USER_PERMISSION_ALREADY_EXIST = "The user permission with user id {0}, project {1}, role {2} already exists.";
        public static readonly string USER_PERMISSION_NOT_FOUND = "The user permission with user id {0}, project {1}, role {2} does not exist or you don't have permission to access it.";

        public static readonly string FEATURE_VIEW_ALREADY_EXIST = "The feature view with name {0} already exists in project {1}.";
        public static readonly string FEATURE_VIEW_NOT_FOUND = "The feature view with name {0} does not exist in project {1} or you don't have permission to access it.";

        public static readonly string FEATURE_SERVICE_ALREADY_EXIST = "The feature service with name {0} already exists in project {1}.";
        public static readonly string FEATURE_SERVICE_NOT_FOUND = "The feature service with name {0} does not exist in project {1} or you don't have permission to access it.";

        public static readonly string ENTITY_ALREADY_EXIST = "The entity with name {0} already exists in project {1}.";
        public static readonly string ENTITY_NOT_FOUND = "The entity with name {0} does not exist in project {1} or you don't have permission to access it.";

        public static readonly string PROJECT_ALREADY_EXIST = "The project with name {0} already exists.";
        public static readonly string PROJECT_NOT_FOUND = "The project with name {0} does not exist or you don't have permission to access it.";
        public static readonly string PROJECT_CAN_NOT_BE_DELETED = "The project with name {0} contains other objects and can not be deleted. Delete all objects and try again.";

        public static readonly string VALUE_DOES_NOT_MATCH = "Value of {0} in the URL doesn't match the value in request body.";
        public static readonly string MISSING_PARAMETER = "The value of required parameter {0} is not provided.";
        public static readonly string INTERNAL_SERVER_ERROR = "The server encountered an internal error and was unable to complete your request.";

    }

    public class ErrorUtils
    {
        public static string FormatErrorMessage(string format, params object?[] args)
        {
            return string.Format(format, args);
        }
    }

    public enum ErrorCode
    {
        OBJECT_ALREDAY_EXIST = 100,
        VALUE_DOES_NOT_MATCH = 101,
        OBJECT_NOT_FOUND = 102,
        MISSING_PARAMETER = 103,
        RBAC_ALREADY_INITIALIZED = 104,
        PROJECT_NOT_EMPTY = 105,
        // Add new error codes below

        // Add new error codes above
        UNKNOWN = 900,
    }
}
