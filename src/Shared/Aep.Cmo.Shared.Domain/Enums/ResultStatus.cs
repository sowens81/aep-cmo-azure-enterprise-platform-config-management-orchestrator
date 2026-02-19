using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aep.Cmo.Shared.Domain.Enums;

/// <summary>
/// Represents the outcome classification of a result.
/// </summary>
public enum ResultStatus
{
    Success = 0,

    // Business failures
    ValidationError = 1,
    NotFound = 2,
    Conflict = 3,

    // Security
    Unauthorized = 10,
    Forbidden = 11,

    // Infrastructure
    TransientFailure = 50,
    PermanentFailure = 51,

    // Processing decisions
    Retry = 100,
    DeadLetter = 101,
    Ignore = 102
}