using Aep.Cmo.Shared.Domain.Enums;

namespace Aep.Cmo.Shared.Domain.Results;

public readonly struct Result
{
    public bool IsSuccess => Status == ResultStatus.Success;

    public ResultStatus Status { get; }

    public string? Error { get; }

    public string? ErrorCode { get; }

    public bool IsRetryable =>
        Status is ResultStatus.Retry or ResultStatus.TransientFailure;

    public bool ShouldDeadLetter =>
        Status is ResultStatus.DeadLetter
        or ResultStatus.PermanentFailure
        or ResultStatus.ValidationError;

    private Result(
        ResultStatus status,
        string? error,
        string? errorCode)
    {
        Status = status;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() =>
        new(ResultStatus.Success, null, null);

    public static Result Failure(
        ResultStatus status,
        string error,
        string? errorCode = null)
    {
        if (status == ResultStatus.Success)
            throw new ArgumentException(
                "Failure result cannot have Success status.",
                nameof(status));

        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new(status, error, errorCode);
    }
}
