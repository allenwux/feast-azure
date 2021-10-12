using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Azure.Feast.Utils
{
    public class FeastCoreUnauthorizedUserException : FeastCoreUserException
    {
        public FeastCoreUnauthorizedUserException(string errorMessage,
            int errorCode,
            string traceId) :
            base(errorMessage, errorCode, HttpStatusCode.Unauthorized, traceId)
        {

        }
    }

    public class FeastCoreBadRequestUserException : FeastCoreUserException
    {
        public FeastCoreBadRequestUserException(string errorMessage,
            int errorCode,
            string traceId) :
            base(errorMessage, errorCode, HttpStatusCode.BadRequest, traceId)
        {

        }
    }

    public class FeastCoreConflictUserException : FeastCoreUserException
    {
        public FeastCoreConflictUserException(string errorMessage,
            int errorCode,
            string traceId) :
            base(errorMessage, errorCode, HttpStatusCode.Conflict, traceId)
        {

        }
    }

    public class FeastCoreNotFoundUserException : FeastCoreUserException
    {
        public FeastCoreNotFoundUserException(string errorMessage,
            int errorCode,
            string traceId) :
            base(errorMessage, errorCode, HttpStatusCode.NotFound, traceId)
        {

        }
    }

    public class FeastCoreUserException: BaseFeastCoreException
    {
        public FeastCoreUserException(string errorMessage, 
            int errorCode, 
            HttpStatusCode httpStatusCode, 
            string traceId) : 
            base(errorMessage, errorCode, httpStatusCode, traceId)
        {

        }
    }

    public class FeastCoreServerException : BaseFeastCoreException
    {
        public FeastCoreServerException(string errorMessage, string traceId) :
            base(errorMessage, -1, HttpStatusCode.InternalServerError, traceId)
        {
        }

        public override ContentResult GetHttpResult()
        {
            // Don't show error message to users for internal server error.
            var content = new FeastCoreServerException(ErrorMessages.INTERNAL_SERVER_ERROR, this.TraceId);

            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = "application/json",
                StatusCode = (int)this.HttpStatusCode
            };

            return result;
        }
    }

    public class BaseFeastCoreException : Exception
    {
        public BaseFeastCoreException(string errorMessage, 
            int errorCode, 
            HttpStatusCode httpStatusCode, 
            string traceId)
        {
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
            this.TraceId = traceId;
            this.HttpStatusCode = httpStatusCode;
        }

        public string ErrorMessage { get; set; }

        public int ErrorCode { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string TraceId { get; set; }

        public override string ToString()
        {
            return $"Message: {ErrorMessage}. Error Code: {ErrorCode}.";
        }

        public virtual ContentResult GetHttpResult()
        {
            var result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(this),
                ContentType = "application/json",
                StatusCode = (int)this.HttpStatusCode
            };

            return result;
        }
    }

    public class ExceptionUtils
    {
        public static ContentResult HandleException(Exception ex, ILogger logger, FeastCoreRequestHeaders headers)
        {
            BaseFeastCoreException feastEx = null;
            if (ex is BaseFeastCoreException)
            {
                feastEx = (BaseFeastCoreException)ex;
            }
            else
            {
                // If it is not a know type of exception, convert it into a server exception.                    
                feastEx = new FeastCoreServerException(ex.Message, headers.TraceId);
            }

            logger.LogError(feastEx.ToString());
            return feastEx.GetHttpResult();

        }
    }
}
