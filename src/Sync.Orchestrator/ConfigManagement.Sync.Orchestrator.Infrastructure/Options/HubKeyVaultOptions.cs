using ConfigManagement.Shared.KeyVault.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.Options;

public class HubKeyVaultOptions : IKeyVaultOptions
{
    public required string KeyVaultUri { get; init; }
    public static readonly string ConfigKey = String.Empty;
}