using System.Text.RegularExpressions;

namespace WasteToValue.Recovery.Tests.Metadata;

public sealed class RecoveryReasoningSourceTests
{
    [Fact]
    public void Member_two_source_and_documentation_have_no_recognizable_credentials()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "docs", "ownership.md")))
            root = root.Parent;
        Assert.NotNull(root);
        var patterns = new[]
        {
            @"AIza[0-9A-Za-z_-]{35}",
            @"AKIA[0-9A-Z]{16}",
            @"-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----",
            @"(?i)(?:postgres|postgresql)://[^\s/:]+:[^\s@]+@",
            "(?i)(?:api[_-]?key|password|client[_-]?secret)\\s*[:=]\\s*[\"'][A-Za-z0-9/+_-]{20,}"
        };
        var folders = new[]
        {
            "agents/recovery", "backend/src/WasteToValue.Api/Modules/Recovery",
            "backend/tests/Recovery", "docs/members/member-2",
            "frontend/src/modules/recovery", "mobile/lib/features/recovery"
        };
        var extensions = new HashSet<string> { ".cs", ".md", ".json", ".js", ".jsx", ".dart", ".css", ".ps1" };
        var suspiciousFiles = new List<string>();
        foreach (var folder in folders)
        foreach (var file in Directory.EnumerateFiles(Path.Combine(root!.FullName, folder), "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root.FullName, file);
            if (relative.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj" or ".artifacts") ||
                !extensions.Contains(Path.GetExtension(file))) continue;
            var text = File.ReadAllText(file);
            if (patterns.Any(pattern => Regex.IsMatch(text, pattern)))
                suspiciousFiles.Add(relative);
        }
        // Report only filenames, never matched secrets.
        Assert.True(suspiciousFiles.Count == 0, "Potential credentials in: " + string.Join(", ", suspiciousFiles));
    }
}
