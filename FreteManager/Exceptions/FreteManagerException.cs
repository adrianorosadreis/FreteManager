using System;

namespace FreteManager.Exceptions
{
    // Exceção base para todas as exceções de negócio da aplicação
    public abstract class FreteManagerException : Exception
    {
        public string ErrorCode { get; }

        protected FreteManagerException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        protected FreteManagerException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    // Exceções específicas
    public class EntityNotFoundException : FreteManagerException
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public EntityNotFoundException(string entityName, object entityId)
            : base($"A entidade {entityName} com ID {entityId} não foi encontrada.", "ENTITY_NOT_FOUND")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }

    public class BusinessRuleViolationException : FreteManagerException
    {
        public BusinessRuleViolationException(string message)
            : base(message, "BUSINESS_RULE_VIOLATION")
        {
        }
    }

    public class UnauthorizedOperationException : FreteManagerException
    {
        public UnauthorizedOperationException(string message)
            : base(message, "UNAUTHORIZED_OPERATION")
        {
        }
    }

    public class DataIntegrityException : FreteManagerException
    {
        public DataIntegrityException(string message, Exception innerException = null)
            : base(message, "DATA_INTEGRITY_ERROR", innerException)
        {
        }
    }
}