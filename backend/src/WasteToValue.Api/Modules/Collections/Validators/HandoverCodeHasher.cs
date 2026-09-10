using System.Security.Cryptography;
using System.Text;

namespace WasteToValue.Api.Modules.Collections.Validators;

public static class HandoverCodeHasher
{
    public static string GeneratePlainCode()
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000);
        return code.ToString("D6");
    }

    public static string FormatVerificationString(Guid pickupId, string plainCode, TimeSpan? validDuration = null)
    {
        var duration = validDuration ?? TimeSpan.FromHours(24);
        var expiresAt = DateTimeOffset.UtcNow.Add(duration);
        var hash = ComputeHash(pickupId, plainCode);
        return $"{hash}|{expiresAt.ToUnixTimeSeconds()}|0|0";
    }

    public static HandoverVerificationResult Verify(Guid pickupId, string? storedString, string inputCode, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(storedString))
        {
            return new HandoverVerificationResult(false, "No verification code exists for this pickup.", null);
        }

        var parts = storedString.Split('|');
        if (parts.Length != 4)
        {
            // Legacy / simple plain text code fallback
            if (storedString.Equals(inputCode, StringComparison.OrdinalIgnoreCase))
            {
                return new HandoverVerificationResult(true, null, "USED");
            }
            return new HandoverVerificationResult(false, "Invalid one-time code.", storedString);
        }

        var storedHash = parts[0];
        if (!long.TryParse(parts[1], out var expiryUnix))
        {
            return new HandoverVerificationResult(false, "Invalid verification code metadata.", storedString);
        }

        if (!int.TryParse(parts[2], out var failedAttempts))
        {
            failedAttempts = 0;
        }

        var isUsed = parts[3] == "1";

        if (isUsed)
        {
            return new HandoverVerificationResult(false, "One-time code has already been used.", storedString);
        }

        if (failedAttempts >= 5)
        {
            return new HandoverVerificationResult(false, "Maximum verification attempts exceeded.", storedString);
        }

        if (now.ToUnixTimeSeconds() > expiryUnix)
        {
            return new HandoverVerificationResult(false, "One-time code has expired.", storedString);
        }

        var inputHash = ComputeHash(pickupId, inputCode);
        if (CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(storedHash), Encoding.UTF8.GetBytes(inputHash)))
        {
            // Successful verification: mark as used
            var updatedString = $"{storedHash}|{expiryUnix}|{failedAttempts}|1";
            return new HandoverVerificationResult(true, null, updatedString);
        }
        else
        {
            // Failed verification: increment attempts
            var newAttempts = failedAttempts + 1;
            var updatedString = $"{storedHash}|{expiryUnix}|{newAttempts}|0";
            var errMsg = newAttempts >= 5
                ? "Maximum verification attempts exceeded."
                : $"Invalid one-time code. ({5 - newAttempts} attempt(s) remaining)";
            return new HandoverVerificationResult(false, errMsg, updatedString);
        }
    }

    private static string ComputeHash(Guid pickupId, string code)
    {
        var rawBytes = Encoding.UTF8.GetBytes($"{pickupId}:{code.Trim()}");
        var hashBytes = SHA256.HashData(rawBytes);
        var hex = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return hex.Substring(0, 32);
    }
}

public sealed record HandoverVerificationResult(
    bool IsSuccess,
    string? ErrorMessage,
    string? UpdatedVerificationString);
